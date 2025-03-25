using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FireEventManager
{
    public static event Action<FireEventData> OnFireDetection;

    public static void FireDetected(FireEventData data)
    {
        OnFireDetection?.Invoke(data);
    }
}