using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEventData
{
    public bool IsNearFire;
    public float DistanceToFire;
    public float HeatIntensity;
    public float MaxHeatTemp = 30f;
    public Transform FireSource;

    public FireEventData(bool isNearFire, float distance, float heat, float maxTemp, Transform source)
    {
        IsNearFire = isNearFire;
        DistanceToFire = distance;
        HeatIntensity = heat;
        MaxHeatTemp = maxTemp;
        FireSource = source;
    }
}
