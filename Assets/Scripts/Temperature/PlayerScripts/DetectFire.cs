using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectFire : MonoBehaviour
{
    // private bool nearHotObject = false;

    // private Specs specs;

    // public float temperature = 10f;
    // public float maxTemperature = 40f;
    // public float heatRate = 5f;
    // public float coolRate = 2f;

    // private bool nearFire = false;
    private Transform currentFire = null;

    // public void SetSpecs(Specs specs)
    // {
    //     this.specs = specs;
    // }

    // Update is called once per frame
    // Start is called before the first frame update
    void OnTriggerEnter(Collider triggerObject)
    {
        if (triggerObject.CompareTag("HotObject"))
        {
            // nearHotObject = true;
            currentFire = triggerObject.transform;
            float distance = Vector3.Distance(transform.position, currentFire.position);
            float heatIntensity = Mathf.Clamp(10f * distance, 0, 150f);

            FireEventData fireData = new FireEventData(true, distance, heatIntensity, triggerObject.GetComponent<fireStats>().maxTemp, currentFire);
            FireEventManager.FireDetected(fireData);
        }
    }

    void OnTriggerExit(Collider hotObject)
    {
        if (hotObject.CompareTag("HotObject"))
        {
            // nearHotObject = false;
            FireEventData fireData = new FireEventData(false, 0f, 0f, 0f, null);
            FireEventManager.FireDetected(fireData);
            currentFire = null;
        }
    }

    

}