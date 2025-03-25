using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class findIteractables : MonoBehaviour
{
    private Transform mainCamera;
    [SerializeField] float pickupRange = 10f;

    [NonSerialized] public PickableItems nearbyObject;
    public TextMeshProUGUI interactMessage;

    private RaycastHit hit;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    private void CheckForInteractables()
    {
        // RaycastHit hit;
        // Debug.Log(mainCamera.forward);
        // Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height)); // Cast from screen center
        Vector3 origin = transform.position + Vector3.up * 1.5f; // Start from player's chest/head
        Vector3 direction = mainCamera.transform.forward; // Use camera forward for aiming
        float sphereRadius = 0.5f; // Small radius for better accuracy

    
        // if (Physics.Raycast(mainCamera.position, mainCamera.forward, out hit, pickupRange))
        // if (Physics.Raycast(ray, out hit, pickupRange))
        if (Physics.SphereCast(origin, sphereRadius, direction, out hit, pickupRange))
        {
            
            // Debug.Log("ray hit");
            if (hit.collider.TryGetComponent<PickableItems>(out var interactable))
            {
                // Debug.Log("object found: " + interactable.itemName);
                nearbyObject = interactable;
                interactMessage.text = nearbyObject.itemName + " (f)";

                
                // interactMessage.SetActive(true); // Show interaction message
                return;
            }
        }

        nearbyObject = null;
        interactMessage.text = "";
        // interactMessage.SetActive(false); // Hide message if no object is nearby
    }

    void Update()
    {
        CheckForInteractables();
    }
}
