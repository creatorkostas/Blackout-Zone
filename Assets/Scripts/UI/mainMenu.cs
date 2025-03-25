using UnityEngine;
using UnityEngine.SceneManagement;
// using GameUIEnums;

// public enum Scenes{
//     MainMenu=0,
//     Game=1,
//     Settings=2
// }

public class mainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void LoadGame(){
        SceneManager.LoadScene(Enums.Scenes.Game.GetHashCode());
    }
    public void GoToSettings(){
        SceneManager.LoadScene(Enums.Scenes.Settings.GetHashCode());
    }

    public void QuitGame(){
        Application.Quit();
    }

    void Start()
    {
        settings.LoadSettings();   
    }
    
}
