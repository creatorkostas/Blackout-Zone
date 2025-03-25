using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MShot
{
    public class PhotoMode : MonoBehaviour
    {
        [SerializeField]
        bool showUI;
        [SerializeField]
        private KeyCode screenshotKey = KeyCode.F12;


        [SerializeField]
        private string screenshotFolder = "Screenshots";

        public static PhotoMode instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(screenshotKey))
            {
                MShot.PhotoMode.instance.CaptureScreenshot(false, showUI, null, null);
            }
        }

        public Texture2D CaptureScreenshot(bool saveImage, bool showUI, string screenShotName = default(string), Camera camera = null)
        {
            string folderPath = System.IO.Path.Combine(Application.persistentDataPath, screenshotFolder);
            Dictionary<Canvas, Tuple<RenderMode, Camera>> canvasData = new Dictionary<Canvas, Tuple<RenderMode, Camera>>();
            System.IO.Directory.CreateDirectory(folderPath);
            if (camera == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("No camera provided, and no main camera found in the scene.");
                    return null;
                }
                camera = mainCamera;
            }
            if (showUI)
            {

                foreach (Canvas canvas in FindObjectsOfType<Canvas>())
                {
                    canvasData.Add(canvas, new Tuple<RenderMode, Camera>(canvas.renderMode, canvas.worldCamera));
                }

                foreach (var pair in canvasData)
                {
                    Canvas canvas = pair.Key;
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = camera;
                    canvas.planeDistance = 1;
                }
            }
            else
            {


                foreach (Canvas canvas in FindObjectsOfType<Canvas>())
                {
                    canvasData.Add(canvas, new Tuple<RenderMode, Camera>(canvas.renderMode, canvas.worldCamera));
                }
                foreach (var pair in canvasData)
                {
                    Canvas canvas = pair.Key;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.worldCamera = camera;
                }
            }
            RenderTexture originalTargetTexture = camera.targetTexture;

            string fileName = string.IsNullOrEmpty(screenShotName)
                ? $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}"
                : $"{screenShotName}";


            int count = 0;
            string baseFileName = fileName;
            string filePath = System.IO.Path.Combine(folderPath, $"{baseFileName}.png");

            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            camera.targetTexture = renderTexture;

            Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height);
            camera.Render();
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenshot.Apply();
            camera.targetTexture = originalTargetTexture;
            RenderTexture.active = null;
            Destroy(renderTexture);
            foreach (var pair in canvasData)
            {
                Canvas canvas = pair.Key;
                (canvas.renderMode, canvas.worldCamera) = pair.Value;
            }



            if (saveImage)
            {
                while (File.Exists(filePath))
                {
                    count++;
                    baseFileName = $"{fileName}_{count}";
                    filePath = System.IO.Path.Combine(folderPath, $"{baseFileName}.png");
                }
                byte[] bytes = screenshot.EncodeToPNG();
                System.IO.File.WriteAllBytes(filePath, bytes);

                Debug.Log($"Screenshot saved to: {filePath}");
            }
            else
            {
                Debug.Log("ScreenShot not saved to player directory.");
            }
            return screenshot;
        }
    }
}