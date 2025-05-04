using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DayNightEventManager
{
    public static event Action<DayNightEventData> OnCycleCompletion;

    public static void CycleCompletion(DayNightEventData data)
    {
        OnCycleCompletion?.Invoke(data);
    }
}