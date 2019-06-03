using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the player and has been derived from this 2D Unity tutorial: https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/writing-player-script?playlist=17150
// Author: Hannah Sheahan, sheahan.hannah@gmail.com
// Date: 13/05/2019

public class PlayerController : MovingObject
{

    private Animator animator;
    private Timer playerControllerTimer;     // use this to discretize movement input
    private float minTimeBetweenMoves;
    private string animateHow;
    private bool jumpingNow = false;

    // control input values
    private int horizontal = 0;
    private int vertical = 0;
    private int jump = 0;
    
    // ********************************************************************** //
    //Start overrides the Start function of MovingObject
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        playerControllerTimer = new Timer();
        playerControllerTimer.Reset();
        minTimeBetweenMoves = GameController.control.minTimeBetweenMoves; 

        base.Start(); // trigger the Start function from the MovingObject parent class
    }

    // ********************************************************************** //

    void Update()
    {
        horizontal = (int) (Input.GetAxisRaw("Horizontal"));
        vertical = (int) (Input.GetAxisRaw("Vertical"));
        jump = (int)(Input.GetAxisRaw("Jump"));

        // prevent player from moving diagonally
        if (horizontal != 0) 
        { 
            vertical = 0;
        }

        //prevent player from jumping at same time as moving
        if (jump != 0) 
        {
            horizontal = 0;
            vertical = 0;

            if (playerControllerTimer.ElapsedSeconds() >= minTimeBetweenMoves)
            {
                jumpingNow = true;  // we do this because otherwise jump is updated too quick for the OnTriggerStay2D function and we could miss the boulder lifting action
                animateHow = "jump";
                AnimateNow();
                playerControllerTimer.Reset();
            }
        }

        // if we are attempting to move, check that we can actually move there
        if ((horizontal != 0) || (vertical != 0)) 
        {
            if (playerControllerTimer.ElapsedSeconds() >= minTimeBetweenMoves)
            {
                jumpingNow = false;
                animateHow = (horizontal+1 <= 0) ? "left" : "right";
                AnimateNow();
                playerControllerTimer.Reset();
                AttemptMove<Wall>(horizontal, vertical);
            }
        }
    }

    // ********************************************************************** //

    private void AnimateNow() 
    {
        // Start the appropriate player animation
        switch (animateHow)
        {
            case "jump":
                animator.SetTrigger("playerJump");
                GameController.control.OpenBox();
                break;
            case "left":
                animator.SetTrigger("playerStepLeft");
                GameController.control.PlayMovementSound();
                break;
            case "right":
                animator.SetTrigger("playerStepRight");
                GameController.control.PlayMovementSound();
                break;
        }
    }

    // ********************************************************************** //
    // OnCantMove currently doesnt do anything and I dont think we care but its a good placeholder. We just dont want to move off the grid
    protected override void OnCantMove <T> (T component) 
    {
        Wall hitWall = component as Wall; // cast component that we hit as a Wall
        // do nothing
        Debug.Log("Placeholder: Not sure what OnCantMove should do yet.");
    }

    // ********************************************************************** //

    private void OnTriggerEnter2D (Collider2D other) 
    { 
        if (other.tag == "boulder") 
        {
            //Debug.Log("You just hit a boulder! Yay!");
            GameController.control.OpenBoxQuestion(true);
        }
        else if (other.tag == "reward") 
        {
            //Debug.Log("You just hit a reward! Yay!");
        }
        else if (other.tag == "bridge") 
        {
            //Debug.Log("Passing over a bridge. Woohoo!");
        }
        else 
        {
            Debug.Log("You just hit a mystery object");
        }
    }

    // ********************************************************************** //

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "boulder")
        {
            if (jumpingNow) 
            {
                GameController.control.LiftingBoulder();                // transition the state machine
                GameController.control.OpenBoxQuestion(false);
                GameController.control.giftWrapState[other.gameObject.GetComponent<PresentRevealScript>().presentIndex] = 0; // effectively a bool, but shorter to write as string to file 
                GameController.control.RecordGiftStates();              // save a timestamp and the gift states
                other.gameObject.SetActive(false);
            }
        }
    }

    // ********************************************************************** //

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "boulder")
        {
            GameController.control.OpenBoxQuestion(false);
        }
    }

    // ********************************************************************** //


}
