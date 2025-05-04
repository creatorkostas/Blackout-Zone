using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectiveData
{
    public int objectiveID;
    public string objectiveName;
    public string objectiveDescription;
    public bool isCompleted = false;
    public string completionEvent;
    // Start is called before the first frame update
    public static event Action OnObjectiveCompletion;
    
   
    public ObjectiveData(string title, string description, string completionEvent, Action OnObjectiveCompletionFunction = null)
    {
        objectiveName = title;
        objectiveDescription = description;
        this.completionEvent = completionEvent;
        OnObjectiveCompletion += OnObjectiveCompletionFunction;
    }

    public bool CheckIfCompleted(string triggeredEvent)
    {
        if (isCompleted)
            return false;

        if (triggeredEvent == completionEvent)
        {
            isCompleted = true;
            Debug.Log($"Objective Completed: {objectiveName}");
            
            if (OnObjectiveCompletion != null)
                OnObjectiveCompletion();

            return true;
        }

        return false;
    }
}