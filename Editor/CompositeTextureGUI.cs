using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Normal2Roughness
{
    /// <summary>
    /// Editor Window for Normal2Roughness
    /// </summary>
    public class CompositeTextureGUI : EditorWindow
    {
        static List<Texture2D> normalMaps = new List<Texture2D>();

        Vector2 scrollPos;
        Vector2 lastPos = new Vector2(0f, 0f);
        Rect lastRect = new Rect(0f, 0f, 0f, 0f);

        Material matTemplate;
        Texture2D textureAToAdd;
        Texture2D textureBToAdd;
        CompositeModes cmToAdd = CompositeModes.normalToRoughnessAlpha;

        string lastAddedCT_ID = "";
        bool shouldScroll = false;
        String[] modesAsArray = Enum.GetNames(typeof(CompositeModes));

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Normal2Rougness")]
        static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow window = GetWindow(typeof(CompositeTextureGUI), false, "Normal2Rougness");
            window.Show();
        }

        void Awake()
        {
            CompositeTextureData.texturesChanged.AddListener(refresh);
        }

        static void refresh()
        {
            EDebug.Log("refresh requested by GUI");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Height(150f));

            GUILayout.Label("New Composite Texture", EditorStyles.boldLabel);

            matTemplate = EditorGUILayout.ObjectField("Import from material", matTemplate, typeof(Material), true) as Material;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 70;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Texture A", style);
            textureAToAdd = EditorGUILayout.ObjectField("", textureAToAdd, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70)) as Texture2D;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Texture B", style);

            textureBToAdd = EditorGUILayout.ObjectField("", textureBToAdd, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70)) as Texture2D;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Mode");
            cmToAdd = (CompositeModes)EditorGUILayout.Popup("", (int)cmToAdd, modesAsArray, GUILayout.Width(170));

            string texA_guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(textureAToAdd));

            // create texture
            bool texutresInPlace = false;
            if (textureAToAdd != null && textureBToAdd != null)
            {
                if (textureAToAdd.height == textureBToAdd.height && textureAToAdd.width == textureBToAdd.width)
                {
                    texutresInPlace = true;
                }
                else
                {
                    texutresInPlace = true;
                    EDebug.Log("textures not of same size");
                    EditorGUILayout.HelpBox("Texture dimensions don't match", type: MessageType.Info);
                }
            }

            EditorGUI.BeginDisabledGroup(texutresInPlace == false);

            if (GUILayout.Button("Create Composite", GUILayout.Width(170)))
            {
                string errorMsg = CompositeTextureController.createCompositeTexture(texA_guid, textureAToAdd, textureBToAdd, 1f, cmToAdd);

                matTemplate = null; // clear the previous fields after creating a new CT
                textureAToAdd = null;
                textureBToAdd = null;

                lastAddedCT_ID = texA_guid;
                //shouldScroll = true;

                EDebug.Log(errorMsg);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            ////////////////// MATERIAL TYPE DETECTION
            if (matTemplate != null)
            {
                textureBToAdd = (Texture2D)matTemplate.GetTexture("_BumpMap");

                String shaderName = matTemplate.shader.name;
                // shaderName Standard (Roughness setup), Standard, Standard (Specular setup)
                // 
                //Debug.Log("shadername " + shaderName);

                if (shaderName == "Standard")
                {
                    cmToAdd = CompositeModes.normalToRoughnessRGB;
                }
                else if (shaderName == "Standard (Specular setup)")
                {
                    cmToAdd = CompositeModes.standardShaderSpecularSetup;
                }
                else if (shaderName == "Standard (Roughness setup)")
                {
                    cmToAdd = CompositeModes.normalToRoughnessAlpha;
                }

                if (matTemplate.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"))
                {
                    textureAToAdd = (Texture2D)matTemplate.GetTexture("_MainTex");

                }
                else if (matTemplate.HasProperty("_SpecGlossMap"))
                { // standard and roughness setup have this
                    if (matTemplate.GetTexture("_SpecGlossMap") != null)
                    {
                        textureAToAdd = (Texture2D)matTemplate.GetTexture("_SpecGlossMap");
                    }
                }
                else if (matTemplate.HasProperty("_MetallicGlossMap"))
                {
                    if (matTemplate.GetTexture("_MetallicGlossMap") != null)
                    {
                        textureAToAdd = (Texture2D)matTemplate.GetTexture("_MetallicGlossMap");
                    }
                }
                else
                {
                    textureAToAdd = null;
                    textureBToAdd = null;
                }
                matTemplate = null; //clear material area
            }

            EditorGUILayout.EndVertical();

            List<CompositeTexture> removalList = new List<CompositeTexture>();

            float scrollWidth = position.width;
            float scrollHeight = position.height - 160f;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(scrollWidth), GUILayout.Height(scrollHeight));

            float itemHeight = 135f;
            List<CompositeTexture> ctsAll = CompositeTextureData.getTextures();
            List<CompositeTexture> cts = GetCTinUIVisible(ctsAll, (int)itemHeight, (int)scrollPos.y);

            int fullScrollableHeight = (int)itemHeight * ctsAll.Count;
            GUILayout.BeginVertical("", GUIStyle.none, GUILayout.Height(fullScrollableHeight));

            int firstindex = (int)(scrollPos.y / itemHeight);
            GUILayout.Space(firstindex * itemHeight);

            foreach (CompositeTexture ct in cts)
            {
                if (ct.filesExist() == false)
                {
                    EDebug.Log("missing a file");
                    continue;
                }

                Texture2D texA = ct.getTextureA();
                Texture2D texB = ct.getTextureB();

                Texture2D texPreview = null;

                if (!AssetPreview.IsLoadingAssetPreview(texA.GetInstanceID()))
                    texPreview = AssetPreview.GetMiniThumbnail(texA);


                Rect lastRect2 = EditorGUILayout.BeginVertical("box", GUILayout.Height(itemHeight));
                GUIStyle lab = EditorStyles.label;

                if (lastAddedCT_ID == ct.id)
                    lab = EditorStyles.boldLabel;

                if (lastAddedCT_ID == ct.id && shouldScroll)
                {
                    if (lastRect2.y > 0f)
                    {
                        //lastAddedCT_ID = null;
                        EDebug.Log(lastRect2.y);
                        scrollPos = lastRect2.position;
                        shouldScroll = false;
                    }
                }
                /////////
                GUILayoutOption[] ctrlButtonOptions = new GUILayoutOption[1];
                ctrlButtonOptions[0] = GUILayout.Width(20f);
                GUILayoutOption[] toggleOptions = new GUILayoutOption[1];
                toggleOptions[0] = GUILayout.Width(11f);

                EditorGUILayout.BeginHorizontal();
                var toggle = GUILayout.Toggle(ct.enabled, "", toggleOptions);
                GUILayout.Label(texA.name, lab);



                if (GUILayout.Button("X", ctrlButtonOptions))
                    removalList.Add(ct);



                EditorGUILayout.EndHorizontal();

                if (toggle != ct.enabled)
                {
                    if (ct.enabled == true)
                        CompositeTextureController.enableTexture(ct.id, false);
                    else
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        CompositeTextureController.enableTexture(ct.id, true);
                        sw.Stop();

                        EDebug.Log("ELAPSED " + sw.Elapsed.Milliseconds);
                    }
                }

                //if (GUILayout.Button("mark unreadable"))
                //{
                //    texB.setReadable(true);
                //    String assetPath = AssetDatabase.GetAssetPath(texB);
                //    AssetDatabase.ImportAsset(assetPath);

                //}

                EditorGUI.BeginDisabledGroup(ct.enabled == false);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Box(texPreview, GUILayout.Width(105), GUILayout.Height(105));

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.BeginChangeCheck();
                //int texB_ID = EditorGUILayout.ObjectField("composite texture", texB, typeof(Texture2D), true).GetInstanceID();
                string texB_ID = CompositeTexture.assetToGUID((Texture2D)EditorGUILayout.ObjectField("composite texture", texB, typeof(Texture2D), true));

                CompositeModes cm = (CompositeModes)EditorGUILayout.Popup("Composit mode", (int)ct.compositeMode, modesAsArray);

                float strength = EditorGUILayout.FloatField("Strength", ct.strength);

                if (EditorGUI.EndChangeCheck())
                {
                    ct.update(texB_ID, strength, cm);
                    CompositeTextureController.enableTexture(ct.id, true);
                    EDebug.Log("value changed! " + texA.name);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();

            }
            GUILayout.EndVertical();

            foreach (CompositeTexture ctr in removalList)
                CompositeTextureController.removeTexture(ctr);

            EditorGUILayout.EndScrollView();
        }

        public List<CompositeTexture> GetCTinUIVisible(List<CompositeTexture> cts, int itemHeight, int scrollPos)
        {



            int maxScrollHeight = (int)position.height;

            int startIndex = scrollPos / itemHeight; // 150/150
            int endIndex = startIndex + (maxScrollHeight / itemHeight) + 1;

            if (endIndex > cts.Count)
                endIndex = cts.Count;

            //Debug.Log("rendering " + (endIndex - startIndex) + " of " + cts.Count + " from " + startIndex + " to " + (startIndex + (endIndex - startIndex)));

            return cts.GetRange(startIndex, endIndex - startIndex);
        }
    }
}