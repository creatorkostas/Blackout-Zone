using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMain : MonoBehaviour
{
    // public GameObject pauseMenu;
    private Specs specs;
    private findIteractables findInteractablesObjects;
    private PlayerMovement playerMovement;

    private Inventory inventory;
    private float previusHeatIntensity = 0f;

    private GameObject pauseMenu;

    private GameObject messageText;
    private GameObject noteTextScreen;
    private int dayCounter = 0;
    
    private ObjectivesManager objectivesManager;

    // [SerializeField] private Cinemachine.CinemachineFreeLook CinemachineCamera;
    [SerializeField] private bool lockCursor = true;
    // Start is called before the first frame update

    void OnEnable(){
        FireEventManager.OnFireDetection += HandleFireDetection;
        DayNightEventManager.OnCycleCompletion += HandleDayNightCycleCompletion;
    }

    void OnDisable(){
        FireEventManager.OnFireDetection -= HandleFireDetection; // To prevent memory leaks
    }

    static public void SavePlayerLocation(Vector3 position){
        PlayerPrefs.SetFloat("PlayerX", position.x);
        PlayerPrefs.SetFloat("PlayerY", position.y);
        PlayerPrefs.SetFloat("PlayerZ", position.z);
    }

    static public Vector3 LoadPlayerLocation(){
        return new Vector3(PlayerPrefs.GetFloat("PlayerX"), PlayerPrefs.GetFloat("PlayerY"), PlayerPrefs.GetFloat("PlayerZ"));
    }

    void Start()
    {
        // playerAnimations = GetComponent<PlayerAnimations>();
        objectivesManager = GameObject.FindGameObjectWithTag("objectivesManager").GetComponent<ObjectivesManager>();
        messageText = GameObject.FindGameObjectWithTag("messageText");
        messageText.GetComponent<TextMeshProUGUI>().text = "";
        messageText.SetActive(false);
        playerMovement = GetComponent<PlayerMovement>();
        specs = GetComponent<Specs>();
        findInteractablesObjects = GetComponent<findIteractables>();
        // mainCamera = Camera.main.transform;
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        pauseMenu = GameObject.FindGameObjectWithTag("pauseScreen");

        inventory = transform.GetComponent<Inventory>();
        inventory.SetMaxCarryWeight(specs.maxCarryWeight);
        playerMovement.SetVariables(specs, findInteractablesObjects, inventory, pauseMenu);

        specs.StartFoodDrop();
        specs.StartWaterDrop();
        specs.TempChange(State.startDrop, null);
        specs.HealthChange(State.startDrop);
        noteTextScreen = GameObject.FindGameObjectWithTag("noteTextScreen");
        noteTextScreen.SetActive(false);
        pauseMenu.SetActive(false);
        transform.SetLocalPositionAndRotation(LoadPlayerLocation(), new Quaternion(0, 0, 0, 0));
        SetupObjectives();
        // objectivesManager.AddObjective(
        //     new ObjectiveData("passDay1", "Survive the first day", "day1", () => { 
        //         DisplayMessage("You survived the first day!");
        //         objectivesManager.AddObjective(new ObjectiveData("pass5Days", "Survive for 5 days", "day5", () => {
        //             DisplayMessage("You survived for 5 days!");
        //         }));  
                
        //         }) 
        
        // );
        // inventoryScreen.SetActive(false); // Close the inventory screen
    }

    

    // TODO change heat intensity to be relative to distance
    void HandleFireDetection(FireEventData fireData)
    {
        if (fireData.HeatIntensity != previusHeatIntensity){
            previusHeatIntensity = fireData.HeatIntensity;
            if (fireData.IsNearFire)
            {
                specs.TempChange(State.startRaise, fireData);
            }
            else
            {
                specs.TempChange(State.startDrop, null);
            }
        }
    }

    void HandleDayNightCycleCompletion(DayNightEventData DayNighteData)
    {
        // TODO add day counter
        dayCounter++;
        Debug.Log("DayNighteData.hasCompletedCycle: " + DayNighteData.hasCompletedCycle);
        ObjectivesManager.TriggerEvent(objectivesManager, "day"+dayCounter);
        DisplayMessage("Day " + dayCounter);
    }

    void DisplayMessage(string message)
    {
        if (!messageText.activeSelf)
            messageText.SetActive(true);
        messageText.GetComponent<TextMeshProUGUI>().text += message + "\n";
        StartCoroutine(DisableObjectAfterDelay(messageText, 7f, true));
    }

    IEnumerator DisableObjectAfterDelay(GameObject objectToDisable, float delay, bool clearText = false){
        yield return new WaitForSeconds(delay);
        objectToDisable.SetActive(false);
        if (clearText)
            messageText.GetComponent<TextMeshProUGUI>().text = "";
    }

    IEnumerator ExecuteAfterDelay(float delay, Action OnObjectiveCompletion){
        yield return new WaitForSeconds(delay);
        OnObjectiveCompletion();
        
    }
    
    
    void FixedUpdate()
    {
        playerMovement.MovePlayer();
        playerMovement.ApplyGravity();
        

        if(specs.currentHealth <= 0){
            Debug.Log("Game Over");
            inventory.DropAllItems();
            SceneManager.LoadScene("GameOver");
        }else if(specs.currentFood == 0 || specs.currentWater == 0){
            specs.HealthChange(State.startDrop);
        }else{
            specs.HealthChange(State.stopAll);
        }

        // StartCoroutine(LogStats());
    }

    IEnumerator LogStats(){
        while(true){
            Debug.Log("(PlayerMain::FixedUpdate) Health: " + specs.currentHealth);
            Debug.Log("(PlayerMain::FixedUpdate) Food: " + specs.currentFood);
            Debug.Log("(PlayerMain::FixedUpdate) Water: " + specs.currentWater);
            Debug.Log("(PlayerMain::FixedUpdate) Cold: " + specs.currentCold);
            yield return new WaitForSeconds(1);
        }
    }

    public void displayNote(string noteText){
        GameObject.FindGameObjectWithTag("noteText").GetComponent<TextMeshProUGUI>().text = noteText;
        GameObject.FindGameObjectWithTag("noteTextScreen").SetActive(true);
    }

    // -------------------------------------------------------------
    //                            GAMEPLAY
    // -------------------------------------------------------------
    private void SetupObjectives(){
        objectivesManager.AddObjective(
            new ObjectiveData("firstCamp", "Find the nearby camp", "firstCamp", () => { 
                DisplayMessage("You have found the camp!");
                objectivesManager.AddObjective(
                    new ObjectiveData("findTheSupplies", "Find the nessecery supplies to survive", "findSupplies")
                    );
            })
        );

        objectivesManager.AddObjective(
            new ObjectiveData("surviveDay1", "Survive the first day", "day1", () => { 
                DisplayMessage("You survived the first day!");

                objectivesManager.AddObjective(
                    new ObjectiveData("survivefor5Days", "Survive for 5 days untily rescure arive at the camp", "campDay5", () => {
                        DisplayMessage("You survived untile rescure arrived at the camp!");
                    })
                );
                DisplayMessage("Make sure to GET IN the first camp in day 5!");
            })
        );
    }

    void OnTriggerEnter(Collider triggerObject)
    {
        if (triggerObject.CompareTag("firstCamp"))
        {
            DisplayMessage("The first camp");
            ObjectivesManager.TriggerEvent(objectivesManager, "firstCamp");
            if (dayCounter == 5) 
            {
                ObjectivesManager.TriggerEvent(objectivesManager, "campDay5");   
            } else if (dayCounter >= 5){
                DisplayMessage("You missed the rescure window!");
                DisplayMessage("Game Over!");
                StartCoroutine(ExecuteAfterDelay(20, () => {
                    Debug.Log("Game Over");
                    inventory.DropAllItems();
                    SceneManager.LoadScene("GameOver");
                }));
            }
        }
    }
}
