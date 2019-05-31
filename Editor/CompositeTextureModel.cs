using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Normal2Roughness
{
    [Serializable]
    class CompositeTextureList
    {
        public List<CompositeTexture> compositeTextures;
        public CompositeTextureList()
        {
            compositeTextures = new List<CompositeTexture>();
        }
    }

    public static class CompositeTextureData
    {
        static CompositeTextureList csList = new CompositeTextureList();

        public static UnityEvent texturesChanged = new UnityEvent();

        public static List<CompositeTexture> getTextures()
        {
            Load();
            return csList.compositeTextures;
        }

        public static void addTexture(string p_id, Texture2D p_textureA, Texture2D p_textureB, float p_strength, CompositeModes mode)
        {
            CompositeTexture tex = CTScriptableObject.CreateMyAsset(p_id, p_textureA, p_textureB, p_strength, mode);
            if (validateTexture(tex))
            {
                csList.compositeTextures.Add(tex);
                //csList.compositeTextures.Sort();
                texturesChanged.Invoke();
                //Save();
            } else
            {
                EDebug.Log("texture invalid");
            }
        }

        public static void removeTexture(CompositeTexture ct)
        {
            //CompositeTexture texture = csList.compositeTextures.Find(x => x.id == idToRemove);
            CompositeTexture tex = csList.compositeTextures.Find(x => x.id == ct.id);
            tex.Destroy();
            csList.compositeTextures.Remove(tex);
            texturesChanged.Invoke();
        }

        public static void updateTexture(string id, string p_textureBID, float p_strength, CompositeModes mode)
        {
            CompositeTexture tex = csList.compositeTextures.Find(x => x.id == id);
            tex.textureBid = p_textureBID;
            tex.strength = p_strength;
            tex.compositeMode = mode;
            texturesChanged.Invoke();
            EditorUtility.SetDirty(tex);
            AssetDatabase.SaveAssets();
        }

        public static void enableTexture(string id, bool state)
        {
            CompositeTexture tex = csList.compositeTextures.Find(x => x.id == id);
            tex.enabled = state;
            texturesChanged.Invoke();
            EditorUtility.SetDirty(tex);
            AssetDatabase.SaveAssets();
        }

        public static CompositeTexture getTexture(string id)
        {
            CompositeTexture cts = csList.compositeTextures.Find(x => x.id == id);
            return cts;
        }

        public static CompositeTexture getTexture(int id)
        {
            CompositeTexture cts = csList.compositeTextures.Find(x => x.getInstanceID() == id);
            return cts;
        }

        public static CompositeTexture getTextureByAPath(string path)
        {
            CompositeTexture cts = csList.compositeTextures.Find(x => x.getPath() == path);
            return cts;
        }

        public static bool Exists(string id)
        {
            bool exists = csList.compositeTextures.Exists(x => x.id == id);
            return exists;
        }

        public static bool ExistsByInstanceID(int id)
        {
            bool exists = csList.compositeTextures.Exists(x => x.getInstanceID() == id);
            return exists;

        }

        public static bool ExistsByPathA(string path)
        {
            bool exists = csList.compositeTextures.Exists(x => x.existsAtPathA(path));
            return exists;
        }

        public static void Load()
        {
            var socList = AssetDatabase.FindAssets("t:CompositeTexture");

            //EDebug.Log("resources found " + socList.Count());

            csList.compositeTextures.Clear();
            foreach (var guid in socList)
            {
                String path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object soc = AssetDatabase.LoadAssetAtPath(path, typeof(CompositeTexture));

                CompositeTexture ct = soc as CompositeTexture;
                csList.compositeTextures.Add(ct);
            }
        }

        public static void CTnamechanged() //@todo, call this from somewhere
        {
            csList.compositeTextures.Sort();
        }

        public static bool validateTexture(CompositeTexture tex)
        {
            bool exists = csList.compositeTextures.Exists(x => x.id == tex.id);
            return !exists;
        }
    }

    public enum CompositeModes
    {
        normalToRoughnessAlpha,
        normalToRoughnessRGB,
        standardShaderSpecularSetup,
    }
}