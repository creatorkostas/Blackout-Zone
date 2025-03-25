using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private float currentWeight = 0f;
    private float maxCarryWeight = 50f;
    [NonSerialized] public GameObject inventoryScreen;

    private Transform inventoryGrid;

    //     private float currentWeight = 0f;
//     private float maxCarryWeight = 50f;
//     public GameObject inventoryScreen;
//     private Transform inventoryGrid;

    
    

    

//     public List<PickableItems> inventory = new List<PickableItems>();


    public void SetMaxCarryWeight(float maxCarryWeight){
        this.maxCarryWeight = maxCarryWeight;
    }
    // float totalWeight = inventory.GetTotalWeight();
    // float weightFactor = Mathf.Clamp01(1 - (currentWeight / maxCarryWeight));
    // float currentSpeed = Mathf.Lerp(minSpeed, baseSpeed, weightFactor);

    [Header("Inventory")]
    public List<PickableItems> inventory = new List<PickableItems>();
    // Start is called before the first frame update
    public float GetReducedSpeed(float minSpeed, float baseSpeed){
        float weightFactor = Mathf.Clamp01(1 - (currentWeight / maxCarryWeight));
        return Mathf.Lerp(minSpeed, baseSpeed, weightFactor);
    }

    public void AddGUIItem(Transform parent, string itemName, bool isUsable, Transform item){
        GameObject panelObjParent = new GameObject(item.name);
        panelObjParent.transform.SetParent(parent, false);
        // RectTransform panelRect = 
        panelObjParent.AddComponent<RectTransform>();
        Button panelObj = panelObjParent.AddComponent<Button>();
        // GameObject panelObj = panelObjParent.GetComponent<Button>().gameObject;
        // panelRect.sizeDelta = new Vector2(300, 400); // Set panel size
        panelObj.onClick.AddListener(() => inventoryScreen.transform.GetChild(0).GetComponent<inventoryUI>().ShowContextMenu(item));

        panelObj.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f); // Semi-transparent background

        // Add Grid Layout Group
        GridLayoutGroup grid = panelObj.gameObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 1; // One column

        // Create Image Object
        UILib.Create.AddImage(panelObj.transform, new Vector2(100, 100), "your_image_name");
        // GameObject imageObj = new GameObject();
        // imageObj.transform.SetParent(panelObj.transform, false);
        // Image itemImage = imageObj.AddComponent<Image>();
        // RectTransform imageRect = imageObj.GetComponent<RectTransform>();
        // imageRect.sizeDelta = new Vector2(100, 100); // Set size

        // // Load a sample sprite (Make sure it's in Resources folder)
        // itemImage.sprite = Resources.Load<Sprite>("your_image_name");

        // Create Text Object
        UILib.Create.AddTextItem(panelObj.transform, itemName);
    }

    

    

    public void AddItemToInventory(PickableItems pickableItem){
        inventory.Add(pickableItem);
        if (pickableItem.TryGetComponent<usableItem>(out usableItem usableItem)){
            AddGUIItem(inventoryGrid, pickableItem.itemName, true, pickableItem.gameObject.transform);
        }else{
            AddGUIItem(inventoryGrid, pickableItem.itemName, false, null); // TODO check if null is edge case
        }
        currentWeight += pickableItem.waight;
        Debug.Log("Added item to inventory: " + pickableItem.itemName + " weight: " + pickableItem.waight);
    }

    public void RemoveItemFromInventory(PickableItems pickableItem){
        inventory.Remove(pickableItem);
        currentWeight -= pickableItem.waight;
        Destroy(GameObject.Find(pickableItem.gameObject.name));
        Debug.Log("Removed item from inventory: " + pickableItem.itemName + " weight: " + pickableItem.waight);
    }

    void Start()
    {
        inventoryScreen = GameObject.FindGameObjectWithTag("inventoryScreen");
        inventoryScreen.SetActive(false); // Close the inventory screen
        inventoryGrid = inventoryScreen.transform.GetChild(0).transform.GetChild(0).transform;
        inventoryScreen.transform.GetChild(0).GetComponent<inventoryUI>().setInventory(this);
        // inventoryGrid = GameObject.FindGameObjectWithTag("inventoryGrid").transform;
        
    }

    

    

    

    void Update()
    {
        // seeIfOutSideOfContextMenu();
    }
    

}