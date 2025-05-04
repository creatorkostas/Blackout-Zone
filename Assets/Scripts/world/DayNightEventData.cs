using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightEventData
{
    public bool hasCompletedCycle;
    // TODO add more data like achivement unlocked

    public DayNightEventData(bool hasCompletedCycle)
    {
        this.hasCompletedCycle = hasCompletedCycle;
    }
}
