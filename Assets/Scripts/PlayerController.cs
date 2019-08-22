using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

// This script controls the player and has been derived from this 2D Unity tutorial: https://unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial/writing-player-script?playlist=17150
// Author: Hannah Sheahan, sheahan.hannah@gmail.com
// Date: 13/05/2019

public class PlayerController : MovingObject
{

    private Animator animator;
    private Timer playerControllerTimer;     // use this to discretize movement input
    private Vector2 currentPlayerPosition;
    private Vector2 goalPosition;
    private Vector2 subgoalPosition; 
    private float minTimeBetweenMoves;
    private string animateHow;
    private bool jumpingNow = false;

    // human control input values
    private int horizontal = 0;
    private int vertical = 0;
    private int jump = 0;                // currently disabled

    // computer control input values
    private int[] shortestStep;
    private bool previousStepHorizontal; 

    // ********************************************************************** //
    //Start overrides the Start function of MovingObject
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        playerControllerTimer = new Timer();
        playerControllerTimer.Reset();
        minTimeBetweenMoves = GameController.control.minTimeBetweenMoves;
        previousStepHorizontal = false;

        base.Start(); // trigger the Start function from the MovingObject parent class
    }

    // ********************************************************************** //

    void Update()
    {
        jump = 0;   // disabling jump control

        switch (GameController.control.controlState)
        {
            case GameController.CONTROL_HUMAN:
                // Get the control input from the keyboard/human player

                horizontal = (int)(Input.GetAxisRaw("Horizontal"));
                vertical = (int)(Input.GetAxisRaw("Vertical"));
                //jump = (int)(Input.GetAxisRaw("Jump"));   // disabling jump control

                // prevent player from moving diagonally
                if (horizontal != 0)
                {
                    vertical = 0;
                }

                break;

            case GameController.CONTROL_COMPUTER:
                // Once we have a goal in mind, consider our current player state, 
                // and implement the single allowable control move that takes us closer to our goal
                horizontal = 0;
                vertical = 0;    // currently just stay still in computer mode.

                currentPlayerPosition = transform.position;

                // Figure out where we want the agent to get to
                subgoalPosition = DetermineGoal(currentPlayerPosition);
                Debug.Log("Computer agent goal position: " + subgoalPosition.x + ", " + subgoalPosition.y);

                // ***HRS change this so that it uses navmesh to control it towards the final goal. We can still use TakeOneStepToGoal but just make it discretise.

                // Take our current position and our goal position in same room, and move towards the goal.
                shortestStep = TakeOneStepToGoal(currentPlayerPosition, subgoalPosition);
                horizontal = shortestStep[0];
                vertical = shortestStep[1];

                // remember the direction of our last move so that we move mostly straight
                previousStepHorizontal = (horizontal != 0); 

                break;

            default:
                Debug.Log("ERROR: Control state not specified, avatar will not be controllable.");
                horizontal = 0;
                vertical = 0;

                break;
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
                animateHow = (horizontal + 1 <= 0) ? "left" : "right";
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
    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall; // cast component that we hit as a Wall
        // do nothing
        Debug.Log("Placeholder: Not sure what OnCantMove should do yet.");
    }

    // ********************************************************************** //

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "boulder")
        {
            //Debug.Log("You just hit a boulder! Yay!");
            // GameController.control.OpenBoxQuestion(true); // Ask player to confirm if they want to open the box (space bar)
            GameController.control.OpenBox();                // Just make the opening sound
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
            // If we want player to 'jump' (space bar) to open box
            /*
            if (jumpingNow) 
            {
                GameController.control.LiftingBoulder();                // transition the state machine
                GameController.control.OpenBoxQuestion(false);
                GameController.control.giftWrapState[other.gameObject.GetComponent<PresentRevealScript>().presentIndex] = 0; // effectively a bool, but shorter to write as string to file 
                GameController.control.RecordGiftStates();              // save a timestamp and the gift states
                other.gameObject.SetActive(false);
            }
            */
            // If we want player to open box on contact
            GameController.control.LiftingBoulder();                // transition the state machine
            GameController.control.giftWrapState[other.gameObject.GetComponent<PresentRevealScript>().presentIndex] = 0; // effectively a bool, but shorter to write as string to file 
            GameController.control.SwitchControlState();
            GameController.control.RecordGiftStates();              // save a timestamp and the gift states
            GameController.control.RecordControlStates();           // save a timestamp and the control states
            other.gameObject.SetActive(false);

        }
    }

    // ********************************************************************** //
    // Only used when player has to press space bar/jump to open box
    /*
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "boulder")
        {
            GameController.control.OpenBoxQuestion(false);
        }
    }
    */
    // ********************************************************************** //

    private int[] TakeOneStepToGoal(Vector2 startPosition, Vector2 endPosition)
    {
        int[] step = new int[2] { -1, 0 };
        Vector2 continuousStep = new Vector2();

        // get x,y coordinates of the beeline movement direction
        continuousStep = Vector2.MoveTowards(startPosition, endPosition, 1) - startPosition; 

        // Method for moving mostly in straight discrete steps
        if (Math.Abs(continuousStep.x) < 0.01f) 
        {
            step = moveVertical(continuousStep); // move vertically only
        }
        else if (Math.Abs(continuousStep.y) < 0.01f) 
        {
            step = moveHorizontal(continuousStep); // move horizontally only
        }
        else // bias our movements to remain in the same direction
        {
            step = previousStepHorizontal ? moveHorizontal(continuousStep) : moveVertical(continuousStep);
        }

        // Method for moving in discrete zig-zag steps every time
        /*
        if (Math.Abs(continuousStep.x) >= Math.Abs(continuousStep.y))  // break diagonal ties by moving horizontally
        {
            step[0] = Math.Sign(continuousStep.x); // move horizontally only
            step[1] = 0;
        }
        else if (Math.Abs(continuousStep.x) < Math.Abs(continuousStep.y))
        {
            step[1] = Math.Sign(continuousStep.y); // move vertically only
            step[0] = 0;
        }
        */

        return step;
    }

    // ********************************************************************** //

    private int[] moveVertical(Vector2 direction) 
    {
        int[] step = new int[2] { 0, 0 };
        step[1] = Math.Sign(direction.y); // move vertically only
        step[0] = 0;
        return step;
    }

    // ********************************************************************** //

    private int[] moveHorizontal(Vector2 direction)
    {
        int[] step = new int[2] { 0, 0 };
        step[0] = Math.Sign(direction.x); // move horizontally only
        step[1] = 0;
        return step;
    }

    // ********************************************************************** //

    private Vector2 DetermineGoal(Vector2 currentPlayerPosition) 
    {

        // ***HRS should really make this so that it just chooses the final goal
        // and then we use navmesh and automatic pathplanning so that it takes us to that final goal (dont hard code the subgoals)

        string currentRoom;
        int boxesLeft;
        Vector2 goalPosition = new Vector2();

        // Define the possible boulder locations (same index order as giftWrapState array)
        Vector2[] boulderLocations = new Vector2[4];
        boulderLocations[0] = new Vector2(-4, -4); // bottom left
        boulderLocations[1] = new Vector2(4, -4);  // bottom right
        boulderLocations[2] = new Vector2(-4, 4);  // top left
        boulderLocations[3] = new Vector2(4, 4);   // top right

        // Define the actual reward locations
        Vector2[] rewardLocations = new Vector2[2];
        rewardLocations[0] = new Vector2(GameController.control.rewardSpawnLocations[0].x, GameController.control.rewardSpawnLocations[0].y);
        rewardLocations[1] = new Vector2(GameController.control.rewardSpawnLocations[1].x, GameController.control.rewardSpawnLocations[1].y);

        // Determine the boulder positions that dont have rewards in them
        Vector2[] nonrewardLocations = new Vector2[2];
        int count = 0;
        for (int i = 0; i < boulderLocations.Length; i++)
        {
            if (!Array.Exists(rewardLocations, element => element == boulderLocations[i]))
            {
                nonrewardLocations[count] = boulderLocations[i];
                count++;
            }
        }

        currentRoom = GameController.control.PlayerInWhichRoom(currentPlayerPosition);
        boxesLeft = GameController.control.giftWrapState.Sum();

        if (boxesLeft == GameController.control.giftWrapState.Length)
        { 
            // Just search the current room because no boxes have been opened yet :)
            switch (currentRoom) 
            {
                case "blue":     // bottom left
                    goalPosition = boulderLocations[0];
                    break;

                case "red":      // bottom right
                    goalPosition = boulderLocations[1];
                    break;
                
                case "yellow":   // top left
                    goalPosition = boulderLocations[2];
                    break;
                
                case "green":    // top right
                    goalPosition = boulderLocations[3];
                    break;

                default:
                    break;
            }
            Debug.Log("Computer agent searching in current room");
        }
        else
        { 
            // One box has been opened, so check the config file policy as to where to search next (take either correct or incorrect box)

            // using the known locations of the correct vs incorrect boxes, move to open the closest one that satisfies the criteria
            if (GameController.control.computerAgentCorrect) 
            {
                // Determine which correct reward is closest
                // compute path distance to rewardLocations[0]...

                // computer path distance to rewardLocations[1] ...

                // set whichever is closest as the goal
                // HRS to do...

            }
            else 
            {
                // Determine which correct reward is closest
                // compute path distance to nonrewardLocations[0]...

                // computer path distance to nonrewardLocations[1] ...

                // set whichever is closest as the goal
                // HRS to do...
            }

            // ... just for now HRS
            goalPosition = new Vector2(1,1);
        }
        return goalPosition;
    }

    // ********************************************************************** //

}
