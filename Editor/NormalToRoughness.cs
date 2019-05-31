using System;
using UnityEngine;

namespace Normal2Roughness
{
    /// <summary>
    /// Texutre processing techniques and logic.
    /// </summary>
    public class NormalToRoughness
    {
        delegate Color blendingTechnqiue(Color roughness, float normalStdDev);
        blendingTechnqiue bt;
        TexelIndex texelIndex;

        // the latest method for generating 
        public void generateNormalToRoughnessTextureNew(CompositeTexture ct, Texture2D texC)
        {
            Texture2D texA = texC;
            Texture2D texB = ct.getTextureB();

            // handle user deleting normalmap texture in GUI
            if (texA == null || texB == null)
                return;

            Texture2D LoadedImage = new Texture2D(texB.width, texB.height, texB.format, true);
            LoadedImage.LoadRawTextureData(texB.GetRawTextureData());

            // resize the textures if unevenly matched
            if (texB.mipmapCount < texA.mipmapCount)
            {
                EDebug.Log("sizing " + texA.width);

                // RGBA32, ARGB32, RGB24, RGBAFloat or RGBAHalf
                texB.filterMode = FilterMode.Trilinear;
                RenderTexture rt = RenderTexture.GetTemporary(texA.width, texA.height);
                rt.filterMode = FilterMode.Trilinear;
                RenderTexture.active = rt;
                Graphics.Blit(texB, rt);
                Texture2D nTex = new Texture2D(texA.width, texA.height, TextureFormat.ARGB32, false);
                nTex.ReadPixels(new Rect(0, 0, texA.width, texA.height), 0, 0);
                RenderTexture.active = null;
                texB = nTex;
            }
            else
            {
                texB = LoadedImage; // this makes the texture readable
            }

            int mipLevels = texA.mipmapCount;

            texelIndex = new TexelIndex(texB.GetPixels());

            // do different things depending on blending mode requested
            if (ct.compositeMode == CompositeModes.normalToRoughnessAlpha)
            {

                bt = delegate (Color roughness, float normalStdDev) // standard shader roughness mode calculation
                {
                    normalStdDev *= ct.strength;
                    return new Color(roughness.r + normalStdDev, roughness.g + normalStdDev, roughness.b + normalStdDev, 1.0f);
                };
            }
            else if (ct.compositeMode == CompositeModes.normalToRoughnessRGB)
            {

                bt = delegate (Color roughness, float normalStdDev) // standard shader w/ metalic alpha
                {
                    normalStdDev *= ct.strength;
                    return new Color(roughness.r, roughness.g, roughness.b, roughness.a - (2 * normalStdDev));
                };
            }
            else if (ct.compositeMode == CompositeModes.standardShaderSpecularSetup)
            {

                bt = delegate (Color roughness, float normalStdDev) // standard (specular setup)
                {
                    normalStdDev *= ct.strength;
                    return new Color(roughness.r, roughness.g, roughness.b, roughness.a - (2 * normalStdDev));
                //return new Color(roughness.r, roughness.g, roughness.b, roughness.a + (2 * normalStdDev));
            };
            }

            int mipDelta = texB.mipmapCount - texA.mipmapCount;
            EDebug.Log("mipdelta: " + mipDelta);

            int startingMipLevel = 1; // start from here for equally sized textures
            if (mipDelta > 0)
                startingMipLevel = 0;
            if (mipDelta < 0)
                mipDelta = 0;

            for (int mipLevel = startingMipLevel; mipLevel < mipLevels - mipDelta; mipLevel++)
            {
                int mipSize = texA.width / (int)Math.Pow(2, mipLevel);
                //EDebug.Log("generating rougness mipsize:" + mipSize);
                Color32[] values = generateNormalToRoughnessValues(mipSize, mipLevel, rTexture: texA, p_mipDelta: mipDelta);

                texC.SetPixels32(values, mipLevel);
            }
        }

        // Combines normals and roughness values for a given normal texturea and roughness texture and roughness mip level
        public Color32[] generateNormalToRoughnessValues(int mipSize, int rMipLevel, Texture2D rTexture, int p_mipDelta)
        {
            // NOTE you cannot use Texture2d.set pixels here, you must create actual on disk file, (DDS)

            int arraySize = mipSize * mipSize;

            Color32[] pixelList = new Color32[arraySize];

            for (int i = 0; i < arraySize; i++) // for each pixel in the mip
            {
                float value = texelIndex.texelLevels[rMipLevel - 1 + p_mipDelta].getTexel(i).getStdDev();
                Color baseR = getBaseRoughness3(i, rMipLevel, mipSize, rTexture); // 5ms

                pixelList[i] = bt(baseR, value);
            }

            //EDebug.Log("genearated roughness, pixels generated " + index + " for mipsize " + mipSize + "array size " + arraySize);
            return pixelList;
        }

        private Color getBaseRoughness3(int i, int mipLevel, int rMipSize, Texture2D rTexture)  //@TODO, BIG PROBLEM HERE WITH THE MIP LEVEL BEING A POOR PROXY
        {
            // this function takes 100ms for 4k texture
            int x = i % rMipSize;
            int y = i / rMipSize;

            return rTexture.GetPixels(x, y, blockWidth: 1, blockHeight: 1, miplevel: mipLevel)[0]; //@todo this wont work for alpha
        }
    }
}