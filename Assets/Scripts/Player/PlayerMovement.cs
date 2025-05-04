using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;



using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;
using Quaternion = UnityEngine.Quaternion;
enum AnimationState
{
    idle=-1,
    walk=0,
    jump=1,
    walk_right=2,
    walk_left=3,
    back_walk_right=4,
    back_walk_left=5,
    forward_walk_right=6,
    forward_walk_left=7,
    back_walk=8,
    run=9,
}

// enum MovementVectors
// {
//     jump = new Vector3(0f, 1f, 0f)
// }

public class PlayerMovement : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    
    private Specs specs;
    private findIteractables findInteractablesObjects;

    private Inventory inventory;
    // private bool isInventoryOpen = false;
    // private bool isPauseMenuOpen = false;
    private GameObject pauseMenu;

    public void SetVariables(Specs specs, findIteractables findInteractablesObjects, Inventory inventory, GameObject pauseMenu){
        this.specs = specs;
        this.findInteractablesObjects = findInteractablesObjects;
        this.inventory = inventory;
        this.pauseMenu = pauseMenu;
        // isInventoryOpen = inventory.inventoryScreen.activeSelf;
        // isPauseMenuOpen = pauseMenu.activeSelf;
    }

    private Transform mainCamera;
    private Vector3 _direction;
    private Vector2 angle;
    private float speed = 10f;
    
    private PlayerAnimations playerAnimations;
    private CharacterController controller;
    // private Animator animator;
    

    public float jumpHeight = 2f; // Adjust jump strength
    public float gravity = -9.81f; // Standard gravity
    private float verticalVelocity; // For jump and gravity
    private bool isJumping;
    private bool isRunning;

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputs = context.ReadValue<Vector2>();
        _direction = new Vector3(inputs[0], 0f, inputs[1]);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        
        
        throw new System.NotImplementedException();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {

        // throw new System.NotImplementedException();
        // GetComponent<Animator>()>.SetVariable("v", 2);
        // if (!controller.isGrounded) return;
        Debug.Log("jump");
        // _direction = new Vector3(0, 1f, 0);
        if (context.started && controller.isGrounded) // Jump only when on ground
        {
            Debug.Log("jump entered");
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // Physics-based jump
            isJumping = true;
        }

        // Debug.Log("jump");
        // throw new System.NotImplementedException();
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        // Debug.Log("(PlayerMovement::OnSprint) Stamina: " + specs.currentStamina);
        if(context.started){
			specs.StaminaChange(State.startDrop);
			isRunning = true;
			speed *= specs.runSpeedMultiplaier;
        }else if (context.canceled) {
			specs.StaminaChange(State.startRaise);
            isRunning = false;
            speed = specs.walkSpeed;
		}

    }

    public void OnPickUp(InputAction.CallbackContext context){
        // Debug.Log("Pick up");
        if (context.started && findInteractablesObjects.nearbyObject != null){
            // findInteractablesObjects.nearbyObject.PickUp();
            findInteractablesObjects.nearbyObject.PickUp(transform);
            Debug.Log(findInteractablesObjects.nearbyObject);
            inventory.AddItemToInventory(findInteractablesObjects.nearbyObject);
            Debug.Log("3");
            speed = inventory.GetReducedSpeed(specs.minSpeed, specs.walkSpeed);
        }
        // Debug.Log(findInteractablesObjects.nearbyObject.itemName);
    }

    // Start is called before the first frame update
    

    public void MovePlayer()
    {
        if (isRunning && specs.currentStamina <= 0){
            speed = specs.walkSpeed;
            isRunning = false;
		}
        playerAnimations.MakeAnimation(controller.isGrounded, _direction, isRunning);

        if (_direction.sqrMagnitude < 0.01f) return;

        // Get camera forward direction (ignoring Y)
        Vector3 cameraForward = mainCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Get camera right direction
        Vector3 cameraRight = mainCamera.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 cameraUp = mainCamera.up;
        cameraUp.y = 0;
        cameraUp.Normalize();


        // Convert moveInput to world direction
        Vector3 moveDirection = cameraRight * _direction.x + cameraForward * _direction.z + cameraUp * _direction.y;
        moveDirection.Normalize();

        // Rotate player towards movement direction
        if (moveDirection != Vector3.zero)
        {
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            float rotationSpeed = 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move the player
        // Debug.Log("Vertical velosity: " + verticalVelocity);
        controller.Move((moveDirection * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    public void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (!isJumping)
                verticalVelocity = -0.5f; // Small value to keep grounded
            isJumping = false;
        }
        else
        {
            verticalVelocity += gravity; //* Time.deltaTime; if it is not in fixed update // Apply gravity over time
        }
    }

    void Start()
    {
        mainCamera = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        playerAnimations = GetComponent<PlayerAnimations>();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        // Debug.Log("(PlayerMovement::OnInventory) isInventoryOpen: " + isInventoryOpen);
        if(context.started){
            if(inventory.inventoryScreen.activeSelf){
                
                inventory.inventoryScreen.SetActive(false);
                // isInventoryOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else{
                // isInventoryOpen = true;
                inventory.inventoryScreen.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if(context.started){
            if(pauseMenu.activeSelf){
                
                pauseMenu.SetActive(false);
                // isPauseMenuOpen = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else{
                // isPauseMenuOpen = true;
                pauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
        }
    }
}
