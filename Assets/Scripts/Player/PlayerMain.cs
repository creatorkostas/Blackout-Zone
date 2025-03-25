using System.Collections;
using System.Collections.Generic;
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
    

    // [SerializeField] private Cinemachine.CinemachineFreeLook CinemachineCamera;
    [SerializeField] private bool lockCursor = true;
    // Start is called before the first frame update

    void OnEnable(){
        FireEventManager.OnFireDetection += HandleFireDetection;
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

        inventory = GetComponent<Inventory>();
        inventory.SetMaxCarryWeight(specs.maxCarryWeight);
        playerMovement.SetVariables(specs, findInteractablesObjects, inventory, pauseMenu);

        specs.StartFoodDrop();
        specs.StartWaterDrop();
        specs.TempChange(State.startDrop, null);
        specs.HealthChange(State.startDrop);
        pauseMenu.SetActive(false);
        Vector3 loadedLocation = LoadPlayerLocation();
        transform.SetLocalPositionAndRotation(loadedLocation, new Quaternion(0, 0, 0, 0));
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
    
    void FixedUpdate()
    {
        playerMovement.MovePlayer();
        playerMovement.ApplyGravity();
        

        if(specs.currentHealth <= 0){
            Debug.Log("Game Over");
            Application.Quit();
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
}
