using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace UILib
{
    class Create
    {
        // Default text in center
        public static Button CreateButton(string buttonText, Transform parent, bool alignCenter=true)
        {
            GameObject buttonObj = new GameObject(buttonText);
            buttonObj.transform.SetParent(parent, false);

            Button button = buttonObj.AddComponent<Button>();
            buttonObj.AddComponent<Image>().color = new Color(1, 1, 1, 0.8f); // Light gray background

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(180, 40); // Button size
            AddTextItem(buttonObj.transform, buttonText, alignCenter);

            return button;
        }

        public static void AddTextItem(Transform parent, String text, bool alignCenter=false){ 
            GameObject textObj = new GameObject("ItemText");
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI itemText = textObj.AddComponent<TextMeshProUGUI>();
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(100, 50); // Set size
            itemText.text = text;
            itemText.fontSize = 15;
            if (alignCenter){
                itemText.alignment = TextAlignmentOptions.Center;
            }else{
                itemText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            }
            itemText.color = Color.white;
        }

        public static void AddImage(Transform parent, Vector2 size ,string image){
            GameObject imageObj = new GameObject();
            imageObj.transform.SetParent(parent, false);
            Image itemImage = imageObj.AddComponent<Image>();
            RectTransform imageRect = imageObj.GetComponent<RectTransform>();
            imageRect.sizeDelta = size; // Set size

            // Load a sample sprite (Make sure it's in Resources folder)
            itemImage.sprite = Resources.Load<Sprite>(image);
            
        }

        public static void AddObjectiveText(Transform parent, string text, string name){
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI itemText = textObj.AddComponent<TextMeshProUGUI>();
            itemText.text = text;
            itemText.enableAutoSizing = true;
            itemText.fontStyle = FontStyles.Bold;

            // TODO fix text color
            itemText.overrideColorTags = false;
            itemText.color = Color.white;
        }

        public static void RemoveObjectiveText(string name){
            UnityEngine.Object.Destroy(GameObject.Find(name));
        }
    };

    

    
    
}