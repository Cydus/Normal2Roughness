using System;
using UnityEditor;
using UnityEngine;

namespace Normal2Roughness
{
    /// <summary>
    /// Datastructure for storing composite textures. (2 textures that are to be combined somehow)
    /// </summary>
    public class CompositeTexture : ScriptableObject
    {
        public string id;
        public bool enabled = true;
        public string textureAid;
        public string textureBid;
        public float strength;
        public CompositeModes compositeMode;

        public void setValues(string p_id, Texture2D p_textureA, Texture2D p_textureB, float p_strength, CompositeModes mode)
        {
            id = p_id; textureAid = assetToGUID(p_textureA); textureBid = assetToGUID(p_textureB); strength = p_strength; compositeMode = mode;
        }

        public void Destroy()
        {
            string path = AssetDatabase.GetAssetPath(this);
            AssetDatabase.DeleteAsset(path);
        }

        public Texture2D getTextureA()
        {
            return loadAssetAtGUID(textureAid);
        }

        public Texture2D getTextureB()
        {
            return loadAssetAtGUID(textureBid);
        }

        public void update(string p_textureBID, float p_strength, CompositeModes mode)
        {
            textureBid = p_textureBID;
            strength = p_strength;
            compositeMode = mode;

            CompositeTextureData.texturesChanged.Invoke();
        }

        public int CompareTo(CompositeTexture other)
        {
            string A = loadAssetAtGUID(textureAid).name;
            string oB = loadAssetAtGUID(other.id).name;

            return A.CompareTo(oB);
        }

        public int getInstanceID()
        {
            return loadAssetAtGUID(textureAid).GetInstanceID();
        }

        public String getPath()
        {
            return AssetDatabase.GUIDToAssetPath(textureAid);
        }

        public bool existsAtPathA(string path)
        {
            bool result = false;
            string assetAPath = AssetDatabase.GUIDToAssetPath(textureAid);
            if (path == assetAPath)
                result = true;
            return result;
        }

        public bool filesExist() //@todo, THIS IS CURRENTLY BEING USED TO HIDE INVALID CTs, NOT REMOVE THEM
        {
            bool existance = true;
            if (this.getTextureA() == null || this.getTextureB() == false)
            {
                existance = false;
                this.Destroy();
            }

            return existance;
        }

        public static Texture2D loadAssetAtGUID(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
            return tex;
        }

        public static string assetToGUID(Texture2D tex)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(tex));
            return guid;
        }
    }
}