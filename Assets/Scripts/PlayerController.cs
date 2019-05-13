using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script controls the player and has been derived from this 2D Unity tutorial: https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/writing-player-script?playlist=17150
// Author: Hannah Sheahan, sheahan.hannah@gmail.com
// Date: 13/05/2019

public class PlayerController : MovingObject
{


    private Animator animator;
    private bool playersTurn = true;
    private Timer playerControllerTimer;     // use this to discretize movement input
    private float timeBetweenMoves;
    private string animateHow;

    //Start overrides the Start function of MovingObject
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        playerControllerTimer = new Timer();
        playerControllerTimer.Reset();
        timeBetweenMoves = 0.3f;         // ***HRS later we will read this in from GameController.cs

        base.Start(); // trigger the Start function from the MovingObject parent class
    }

    // Update is called once per frame
    void Update()
    {
        //if (!playersTurn) return; // will eventually be (!GameController.control.playersTurn)

        int horizontal = 0;
        int vertical = 0;
        int jump = 0;

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

            if (playerControllerTimer.ElapsedSeconds() >= timeBetweenMoves)
            {
                animateHow = "jump";
                AnimateNow();
                playerControllerTimer.Reset();
                //AttemptMove<Wall>(horizontal, vertical);
            }
        }


        // if we are attempting to move, check that we can actually move there
        if ((horizontal != 0) || (vertical != 0)) 
        {
            if (playerControllerTimer.ElapsedSeconds() >= timeBetweenMoves)
            {
                //animateHow = (horizontal <= 0_) ? "left" : "right";
                if (Mathf.Approximately(horizontal+1, 0f))
                {
                    animateHow = "left"; 
                }
                else
                {
                    animateHow = "right";
                }

                AnimateNow();
                AttemptMove<Wall>(horizontal, vertical);
            }
        }
    }

    private void AnimateNow() 
    {
        // Start the appropriate player animation
        switch (animateHow)
        {
            case "jump":
                animator.SetTrigger("playerJump");
                break;
            case "left":
                animator.SetTrigger("playerStepLeft");
                break;
            case "right":
                animator.SetTrigger("playerStepRight");
                break;
        }
    }

    protected override void AttemptMove <T> (int xDir, int yDir) 
    //protected override void AttemptMove (int xDir, int yDir)
    {
        base.AttemptMove <T> (xDir, yDir);
        //base.AttemptMove (xDir, yDir);
        RaycastHit2D hit;


        if (Move(xDir, yDir, out hit))
        {
             //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
        }

        playerControllerTimer.Reset();

    }


    // this currently doesnt do anything and I dont think we care. We just dont want to move off the grid
    protected override void OnCantMove <T> (T component) 
    {
        Wall hitWall = component as Wall; // cast component that we hit as a Wall
        // do nothing
        Debug.Log("Placeholder: Not sure what OnCantMove should do yet.");
    }


        
    private void OnTriggerEnter2D (Collider2D other) 
    { 
        if (other.tag == "boulder") 
        {
            Debug.Log("You just hit a boulder! Yay!");
        }
        else if (other.tag == "reward") 
        {
            Debug.Log("You just hit a reward! Yay!");
        }
        else if (other.tag == "bridge") 
        {
            Debug.Log("Passing over a bridge. Woohoo!");
        }
        else 
        {
            Debug.Log("You just hit a mystery object");
        }

    }


}
