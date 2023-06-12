using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldMovement : MonoBehaviour
{
    public CharacterController controller;

    public float gravity = -9.81f;
    public float speed = 6f;
    public float crouchSpeed = 3f;
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float boostSpeed = 15f;
    public float slideLength = 3f;
    public float jumpHeight = 1f;
    public float crouchAnimationLength = 1f;

    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    private bool isGrounded;
    
    private bool isSprinting;
    private bool isCrouching;
    private bool isSliding;
    private bool isLerpingSpeed;
    private bool isLerpingCrouch;
    private float crouchTimer = 0f;
    private float slideTimer = 0f;
    private float speedDown = 0f;
    private float sprintButtonPressed = 0f;
    private float numJumps = 0f;
    private float crouchSmoothness = 5f;

    Vector3 velocity;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        HandleSprinting(x, z);
        HandleSliding();
        HandleCrouching();
        

        controller.Move(move * speed * Time.deltaTime);

        HandleJumping();

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleSprinting(float x, float z) {
        if (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) { // If Left JoyStick pressed, it's pressed
            if (Input.GetButton("JoySprint")) {
                isSprinting = true;
                sprintButtonPressed = 1f;
            }
        }

        if ((Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) || // L JoyStick
            (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))) { // WASD
            if (Input.GetButtonDown("Sprint") || sprintButtonPressed == 1f) { // Sprint pressed
                if (isGrounded) {
                    isSprinting = true;
                    if (isSprinting) {
                        speed = sprintSpeed;
                    }
                }
            }
        }

        if ((Mathf.Abs(x) <= 0.1f && Mathf.Abs(z) <= 0.1f) &&
            (!Input.GetButton("Horizontal") && !Input.GetButton("Vertical"))) { // If not moving, stop sprinting
            speed = walkSpeed;
            isSprinting = false;
            sprintButtonPressed = 0f;
        }
    }

    void HandleCrouching() {
        if (isGrounded) {
            if (Input.GetButtonDown("Crouch")) { // If crouch, crouch
                isCrouching = !isCrouching;
                crouchTimer = 0f;
                slideTimer = 0f;
                isLerpingCrouch = true;
            }

            if (isCrouching) {
                if (Input.GetButtonDown("Sprint") || sprintButtonPressed == 1f) { //If sprint, cancel crouch
                    isCrouching = !isCrouching;
                    crouchTimer = 0f;
                    isLerpingCrouch = true;
                }

                if (Input.GetButtonDown("Jump")) { // If jump, cancel crouch
                    
                    controller.height = 2f;
                    if (isSliding) {
                        speed = boostSpeed;
                    }
                    isCrouching = !isCrouching;
                }
            }
        }

        if (isLerpingCrouch) {
            crouchTimer += Time.deltaTime;
            //slideTimer += Time.deltaTime;
            float p = crouchTimer / crouchAnimationLength;
            p = 1f - Mathf.Pow(1f - p, crouchSmoothness);

            if (!isSprinting) {
                if (isCrouching) {
                    speed = crouchSpeed;
                    controller.height = Mathf.Lerp(controller.height, 1f, p); // Lerp down to short
                } else {
                    speed = walkSpeed;
                    controller.height = Mathf.Lerp(controller.height, 2f, p); // Lerp up to tall
                }

                if (isCrouching) {
                    if (!isGrounded) {
                        controller.height = 2f;
                    }
                }

                if (p > 1 || !isGrounded) {
                    crouchTimer = 0f;
                    isLerpingCrouch = false; // Reset crouch animation
                }
            }
            else if (isSprinting) {
                if (isCrouching) {
                    isSliding = true;
                    speed = sprintSpeed;
                    if (slideTimer >= slideLength) {
                        speed = crouchSpeed;
                        slideTimer = 0;
                    }
                    controller.height = Mathf.Lerp(controller.height, 1f, p); // Lerp down w/o speed change
                } else {
                    //speed = walkSpeed;
                    isSliding = false;
                    controller.height = Mathf.Lerp(controller.height, 2f, p); // Lerp up w/o speed change
                }

                if (isCrouching) {
                    if (!isGrounded) {
                        controller.height = 2f;
                    }
                }

                if (p > 1 || !isGrounded) {
                    crouchTimer = 0f;
                    isLerpingCrouch = false; // Reset crouch animation
                }
            }
        }
    }

    void HandleSliding() {
        if (isSprinting) {
            if (Input.GetButtonDown("Crouch")) {
                isSliding = !isSliding;
            }
        }


    }

    void HandleJumping() {
        if (Input.GetButtonDown("Jump")) {
            if (numJumps < 2f) {
                if (isSprinting) {
                    if (isSliding)
                        speed = boostSpeed;
                }

                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                numJumps++;
            }
        }

        if (isGrounded)
            numJumps = 0f;
    }
}
