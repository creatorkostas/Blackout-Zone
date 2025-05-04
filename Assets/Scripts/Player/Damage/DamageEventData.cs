using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEventData
{
    public float DamageAmount;
    public Transform DamageSource;

    public DamageEventData(float DamageAmount, Transform DamageSource)
    {
        this.DamageAmount = DamageAmount;
        this.DamageSource = DamageSource;
    }
}
