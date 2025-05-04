using System;
using System.Collections;
using UnityEngine;

public class DayNight : MonoBehaviour
{
    public float timeChangeRateInSeconds = 1f;
    public Light sun;

    private float startRogDensity;
    private Color fogColor;

    private Coroutine dayNightCoroutine = null;
    private float startingRotation = 0f;

    public float nightFogDensity = 0.6f;

    private bool isPhenomenon = false;
    
    [Range(0,100)] public int PhenomenonChance = 30;
    void Start()
    {
        startRogDensity = RenderSettings.fogDensity;
        fogColor = RenderSettings.fogColor;
        if (sun != null){
            dayNightCoroutine = StartCoroutine(StartDayTime());
            startingRotation = sun.transform.eulerAngles.x;
        }else{ 
            throw new Exception("DayNight::ERROR The directional light is null"); 
        }
    }

    private void PhenomenonStart(){
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        if (UnityEngine.Random.Range(0,100) <= PhenomenonChance){
            isPhenomenon = true;
            // RenderSettings.fogColor = Color.black;
            RenderSettings.fogDensity = nightFogDensity;
            Debug.Log("Phenomenon started");
        } else {
            isPhenomenon = false;
            Debug.Log("Phenomenon ended");
            // RenderSettings.fogColor = fogColor;
            // RenderSettings.fogDensity = startRogDensity;
        }
    }
    

    // IEnumerator StartDayTime(){
    //     while(true){
    //         sun.transform.Rotate(new Vector3(1,0,0)); 
    //         if(sun.transform.eulerAngles.x == startingRotation){
    //             DayNightEventManager.CycleCompletion(new DayNightEventData(true));
    //         }
    //         yield return new WaitForSeconds(timeChangeRateInSeconds);
    //     }
    // }

    IEnumerator StartDayTime(){
    while (true)
    {
        // Rotate sun
        // sun.transform.Rotate(new Vector3(1, 0, 0));

        // Get sun angle
        float sunAngle = sun.transform.eulerAngles.x;

        // Normalize angle to [0, 360)
        // if (sunAngle > 360f) sunAngle -= 360f;
        PhenomenonStart();

        if (!isPhenomenon){
            // Compute fog density
            // Assume 0° (sunrise) to 180° (sunset) = Daytime, and 180° to 360° = Night
            float fogDensity;
            if (sunAngle < 180f)
            {
                // Daytime: fog goes from night value (0.6) to day value (0.018)
                fogDensity = Mathf.Lerp(nightFogDensity, startRogDensity, sunAngle / 180f);
            }
            else
            {
                // Nighttime: fog goes from day value (0.018) to night value (0.6)
                fogDensity = Mathf.Lerp(startRogDensity, nightFogDensity, (sunAngle - 180f) / 180f);
            }

            RenderSettings.fogDensity = fogDensity;
        }

        // Optional: trigger full cycle event
        if (Mathf.Approximately(sunAngle, startingRotation))
        {
            DayNightEventManager.CycleCompletion(new DayNightEventData(true));
        }

        yield return new WaitForSeconds(timeChangeRateInSeconds);
    }
}

}
