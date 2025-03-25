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
        rb = GetComponent<Rigidbody>();
    }

    // public void PickUp()
    public void PickUp(Transform newParent)
    {
        isHeld = true;
        pickUpParent = newParent;
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.SetParent(pickUpParent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false); // Hide object when picked up
    }

    public bool IsHeld() => isHeld;
}
