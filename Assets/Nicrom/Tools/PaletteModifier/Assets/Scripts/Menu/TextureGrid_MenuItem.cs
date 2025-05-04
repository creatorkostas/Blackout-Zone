using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif 

namespace Nicrom.PM {
    public class TextureGrid_MenuItem {
#if UNITY_EDITOR
        const string itemName = "NewTextureGrid";

        [MenuItem("GameObject/Palette Modifier/Texture Grid", false, 11),
        MenuItem("Assets/Create/Palette Modifier/Texture Grid", false, 101)]
        public static void CreateAsset()
        {
            CreateAsset(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, itemName);
        }

        public static void CreateAsset(Type type, string baseName, bool focus = true)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (Path.GetExtension(path) != "") path = Path.GetDirectoryName(path);
            if (path == "") path = "Assets";

            CreateAsset(type, path, baseName, focus);
        }

        public static void CreateAsset(Type type, string folder, string baseName, bool focus = true)
        {
            ScriptableObject so = ScriptableObject.CreateInstance<TextureGrid>();

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            Texture2D tex = null;

            foreach (object o in Selection.objects)
            {
                if (o.GetType() == typeof(Texture2D))
                {
                    tex = o as Texture2D;
                    break;
                }
            }

            if (tex != null)
                baseName =tex.name + "_TG";

            string name = folder + "/" + baseName + ".asset";
            int id = 0;

            while (AssetDatabase.LoadAssetAtPath(name, type) != null)
            {
                id += 1;
                name = folder + "/" + baseName + id + ".asset";
            }

            AssetDatabase.CreateAsset(so, name);
            AssetDatabase.SaveAssets();

            if (focus)
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = so;
            }

            if (tex != null)
            {
                TextureGrid tg = so as TextureGrid;
                tg.texAtlas = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(tex), typeof(Texture2D));
            }
        }
#endif
    }
}
