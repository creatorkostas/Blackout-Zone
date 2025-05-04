using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItems : MonoBehaviour
{
    public string itemName; // Name of the item for inventory
    public float waight = 1f;
    public Transform pickUpParent; 
    private Rigidbody rb;
    private bool isHeld = false;

    // public string GetItemName() => itemName;
    private void Start()
    {
        // if (pickUpParent == null)
        // {
        //     pickUpParent = GameObject.FindGameObjectWithTag("Player").transform;
        // }

        // if (pickUpParent == null){
        //     Debug.LogError("No player found");
        // }
        rb = GetComponent<Rigidbody>();
        // transform.SetParent(pickUpParent);
    }

    // public void PickUp()
    public void PickUp(Transform newParent)
    {
        isHeld = true;
        pickUpParent = newParent;
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false); // Hide object when picked up
    }

    public void Drop(){
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.localPosition = pickUpParent.transform.position;
        gameObject.SetActive(true); // Show object when dropped
    }

    public bool IsHeld() => isHeld;
}
