using UnityEditor;
using UnityEngine;

namespace MShot
{
    public class CameraContextMenu : EditorWindow
    {
        [MenuItem("CONTEXT/Camera/Take Screen Shot")]
        private static void TakeScreenshot(MenuCommand menuCommand)
        {
            Camera camera = menuCommand.context as Camera;

            if (camera != null)
            {
                ScreenshotToolWindow window = CreateInstance<ScreenshotToolWindow>();

                window.TakeScreenShot(camera, window._path, window.imageFormat);

            }
        }
    }
}
