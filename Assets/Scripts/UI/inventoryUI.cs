using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class inventoryUI : MonoBehaviour
{
    private GameObject contextMenu;
    private Button useButton;
    private Button dropButton;
    private Transform selectedItem;

    private Inventory inventory;

    public void setInventory(Inventory inventory){
        this.inventory = inventory;
    }

    private void CreateContextMenu()
    {
        contextMenu = new GameObject("ContextMenu");
        contextMenu.transform.SetParent(transform, false);
        contextMenu.AddComponent<CanvasRenderer>();

        RectTransform rectTransform = contextMenu.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 100); // Menu size

        Image bgImage = contextMenu.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black

        // Add Grid Layout
        VerticalLayoutGroup layout = contextMenu.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 5;

        // Create Use Button
        useButton = UILib.Create.CreateButton("Use", contextMenu.transform);
        useButton.onClick.AddListener(OnUseButtonClick);

        // Create Drop Button
        dropButton = UILib.Create.CreateButton("Drop", contextMenu.transform);
        dropButton.onClick.AddListener(OnDropButtonClick);

        contextMenu.SetActive(false); // Hide initially
    }

    private void seeIfOutSideOfContextMenu(){
        if (contextMenu.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(contextMenu.GetComponent<RectTransform>(), Input.mousePosition))
            {
                contextMenu.SetActive(false);
            }
        }
    }

    public void ShowContextMenu(Transform item)
    {
        if (contextMenu.activeSelf) return;
        selectedItem = item;
        contextMenu.SetActive(true);
        Debug.Log("Showing context menu");

        // Position the menu near the mouse cursor
        Vector2 mousePos = Input.mousePosition;
        contextMenu.transform.position = mousePos + new Vector2(50, -50);
    }
    // Start is called before the first frame update
    void Start()
    {
        CreateContextMenu();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        seeIfOutSideOfContextMenu();
    }

    private void OnUseButtonClick()
    {
        Debug.Log(selectedItem);
        if (selectedItem != null)
        {
            selectedItem.GetComponent<usableItem>().UseItem();
            inventory.RemoveItemFromInventory(selectedItem.GetComponent<PickableItems>());
            Debug.Log("Using item: " + selectedItem.GetComponent<PickableItems>().itemName);
            contextMenu.SetActive(false);
        }
    }

    private void OnDropButtonClick()
    {
        Debug.Log(selectedItem);
        if (selectedItem != null)
        {
            inventory.RemoveItemFromInventory(selectedItem.GetComponent<PickableItems>());
            Debug.Log("Dropped item: " + selectedItem.GetComponent<PickableItems>().itemName);
            contextMenu.SetActive(false);
        }
    }
}
