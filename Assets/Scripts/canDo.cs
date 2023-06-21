using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class canDo : MonoBehaviour
{
    [SerializeField] private bool CanJump = true;
    [SerializeField] private bool CanSprint = true;
    [SerializeField] private bool CanCrouch = true;
    [SerializeField] private bool CanBoost = true;
    [SerializeField] private bool CanSlide = true;

    private void OnTriggerStay(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Access the player movement script
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            
            playerMovement.canJump = CanJump;
            playerMovement.canSprint = CanSprint;
            playerMovement.canCrouch = CanCrouch;
            playerMovement.canBoost = CanBoost;
            playerMovement.canSlide = CanSlide;
        }
    }
}