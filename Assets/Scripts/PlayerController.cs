using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public bool canMove { get; private set; } = true;
    private bool shouldJump => Input.GetButtonDown("Jump");
    private bool shouldDash => Input.GetButtonDown("Dash");

    /*private enum PostJumpAction
    {
        Crouch,
        Sprint,
        None
    }*/

    //private PostJumpAction postJumpAction = PostJumpAction.None;

    [Header("Functional Options")]
    [SerializeField] public bool canJump = true;
    [SerializeField] public bool canDash = true;


    [Header("Movement Parameters")]
    [SerializeField] private float currentSpeed = 0.0f;
    [SerializeField] private float walkSpeed = 8.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float jumpsAllowed = 2.0f;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float airSpeed = 4.0f;
    [SerializeField] private float airFriction = 0.65f;
    [SerializeField] private float jumpsCompleted = 1.0f;
    private bool hasJumped = false;
    private Vector3 jumpVelocity = Vector3.zero;

    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashTime = 0.5f;

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

        HandleMouseLook();

        if (canMove) {
            HandleMovementInput(x, z);

            if (canJump)
                HandleJumping();

            if (canDash)
                HandleDashing();

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput(float x, float z) {

        currentSpeed = walkSpeed;



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

    private void HandleDashing() {
        if (shouldDash)
            StartCoroutine(Dash());
    }
    
    private void ApplyFinalMovements() {
        if (!CharacterController.isGrounded) 
            moveDirection.y -= gravity * Time.deltaTime;

        CharacterController.Move(moveDirection * Time.deltaTime);
    }
    



    private IEnumerator Dash() {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime) {
            CharacterController.Move(moveDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

    }
}