using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public bool canMove { get; private set; } = true;
    private bool isSprinting;
    private bool shouldJump => Input.GetButtonDown("Jump");
    private bool shouldCrouch = false;
    private bool shouldSwing => Input.GetButtonDown("Swing");
    private bool shouldDash => Input.GetButtonDown("Dash");

    /*private enum PostJumpAction
    {
        Crouch,
        Sprint,
        None
    }*/

    //private PostJumpAction postJumpAction = PostJumpAction.None;

    [Header("Functional Options")]
    [SerializeField] public bool canSprint = true;
    [SerializeField] public bool canJump = true;
    [SerializeField] public bool canCrouch = true;
    [SerializeField] public bool canSlide = true;
    [SerializeField] public bool canBoost = true;
    [SerializeField] public bool canDash = true;
    [SerializeField] public bool canSwing = true;
    [SerializeField] public bool canSway = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftShift;

    [Header("Movement Parameters")]
    [SerializeField] private float currentSpeed = 0.0f;
    [SerializeField] private float walkSpeed = 8.0f;
    [SerializeField] private float sprintSpeed = 12.0f;
    [SerializeField] private float boostSpeed = 18.0f;
    [SerializeField] private float crouchSpeed = 4.0f;
    private float sprintButtonPressed = 0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float jumpsAllowed = 2.0f;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float jumpsCompleted = 1.0f;
    private bool hasJumped = false;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Sliding Parameters")]
    [SerializeField] private float timeToSlide = 2.0f;
    [SerializeField] private bool shouldSlide = false;
    private bool isSliding;
    private bool duringSlide;

    [Header("Boost Parameters")]
    [SerializeField] private float timeToBoost = 3.0f;
    [SerializeField] private bool shouldBoost = false;
    private bool isBoosting;
    private bool duringBoost;

    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashTime = 0.5f;


    [Header("Sway Parameters")]
    [SerializeField] private float walkSwaySpeed = 14f;
    [SerializeField] private float walkSwayAmount = 0.5f;
    [SerializeField] private float sprintSwaySpeed = 18f;
    [SerializeField] private float sprintSwayAmount = 1f;
    [SerializeField] private float crouchSwaySpeed = 14f;
    [SerializeField] private float crouchSwayAmount = 0.25f;
    private float defaultYPos = 0;
    private float timer;


    [Header("Sound Effects")]
    [SerializeField] private AudioSource jumpFX1;
    [SerializeField] private AudioSource jumpFX2;
    [SerializeField] private AudioSource slideFX;
    [SerializeField] private AudioSource sprintFX;
    
    private Camera playerCamera;
    private CharacterController CharacterController;
    private Animator Animator;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float xRotation = 0f;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        CharacterController = GetComponent<CharacterController>();
        Animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (canMove) {
            HandleMovementInput(x, z);
            HandleMouseLook();

            if (canSprint)
                HandleSprinting(x, z);

            if (canJump)
                HandleJumping();

            if (canCrouch)
                HandleCrouching();
            
            if (canSlide)
                HandleSliding();

            if (canBoost)
                HandleBoosting();

            if (canDash)
                HandleDashing();

            if (canSwing)
                HandleSwinging();

            if (canSway)
                HandleSwaying();

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput(float x, float z) {
        if (!isSliding) {
            currentSpeed = isBoosting ? boostSpeed : isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed;
        }


        /*currentSpeed = this switch
        {
            { isCrouching: true } => crouchSpeed,
            { isSprinting: true } => sprintSpeed,
            _ => walkSpeed,
        };*/


        currentInput = new Vector2(currentSpeed * z, currentSpeed * x);

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + 
                        (transform.TransformDirection(Vector3.right) * currentInput.y) *
                        ((CharacterController.isGrounded) ? 1f : 0.5f);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook() {
        float MouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float MouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -upperLookLimit, lowerLookLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0, MouseX, 0);
    }

    void HandleSprinting(float x, float z) {
        if (Input.GetButton("JoySprint")) { // If Left JoyStick pressed, it's pressed
            isSprinting = true;
            sprintButtonPressed = 1f;
        }

        if (Input.GetButtonDown("Sprint") || sprintButtonPressed == 1f) { // Sprint pressed
            if (CharacterController.isGrounded) {
                sprintFX.Play();
                isSprinting = true;
            }
        }          

        if ((Mathf.Abs(x) <= 0.1f && Mathf.Abs(z) <= 0.1f) &&
            (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))) { // If not moving, stop sprinting
            isSprinting = false;
            isBoosting = false;
            sprintButtonPressed = 0f;
        }
    }

    private void HandleJumping() {  

        if (shouldJump) {
            if (jumpsCompleted < jumpsAllowed) {
                jumpsCompleted++;
                
                if (jumpsCompleted == 1f) {
                    jumpFX1.Play();
                    moveDirection.y = jumpForce;
                } else if (jumpsCompleted == 2f) {
                    jumpFX2.Play();
                    moveDirection.y = jumpForce + 4;
                }
            }
        } else if (CharacterController.isGrounded) {
            jumpsCompleted = 0f;
        }

        
    }

    private void HandleCrouching() {
        if (Input.GetButtonDown("Crouch") && !duringCrouchAnimation)
            //postJumpAction = PostJumpAction.Crouch;
            shouldCrouch = true;
        
        //postJumpAction == PostJumpAction.Crouch
        if (shouldCrouch) {
            if (CharacterController.isGrounded) {
                StartCoroutine(CrouchStand());
                shouldCrouch = false;
            }
        }

        if (isCrouching && (Input.GetButtonDown("Jump") || (Input.GetButtonDown("Sprint") || sprintButtonPressed == 1f)))
            StartCoroutine(CrouchStand());
    }

    private void HandleSliding() {
        if (Input.GetButtonDown("Crouch") && isSprinting) 
            shouldSlide = true;

        if (shouldSlide) {
            if (CharacterController.isGrounded) {
                slideFX.Play();
                isSprinting = false;
                isSliding = true;
                StartCoroutine(Slide());
                shouldSlide = false;
            }
        }
    }

    private void HandleBoosting() {        
        if (shouldBoost) {
            isBoosting = true;
            currentSpeed = boostSpeed;
            StartCoroutine(Boost());
            shouldBoost = false;
        }
    }

    private void HandleDashing() {
        if (shouldDash)
            StartCoroutine(Dash());
    }

    private void HandleSwinging() {
        if (shouldSwing)
            Animator.SetTrigger("SwingAttack");
    }

    private void HandleSwaying() {

    }
    
    private void ApplyFinalMovements() {
        if (!CharacterController.isGrounded) 
            moveDirection.y -= gravity * Time.deltaTime;

        CharacterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand() {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;
        
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = CharacterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = CharacterController.center;

        while(timeElapsed < timeToCrouch) {
            CharacterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            CharacterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        CharacterController.height = targetHeight;
        CharacterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator Slide() {  
        duringSlide = true;

        float timeElapsed = 0;
        float targetSpeed = crouchSpeed;
        float currSpeed = currentSpeed;

        while(timeElapsed < timeToSlide) {
            currentSpeed = Mathf.Lerp(currSpeed, targetSpeed, timeElapsed / timeToSlide);
            timeElapsed += Time.deltaTime;

            if (isSprinting) {
                isSliding = false;
                duringSlide = false;
                slideFX.Stop();
                yield break;
            }

            if (Input.GetButtonDown("Jump")) {
                isSliding = false;
                duringSlide = false;
                shouldBoost = true;
                slideFX.Stop();
                yield break;
            }

            yield return null;
        }

        isSliding = false;
        duringSlide = false;
    }

    private IEnumerator Boost() {  
        duringBoost = true;

        float timeElapsed = 0;
        float targetSpeed = sprintSpeed;
        float currSpeed = currentSpeed;

        while(timeElapsed < timeToBoost) {
            currentSpeed = Mathf.Lerp(currSpeed, targetSpeed, timeElapsed / timeToBoost);
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        
        isSprinting = true;
        isBoosting = false;
        duringBoost = false;
    }

    private IEnumerator Dash() {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime) {
            CharacterController.Move(moveDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

    }
}
