using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void GoToMainMenu(PlayerMain player){
        Vector3 position;
        Quaternion rotation;
        player.transform.GetLocalPositionAndRotation(out position, out rotation);
        // PlayerMain.SavePlayerLocation(position);
        SceneManager.LoadScene(Enums.Scenes.MainMenu.GetHashCode());
    }

    public void ResumeGame(){
        this.gameObject.SetActive(false);
    }

    public void QuitGame(PlayerMain player){
        Vector3 position;
        Quaternion rotation;
        player.transform.GetLocalPositionAndRotation(out position, out rotation);
        // PlayerMain.SavePlayerLocation(position);
        Application.Quit();
    }
}
