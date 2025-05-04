using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Nicrom.PM {
    [CustomEditor(typeof(TextureGrid))]
    public class TextureGrid_Editor : Editor {

        private bool isReadable = false;
        private bool hasSuportedFormat = false;

        private void OnEnable()
        {
            TextureGrid tg = target as TextureGrid;

            if (tg.texAtlas != null)
            {
                isReadable = PM_Utils.IsTextureReadable(tg.texAtlas);
                hasSuportedFormat = PM_Utils.HasSuportedTextureFormat(tg.texAtlas);
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "Modified Texture Grid");

            TextureGrid tg = target as TextureGrid;

            EditorGUI.BeginChangeCheck();
            tg.texAtlas = (Texture2D)EditorGUILayout.ObjectField("Texture Atlas", tg.texAtlas, typeof(Texture2D), true);

            if (EditorGUI.EndChangeCheck() )
            {
                if (tg.texAtlas != null)
                {
                    isReadable = PM_Utils.IsTextureReadable(tg.texAtlas);
                    hasSuportedFormat = PM_Utils.HasSuportedTextureFormat(tg.texAtlas);

                    if (isReadable && hasSuportedFormat)
                    {
                        tg.texAtlasSize = new Vector2Int(tg.texAtlas.width, tg.texAtlas.height);
                        tg.GetOriginalTextureColors();
                    }
                }
                else
                {
                    tg.originTexAtlas = null;
                }
            }

            if (tg.texAtlas == null)
            {
                EditorGUILayout.HelpBox("Please add a texture atlas!", MessageType.Warning);
            }
            else
            {
                List<TextureGrid> tgList = PM_Utils.FindAssetsByType<TextureGrid>();

                for (int i = 0; i < tgList.Count; i++)
                {
                    if (tg != tgList[i] && tg.texAtlas == tgList[i].texAtlas)
                    {
                        EditorGUILayout.HelpBox("The texture atlas " + tg.texAtlas.name + " is already referenced by the Texture Grid asset " + tgList[i].name + ". It is not recommended to have multiple Texture Grid assets that reference the same texture atlas.", MessageType.Warning);
                        break;
                    }
                }

                if (!isReadable)
                {
                    EditorGUILayout.HelpBox("Texture " + tg.texAtlas.name + " is not readable. You can make the " 
                        + "texture readable in the Texture Import Settings.", MessageType.Warning);
                }

                if (!hasSuportedFormat)
                {
                    EditorGUILayout.HelpBox("Texture format needs to be ARGB32, RGBA32, or RGB24. " 
                        + "You can change the texture format in the Texture Import Settings", MessageType.Warning);
                }

                if ((!isReadable || !hasSuportedFormat) && GUILayout.Button("Fix"))
                    Fix(tg);

                if (tg.originTexAtlas == null && PM_Utils.IsTextureReadable(tg.texAtlas) && PM_Utils.HasSuportedTextureFormat(tg.texAtlas))
                {
                    tg.GetOriginalTextureColors();
                }

                EditorGUILayout.HelpBox("Name: " + tg.texAtlas.name, MessageType.None);
                EditorGUILayout.HelpBox("Size: " + tg.texAtlas.width + "x" + tg.texAtlas.height, MessageType.None);
                EditorGUILayout.HelpBox("Format: " + tg.texAtlas.format, MessageType.None);

                GUILayout.Space(16);
                if (isReadable && hasSuportedFormat && GUILayout.Button("Open Grid Editor"))
                    TextureGrid_Window.ShowWindow(tg);

                GUILayout.Space(8);
                if (GUILayout.Button("Clear Internal Texture Data"))
                {
                    tg.texAtlas = null;
                    tg.originTexAtlas = null;
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void Fix(TextureGrid tg)
        {
            string texturePath = AssetDatabase.GetAssetPath(tg.texAtlas);
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);

            if (!PM_Utils.IsTextureReadable(tg.texAtlas))
            {
                textureImporter.isReadable = true;
                isReadable = true;
            }

            if (!PM_Utils.HasSuportedTextureFormat(tg.texAtlas))
            {
                textureImporter.ClearPlatformTextureSettings("Standalone");
                textureImporter.ClearPlatformTextureSettings("Web");
                textureImporter.ClearPlatformTextureSettings("iPhone");
                textureImporter.ClearPlatformTextureSettings("Android");

                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                hasSuportedFormat = true;
            }

            AssetDatabase.ImportAsset(texturePath);
            AssetDatabase.Refresh();
            serializedObject.Update();

            tg.GetOriginalTextureColors();        
        }
    }
}
