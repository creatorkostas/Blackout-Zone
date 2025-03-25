using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCategory{
    food,
    water,
    health,
    stamina,
    temperature,
    other
}

public class usableItem : MonoBehaviour
{
    public ItemCategory itemCategory;
    public float restore;
    
    // Start is called before the first frame update
    public void UseItem(){
        Specs specs = this.gameObject.GetComponentInParent<Specs>();
        if(itemCategory == ItemCategory.food){
            specs.currentFood += restore;
        } else if(itemCategory == ItemCategory.water){
            specs.currentWater += restore;
        } else if(itemCategory == ItemCategory.health){
            specs.currentHealth += restore;
        }
        
    }
}
