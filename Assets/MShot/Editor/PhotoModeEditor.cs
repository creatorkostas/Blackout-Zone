using UnityEditor;
using UnityEngine;

namespace MShot
{
    [CustomEditor(typeof(PhotoMode))]
    public class PhotoModeEditor : Editor
    {
        SerializedProperty showUI;
        SerializedProperty screenshotKey;
        SerializedProperty screenshotFolder;

        private void OnEnable()
        {
            showUI = serializedObject.FindProperty("showUI");
            screenshotKey = serializedObject.FindProperty("screenshotKey");
            screenshotFolder = serializedObject.FindProperty("screenshotFolder");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("This will create an 'AppData' folder for the player to save the images to.", MessageType.Info);
            EditorGUILayout.HelpBox($"Make sure to change the company name and the product name in the player settings, as this will determine the name of the directory inside 'Appdata'.\n Current company name: {PlayerSettings.companyName}. \n Current product name: {PlayerSettings.productName}.", MessageType.Warning);
            EditorGUILayout.PropertyField(showUI, new GUIContent("Show UI"));
            EditorGUILayout.PropertyField(screenshotKey, new GUIContent("Screenshot Key"));
            EditorGUILayout.PropertyField(screenshotFolder, new GUIContent("Screenshot Folder Name"));



            serializedObject.ApplyModifiedProperties();
        }
    }
}
