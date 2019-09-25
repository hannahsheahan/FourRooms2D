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
    public GameObject gridManager;
    private Grid grid;
    private Animator animator;
    private Timer playerControllerTimer;     // use this to discretize movement input
    private Vector2 currentPlayerPosition;
    private Vector2 previousPlayerPosition;
    private float minTimeBetweenMoves;
    private string animateHow;
    private bool jumpingNow = false;

    // human control input values
    private int horizontal = 0;
    private int vertical = 0;
    private int jump = 0;                // currently disabled

    // computer control input values
    private int[] shortestStep;
    private Pathfinder pathfinder;         // implements A* pathfinding
    private string controlState;
    private string previousControlState;
    private List<Node> plannedPath;
    private List<Node> possiblePath;
    private int pathStep;
    private Vector2 targetPosition;
    private Vector2 nextPosition;
    private float stepTolerance = 0.05f;      // tolerate sub-threshold differences between desired and actual agent positions  
    private float minAgentPlanningTime = 1.0f;
    private Timer agentPlanningTimer;         // use this to give our computer agent some fake 'planning' pause time when control switches to the computer agent


    Vector2[] bridgePositions = new Vector2[4];

    // ********************************************************************** //
    //Start overrides the Start function of MovingObject
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        grid = gridManager.GetComponent<Grid>();

        playerControllerTimer = new Timer();
        playerControllerTimer.Reset();
        minTimeBetweenMoves = GameController.control.minTimeBetweenMoves;

        // initialize computer control variables
        controlState = "Human"; // to make sure we plan the first loop under computer control
        previousControlState = controlState;
        pathfinder = new Pathfinder(grid);

        agentPlanningTimer = new Timer();
        agentPlanningTimer.Reset();

        bridgePositions[0] = new Vector2(0f, 3f);  // top bridge
        bridgePositions[1] = new Vector2(3f, 0f);  // right bridge
        bridgePositions[2] = new Vector2(0f, -3f); // bottom bridge
        bridgePositions[3] = new Vector2(-3f, 0f); // left bridge

        base.Start(); // trigger the Start function from the MovingObject parent class
    }

    // ********************************************************************** //

    void Update()
    {

        if (GameController.control.State >= GameController.STATE_GO)
        {
            jump = 0;   // disabling jump control
            controlState = GameController.control.controlState; // only update this once per loop
            currentPlayerPosition = new Vector2(transform.position.x, transform.position.y);

            // HRS hack for the triggering issue of freeze states on bridges 
            // (linecasting means we have to enable/disable player collidor with movement, triggering freeze state again (if using colliders) whenever we try to move on bridge)
            if ((previousPlayerPosition - currentPlayerPosition).magnitude > stepTolerance)
            { 
                for (int i=0; i < bridgePositions.Length; i++)
                {
                    Vector2 bridge = bridgePositions[i];
                    if ((currentPlayerPosition-bridge).magnitude < stepTolerance) 
                    {
                        // just moved on to a bridge
                        Debug.Log("Gonna freeze now");
                        GameController.control.HallwayFreeze(i);
                    }
                }
            }


            switch (controlState)
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

                    nextPosition = new Vector2(currentPlayerPosition.x + horizontal, currentPlayerPosition.y + vertical);
                    break;

                case GameController.CONTROL_COMPUTER:
                    // Once we have a goal in mind, consider our current player state, 
                    // and implement the single allowable control move that takes us closer to our goal

                    // When we first switch control states to computer control, compute the desired goals
                    if (previousControlState == GameController.CONTROL_HUMAN)
                    {
                        targetPosition = DetermineGoal(currentPlayerPosition);
                        Debug.Log("Computer agent goal position: " + targetPosition.x + ", " + targetPosition.y);

                        // Create an A* pathfinder to plan our path to goal
                        plannedPath = pathfinder.FindPath(currentPlayerPosition, targetPosition);
                        pathStep = 0;
                        for (int i=0; i < plannedPath.Count; i++) 
                        {
                            Debug.Log("path step: " + plannedPath[i].Position.x + ", " + plannedPath[i].Position.y);
                        }
                        LoadNextNode();
                        agentPlanningTimer.Reset();
                    }

                    // Determine the next step to take along our planned path
                    // Debug.Log("pathstep: " + pathStep);

                    if (pathStep < plannedPath.Count) 
                    { 
                        if (agentPlanningTimer.ElapsedSeconds() >= minAgentPlanningTime) // give the agent some fake 'planning time' so it pauses at the start of control takeover
                        { 
                            // Take our current position and our goal position in same room, and move towards the goal.
                            shortestStep = TakeOneStepToGoal(currentPlayerPosition, nextPosition);
                            horizontal = shortestStep[0];
                            vertical = shortestStep[1];
                        }
                        else 
                        {
                            horizontal = 0;
                            vertical = 0;
                        }
                    }
                    else
                    {
                        horizontal = 0;
                        vertical = 0;
                        Debug.Log("Computer agent has reached goal! Yay!");   
                    }
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
            if (playerControllerTimer.ElapsedSeconds() >= minTimeBetweenMoves)
            {
                if ((horizontal != 0) || (vertical != 0))
                {
                    jumpingNow = false;
                    animateHow = (horizontal + 1 <= 0) ? "left" : "right";
                    AnimateNow();
                    playerControllerTimer.Reset();
                    AttemptMove<Wall>(horizontal, vertical);
                }

                if (controlState == GameController.CONTROL_COMPUTER)
                { 
                    if ((currentPlayerPosition - nextPosition).magnitude < stepTolerance)
                    {
                        pathStep++;   // HRS to be careful of this! If the pathstep is updated at the wrong time then our planned policy wont take us all the way there.
                        LoadNextNode();
                    }
                }
            }
            previousControlState = controlState;
            previousPlayerPosition = currentPlayerPosition;
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
            Debug.Log("triggered");
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
        int[] step = new int[2] { 0, 0 };
        Vector2 continuousStep = new Vector2();

        // get x,y coordinates of the beeline movement direction
        continuousStep = Vector2.MoveTowards(startPosition, endPosition, 2) - startPosition;


        // We need a check in here that prevents taking another step if the difference is < epsilon
        if ((currentPlayerPosition - nextPosition).magnitude >= stepTolerance)
        { 
            // Method for moving in discrete zig-zag steps every time
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
        }
        return step;
    }

    // ********************************************************************** //

    private Vector2 PickRewardLocation(List<Vector2> rewardLocations) 
    {
        int[] pathLength = new int[rewardLocations.Count];
        Vector2 goalPosition;

        for (int i = 0; i < rewardLocations.Count; i++)
        {
            possiblePath = pathfinder.FindPath(currentPlayerPosition, rewardLocations[i]);
            pathLength[i] = possiblePath.Count;
        }
        int shortestPathIndex = Array.IndexOf(pathLength, pathLength.Min());
        goalPosition = rewardLocations[shortestPathIndex];
        Debug.Log("Computer agent is picking a boulder that will be rewarded.");

        return goalPosition;
    }

    // ********************************************************************** //

        private Vector2 PickNonRewardLocation(List<Vector2> nonRewardLocations) 
        {
        // compute path distance to nonrewardLocations[0] and nonrewardLocations[1]...
        int[] pathLength = new int[nonRewardLocations.Count];
        Vector2 goalPosition;

        for (int i = 0; i < nonRewardLocations.Count; i++)
        {
            possiblePath = pathfinder.FindPath(currentPlayerPosition, nonRewardLocations[i]);
            pathLength[i] = possiblePath.Count;
        }
        int shortestPathIndex = Array.IndexOf(pathLength, pathLength.Min());
        goalPosition = nonRewardLocations[shortestPathIndex];
        Debug.Log("Computer agent is picking a boulder that will not be rewarded.");

        return goalPosition;
    }

    // ********************************************************************** //

    private Vector2 DetermineGoal(Vector2 playerPosition) 
    {
        // This just determines the final goal for the computer agent
        string currentRoom;
        int boxesLeft;
        int shortestPathIndex;

        Vector2 goalPosition = new Vector2();
        boxesLeft = GameController.control.giftWrapState.Sum();

        // Define the possible boulder locations (same index order as giftWrapState array)
        Vector2[] boulderLocations = new Vector2[4];
        boulderLocations[0] = new Vector2(-4, -4); // bottom left
        boulderLocations[1] = new Vector2(4, -4);  // bottom right
        boulderLocations[2] = new Vector2(-4, 4);  // top left
        boulderLocations[3] = new Vector2(4, 4);   // top right

        // Define the actual reward locations that may be remaining
        Vector2[] possRewardLocations = new Vector2[2];
        possRewardLocations[0] = new Vector2(GameController.control.rewardSpawnLocations[0].x, GameController.control.rewardSpawnLocations[0].y);
        possRewardLocations[1] = new Vector2(GameController.control.rewardSpawnLocations[1].x, GameController.control.rewardSpawnLocations[1].y);

        // Determine the possible remaining boulder positions that dont have rewards in them
        Vector2[] possNonRewardLocations = new Vector2[2];
        int count = 0;
        for (int i = 0; i < boulderLocations.Length; i++)
        {
            if (!Array.Exists(possRewardLocations, element => element == boulderLocations[i]))
            {
                possNonRewardLocations[count] = boulderLocations[i];
                count++;
            }
        }

        List<Vector2> rewardLocations = new List<Vector2>();
        List<Vector2> nonRewardLocations = new List<Vector2>();

        // Consider now only the unexplored boulders (that do and dont contain rewards)
        for (int i=0; i < boulderLocations.Length; i++) 
        { 
            if (GameController.control.giftWrapState[i] != 0) 
            {
                // Add the unopened reward locations to a final list of them
                for (int j=0; j < possRewardLocations.Length; j++) 
                { 
                    if (possRewardLocations[j] == boulderLocations[i]) 
                    {
                        rewardLocations.Add(possRewardLocations[j]);
                    }
                }

                // Add the unopened nonreward locations to a final list of them
                for (int j = 0; j < possNonRewardLocations.Length; j++)
                {
                    if (possNonRewardLocations[j] == boulderLocations[i])
                    {
                        nonRewardLocations.Add(possNonRewardLocations[j]);
                    }
                }
            }
        }

        // No boxes have been opened yet (i.e computer makes first move)
        if (boxesLeft == GameController.control.giftWrapState.Length)
        {
            currentRoom = GameController.control.PlayerInWhichRoom(currentPlayerPosition);

            // Just search the current room because no boxes have been opened yet
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
                // compute path distance to rewardLocations[0] and rewardLocations[1]...
                if (rewardLocations.Count > 0) 
                {
                    goalPosition = PickRewardLocation(rewardLocations);
                }
                else 
                {
                    Debug.Log("Computer agent wanted to go to reward location but none left, so going to a non-reward location.");
                    goalPosition = PickNonRewardLocation(nonRewardLocations);
                }
            }
            else 
            {
                if (nonRewardLocations.Count > 0)
                {
                    // compute path distance to nonrewardLocations[0] and nonrewardLocations[1]...
                    goalPosition = PickNonRewardLocation(nonRewardLocations);
                }
                else 
                {
                    Debug.Log("Computer agent wanted to go to non-reward location but none left, so going to a reward location.");
                    goalPosition = PickRewardLocation(rewardLocations);
                }
            }
        }
        return goalPosition;
    }

    // ********************************************************************** //

    void LoadNextNode() 
    {
        nextPosition = new Vector2(plannedPath[pathStep].Position.x, plannedPath[pathStep].Position.y);
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

}
