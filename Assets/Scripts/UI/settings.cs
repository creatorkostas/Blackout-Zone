using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public class settings : MonoBehaviour
{
    public GameObject qualityDropdown;
    // Start is called before the first frame update
    public void GoToMainMenu(){
        SceneManager.LoadScene(Enums.Scenes.MainMenu.GetHashCode());
    }

    static public void ChangeOveralQuality(int level){
        Debug.Log("Quality level changed to " + level);
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt("QualityLevel", level);
        if (level == 1){
            QualitySettings.vSyncCount = 0;
        }
        else if (level == 2){
            QualitySettings.vSyncCount = 1;
        }
        else if (level == 3){
            QualitySettings.vSyncCount = 2;
        }
    }


    void Start()
    {
        qualityDropdown.GetComponent<TMP_Dropdown>().SetValueWithoutNotify(PlayerPrefs.GetInt("QualityLevel")-1);
    }

    // public void QualitySettingsDropdown_OnValueChanged(int change)
    // {
    //     ChangeOveralQuality(change);
    // }

    static public void LoadSettings(){
        ChangeOveralQuality(PlayerPrefs.GetInt("QualityLevel"));
    }
}
