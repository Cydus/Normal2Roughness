using UnityEngine;
using UnityEditor;

namespace Normal2Roughness
{
    class PreProcessCompositeTexture : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            EDebug.Log("onPreProcessTexture " + assetPath);

            TextureImporter ti = (TextureImporter)assetImporter;

            if (CompositeTextureData.ExistsByPathA(ti.assetPath) &&
                CompositeTextureData.getTextureByAPath(ti.assetPath).enabled)
            {
                ti.isReadable = true;
                ti.mipmapEnabled = true;
            }
        }
    }

    public class PostprocessCompositeTexture : AssetPostprocessor
    {
        CompositeTexture ct;

        void OnPostprocessTexture(Texture2D texture)
        {
            bool isCompositeTexture = false;

            try
            {
                //id = CompositeTexture.assetToGUID(texture);
                int instanceID = AssetDatabase.LoadMainAssetAtPath(assetPath).GetInstanceID();
                EDebug.Log("OnPostProcessTexture " + instanceID);
                if (CompositeTextureData.ExistsByInstanceID(instanceID) && CompositeTextureData.getTexture(instanceID).enabled)
                {
                    EDebug.Log("is an enabled composite texture");
                    isCompositeTexture = true;
                    ct = CompositeTextureData.getTexture(instanceID);
                }
            }
            catch (System.Exception)
            {
                EDebug.Log("exception in texture processor");
            }

            if (!isCompositeTexture)
                return;

            EDebug.Log("do work");
            NormalToRoughness nr = new NormalToRoughness();

            ProcessTexture(ct, texture);
            ct.getTextureA().setReadable(false);

            // Instead of setting pixels for each mip map levels, you can also
            // modify only the pixels in the highest mip level. And then simply use
            // texture.Apply(true); to generate lower mip levels.
        }

        private void ProcessTexture(CompositeTexture ct, Texture2D texture)
        {

            NormalToRoughness ntr = new NormalToRoughness();
            ntr.generateNormalToRoughnessTextureNew(ct, texture);
        }
    }
}