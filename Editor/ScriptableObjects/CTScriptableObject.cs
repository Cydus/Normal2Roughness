using UnityEngine;
using System;
using UnityEditor;

namespace Normal2Roughness
{
    /// <summary>
    /// Scriptable object for serializing composite textures
    /// </summary>
    public class CTScriptableObject
    {
        private static string packageName = "composite_textures_DATA";
        private static string containingFolder = "Editor Default Resources";

        public static CompositeTexture CreateMyAsset(string p_id, Texture2D p_textureA, Texture2D p_textureB, float p_strength, CompositeModes mode)
        {
            CompositeTexture asset = ScriptableObject.CreateInstance<CompositeTexture>();
            //asset.setValues(texA, texB, str, compositeMode);

            asset.setValues(p_id, p_textureA, p_textureB, p_strength, mode);

            string g = Guid.NewGuid().ToString();
            string name = g + ".asset";

            CreateFolders();
            AssetDatabase.CreateAsset(asset, "Assets/" + containingFolder + "/" + packageName + "/" + name);
            AssetDatabase.SaveAssets();

            return asset;
        }
        private static void CreateFolders()
        {
            bool status = AssetDatabase.IsValidFolder("Assets/" + containingFolder + "/" + packageName);
            EDebug.Log("status is" + status);
            if (status == false)
            {
                if (AssetDatabase.IsValidFolder("Assets/" + containingFolder) == false)
                {
                    AssetDatabase.CreateFolder("Assets", containingFolder);
                }

                if (AssetDatabase.IsValidFolder("Assets/" + containingFolder + "/" + packageName) == false)
                {
                    AssetDatabase.CreateFolder("Assets/" + containingFolder, packageName);
                }
            }
        }
    }
}