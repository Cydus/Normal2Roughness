using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Normal2Roughness
{
    [InitializeOnLoad]
    public class FileModificationWarning : UnityEditor.AssetModificationProcessor
    {
        static bool canSave = true;
        static string[] seenPaths = new string[0];

        static IDictionary matPriors = new Dictionary<String, MaterialPrior>();

        static FileModificationWarning()
        {

            //EDebug.Log("populating priors");
            populateMatPriors(); // run once!
            EditorApplication.update += Update;
        }

        static void populateMatPriors()
        {
            matPriors.Clear();


            string[] matGuids = AssetDatabase.FindAssets("t:Material");
            foreach (string guid in matGuids)
            {
                Material mat = (Material)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Material));
                int? rTextureID = getTextureIDFromMat(mat, "_SpecGlossMap");
                int? nTextureID = getTextureIDFromMat(mat, "_BumpMap");
                String matID = guid;

                try
                {
                    matPriors.Add(matID, new MaterialPrior(matID, rTextureID, nTextureID));
                }
                catch (Exception)
                { //@todo inelegant
                  //EDebug.Log(e);
                  // EDebug.Log(mat.name + " " + matID + " " + AssetDatabase.GUIDToAssetPath(guid));

                }
            }
        }

        static int? getTextureIDFromMat(Material mat, String mapname)
        {
            int? textureID = null;
            try
            {
                if (mat.HasProperty(mapname))
                {
                    textureID = (mat.GetTexture(name: mapname) as Texture2D).GetInstanceID();
                }
            }
            catch (Exception) // no texture assigned?
            {
                textureID = null;
            }
            return textureID;
        }

        static string[] OnWillSaveAssets(string[] paths)
        {

            if (canSave)
            {
                return paths;
            }
            else
            {

                //foreach (string path in paths)
                //{
                //    EDebug.Log(path);
                //}

                var myPaths = paths.Except(seenPaths);

                foreach (string path in paths)
                {

                    if (path.EndsWith(".mat"))
                    {
                        //EDebug.Log(path);
                        Material mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
                        string matID = AssetDatabase.AssetPathToGUID(path);


                        int? rTextureID = null;
                        int? nTextureID = null;

                        rTextureID = getTextureIDFromMat(mat, "_SpecGlossMap");
                        nTextureID = getTextureIDFromMat(mat, "_BumpMap");


                        if (matPriors.Contains(matID))
                        {
                            if (rTextureID != (((MaterialPrior)matPriors[matID]).rTextureID))
                            {
                                EDebug.Log("ROUGHNESS changed @" + mat.name);
                            }
                            if (nTextureID != (((MaterialPrior)matPriors[matID]).nTextureID))
                            {
                                EDebug.Log("NORMAL changed @" + mat.name);
                            }
                        }


                        // log this as a prior
                        matPriors.Remove(matID);
                        matPriors.Add(matID, new MaterialPrior(matID, rTextureID, nTextureID));

                        //EDebug.Log(rTextureID + " " + nTextureID);

                    }
                }

                // these are our UNSAVED CHANGES in the project.
                // these operations occour between actaul saves...
                // check if either the normal or roughness map was changed
                // 

                foreach (string path in myPaths)
                {
                    EDebug.Log("watching for changes @" + path);
                }

                seenPaths = paths;


                string[] empty = new string[0];

                return empty;
            }
        }

        static void Update()
        {
            canSave = false;
            AssetDatabase.SaveAssets();
            canSave = true;
        }

        //static bool IsOpenForEdit(string strA, string strB)
        //{
        //    EDebug.Log(strA);

        //    return true;
        //}
    }

    public class MaterialPrior
    {
        public String materialID;
        public int? rTextureID;
        public int? nTextureID;

        public MaterialPrior(String p_materialID, int? p_rTextureID, int? p_nTextureID)
        {
            materialID = p_materialID;
            rTextureID = p_rTextureID;
            nTextureID = p_nTextureID;
        }
    }
}