using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectDamage : MonoBehaviour
{
    private Transform currentDamageSource = null;

    void OnTriggerEnter(Collider triggerObject)
    {
        if (triggerObject.CompareTag("deer"))
        {
            gameObject.GetComponent<Specs>().currentHealth -= 20;
            // // nearHotObject = true;
            // currentDamageSource = triggerObject.transform;

            // DamageEventData DamageData = new DamageEventData(20f, currentDamageSource);
            // DamageEventManager.DamageDetected(DamageData);
        }
    }

    void OnTriggerExit(Collider triggerObject)
    {
        if (triggerObject.CompareTag("deer"))
        {
            // // nearHotObject = false;
            // DamageEventData fireData = new DamageEventData(0f, null);
            // DamageEventManager.DamageDetected(fireData);
            // currentDamageSource = null;
        }
    }

    

}