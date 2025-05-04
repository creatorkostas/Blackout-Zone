using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameOverMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void GoToMainMenu(PlayerMain player){
        // Vector3 position;
        // Quaternion rotation;
        // player.transform.GetLocalPositionAndRotation(out position, out rotation);
        // PlayerMain.SavePlayerLocation(position);
        SceneManager.LoadScene(Enums.Scenes.MainMenu.GetHashCode());
    }

    public void RestartFromSave(PlayerMain player){
        player.transform.SetLocalPositionAndRotation(PlayerMain.LoadPlayerLocation(), new Quaternion(0, 0, 0, 0));
        this.gameObject.SetActive(false);
    }

    public void QuitGame(PlayerMain player){
        // Vector3 position;
        // Quaternion rotation;
        // player.transform.GetLocalPositionAndRotation(out position, out rotation);
        // PlayerMain.SavePlayerLocation(position);
        Application.Quit();
    }
}
