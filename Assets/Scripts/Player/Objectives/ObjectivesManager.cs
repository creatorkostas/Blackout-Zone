using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectivesManager : MonoBehaviour
{
    private GameObject objectivesList;

    public List<ObjectiveData> activeObjectives = new List<ObjectiveData>();

    void Start()
    {
        objectivesList = GameObject.FindGameObjectWithTag("objectivesList");
    }

    public void AddObjective(ObjectiveData newObjective)
    {
        activeObjectives.Add(newObjective);
        UILib.Create.AddObjectiveText(objectivesList.transform, newObjective.objectiveDescription, newObjective.objectiveName);
    }

    // <--- Here's the new important function
    public static void TriggerEvent(ObjectivesManager objectivesManager, string eventName)
    {
        foreach (ObjectiveData obj in objectivesManager.activeObjectives)
        {
            if (obj.CheckIfCompleted(eventName))
            {
                Debug.Log($"Triggered completion of: {obj.objectiveName}");
                UILib.Create.RemoveObjectiveText(obj.objectiveName);
                objectivesManager.activeObjectives.Remove(obj);
                // Change to break to make something else after completion
                return;
            }
        }

    }
}
