using System;
using UnityEngine;

namespace Normal2Roughness
{
    /// <summary>
    /// Optimized data structures for processing normal to roughness conversion.
    /// </summary>
    public struct TexelIndex
    {
        public TexelLevel[] texelLevels;

        public TexelIndex(Color[] texColors)
        {
            int mipLevels = (int)(Math.Log(texColors.Length) / Math.Log(4.0));
            EDebug.Log("TI miplevels " + mipLevels);

            int pixelCount = texColors.Length;
            //setup texels
            texelLevels = new TexelLevel[mipLevels];
            for (int i = 0; i < texelLevels.Length; i++)
            {
                texelLevels[i] = new TexelLevel(pixelCount);
                pixelCount /= 4;
            }

            int sideDimension = (int)Math.Sqrt(texColors.Length); //256 becomes 16 , the side dimension

            for (int i = 0; i < texColors.Length; i += sideDimension * 2)
            {
                for (int j = 0; j < sideDimension; j += 2)
                {
                    int l = j + i;

                    Color[] targetC = new Color[4];
                    targetC[0] = texColors[l];
                    targetC[1] = texColors[l + 1];
                    targetC[2] = texColors[l + sideDimension];
                    targetC[3] = texColors[l + sideDimension + 1];

                    Texel tex = new TexelCalculation(targetC).getTexel();

                    texelLevels[0].addTexel(tex);
                }
            }

            TexelOps to = new TexelOps();
            Texel[] target = new Texel[4];

            for (int i = 0; i < texelLevels.Length - 1; i++) // for each level (except the last one)
            {
                TexelLevel texelLevel = texelLevels[i];
                int texelCount = texelLevels[i].getTexelCount();
                int mipSideDimension = (int)Math.Sqrt(texelCount);

                int capacity = (mipSideDimension ^ 2) / 4;
                //Debug.Log("mip side dimension :  " + mipSideDimension * 2);


                // combine 4 texels together and add the resulting texel to the next level
                for (int j = 0; j < texelCount; j += mipSideDimension * 2)
                {
                    for (int k = 0; k < mipSideDimension; k += 2)
                    {
                        int l = j + k;

                        target[0] = texelLevel.getTexel(l);
                        target[1] = texelLevel.getTexel(l + 1);
                        target[2] = texelLevel.getTexel(l + mipSideDimension);
                        target[3] = texelLevel.getTexel(l + mipSideDimension + 1);

                        Texel combinedTex = to.CombineTexels(target);

                        // add combined texel to next 
                        texelLevels[i + 1].addTexel(combinedTex);

                        capacity++;
                    }
                }
            }
        }
    }

    public class TexelLevel
    {
        Texel[] texels;
        int index = 0;
        int capacity;

        public TexelLevel(int p_capacity)
        {
            capacity = p_capacity;
           texels = new Texel[p_capacity];
        }
        public void addTexel(Texel tex)
        {
            texels[index] = tex;
            index++;
        }

        public Texel getTexel(int i)
        {
            return texels[i];
        }

        public int getTexelCount()
        {
            return capacity;
        }

        public float[] getStdDevs()
        {
            float[] values = new float[capacity];
            for (int i = 0; i < capacity; i++)
            {
                values[i] = texels[i].getStdDev();
            }
            return values;
        }
    }

    public class TexelCalculation
    {
        public float sum; // sum
        public float S;
        public int k; //number of elements
        public float M; //mean

        public float sumComponents;

        public TexelCalculation(Color[] arr)
        {
            sum = 0.0f;
            S = 0.0f;
            k = 1;
            M = 0.0f;
            sumComponents = 0;

            //@todo, only this matters: Texel testTexlA = new Texel(texA.k, texA.sum, texA.sumComponents);

            int length = arr.Length;
            for (int i = 0; i < length; i++)
            {
                float tmpM = M;
                float blue = arr[i].b;
                sum += blue;
                //sumComponents += Mathf.Pow(blue, 2.0f);
                sumComponents += blue * blue;
                M += (blue - tmpM) / k;
                S += (blue - tmpM) * (blue - M);
                k++;
            }
            //stdDev = Mathf.Sqrt(S / length);
            //EDebug.Log("S: " + S + " k:" + k + " M:" + M);
        }

        public Texel getTexel()
        {
            return new Texel(k, sum, sumComponents);
        }
    }
    /// <summary>
    /// Texel Ops. Used to combine prior texels from higher mips
    /// with texels belonging to lower mips.
    /// </summary>
    public struct TexelOps
    {
        public float CombineCombineCalc(TexelCalculation[] calcs)
        {
            float cN = 0.0f;
            float cS = 0.0f;
            float cSumC = 0.0f;

            int length = calcs.Length;
            for (int i = 0; i < length; i++)
            {
                cN += calcs[i].k - 1.0f;
                cS += calcs[i].sum;
                cSumC += calcs[i].sumComponents;
            }

            float cA = cS / cN;

            //EDebug.Log("doing a combined: SUM: " + cS + " cSum: " + cSumC + " cAvg: " + cA + " cNumber of elements : " + cN);

            float stdDev = 1.0f / cN * cSumC - Mathf.Pow(cA, 2.0f);
            //EDebug.Log(stdDev);
            return Mathf.Sqrt(stdDev);
        }

        public Texel CombineTexels(Texel[] texels)
        {
            float cN = 0.0f;
            float cS = 0.0f;
            float cSumC = 0.0f;

            int length = texels.Length;
            for (int i = 0; i < length; i++)
            {
                cN += texels[i].k - 1;
                cS += texels[i].sum;
                cSumC += texels[i].cSumC;
            }

            return new Texel(cN + 1, cS, cSumC); //@todo, does +1 need to scale with size scale here? ... NOPE!
        }
    }

    /// <summary>
    /// Represents a Texel and its Components
    /// </summary>
    public struct Texel
    {
        public float k;
        public float sum;
        public float cSumC;

        public Texel(float p_k, float p_sum, float p_cSumC)
        {
            k = p_k; sum = p_sum; cSumC = p_cSumC;
        }

        // Obtain standard deviation (of normals)
        public float getStdDev()
        {
            float cS = sum; //@todo remove
            float cN = k - 1.0f;
            float cA = cS / cN;
            float stdDev = 1.0f / cN * cSumC - Mathf.Pow(cA, 2.0f);
            return Mathf.Sqrt(stdDev);
        }
    }
}