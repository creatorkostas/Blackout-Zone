using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Nicrom.PM {
    [CustomEditor(typeof(PaletteModifier))]
    public class PaletteModifier_Editor : Editor {
        /// <summary>
        /// A list of all the reordable lists.  
        /// </summary>
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        /// <summary>
        /// List of warning messages.
        /// </summary>
        private List<string> warningMessages = new();
        /// <summary>
        /// Serialized Property for palettesList list.  
        /// </summary>
        private SerializedProperty palettesList;
        /// <summary>
        /// The mesh of the current object.  
        /// </summary>
        private Mesh mesh;
        private Texture2D youTubeIcon;
        private Texture2D discordIcon;
        private Texture2D documentationIcon;
        private Texture2D reviewIcon;
        private Texture2D upgradeIcon;
        private Texture2D headerBackground;
        private Texture2D headerLogoText;
        /// <summary>
        /// Reference to the texture atlas used by the current object.  
        /// </summary>
        private Texture2D tex;
        /// <summary>
        /// A material used to change the color of those texture atlas parts that have a texture pattern.   
        /// </summary>
        private Material texPatternMaterial;
        /// <summary>
        /// Instance of a texture that has the same colors the texture atlas had before any changes were made.
        /// </summary>
        private Texture2D origTexture;
        /// <summary>
        /// Render texture used for storing a modified texture.
        /// </summary>
        private RenderTexture texPatternRT;
        /// <summary>
        /// Palette button style.  
        /// </summary>
        private GUIStyle bStyle;
        /// <summary>
        /// An array that stores the current pixel colors of the texture atlas. 
        /// </summary>
        private Color32[] currentTexColors;
        /// <summary>
        /// An array that stores the pixel colors the texture atlas had before any changes were made.
        /// </summary>
        private Color32[] origTexColors;
        /// <summary>
        /// An array that stores the current object mesh UVs.  
        /// </summary>
        private Vector2[] UVs;
        /// <summary>
        /// Used to determine if the texture atlas can be modified. A texture can be modified if it
        /// is readable and has the format ARGB32, RGBA32 or RGB24.  
        /// </summary>
        private bool texCanBeModified = true;
        /// <summary>
        /// Used to determine if the custom inspector can be drawn.
        /// </summary>
        private bool canDrawInspector = true;
        /// <summary>
        /// Used to determine if the custom inspector can be drawn.
        /// </summary>
        private bool drawTexNameOnly = false;
        /// <summary>
        /// Palette name.  
        /// </summary>
        private string headerText = "";
        /// <summary>
        /// Current Palette Modifier version.  
        /// </summary>
        private string pmVersion = "1.0.1";
        /// <summary>
        /// Tool bar titels.  
        /// </summary>
        private string[] toolBarTitles = { "Texture", "Gradient", "Settings" };

        void OnEnable()
        {
            PaletteModifier pMod = (PaletteModifier)target;
            palettesList = serializedObject.FindProperty("palettesList");

            InitializePMData(pMod);

            headerBackground = Resources.Load<Texture2D>("PM_Header_Background");
            headerLogoText = Resources.Load<Texture2D>("PM_Header_Text");

            youTubeIcon = Resources.Load<Texture2D>("PM_Footer_YouTube");
            documentationIcon = Resources.Load<Texture2D>("PM_Footer_Documentation");
            discordIcon = Resources.Load<Texture2D>("PM_Footer_Discord");
            reviewIcon = Resources.Load<Texture2D>("PM_Footer_Review");
            upgradeIcon = Resources.Load<Texture2D>("PM_Footer_Upgrade");
        }

        private void OnDisable()
        {
            PaletteModifier pModifier = (PaletteModifier)target;

            if (pModifier != null)
            {
                if (pModifier.texGrid != null && HasTexturePattern(pModifier))
                    SaveTintColotInTextureGrid(pModifier);
            }
        }

        private void InitializePMData(PaletteModifier pMod)
        {
            if (pMod.GetComponent<MeshFilter>() != null)
                mesh = pMod.GetComponent<MeshFilter>().sharedMesh;

            if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
                mesh = pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            if (mesh == null)
                return;

            UVs = mesh.uv;
            tex = null;

            if (pMod.GetComponent<Renderer>().sharedMaterial.HasProperty(pMod.textureName))
                tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (tex == null)
                return;

            PM_Utils.SetTextureGridReference(pMod, tex);

            if (pMod.texGrid == null)
                return;

            if (pMod.texGrid.originTexAtlas == null)
                pMod.texGrid.GetOriginalTextureColors();

            if (PM_Utils.IsTextureReadable(tex) && PM_Utils.HasSuportedTextureFormat(tex))
            {
                currentTexColors = tex.GetPixels32();

                if (pMod.texGrid.originTexAtlas != null)
                    origTexColors = pMod.texGrid.originTexAtlas.GetPixels32();

                if (pMod.generatePaletteModifierData)
                {
                    GeneratePaletteModifierData(pMod);
                    pMod.generatePaletteModifierData = false;
                }
                else
                {
                    GetCellsColorFromCurrentTexture(pMod);
                }

                if (HasTexturePattern(pMod))
                {
                    SetTextureAndMaterialReferences(pMod);
                }

                serializedObject.Update();
                CreateReorderableLists(pMod);
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Modified Inspector");
            PaletteModifier pMod = (PaletteModifier)target;

            if (Event.current.type == EventType.Layout)
                CheckReferenceValues(pMod);

            if (bStyle == null)
                bStyle = new GUIStyle(GUI.skin.button);

            serializedObject.Update();

            if (pMod.showHeader)
                DrawHeaderBackgroundAndLogo();

            if (warningMessages.Count > 0)
                DrawWarningMessages();

            if (drawTexNameOnly)
            {
                pMod.textureName = EditorGUILayout.TextField(new GUIContent("Main Texture Name"), pMod.textureName);

                if (GUILayout.Button(new GUIContent("Initialize PM Data")))
                {
                    InitializePMData(pMod);
                }
            }

            CustomInspector(pMod);

            serializedObject.ApplyModifiedProperties();
            CheckForUndoRedo(pMod);
        }

        /// <summary>
        /// Sets all the texture and material references that are used to apply a color tint to a segment of the main texture.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SetTextureAndMaterialReferences(PaletteModifier pMod)
        {
            texPatternRT = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            texPatternRT.filterMode = tex.filterMode;

            origTexColors = pMod.texGrid.originTexAtlas.GetPixels32();
            origTexture = new Texture2D(tex.width, tex.height, tex.format, false, true);
    
            origTexture.SetPixels32(origTexColors);
            origTexture.Apply();

            texPatternMaterial = new Material(Shader.Find("Hidden/TextureColorTint"));
            texPatternMaterial.SetTexture(pMod.textureName, origTexture);
        }

        /// <summary>
        /// Performs a series of checks to ensure the custom inspector can be drawn.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CheckReferenceValues(PaletteModifier pMod)
        {
            canDrawInspector = true;
            texCanBeModified = true;
            drawTexNameOnly = false;

            warningMessages.Clear();

            if (mesh == null)
            {
                warningMessages.Add("Object doesn't have a MeshFilter component or a mesh asset assigned to it.");
                canDrawInspector = false;
                texCanBeModified = false;
                return;
            }

            if (tex == null)
            {
                warningMessages.Add("Palette Modifier can't be initialised. Possible solutions to fix this:");
                warningMessages.Add("- Make sure the current GameObject has a material and an albedo texture assigned to it.");
                warningMessages.Add("- If you are using a custom shader, make sure the Texture Name is the same as the property name of main texture in the shader.");

                drawTexNameOnly = true;
                canDrawInspector = false;
                texCanBeModified = false;

                return;
            }

            if (pMod.texGrid == null)
            {
                warningMessages.Add("A Texture Grid asset with a reference to the texture atlas used by the material of this object, was not found. "
                    + "Please create a Texture Grid asset for this texture atlas.");

                canDrawInspector = false;
                return;
            }

            if (!PM_Utils.IsTextureReadable(tex))
            {
                warningMessages.Add("The texture " + tex.name + " is not readable. You can make the texture readable in the Texture Import Settings.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            if (!PM_Utils.HasSuportedTextureFormat(tex))
            {
                warningMessages.Add("Texture format needs to be ARGB32, RGBA32, or RGB24.");
                texCanBeModified = false;
                canDrawInspector = false;
            }

            Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (tempTex != null && tempTex != tex)
            {
                if (PM_Utils.IsTextureReadable(tempTex))
                {
                    OnMaterialTextureChange(pMod);
                }
                else
                {
                    warningMessages.Add("The texture " + tempTex.name + " is not readable. You can make the texture readable in the Texture Import Settings.");
                    canDrawInspector = false;
                }
            }
        }

        /// <summary>
        /// Updates different references and array data. Called when the material texture is replaced by the user.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void OnMaterialTextureChange(PaletteModifier pMod)
        {
            Texture2D tempTex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;

            if (HasTexturePattern(pMod))
                SaveTintColotInTextureGrid(pMod);

            if (PM_Utils.SetTextureGridReference(pMod, tempTex))
            {
                if (pMod.texGrid.originTexAtlas == null)
                    pMod.texGrid.GetOriginalTextureColors();

                tex = pMod.GetComponent<Renderer>().sharedMaterial.GetTexture(pMod.textureName) as Texture2D;
                currentTexColors = tex.GetPixels32();

                SetTextureAndMaterialReferences(pMod);
                GetCellsColorFromCurrentTexture(pMod);
            }
            else
            {
                pMod.texGrid = null;
            }     
        }

        /// <summary>
        /// Draws the warning messages.
        /// </summary>
        private void DrawWarningMessages()
        {
            for (int i = 0; i < warningMessages.Count; i++)
            {
                EditorGUILayout.HelpBox(warningMessages[i], MessageType.Warning);
            }
        }

        /// <summary>
        /// Draws the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CustomInspector(PaletteModifier pMod)
        {
            if (pMod.textureUpdate == TextureUpdate.Auto)
                CheckForColorChanges(pMod, true);
            else
                CheckForColorChanges(pMod, false);

            if (canDrawInspector)
            {
                DrawReorderableLists(pMod);

                GUILayout.BeginHorizontal();
                pMod.selectedToolBar = GUILayout.Toolbar(pMod.selectedToolBar, toolBarTitles);
                GUILayout.EndHorizontal();

                if (texCanBeModified && pMod.selectedToolBar == 0)
                    DrawTextureTab(pMod);

                if (canDrawInspector && pMod.selectedToolBar == 1)
                    EditorGUILayout.HelpBox("Gradient feature not included in Lite version.", MessageType.Info);

                if (pMod.texGrid != null && canDrawInspector && pMod.selectedToolBar == 2)
                    DrawMiscellaneousTab(pMod);

                DrawFooterButtons();
            }
        }

        /// <summary>
        /// Draws the header background, unity version, asset name/version.
        /// </summary>
        public void DrawHeaderBackgroundAndLogo()
        {
            var rect = EditorGUILayout.GetControlRect(GUILayout.Height(64));
            EditorGUI.DrawPreviewTexture(rect, headerBackground);
            rect.y -= 6;
            GUI.DrawTexture(rect, headerLogoText, ScaleMode.ScaleToFit, true);
            rect.y += 30;
            GUI.Label(rect, Application.unityVersion);
            rect.x += rect.width - 30;
            GUI.Label(rect, pmVersion);
            GUILayout.Space(5);
        }

        /// <summary>
        /// Draws 3 buttons that open the documentation, discord and review page.
        /// </summary>
        public void DrawFooterButtons()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent(youTubeIcon, "Watch YouTube tutorial"), new GUILayoutOption[2]
            {
                GUILayout.MaxHeight(40f),
                GUILayout.MaxWidth(40f),
            }))
                Application.OpenURL("https://www.youtube.com/watch?v=fLf4WSjlBPI");

            if (GUILayout.Button(new GUIContent(documentationIcon, "Read documentation"), new GUILayoutOption[2]
            {
                GUILayout.MaxHeight(40f),
                GUILayout.MaxWidth(40f),
            }))
                EditorUtility.OpenWithDefaultApp("Assets/Nicrom/Tools/PaletteModifier/Doc/PaletteModifier_Guide.pdf");

            if (GUILayout.Button(new GUIContent(discordIcon, "Get support"), new GUILayoutOption[2]
            {
                GUILayout.MaxHeight(40f),
                GUILayout.MaxWidth(40f),
            }))
                Application.OpenURL("https://discord.com/invite/RCdETwg");

            if (GUILayout.Button(new GUIContent(reviewIcon, "Write review"), new GUILayoutOption[2]
            {
                GUILayout.MaxHeight(40f),
                GUILayout.MaxWidth(40f),
            }))
                Application.OpenURL("https://assetstore.unity.com/packages/tools/painting/palette-modifier-lite-texture-color-editor-for-low-poly-models-278237#reviews");

            if (GUILayout.Button(new GUIContent(upgradeIcon, "Upgrade Palette Modifier Lite"), new GUILayoutOption[2]
{
                GUILayout.MaxHeight(40f),
                GUILayout.MaxWidth(40f),
}))
                Application.OpenURL("https://assetstore.unity.com/packages/tools/painting/palette-modifier-texture-color-editor-for-low-poly-models-154865");

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        /// <summary>
        /// Draws the reorderable lists.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawReorderableLists(PaletteModifier pMod)
        {
            if (pMod.texGrid == null)      
                return;      

            if(reorderableLists.Count != pMod.palettesList.Count)          
                CreateReorderableLists(pMod);       

            for (int i = 0; i < reorderableLists.Count; i++)
            {
                reorderableLists[i].DoLayoutList();          
            }

            if (pMod.colorFieldsInInspector < (pMod.flatColorsOnObject + pMod.texPatternsOnObject))
            {
                int dif = (pMod.flatColorsOnObject + pMod.texPatternsOnObject) - pMod.colorFieldsInInspector;
                GUILayout.Space(-4);
                if(dif == 1)
                    EditorGUILayout.HelpBox(dif + " color is deleted.", MessageType.Info);
                else
                    EditorGUILayout.HelpBox(dif + " colors are deleted.", MessageType.Info);
                GUILayout.Space(10);
            }
        }

        /// <summary>
        /// Draws the Texture options in the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawTextureTab(PaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 100;

                if (pMod.texGrid != null && canDrawInspector)
                {
                    pMod.textureUpdate = (TextureUpdate)EditorGUILayout.EnumPopup(new GUIContent("Texture Update", ""), pMod.textureUpdate);

                    GUILayout.Space(5);

                    if (pMod.textureUpdate == TextureUpdate.Manual)
                    {
                        if (GUILayout.Button(new GUIContent("Update Texture")))
                        {
                            CheckForColorChanges(pMod, true);
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Save Texture")) && EditorUtility.DisplayDialog("Save",
                        "Are you sure you want to overwrite current texture data. ", "Yes", "No"))
                    {
                        SaveTextureChanges(pMod, true);
                    }
                }        
            });
        }

        /// <summary>
        /// Draws the Miscellaneous group of options in the custom inspector.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void DrawMiscellaneousTab(PaletteModifier pMod)
        {
            GUILayout.Space(-4);

            InspectorBox(10, () =>
            {
                EditorGUIUtility.labelWidth = 150;

                if (GUILayout.Button(new GUIContent("Rebuild PM Data")) && EditorUtility.DisplayDialog("Rebuild PM Data",
"This action will reset the inspector colors and remove custom palettes. "
+ "Use this option only when you made changes to the 3D model. For example, removed vertices or changed the UV positions.", "Yes", "No"))
                {
                    RebuildPMData(pMod);
                }

                GUILayout.Space(1);
                pMod.showHeader = EditorGUILayout.Toggle(new GUIContent("Show Header", ""), pMod.showHeader);
            });
        }

        /// <summary>
        /// Used to determine if the current object uses texture patterns. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <returns> Returns true if the object has texture patterns, otherwise returns false. </returns>
        public bool HasTexturePattern(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if (pMod.palettesList[i].cellsList[j].isTexture)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Creates Reorderable lists of colors.  
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CreateReorderableLists(PaletteModifier pMod)
        {
            reorderableLists.Clear();

            if (pMod.palettesList.Count == 0)
                return;

            if (pMod.palettesList.Count == 1 && pMod.palettesList[0].cellsList.Count == 0)
                return;

            for (int i = 0; i < pMod.palettesList.Count; i++)    
                reorderableLists.Add(CreateReorderableList(pMod, i));    
        }

        /// <summary>
        /// Creates a Reorderable list of colors.  
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="listIndex"> The index of the list for which a reaordable list is created. </param>
        private ReorderableList CreateReorderableList(PaletteModifier pMod, int listIndex)
        {
            SerializedProperty paletteElement = palettesList.GetArrayElementAtIndex(listIndex);
            ReorderableList rList = new ReorderableList(serializedObject, paletteElement.FindPropertyRelative("cellsList"), true, true, false, false);

            rList.elementHeight = pMod.palettesList[listIndex].elementHeight;

            rList.drawHeaderCallback = (Rect rect) =>
            {
                headerText = pMod.palettesList[listIndex].paletteName;
                EditorGUI.LabelField(rect, new GUIContent(headerText), EditorStyles.boldLabel);
            };

            rList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = rList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUIUtility.labelWidth = 70;

                string fieldLabel = "";
                bool isTexture = element.FindPropertyRelative("isTexture").boolValue;
                int flatColorCount = 0;
                int texPatternCount = 0;

                for (int i = 0; i <= index; i++)
                {
                    if (pMod.palettesList[listIndex].cellsList[i].isTexture)
                        texPatternCount++;
                    else
                        flatColorCount++;
                }

                if (isTexture)
                    fieldLabel = "Tex " + texPatternCount;
                else
                    fieldLabel = "Color " + flatColorCount;

                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, pMod.palettesList[listIndex].propFieldHeight),
                    element.FindPropertyRelative("currentCellColor"), new GUIContent(fieldLabel));
            };

            return rList;
        }

        /// <summary>
        /// Checks for color changes and updates the texture colors that must be updated.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="isAuto"> Used to determine if texture update mode is set to auto. </param>
        private void CheckForColorChanges(PaletteModifier pMod, bool isAuto)
        {
            int len = pMod.palettesList.Count;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {
                    if (isAuto)
                    {
                        if(!PM_Utils.AreColorsEqual(pMod.palettesList[i].cellsList[j].currentCellColor, pMod.palettesList[i].cellsList[j].previousCellColor))
                        {
                            UpdateTexture(pMod, i, j);
                            pMod.palettesList[i].cellsList[j].previousCellColor = pMod.palettesList[i].cellsList[j].currentCellColor;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether and undo or redo was performed. Updates the texture colors if one was performed. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void CheckForUndoRedo(PaletteModifier pMod)
        {
            if (Event.current.type == EventType.ValidateCommand)
            {
                switch (Event.current.commandName)
                {
                    case "UndoRedoPerformed":
                    {
                        CreateReorderableLists(pMod);

                        for (int i = 0; i < pMod.palettesList.Count; i++)
                        {
                            for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                            {
                                 UpdateTexture(pMod, i, j);
                            }
                        }
                        serializedObject.Update();
                        Repaint();
 
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Rebuilds all the PM data. The custom palettes are removed and the colors are reset to the current texture values. 
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void RebuildPMData(PaletteModifier pMod)
        {
            if (pMod.GetComponent<MeshFilter>() != null)
                mesh = pMod.GetComponent<MeshFilter>().sharedMesh;

            if (pMod.GetComponent<SkinnedMeshRenderer>() != null)
                mesh = pMod.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            UVs = mesh.uv;
            currentTexColors = tex.GetPixels32();
            GeneratePaletteModifierData(pMod);
            serializedObject.Update();

            if (HasTexturePattern(pMod))
            {
                SetTextureAndMaterialReferences(pMod);
                GetCellsColorFromCurrentTexture(pMod);
            }

            serializedObject.Update();
            CreateReorderableLists(pMod);
        }

        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GeneratePaletteModifierData(PaletteModifier pMod)
        {
            pMod.palettesList.Clear();
            pMod.cellStorage.Clear();
            pMod.palettesList.Add(new Palette());

            GenerateCellsDataFromCustomGrids(pMod);
            GetCellsColorFromCurrentTexture(pMod);

            pMod.colorFieldsInInspector = pMod.palettesList[0].cellsList.Count;
            pMod.flatColorsOnObject = 0;
            pMod.texPatternsOnObject = 0;

            for (int i = 0; i < pMod.palettesList[0].cellsList.Count; i++)
            {
                if (pMod.palettesList[0].cellsList[i].isTexture)
                    pMod.texPatternsOnObject++;
                else
                    pMod.flatColorsOnObject++;
            }

            pMod.flatColorsInInspector = pMod.flatColorsOnObject;
        }

        /// <summary>
        /// Generates all the data necessary for Palette Modifier to work correctly.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GenerateCellsDataFromCustomGrids(PaletteModifier pMod)
        {
            Vector2Int texelCoord;
            Rect cell;
            bool colorFound = false;
            bool isPointInsideCG;
            int len = UVs.Length;
            int x, y, len2;
            int count = 0;

            if (pMod.texGrid == null || tex.width != pMod.texGrid.texAtlas.width || tex.height != pMod.texGrid.texAtlas.height)
                return;

            for (int i = 0; i < len; i++)
            {
                Vector2 UV =  Vector2.zero;

                if (UVs[i].y < 0)
                    continue;

                if (Mathf.Abs(UVs[i].x) % 1 == 0)
                {
                    if (Mathf.Abs(UVs[i].x) % 2 == 0)
                        UV.x = 0;
                    else
                        UV.x = 1;
                }
                else
                {
                    UV.x = Mathf.Abs(UVs[i].x) % 1;
                }

                if (Mathf.Abs(UVs[i].y) % 1 == 0)
                {
                    if (Mathf.Abs(UVs[i].y) % 2 == 0)
                        UV.y = 0;
                    else
                        UV.y = 1;
                }
                else
                {
                    UV.y = Mathf.Abs(UVs[i].y) % 1;
                }

                texelCoord = new Vector2Int(Mathf.CeilToInt(UV.x * tex.width - 1), Mathf.CeilToInt(UV.y * tex.height - 1));
                len2 = pMod.palettesList.Count;

                for (int j = 0; j < len2; j++)
                {
                    for (int k = 0; k < pMod.palettesList[j].cellsList.Count; k++)
                    {
                        if (PM_Utils.PointInsideRect(pMod.palettesList[j].cellsList[k].gridCell, texelCoord))
                        {
                            pMod.palettesList[j].cellsList[k].uvIndex.Add(i);
                            colorFound = true;
                            count++;
                            break;
                        }
                    }

                    if (colorFound)
                        break;
                }

                if (!colorFound)
                {
                    bool isTexture;
                    cell = PM_Utils.GetCellRect(pMod, texelCoord, out isPointInsideCG, out isTexture);

                    if(isPointInsideCG)
                    {
                        if (isTexture)
                        {
                            pMod.palettesList[0].cellsList.Add(new CellData(Color.white, cell, isTexture));
                        }
                        else
                        {
                            x = (int)(cell.x + cell.width * 0.5f);
                            y = (int)(cell.y + cell.height * 0.5f);

                            Color pixelColor = tex.GetPixel(x, y);
                            pMod.palettesList[0].cellsList.Add(new CellData(pixelColor, cell, isTexture));
                        }

                        int lastIndex = pMod.palettesList[0].cellsList.Count - 1;
                        pMod.palettesList[0].cellsList[lastIndex].uvIndex.Add(i);

                        count++;
                    }           
                }

                colorFound = false;
            }

            if (count < len)
                Debug.LogWarning("Not all the mesh UVs are inside the Texture Grid. Open the Texture Grid Editor and make " 
                    + "sure all flat colors and texture patterns have a Flat Color Grid/Texture Pattern Rect on top of them." 
                    + " Then go to Misc tab and press Rebuild PM Data button.");
        }

        /// <summary>
        /// Resets the inspector colors to their original values.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void GetCellsColorFromCurrentTexture(PaletteModifier pMod)
        {
            int x, y, len;
            len = pMod.palettesList.Count;

            for (int i = 0; i < len; i++)
            {
                int len2 = pMod.palettesList[i].cellsList.Count;

                for (int j = 0; j < len2; j++)
                {            
                    x = (int)(pMod.palettesList[i].cellsList[j].gridCell.x + pMod.palettesList[i].cellsList[j].gridCell.width * 0.5f);
                    y = (int)(pMod.palettesList[i].cellsList[j].gridCell.y + pMod.palettesList[i].cellsList[j].gridCell.height * 0.5f);
                   
                    if (pMod.palettesList[i].cellsList[j].isTexture)
                    { 
                        Color tintColor = GetTintColorFromTextureGrid(pMod, i, j);

                        pMod.palettesList[i].cellsList[j].currentCellColor = tintColor;
                        pMod.palettesList[i].cellsList[j].previousCellColor = tintColor;
                    }
                    else
                    {
                        Color texelColor = tex.GetPixel(x, y);

                        pMod.palettesList[i].cellsList[j].currentCellColor = texelColor;
                        pMod.palettesList[i].cellsList[j].previousCellColor = texelColor;
                    }
                }
            }
        }

        /// <summary>
        /// Restores the tint colors.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        /// <returns> Returns the color stored in a Custom Grid. </returns>
        private Color GetTintColorFromTextureGrid(PaletteModifier pMod, int  i, int j)
        {
            Color tintColor = Color.white;

            for (int k = 0; k < pMod.texGrid.gridsList.Count; k++)
            {
                Rect cellRect = pMod.palettesList[i].cellsList[j].gridCell;
                Vector2Int gridPos = pMod.texGrid.gridsList[k].gridPos;

                if (cellRect.x == gridPos.x && cellRect.y == gridPos.y && cellRect.width == pMod.texGrid.gridsList[k].gridWidth && cellRect.height == pMod.texGrid.gridsList[k].gridHeight)
                {
                    tintColor = pMod.texGrid.gridsList[k].tintColor;
                }
            }

            return tintColor;
        }

        /// <summary>
        /// Stores the tint colors in the Texture Grid asset Custom Grids.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void SaveTintColotInTextureGrid(PaletteModifier pMod)
        {
            for (int i = 0; i < pMod.palettesList.Count; i++)
            {
                for (int j = 0; j < pMod.palettesList[i].cellsList.Count; j++)
                {
                    if(pMod.palettesList[i].cellsList[j].isTexture)
                    {
                        for (int k = 0; k < pMod.texGrid.gridsList.Count; k++)
                        {
                            Rect cellRect = pMod.palettesList[i].cellsList[j].gridCell;
                            Vector2Int gridPos = pMod.texGrid.gridsList[k].gridPos;

                            if (cellRect.x == gridPos.x && cellRect.y == gridPos.y && cellRect.width == pMod.texGrid.gridsList[k].gridWidth && cellRect.height == pMod.texGrid.gridsList[k].gridHeight)
                            {
                                pMod.texGrid.gridsList[k].tintColor = pMod.palettesList[i].cellsList[j].currentCellColor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Changes the colors of a texture based on the values passed.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        private void UpdateTexture(PaletteModifier pMod, int i, int j)
        {
            UpdateTexPixelColors(pMod, pMod.palettesList[i].cellsList[j].currentCellColor, pMod.palettesList[i].cellsList[j].isTexture, i, j);
        }

        /// <summary>
        /// Updates the colors of the main texture.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        /// <param name="i"> Index of a list item. </param>
        /// <param name="j"> Index of a list item. </param>
        private void UpdateTexPixelColors(PaletteModifier pMod, Color32 color, bool isTexture, int i, int j)
        {
            int minX, minY, maxX, maxY;
            int textHeight = tex.height;

            minX = (int)pMod.palettesList[i].cellsList[j].gridCell.x;
            minY = (int)pMod.palettesList[i].cellsList[j].gridCell.y;
            maxX = (int)(minX + pMod.palettesList[i].cellsList[j].gridCell.width - 1);
            maxY = (int)(minY + pMod.palettesList[i].cellsList[j].gridCell.height - 1);

            if (isTexture)
            {
                int width = (int)(pMod.palettesList[i].cellsList[j].gridCell.width);
                int height = (int)(pMod.palettesList[i].cellsList[j].gridCell.height);

                if ((QualitySettings.activeColorSpace == ColorSpace.Linear))
                    texPatternMaterial.SetFloat("_Linear", 1f);
                else
                    texPatternMaterial.SetFloat("_Linear", 0f);

                texPatternMaterial.SetColor("_TintColor", color);

                RenderTexture.active = texPatternRT;
                GL.Clear(true, true, Color.black);
                Graphics.Blit(origTexture, texPatternRT, texPatternMaterial);
                tex.ReadPixels(new Rect(minX, tex.height - maxY - 1, width, height), minX, minY);
                currentTexColors = tex.GetPixels32();
                RenderTexture.active = null;
            }
            else
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        currentTexColors[textHeight * y + x] = color;
                    }
                }
                tex.SetPixels32(currentTexColors);
            }

            tex.Apply();
        }

        /// <summary>
        /// Saves the changes made to the texture.
        /// </summary>
        private void SaveTextureChanges(PaletteModifier pMod, bool clearTinColors)
        {
            if (clearTinColors)
                pMod.texGrid.ClearTintColors();

            var bytes = tex.EncodeToPNG();
            string path = Application.dataPath + "/../" + AssetDatabase.GetAssetPath(tex);
   
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);

            OnTextureSave(pMod);
        }

        /// <summary>
        /// Updates PM internal data after the texture atlas is saved.
        /// </summary>
        /// <param name="pMod"> The object being inspected. </param>
        private void OnTextureSave(PaletteModifier pMod)
        {
            currentTexColors = tex.GetPixels32();
            origTexColors = tex.GetPixels32();

            pMod.texGrid.originTexAtlas = new Texture2D(tex.width, tex.height, tex.format, true);
            pMod.texGrid.originTexAtlas.SetPixels32(currentTexColors);
            pMod.texGrid.originTexAtlas.Apply();

            if (pMod.texPatternsOnObject > 0)
            {     
                origTexture.SetPixels32(origTexColors);
                origTexture.Apply();           
            }

            GetCellsColorFromCurrentTexture(pMod);
        }

        /// <summary>
        /// Sets the texture import options.
        /// </summary>
        /// <param name="texture"> Reference to a texture asset. </param>
        public void SetTextureImporterOptions(Texture2D texture)
        {
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = true;
                tImporter.wrapMode = TextureWrapMode.Clamp;
                tImporter.filterMode = FilterMode.Point;
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        public void InspectorBox(int aBorder, System.Action inside, int aWidthOverride = 0, int aHeightOverride = 0)
        {
            Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Width(aWidthOverride));
            if (aWidthOverride != 0)
            {
                r.width = aWidthOverride;
            }
            GUI.Box(r, GUIContent.none);
            GUILayout.Space(aBorder);
            if (aHeightOverride != 0)
                EditorGUILayout.BeginVertical(GUILayout.Height(aHeightOverride));
            else
                EditorGUILayout.BeginVertical();
            GUILayout.Space(aBorder);
            inside();
            GUILayout.Space(aBorder);
            EditorGUILayout.EndVertical();
            GUILayout.Space(aBorder);
            EditorGUILayout.EndHorizontal();
        }

        public void BoldFontStyle(System.Action inside)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            inside();
            style.fontStyle = previousStyle;
        }
    }
}
