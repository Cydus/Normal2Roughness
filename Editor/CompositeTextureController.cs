using UnityEditor;
using UnityEngine;



// Static Controller Class
// Note. Validation is done in this controller.
namespace Normal2Roughness
{
    [ExecuteInEditMode]
    [InitializeOnLoad]
    static class CompositeTextureController
    {
        // Use this for initialization

        public static string createCompositeTexture(string p_id, Texture2D p_textureA, Texture2D p_textureB, float p_strength, CompositeModes mode)
        {
            float strength = p_strength;
            if (p_textureA.mipmapCount > p_textureB.mipmapCount)
            {
                int sizeDelta = p_textureA.mipmapCount - p_textureB.mipmapCount;
                strength = 1 + (.5f * sizeDelta);
            }

            CompositeTextureData.addTexture(p_id, p_textureA, p_textureB, strength, mode);
            reimportCompositeTexture(p_id);

            return "COMPOSITE TEXTURE ADDED";
        }

        public static void updateCompositeTexture(string id, string p_textureBID, float p_strength, CompositeModes mode)
        {

            CompositeTextureData.updateTexture(id, p_textureBID, p_strength, mode);
        }

        public static void enableTexture(string p_id, bool p_enabled)
        {
            CompositeTextureData.enableTexture(p_id, p_enabled);
            reimportCompositeTexture(p_id);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        public static void removeTexture(CompositeTexture p_ct)
        {
            Texture2D texA = p_ct.getTextureA();
            CompositeTextureData.removeTexture(p_ct);
            string assetPath = AssetDatabase.GetAssetPath(texA);
            updateAssetUserData(assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private static void reimportCompositeTexture(string p_id)
        {
            Texture2D texA = CompositeTextureData.getTexture(p_id).getTextureA();

            string assetPath = AssetDatabase.GetAssetPath(texA);
            updateAssetUserData(assetPath);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        public static void updateAssetUserData(string path)
        {
            AssetImporter ai = AssetImporter.GetAtPath(path);
            //update metafile to flag the texture for reimport on shared projects
            System.Guid guid = System.Guid.NewGuid();
            ai.userData = guid.ToString();
        }
    }

    public static class TextureExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static bool isReadable(this Texture2D tex)
        {
            string assetPath = AssetDatabase.GetAssetPath(tex);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            EDebug.Log("READABLE " + assetPath + " || " + tImporter.isReadable);
            return tImporter.isReadable;

        }

        public static void setReadable(this Texture2D tex, bool setting)
        {
            if (null == tex) return;

            string assetPath = AssetDatabase.GetAssetPath(tex);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = setting;

                //AssetDatabase.ImportAsset(assetPath);
                //AssetDatabase.Refresh();
            }
        }

        public static string validateCT(string p_id, Texture2D p_textureA, Texture2D p_textureB, float p_strength, CompositeModes mode)
        {
            return null;
        }

        //public static string ConvertWhitespacesToSingleSpaces(this string value)
        //{
        //    return Regex.Replace(value, @"\s+", " ");
        //}
    }
}