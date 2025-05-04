using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DamageEventManager
{
    public static event Action<DamageEventData> OnDamageDetection;

    public static void DamageDetected(DamageEventData data)
    {
        OnDamageDetection?.Invoke(data);
    }
}