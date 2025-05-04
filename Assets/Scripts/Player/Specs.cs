using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public enum State{
    startDrop = 0,
    startRaise = 1,
    stopAll = 2,
    updateRaise = 3,
}

public enum StatPanelChild{
    Health = 0,
    Food = 1,
    Water = 2,
    Stamina = 3,
    Cold = 4,
}

public class Specs : MonoBehaviour
{
    // [Header("UI")]
    private GameObject statPanel;
    
    [Header("Player Stats")]
    public float walkSpeed = 10f;
    public float runSpeedMultiplaier = 1.2f; 
    public float minSpeed = 2f; // Minimum speed when carrying heavy load
    public float maxCarryWeight = 50f; // Max weight before reaching min speed

    [Header("Health")]
    public float maxHealth = 100f;
    public float HealthDropRate = 5f; 
    public float HealthRaiseRate = 5f;
    public float HealthChangeTime = 1f;
    
    [Header("Food")]
    public float maxFood = 100f;
    public float FoodDropRate = 5f;
    public float FoodRaiseRate = 5f;
    public float FoodChangeTime = 1f;
    
    [Header("Water")]
    public float maxWater = 100f;
    public float WaterDropRate = 5f;
    public float WaterRaiseRate = 5f;
    public float WaterChangeTime = 1f;
    
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float StaminaDropRate = 5f;
    public float StaminaRaiseRate = 5f;
    public float StaminaChangeTime = 1f;

    [Header("Temperature")]
    public Vector2 TempDefaultRange = new Vector2(1f, 40f);
    public float TempDropRate = 5f;
    public float TempRaiseRate = 5f;
    public float TempChangeTime = 1f;





    [NonSerialized] public float currentHealth;
    [NonSerialized] public float currentFood;
    [NonSerialized] public float currentWater;
    [NonSerialized] public float currentStamina;
    [NonSerialized] public float currentCold;
    [NonSerialized] public bool isBleeding;

    private int healthChangeState = -1; 
    private int TempChangeState = -1; 

    private Image healthBar;
    private Image foodBar;
    private Image waterBar;
    private Image staminaBar;
    private TextMeshProUGUI tempText;
    private Coroutine tempChangeCoroutine = null;
    private Coroutine staminaChangeCoroutine = null;

    void Start()
    {
        statPanel = GameObject.FindGameObjectWithTag("statsPanel");

        currentHealth = maxHealth;
        currentFood = maxFood;
        currentWater = maxWater;
        currentStamina = maxStamina;
        currentCold = TempDefaultRange[1] - 10f;

        healthBar = GameObject.FindGameObjectWithTag("healthBar").GetComponent<Image>();
        foodBar = GameObject.FindGameObjectWithTag("foodBar").GetComponent<Image>();
        waterBar = GameObject.FindGameObjectWithTag("waterBar").GetComponent<Image>();
        staminaBar = GameObject.FindGameObjectWithTag("staminaBar").GetComponent<Image>();
        tempText = GameObject.FindGameObjectWithTag("temptext").GetComponent<TextMeshProUGUI>();
    }

    void FixedUpdate(){
        healthBar.fillAmount = currentHealth / maxHealth;
        foodBar.fillAmount = currentFood / maxFood;
        waterBar.fillAmount = currentWater / maxWater;
        staminaBar.fillAmount = currentStamina / maxStamina;
        tempText.text = currentCold.ToString() + " °C";
        // statPanel.transform.GetChild(StatPanelChild.Cold.GetHashCode()).GetComponent<Image>().fillAmount = currentCold;
    }


    public void StaminaChange(State state){
        if(state == State.startDrop){
            if (staminaChangeCoroutine != null) StopCoroutine(staminaChangeCoroutine);
            staminaChangeCoroutine = StartCoroutine(StaminaDrop());
        }else if(state == State.startRaise){
            if (staminaChangeCoroutine != null) StopCoroutine(staminaChangeCoroutine);
            staminaChangeCoroutine = StartCoroutine(StaminaRaise());
        }
    }

    IEnumerator StaminaRaise(){
        while(currentStamina < maxStamina){
            currentStamina += StaminaRaiseRate;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            yield return new WaitForSeconds(StaminaChangeTime);
        }
    }

    IEnumerator StaminaDrop(){
        while(currentStamina > 0){
            currentStamina -= StaminaDropRate; // Decreases over time
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            yield return new WaitForSeconds(StaminaChangeTime);
        }
    }

    public void StartFoodDrop(){
        StartCoroutine(FoodDrop());
    }

    public void StartWaterDrop(){
        StartCoroutine(WaterDrop());
    }

    IEnumerator FoodDrop(){
        while(currentFood > 0){
            currentFood -= FoodDropRate; // Decreases over time
            currentFood = Mathf.Clamp(currentFood, 0, maxFood);
            yield return new WaitForSeconds(FoodChangeTime);
        }
    }

    IEnumerator WaterDrop(){
        while(currentWater > 0){
            currentWater -= WaterDropRate; // Decreases over time
            currentWater = Mathf.Clamp(currentWater, 0, maxWater); 
            yield return new WaitForSeconds(WaterChangeTime);
        }
    }

    public void HealthChange(State state){
        if(state == State.startDrop && healthChangeState != 0){
            healthChangeState = 0;
            StopCoroutine(HealthRaise());
            StartCoroutine(HealthDrop());
        }else if(state == State.startRaise && healthChangeState != 1){
            healthChangeState = 1;
            StopCoroutine(HealthDrop());
            StartCoroutine(HealthRaise());
        } else if(state == State.stopAll && healthChangeState != 2){
            healthChangeState = 2;
            StopCoroutine(HealthDrop());
            StopCoroutine(HealthRaise());
        }
    }

    public void TempChange(State state, FireEventData fireData)
    {
        if ((state == State.startDrop && TempChangeState != 0) || fireData == null)
        {
            TempChangeState = 0;
            if (tempChangeCoroutine != null) StopCoroutine(tempChangeCoroutine); // Stop current coroutine
            tempChangeCoroutine = StartCoroutine(TempDrop());
        }
        else if (state == State.startRaise && TempChangeState != 1)
        {
            TempChangeState = 1;
            if (tempChangeCoroutine != null) StopCoroutine(tempChangeCoroutine); // Stop current coroutine
            tempChangeCoroutine = StartCoroutine(TempRaise(fireData));
        }
        else if (state == State.updateRaise)
        {
            if (tempChangeCoroutine != null) StopCoroutine(tempChangeCoroutine); // Stop current coroutine
            tempChangeCoroutine = StartCoroutine(TempRaise(fireData));
        }
    }

    IEnumerator TempDrop(){
        while(true){
            currentCold -= TempDropRate; //TODO Add drop based on temperature
            // currentCold = Mathf.Clamp(currentCold, 0, maxCold);  
            yield return new WaitForSeconds(TempChangeTime);
        }
    }

    IEnumerator TempRaise(FireEventData fireData){
        while(true){ 
            currentCold += fireData.HeatIntensity;
            // currentCold = Mathf.Clamp(currentCold, -500, TempDefaultRange.y + 30); 
            currentCold = Mathf.Clamp(currentCold, -500, fireData.MaxHeatTemp); 
            yield return new WaitForSeconds(TempChangeTime);
        }
    }

    IEnumerator HealthDrop(){
        float dropRate;
        while(currentHealth > 0){
            dropRate = 0;
            if(isBleeding) dropRate += HealthDropRate * 0.5f; // TODO Add bleeding intensity 
            if(currentFood == 0 ) dropRate += HealthDropRate * 0.25f;
            if(currentWater == 0) dropRate += HealthDropRate * 0.5f;
            if(currentCold > TempDefaultRange[1]) dropRate = HealthDropRate * Mathf.Log(10, Mathf.Abs(currentCold)-30); // works best for max 40
            if(currentCold < TempDefaultRange[0]) dropRate += HealthDropRate * Mathf.Log(10, -1*Mathf.Abs(currentCold)-7); // works best for min 8
            currentHealth -= dropRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
            yield return new WaitForSeconds(HealthChangeTime);
        }
    }

    IEnumerator HealthRaise(){
        float raiseRate;
        while(currentHealth < maxHealth){
            raiseRate = 0;
            if(currentFood == 100 ) raiseRate += HealthRaiseRate + HealthRaiseRate * 0.25f;
            if(currentWater == 100 ) raiseRate += HealthRaiseRate + HealthRaiseRate * 0.5f;
            if(currentCold > TempDefaultRange.x && currentCold < TempDefaultRange.y ) raiseRate += HealthRaiseRate + HealthRaiseRate * 0.5f;  
            currentHealth += raiseRate;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            yield return new WaitForSeconds(HealthChangeTime);
        }
    }
}
