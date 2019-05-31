using UnityEngine;
using UnityEditor;

namespace Normal2Roughness
{
    /// <summary>
    /// Scriptable object boilerplate for creating CTScriptableObject
    /// </summary>
    public class CTScriptableObjectClass : ScriptableObject
    {
        public CompositeTexture tex = null;

        public void setValues(string p_texAid, string p_texBid, int str, CompositeModes mode)
        {
            tex.textureAid = p_texAid;
            tex.textureAid = p_texBid;
            tex.strength = str;
            tex.compositeMode = mode;
        }

        public void Destroy()
        {
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.DeleteAsset(path);
        }
    }
}