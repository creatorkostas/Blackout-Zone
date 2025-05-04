using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public enum ItemCategory{
    food,
    water,
    health,
    stamina,
    temperature,
    other
}
[System.Serializable]
public class Category{
    public ItemCategory category;
    public float restore;
    public string noteText;
}

public class usableItem : MonoBehaviour
{
    public Category []itemCategory;
    
    
    // public float restore;
    // public Dictionary<Category, float[]> itemCategory; 
    // = new Dictionary<Category, float[]>();

    
    // Start is called before the first frame update
    public bool IsNote(){
        if (itemCategory.Length == 1 && itemCategory[0].category == ItemCategory.other){
            return true;
        }
        return false;
    }
    public void UseItem(){
        Specs specs = GameObject.FindGameObjectWithTag("Player").GetComponentInParent<Specs>();
        PlayerMain playerMain = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMain>();
        if (itemCategory.Length == 1 && itemCategory[0].category == ItemCategory.other){
            playerMain.displayNote(itemCategory[0].noteText);
        }else{
            foreach(Category category in itemCategory){
                if(category.category == ItemCategory.food){
                    specs.currentFood += category.restore;
                } else if(category.category == ItemCategory.water){
                    specs.currentWater += category.restore;
                } else if(category.category == ItemCategory.health){
                    specs.currentHealth += category.restore;
                }
            }
        }
        
    }
}
