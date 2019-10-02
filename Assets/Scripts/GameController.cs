using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityStandardAssets.Characters.FirstPerson;
using System.IO;
using System.Linq;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// The GameController is a singleton script will control the game flow between scenes, 
    /// and centralise everything so all main processes branch from here.
    /// Author: Hannah Sheahan, sheahan.hannah@gmail.com
    /// Date: 30 Oct 2018
    /// Notes: N/A
    /// Issues: N/A
    /// 
    /// </summary>

    // Persistent controllers for data management and gameplay
    private DataController dataController;
    public static GameController control;
    private FramesPerSecond frameRateMonitor;

    // Game data
    private GameObject Player;
    private GameObject MainCamera;
    private GameData currentGameData;
    private string filepath;

    // Start-of-trial data
    private TrialData currentTrialData;
    private string currentMapName;
    private string currentSceneName;

    public Vector3 playerSpawnLocation;
    public Vector3 playerSpawnOrientation;
    public Vector3[] rewardSpawnLocations;
    public bool doubleRewardTask;
    public bool freeForage;
    public int rewardsRemaining;
    public Vector3[] presentPositions;
    public int numberPresentsPerRoom;
    public bool[] bridgeStates;         // used to enable/disable different bridges
    public string playerRoom = "";
    private string nextScene;
    public QuestionData questionData;
    public string debriefResponse;
   
    // Within-trial data that changes with timeframe
    private bool playerControlActive;
    private bool starFound = false;
    private bool boulderLifted = false;

    // Audio clips
    public AudioClip starFoundSound;
    public AudioClip goCueSound;
    public AudioClip errorSound;
    public AudioClip fallSound;
    public AudioClip openBoxSound;
    public AudioClip movementSound;
    private AudioSource source;

    // Messages to the screen
    public string displayMessage = "noMessage";
    public string textMessage = "";
    public bool displayCue;
    public string rewardType;
    public bool[] rewardsVisible;
    public int maxNRewards = 20;                // ensure this is at least NActualRewards + 1, so that we can animate the canvas reward for scanning
    public int trialScore = 0;
    public int totalScore = 0;
    public int nextScore;
    public bool flashTotalScore = false;
    public bool scoreUpdated = false;
    public bool pauseClock = false;
    public bool flashCongratulations = false;
    public bool congratulated = false;
    private float beforeScoreUpdateTime = 1.2f;  // this is just for display (but maybe relevant for fMRI?)  ***HRS
    public float animationTime;
    public float preRewardAppearTime;
    public float blankTime;
    public bool[] scaleUpReward;
    public bool showCanvasReward;
    public bool debriefResponseRecorded;

    // Timer variables
    private Timer experimentTimer;
    private Timer stateTimer;
    private Timer movementTimer;
    public Timer messageTimer;
    private Timer restbreakTimer;
    private Timer getReadyTimer;
    private Timer debriefResponseTimer;
    private float movementTime;
    public float firstMovementTime;
    public float totalMovementTime;
    public float totalExperimentTime;
    public float currentMovementTime;
    public float[] hallwayFreezeTime;
    public float preFreezeTime;
    public float currentFrozenTime;
    public bool displayTimeLeft;
    public float firstFrozenTime;
    public float debriefResponseTime;

    public float maxMovementTime;
    private float preDisplayCueTime;
    private float goCueDelay;
    private float displayCueTime;
    private float goalHitPauseTime;
    private float finalGoalHitPauseTime;
    public float minDwellAtReward;
    public float displayMessageTime;
    public float errorDwellTime;
    public float restbreakDuration;
    public float elapsedRestbreakTime;
    public float getReadyTime;
    public float getReadyDuration;
    public float oneSquareMoveTime;
    public float minTimeBetweenMoves;

    public float dataRecordFrequency;           // NOTE: this frequency is referred to in TrackingScript.cs for player data and here for state data
    public float timeRemaining;

    private float minFramerate = 30f;           // minimum fps required for decent gameplay on webGL build (fps depends on user's browser and plugins)

    // Error flags
    public bool FLAG_trialError;
    public bool FLAG_trialTimeout;
    public bool FLAG_fullScreenModeError;
    public bool FLAG_dataWritingError;
    public bool FLAG_frameRateError;
    public bool FLAG_cliffFallError;

    public bool blankScreen = false;            // flag for indicating whether showing a between-trial blank screen
    public bool darkTintScreen = false;         // for indicating the darkened screen tint when traversing hallways

    // Game-play state machine states
    public const int STATE_STARTSCREEN = 0;
    public const int STATE_SETUP = 1;
    public const int STATE_BLANKSCREEN = 2;
    public const int STATE_STARTTRIAL = 3;
    public const int STATE_GOALAPPEAR = 4;
    public const int STATE_DELAY = 5;
    public const int STATE_GO = 6;
    public const int STATE_MOVING1 = 7;
    public const int STATE_SHOWREWARD = 8;
    public const int STATE_STAR1FOUND = 9;
    public const int STATE_MOVING2 = 10;
    public const int STATE_STAR2FOUND = 11;
    public const int STATE_FINISH = 12;
    public const int STATE_NEXTTRIAL = 13;
    public const int STATE_INTERTRIAL = 14;
    public const int STATE_TIMEOUT = 15;
    public const int STATE_ERROR = 16;
    public const int STATE_REST = 17;
    public const int STATE_GETREADY = 18;
    public const int STATE_PAUSE = 19;
    public const int STATE_HALLFREEZE = 20;
    public const int STATE_DEBRIEF = 21;
    public const int STATE_EXIT = 22;
    public const int STATE_MAX = 23;

    // Computer/human control states
    public const string CONTROL_HUMAN = "Human";
    public const string CONTROL_COMPUTER = "Computer";

    private string[] stateText = new string[] { "StartScreen", "Setup", "BlankScreen", "StartTrial", "GoalAppear", "Delay", "Go", "Moving1", "ShowReward", "FirstGoalHit", "Moving2", "FinalGoalHit", "Finish", "NextTrial", "InterTrial", "Timeout", "Error", "Rest", "GetReady", "Pause", "HallwayFreeze", "Debrief", "Exit", "Max" };
    public int State;
    public int previousState;     // Note that this currently is not thoroughly used - currently only used for transitioning back from the STATE_HALLFREEZE to the previous gameplay
    public List<string> stateTransitions = new List<string>();   // recorded state transitions (in sync with the player data)

    public string controlState;
    public int controlStateIndex = 0;
    public string[] controlStateOrder;                      // specified transition pattern for control (e.g. human -> computer)
    public bool computerAgentCorrect;
    public string previousControlState;
    public List<string> controlStateTransitions = new List<string>();   // recorded control-state transitions (in sync with the player data)

    public bool waitingForScannerStart = false;
    public bool continueRecordingScanner = false;
    public List<float> scannerTriggerTimes = new List<float>();

    public bool playersTurn;      // for moving in discrete steps on a 2D grid
    public int[] giftWrapState;
    public List<string> giftWrapStateTransitions = new List<string>();   // recorded state of the giftboxes (in sync with the player data)
    public int hallwayTraversed = 0;

    private bool gameStarted = false;

    // ********************************************************************** //

    void Awake()           // Awake() executes once before anything else
    {
        // Make GameController a singleton
        if (control == null)   // if control doesn't exist, make it
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if (control != this) // if control does exist, destroy it
        {
            Destroy(gameObject);
        }
    }

    // ********************************************************************** //

    private void Start()     // Start() executes once when object is created
    {
        dataController = FindObjectOfType<DataController>(); //usually be careful with 'Find' but in this case should be ok. Ok this fetches the instance of DataController, dataController.
        source = GetComponent<AudioSource>();
        frameRateMonitor = GetComponent<FramesPerSecond>();

        // Trial invariant data
        filepath = dataController.filePath;   //this works because we actually have an instance of dataController
        Debug.Log("File path: " + filepath);
        dataRecordFrequency = dataController.GetRecordFrequency();
        restbreakDuration = dataController.GetRestBreakDuration();
        getReadyDuration = dataController.GetGetReadyDuration();

        // Initialise the timers
        experimentTimer = new Timer();
        movementTimer = new Timer();
        messageTimer = new Timer();
        restbreakTimer = new Timer();
        getReadyTimer = new Timer();
        debriefResponseTimer = new Timer();

        // Initialise FSM State
        State = STATE_STARTSCREEN;
        previousState = STATE_STARTSCREEN;
        stateTimer = new Timer();
        stateTimer.Reset();
        stateTransitions.Clear();

        // Initialise giftwrap states
        giftWrapState = new int[4 * dataController.numberPresentsPerRoom];
        giftWrapStateTransitions.Clear();

        // Initialise control states
        controlState = CONTROL_HUMAN;
        controlStateTransitions.Clear();

        // Ensure cue images are off
        displayCue = false;
        rewardsVisible = new bool[maxNRewards];   //default
        scaleUpReward = new bool[maxNRewards];

        StartExperiment();
    }

    // ********************************************************************** //

    private void Update()     // Update() executes once per frame
    {

        // Wait for the scanner to trigger the experient to start (after all consent, participant info etc), then log each subsequent trigger
        if (waitingForScannerStart) 
        {
            //if (Input.GetKeyDown(KeyCode.T))  // the scanner triggers the experiment to start (and start logging time)
            //{
                StartGame(); 
                waitingForScannerStart = false;
                continueRecordingScanner = true;
            //}
        }
        else if (continueRecordingScanner)
        {
            if (Input.GetKeyDown(KeyCode.T))  // log any subsequent 't' code presses (according to Sam)
            {
                scannerTriggerTimes.Add(experimentTimer.ElapsedSeconds());
            }
        }


        UpdateText();

        // Check that everything is working properly
        CheckFullScreen();
        CheckWritingProperly();
        CheckFramerate();

        if (!pauseClock)
        {
            currentMovementTime = movementTimer.ElapsedSeconds();
        }


        switch (State)
        {
            case STATE_STARTSCREEN:
                // Note: we chill out here in this state until all the starting info pages are done
                if (gameStarted)
                {
                    StateNext(STATE_SETUP);
                }
                break;

            case STATE_SETUP:

                if (gameStarted) // this deals with the case that we encounter a writing error during the start screens etc
                { 
                    switch (TrialSetup())
                    {
                        case "StartTrial":
                            // ensure the reward is hidden from sight
                            for (int i = 0; i < rewardsVisible.Length; i++)
                            {
                                rewardsVisible[i] = false;
                            }
                            StateNext(STATE_BLANKSCREEN);

                            break;
                        case "Menus":
                            // fix.  ***HRS this should never have to do anything
                            break;

                        case "GetReady":
                            getReadyTimer.Reset();
                            StateNext(STATE_GETREADY);
                            break;

                        case "RestBreak":
                            restbreakTimer.Reset();
                            StateNext(STATE_REST);
                            break;

                        case "QuestionTime":
                            Cursor.visible = true;
                            debriefResponseTimer.Reset();
                            StateNext(STATE_DEBRIEF);
                            break;

                        case "Exit":
                            totalExperimentTime = experimentTimer.ElapsedSeconds();
                            Cursor.visible = true;
                            StateNext(STATE_EXIT);
                            break;
                    }
                }
                break;


            case STATE_BLANKSCREEN:

                // display a blank screen at the start of each trial, for N seconds while things get set up, and to jitter time between trials for fMRI
                blankScreen = true;

                // Position the camera ready for the upcoming scene
                MoveCamera(playerSpawnLocation);

                // Set the control configuration (human/computer)
                controlState = controlStateOrder[0];

                StartRecording();

                if (stateTimer.ElapsedSeconds() > blankTime) // Note: this can be a different blankTime per trial etc once we jitter in ExperimentConfig
                {
                    blankScreen = false;
                    StateNext(STATE_STARTTRIAL);
                }
                break;

            case STATE_STARTTRIAL:

                // Wait until the goal/target cue appears (will take a TR here)
                if (stateTimer.ElapsedSeconds() >= preDisplayCueTime)
                {
                    Player.GetComponent<PlayerController>().enabled = false;
                    StateNext(STATE_GOALAPPEAR);
                }
                break;


            case STATE_GOALAPPEAR:
                // display the reward type cue
                displayCue = true;
                if (stateTimer.ElapsedSeconds() > displayCueTime)
                {
                    displayCue = false;
                    starFound = false;
                    boulderLifted = false;
                    StateNext(STATE_DELAY);
                    break;
                }
                break;

            case STATE_DELAY:
                // Wait for the go audio cue (will take a TR here)

                if (stateTimer.ElapsedSeconds() >= goCueDelay)
                {
                    source.PlayOneShot(goCueSound, 1F);
                    for (int i = 0; i < rewardsVisible.Length; i++)
                    {
                        rewardsVisible[i] = true;
                    }
                    StateNext(STATE_GO);
                }
                break;

            case STATE_GO:

                // Enable the controller
                Player.GetComponent<PlayerController>().enabled = true;

                // Make a 'beep' go sound and start the trial timer
                movementTimer.Reset();
                displayTimeLeft = true;   // make the trial time countdown visible
                StateNext(STATE_MOVING1);
                break;

            case STATE_MOVING1:

                if (!Player.GetComponent<PlayerController>().enabled)
                {
                    Player.GetComponent<PlayerController>().enabled = true; // let the player move again
                }

                if (movementTimer.ElapsedSeconds() > maxMovementTime)  // the trial should timeout
                {
                    StateNext(STATE_TIMEOUT);
                }

                if (boulderLifted) 
                {
                    Debug.Log("Boulder lifted now.");
                    previousState = State;
                    movementTime = movementTimer.ElapsedSeconds();
                    StateNext(STATE_SHOWREWARD);  // go to blank screen
                }
                break;

            case STATE_SHOWREWARD:

                Player.GetComponent<PlayerController>().enabled = false;

                if (stateTimer.ElapsedSeconds() > minDwellAtReward)  // Note: this is just a proxy for waiting a bit prior to moving to blank screen
                {
                    // turn screen blank
                    //blankScreen = true;

                    if (stateTimer.ElapsedSeconds() > (minDwellAtReward + preRewardAppearTime))
                    {
                        // show the reward when ready
                        if (!showCanvasReward)
                        {
                            if (starFound)
                            {
                                showCanvasReward = true;
                                source.PlayOneShot(starFoundSound, 1F);
                            }
                        }

                        // turn off the reward and transition state
                        if (stateTimer.ElapsedSeconds() > (minDwellAtReward + preRewardAppearTime + goalHitPauseTime))
                        {
                            if (showCanvasReward)
                            {
                                // First reward collected (or not if it's the many-reward, free-foraging case)
                                if (rewardsRemaining > 1)
                                {
                                    firstMovementTime = movementTime;
                                    StateNext(STATE_STAR1FOUND);
                                }
                                else
                                {   // STATE_STAR2FOUND is the state accessed when the FINAL reward to be collected is found
                                    totalMovementTime = movementTime;
                                    StateNext(STATE_STAR2FOUND);
                                }
                            }
                            else
                            {
                                // there was no reward, so go back to previous moving state
                                StateNext(previousState);
                            }
                            //blankScreen = false;
                            showCanvasReward = false;
                        }
                    }
                    boulderLifted = false;
                }
                break;

            case STATE_STAR1FOUND:

                // disable the player control and reset the starFound trigger ready to collect the next star
                starFound = false;

                if (!freeForage) 
                {
                    // Guide a player a little more on practice trials
                    if (currentTrialData.mapName == "Practice")
                    {
                        displayMessage = "keepSearchingMessage";
                    }
                }

                // decrement the counter tracking the number of rewards remaining to be collected
                rewardsRemaining = rewardsRemaining - 1;
                Debug.Log("Rewards remaining: " + rewardsRemaining);

                //Debug.Log("re-enabling player controls now");
                StateNext(STATE_MOVING2);

                break;


            case STATE_MOVING2:

                if (!Player.GetComponent<PlayerController>().enabled)
                {
                    Player.GetComponent<PlayerController>().enabled = true; // let the player move again
                }

                if (movementTimer.ElapsedSeconds() > maxMovementTime)  // the trial should timeout
                {
                    StateNext(STATE_TIMEOUT);
                }

                if (boulderLifted)
                {
                    Debug.Log("Boulder lifted now.");
                    previousState = State;
                    movementTime = movementTimer.ElapsedSeconds();
                    StateNext(STATE_SHOWREWARD);  // go to blank screen
                }
            
                break;

            case STATE_STAR2FOUND:

                // This is the state when the FINAL reward to be collected is found (in the case of 2 or multiple rewards)
                displayTimeLeft = false;             // freeze the visible countdown

                if (stateTimer.ElapsedSeconds() > goalHitPauseTime)
                {
                    CongratulatePlayer();                // display a big congratulatory message
                }

                if (stateTimer.ElapsedSeconds() > beforeScoreUpdateTime)
                {
                    UpdateScore();  // update the total score and flash it on the screen
                }
                if (stateTimer.ElapsedSeconds() > finalGoalHitPauseTime) // we get an extra neural signal in this state
                {
                    flashTotalScore = false;
                    flashCongratulations = false;
                    StateNext(STATE_FINISH);
                }
                break;

            case STATE_FINISH:

                // stop recording the state transitions for this trial
                CancelInvoke("RecordFSMState");

                // end the trial, save the data
                NextScene();
                StateNext(STATE_SETUP);  // be careful with how the data /duration of this blankscreen state is saved - we want to know how long it was.
                break;

            case STATE_TIMEOUT:

                FLAG_trialTimeout = true;
                displayMessage = "timeoutMessage";
                Debug.Log("Trial timed out: (after " + movementTimer.ElapsedSeconds() + " sec)");

                StateNext(STATE_ERROR);
                break;


            case STATE_ERROR:
                // Handle error trials by continuing to record data on the same trial

                if (FLAG_trialError == false)
                {
                    source.PlayOneShot(errorSound, 1F);
                    //displayMessage = "restartTrialMessage";
                    UpdateScore();   // take -20 points off total score
                }
                FLAG_trialError = true;

                firstMovementTime = movementTimer.ElapsedSeconds();
                totalMovementTime = firstMovementTime;

                // Wait a little while in the error state to display the error message
                if (stateTimer.ElapsedSeconds() > errorDwellTime)
                {
                    flashTotalScore = false;

                    // stop recording the state transitions for this trial
                    CancelInvoke("RecordFSMState");

                    // Re-insert the trial further in the sequence for trying again later
                    // NextAttempt();   // Don't restart the trial immediately

                    // if we want to repeat this trial again late (for learning experiments)
                    //RepeatTrialAgainLater();

                    // If we're scanning neural data and dont want experiment to extend too long, dont bother repeating trial later
                    NextScene();  // Just move on to the next trial in the sequence. HRS to check this is saving properly
                    StateNext(STATE_SETUP);

                    // reset the error flags so the trial can correctly restart
                    FLAG_dataWritingError = false;
                    FLAG_fullScreenModeError = false;
                    FLAG_frameRateError = false;
                }
                break;


            case STATE_REST:

                elapsedRestbreakTime = restbreakTimer.ElapsedSeconds();

                if (elapsedRestbreakTime > restbreakDuration)
                {
                    NextScene();
                    StateNext(STATE_SETUP);   // move on to the next trial
                    break;
                }
                break;


            case STATE_GETREADY:

                getReadyTime = getReadyTimer.ElapsedSeconds();

                if (getReadyTime > getReadyDuration)
                {
                    NextScene();
                    StateNext(STATE_SETUP);   // move on to the next trial
                    break;
                }
                break;

            case STATE_PAUSE:
                // Note: this state is triggered by either:
                // - exiting fullscreen mode, and can only be escaped from by re-enabling fullscreen mode.
                // - having an insufficient gameplay framerate.
                // pause the countdown timer display and disable the player controls
                // Note that the Player and FSM will continue to track position and timestamp, so we know how long it was 'paused' for.
                pauseClock = true;
                if (Player != null)
                {
                    Player.GetComponent<PlayerController>().enabled = false;
                }
                else 
                {
                    Player = GameObject.Find("PlayerAvatar");
                    if (Player != null)
                    {
                        Player.GetComponent<PlayerController>().enabled = false;
                    }
                }
                break;

            case STATE_HALLFREEZE:
                Player.GetComponent<PlayerController>().enabled = false;
                darkTintScreen = true;
                currentFrozenTime = currentMovementTime - firstFrozenTime; // dont think this is being used for anything (HRS 14/05/2019)

                if ( (stateTimer.ElapsedSeconds() > preFreezeTime) && (stateTimer.ElapsedSeconds() <= hallwayFreezeTime[hallwayTraversed]) )
                {
                    displayMessage = "traversingHallway";
                }

                if (stateTimer.ElapsedSeconds() > hallwayFreezeTime[hallwayTraversed])
                {
                    darkTintScreen = false;
                    Player.GetComponent<PlayerController>().enabled = true;
                    displayMessage = "noMessage";
                    StateNext(previousState);
                }
                break;

            case STATE_DEBRIEF:
                // We just chill out in this state until the participant has responded with the dropdown menu, 
                // then SetQuestionnaireAnswer() is called, and on the submit button we move to next trial.

                break;

            case STATE_EXIT:
                // Display the total experiment time and wait for the participant to close the application

                // Note: at the moment this is just to save the correct exiting state transition in the datafile
                break;

        }
    }
    // ********************************************************************** //

    public void NextScene()
    {
        // Save the current trial data and move data storage to the next trial
        dataController.AddTrial();
        dataController.SaveData();
    }

    // ********************************************************************** //

    public void RepeatTrialAgainLater()
    {
        // Save the current trial data before the participant comes back to this trial later in the trial sequence
        dataController.ReinsertErrorTrial();
        dataController.SaveData();
    }

    // ********************************************************************** //

    public void NextAttempt()
    {
        // Save the current trial data before the participant tries the trial again
        dataController.AssembleTrialData();
        dataController.SaveData();
    }

    // ********************************************************************** //

    public string TrialSetup()
    {
        // Start the trial with a clean-slate
        blankScreen = false;
        darkTintScreen = false;
        FLAG_trialError = false;
        FLAG_trialTimeout = false;
        FLAG_fullScreenModeError = false;
        FLAG_dataWritingError = false;
        FLAG_frameRateError = false;
        FLAG_cliffFallError = false;
        starFound = false;
        boulderLifted = false;
        displayTimeLeft = false;
        scoreUpdated = false;
        congratulated = false;
        pauseClock = false;
        debriefResponseRecorded = false;
        trialScore = 0;
        debriefResponse = "";
        debriefResponseTime = 0f;
        showCanvasReward = false;
        controlStateIndex = 0;

        for (int i = 0; i < scaleUpReward.Length; i++)
        {
            scaleUpReward[i] = false;
        }

        // Load in the trial data
        currentTrialData = dataController.GetCurrentTrialData();
        nextScene = currentTrialData.mapName;

        // Location and orientation variables
        playerSpawnLocation = currentTrialData.playerSpawnLocation;
        playerSpawnOrientation = currentTrialData.playerSpawnOrientation;
        rewardSpawnLocations = currentTrialData.rewardPositions;
        doubleRewardTask = currentTrialData.doubleRewardTask;
        presentPositions = currentTrialData.presentPositions;
        freeForage = currentTrialData.freeForage;
        bridgeStates = currentTrialData.bridgeStates;
        questionData = currentTrialData.debriefQuestion;
        controlStateOrder = currentTrialData.controlStateOrder;
        computerAgentCorrect = currentTrialData.computerAgentCorrect;

        // Deal with the free-foraging multi-reward case, (HRS can make elegant later)
        rewardsRemaining = 1;           // default
        if (doubleRewardTask)
        {
            rewardsRemaining = freeForage ? 16 : 2;
        }
        for (int i = 0; i < rewardsVisible.Length; i++)
        {
            rewardsVisible[i] = false;
        }

        // Timer variables
        maxMovementTime = currentTrialData.maxMovementTime;
        preDisplayCueTime = currentTrialData.preDisplayCueTime;
        displayCueTime = currentTrialData.displayCueTime;
        goalHitPauseTime = currentTrialData.goalHitPauseTime;
        finalGoalHitPauseTime = currentTrialData.finalGoalHitPauseTime;
        goCueDelay = currentTrialData.goCueDelay;
        minDwellAtReward = currentTrialData.minDwellAtReward;
        displayMessageTime = currentTrialData.displayMessageTime;
        errorDwellTime = currentTrialData.errorDwellTime;
        rewardType = currentTrialData.rewardType;
        hallwayFreezeTime = currentTrialData.hallwayFreezeTime;
        preFreezeTime = currentTrialData.preFreezeTime;
        minTimeBetweenMoves = currentTrialData.minTimeBetweenMoves;
        oneSquareMoveTime = currentTrialData.oneSquareMoveTime;
        blankTime = currentTrialData.blankTime;
        animationTime = currentTrialData.animationTime;
        preRewardAppearTime = currentTrialData.preRewardAppearTime;

        // Start the next scene/trial
        Debug.Log("Upcoming scene: " + nextScene);
        SceneManager.LoadScene(nextScene);

        string[] menuScenesArray = new string[] { "Exit", "RestBreak", "GetReady", "QuestionTime" };

        if (menuScenesArray.Contains(nextScene))  // ***HRS (how is this working if contains is a List method?)
        {
            return nextScene;   // we don't want to record data and do the FSM transitions during the exit and rest break scenes
        }
        else
        {
            return "StartTrial";
        }

    }

    // ********************************************************************** //

    public void StartRecording()
    {
        // Make sure we're found the player and make sure they cant move (and start recording player and FSM data)
        if (Player != null)
        {
            if (Player.GetComponent<PlayerController>().enabled)
            {
                Player.GetComponent<PlayerController>().enabled = false;
            }
        }
        else
        {   // Track the state-transitions at the same update frequency as the PlayerAvatar (and putting it here should sync them too)
            Player = GameObject.Find("PlayerAvatar");
            stateTransitions.Clear();                      // restart the state tracker ready for the new trial
            stateTransitions.Add("Game State");

            RecordFSMState();                              // catch the current state before the update
            InvokeRepeating("RecordFSMState", 0f, dataRecordFrequency);

            // Track the giftbox states whenever a change to the giftbox states is made
            giftWrapStateTransitions.Clear();
            giftWrapStateTransitions.Add("Wrapping State");
            giftWrapState = Enumerable.Repeat(1, giftWrapState.Length).ToArray();
            RecordGiftStates();

            // Track the control states whenever a change in human/computer control in made
            controlStateTransitions.Clear();
            controlStateTransitions.Add("Control State");
            RecordControlStates();

        }
    }

    // ********************************************************************** //

    public void WaitForScannerStart() 
    {
        waitingForScannerStart = true;
    }

    // ********************************************************************** //

    public void StartExperiment()
    {
        NextScene();
        TrialSetup();           // start the experiment participant menu etc
    }

    // ********************************************************************** //

    public void StartGame()
    {
        Debug.Log("The game has started now and into the FSM!");
        experimentTimer.Reset();
        scannerTriggerTimes.Add(experimentTimer.ElapsedSeconds());  // should be 0.00f seconds
        NextScene();
        gameStarted = true;
        Cursor.visible = false;
    }

    // ********************************************************************** //

    public void ContinueToNextMenuScreen()
    {
        NextScene();
        TrialSetup();
    }

    // ********************************************************************** //

    public void ContinueToNextQuestion() 
    {
        StateNext(STATE_FINISH);
    }

    // ********************************************************************** //

    public void CheckFullScreen()
    {
        // Check if playing in fullscreen mode. If not, give warning until we're back in full screen.
        if (!Screen.fullScreen)
        {
            if (State != STATE_STARTSCREEN)
            {
                // if we're in the middle of the experiment, send them a warning and restart the trial
                FLAG_fullScreenModeError = true;
                displayMessage = "notFullScreenError";
                StateNext(STATE_PAUSE);
            }
        }
        else
        {
            if (FLAG_fullScreenModeError)  // they had exited fullscreen mode, but now its back to fullscreen :)
            {
                StateNext(STATE_ERROR);    // record that this error happened and restart the trial
            }
        }
    }

    // ********************************************************************** //

    public void CheckFramerate()
    {
        // Check if the game framerate is sufficiently high. If not, give warning telling them to open in a different browser with fewer plugins or quit.
        if (frameRateMonitor.Framerate < minFramerate)
        {
            if (State != STATE_STARTSCREEN) // If we don't include this, the start button doesnt work for some people. But if we check framerate from the start of the experiment the smoothing from starting at 0fps will mean every computer fails this test.
            {
                FLAG_frameRateError = true;

                // prioritise the writing error messages 
                if ((displayMessage != "dataWritingError") & (displayMessage != "notFullScreenError"))
                {
                    // if we're in the middle of the experiment, send them a warning and pause the experiment
                    displayMessage = "framerateError";
                    Debug.Log("ERROR: Frame rate of " + frameRateMonitor.Framerate.ToString() + " fps is too low for gameplay.");
                    StateNext(STATE_PAUSE);
                }
            }
        }
        else 
        {
            if (FLAG_frameRateError & (messageTimer.ElapsedSeconds() > displayMessageTime))  // they had a framerate error, but now its fixed :)
            {
                displayMessage = "restartTrialMessage";
                StateNext(STATE_ERROR);    // record that this error happened and restart the trial (trial will be repeated later)
            }
        }
    }

    // ********************************************************************** //

    public void CheckWritingProperly()
    {
        // Check if data is writing to file correctly.
        if (!dataController.writingDataProperly)
        {
            FLAG_dataWritingError = true;
            displayMessage = "dataWritingError";

            if (State != STATE_PAUSE) 
            { 
                Debug.Log("There was a data writing error. Trial will save and restart once connection is re-established.");
            }

            // Disable the player controls (can get missed if only triggered from STATE_PAUSE when a trial finishes?)
            StateNext(STATE_PAUSE);

            // Every little while, try another attempt at saving to see if the connection issue resolves (allows error message to be seen for sufficient length of time too)
            if (messageTimer.ElapsedSeconds() > displayMessageTime)
            {
                dataController.SaveData();
                messageTimer.Reset();
            }
        }
        else
        {
            if (FLAG_dataWritingError)  // they had a writing error, but now its fixed :)
            {
                // Writing connection is fixed! So restart the trial
                displayMessage = "restartTrialMessage";
                StateNext(STATE_ERROR);    // record that this error happened and restart the trial (trial will be repeated later)
            }
        }
    }

    // ********************************************************************** //
    // Note this is obsolete, because Application.Quit() does not work for web applications, only local ones.
    public void ExitGame()
    {
        Application.Quit();  // close the application
    }

    // ********************************************************************** //

    public void StateNext(int state)
    {
        // Transition the FSM to the next state
        if (State != state)
        {
            Debug.Log("STATE TRANSITION: " + stateText[State] + " -> " + stateText[state] + ": (" + stateTimer.ElapsedSeconds().ToString("F2") + " sec)");
            State = state;
            stateTimer.Reset();   // start counting how much time we're in this new state
        }
    }

    // ********************************************************************** //

    public void SwitchControlState() 
    {
        // allow our incremented control state index to wrap around the control order array
        controlStateIndex = (controlStateIndex+1 + controlStateOrder.Length) % controlStateOrder.Length;
        controlState = controlStateOrder[controlStateIndex];
        Debug.Log("Switching control state to: " + controlState);
    }

    // ********************************************************************** //

    private void RecordFSMState()
    {
        // add the current stateof the gameplay at this moment to the array
        stateTransitions.Add(State.ToString());
    }

    // ********************************************************************** //

    public void RecordGiftStates()
    {
        // add the current state of the presents to an array
        string giftWrapStateString = string.Format("{0:0.00}", Time.time);    // timestamp the state array with same timer as the positional tracking data
        for (int i = 0; i < giftWrapState.Length; i++)
        {
            giftWrapStateString = giftWrapStateString + " " + giftWrapState[i].ToString();
        }
        giftWrapStateTransitions.Add(giftWrapStateString);
    }

    // ********************************************************************** //

    public void RecordControlStates()
    {
        // add the current state of the human/computer control to a list
        string controlStateString = string.Format("{0:0.00}", Time.time);    // timestamp the state array with same timer as the positional tracking data
        controlStateString = controlStateString + " " + controlState;
        controlStateTransitions.Add(controlStateString);
    }

    // ********************************************************************** //

    private void UpdateText()
    {
        // This is used for displaying boring, white text messages to the player, such as warnings

        // Display regular game messages to the player
        switch (displayMessage)
        {
            case "noMessage":
                textMessage = "";
                messageTimer.Reset();
                break;

            case "wellDoneMessage":  // Note that this happens in an external script now
                //textMessage = "Well done!";
                textMessage = "Super gemacht!";
                if (messageTimer.ElapsedSeconds() > displayMessageTime)
                {
                    displayMessage = "noMessage"; // reset the message
                }
                break;

            case "timeoutMessage":
                //textMessage = "Trial timed out!";
                textMessage = "Zeit ist abgelaufen!";
                if (messageTimer.ElapsedSeconds() > displayMessageTime)
                {
                    displayMessage = "noMessage"; // reset the message
                }
                break;

            case "restartTrialMessage":
                //textMessage = "Restarting trial";
                textMessage = "Beginne den Durchgang neu";
                if (messageTimer.ElapsedSeconds() > displayMessageTime)
                {
                    displayMessage = "noMessage"; // reset the message
                }
                break;

            case "dataWritingError":
                //textMessage = "There was an error sending data to the web server. \n Please check your internet connection. \n If this message does not disappear, \nthen please return HIT and email hiplab@psy.ox.ac.uk";
                textMessage = "Ein Fehler is aufgetreten beim senden deiner Daten zum Web Server.\n Bitte überprüfe deine Internetverbindung.\n Wenn diese Nachricht nicht verschwindet, informiere bitte den Studienleiter.";
                break;

            case "framerateError":
                //textMessage = "The browser-dependent frame rate is insufficient for this HIT. \n Please exit, or try using Chrome/Firefox with no plugins. \n If this message does not disappear, \nthen please return HIT and email hiplab@psy.ox.ac.uk";
                textMessage = "Die browserabhängige Framerate ist unzureichend für diesen HIT. \n Bitte verlasse die Seite oder versuche Chrome/Firefox ohne plugins zu benutzen. \n Wenn diese Nachricht nicht verschwindet, \n informiere bitte den Gruppenleiter. ";
                break;

            case "notFullScreenError":
                //textMessage = "Please return the application to full-screen mode to continue.";
                textMessage = "Bitte kehre die Anwendung zum full-screen mode zurück, um fortzufahren. ";
                break;

            case "keepSearchingMessage":
                //textMessage = "Well done! \n There is one more pineapple to find.";
                textMessage = "Super gemacht! Es gibt noch eine weitere Martini-Glas zu finden. ";
                if (messageTimer.ElapsedSeconds() > displayMessageTime)
                {
                    displayMessage = "noMessage"; // reset the message
                }
                break;

            case "traversingHallway":
                //textMessage = "Crossing a bridge takes time. \n Continue in..."; // + ((int)Mathf.Round(hallwayFreezeTime-1f)).ToString() + " seconds";
                //textMessage = "Moving to the next room..."; // + ((int)Mathf.Round(hallwayFreezeTime-1f)).ToString() + " seconds";
                textMessage = "Begebe dich ins nächste Zimmer...";
                break;

            case "openBoxQuestion":
                textMessage = "Press space-bar to remove the boulder";
                break;

        }

    }

    // ********************************************************************** //

    public void PlayMovementSound()
    {
        source.PlayOneShot(movementSound, 1F);
    }

    // ********************************************************************** //

    public void AnimateRewardOnHit(int rewardIndex) 
    {
        scaleUpReward[rewardIndex] = true;
    }

    // ********************************************************************** //

    public void OpenBox()
    {
        source.PlayOneShot(openBoxSound, 1F);
    }

    // ********************************************************************** //

    public void OpenBoxQuestion(bool visible)
    {
        if (currentTrialData.mapName == "Practice")  // only provide this info on the practice trials
        {
            if (displayMessage == "noMessage")    // don't get rid of any other important messages e.g. the data writing error or restarting trial
            {
                if (visible)
                {
                    displayMessage = "openBoxQuestion";
                }
            }
            if (!visible)
            {
                displayMessage = "noMessage";
            }
        }
    }

    // ********************************************************************** //

    public void SetQuestionnaireAnswer(string response) 
    {
        // Record the chosen response selected from the dropdown meny for this debrief question
        if (response != "Select a room . . .") 
        {
            debriefResponseRecorded = true;
            debriefResponse = response;
            debriefResponseTime = debriefResponseTimer.ElapsedSeconds();
            Debug.Log("You responded with: " + debriefResponse);
            Debug.Log("It took you " + debriefResponseTime + " sec to respond");
        }
        else 
        {
            debriefResponseRecorded = false;     // prevent them from going back and selecting the default option
        }
    }

    // ********************************************************************** //

    public void HallwayFreeze(int hallway)
    {   // Display a message, and track the hallway traversal in the FSM and in the saved data
        previousState = State;
        firstFrozenTime = totalMovementTime;
        hallwayTraversed = hallway;
        StateNext(STATE_HALLFREEZE);
    }

    // ********************************************************************** //

    public string PlayerInWhichRoom(Vector2 playerPosition)
    {
        // Checks which of the four rooms (or a hallway) the player is currently in (having just taken a movement)
        string room = "hallway";

        if (playerPosition.x < 0f)           // yellow or blue
        {
            if (playerPosition.y < 0f)
            {
                room = "blue";
            }
            else if (playerPosition.y > 0f)
            {
                room = "yellow";
            }
        }
        else if (playerPosition.x > 0f)       // green or red
        {
            if (playerPosition.y < 0f)
            {
                room = "red";
            }
            else if (playerPosition.y > 0f)
            {
                room = "green";
            }
        }
        return room;
    }

    // ********************************************************************** //

    public void MoveCamera(Vector2 playerPosition)
    {
        playerRoom = PlayerInWhichRoom(playerPosition);  // this will be read from CameraController.cs attached to the main camera
    }

    // ********************************************************************** //

    public void UpdateScore()
    {   // Note that these bools are read in the script TotalScoreUpdateScript.cs
        if (currentTrialData.mapName != "Practice") // Don't count the practice trials
        {
            if (!scoreUpdated)  // just do this once per trial
            {
                displayTimeLeft = false;   // freeze the visible countdown

                if (State == STATE_ERROR)  // take off 20 points for a mistrial
                {
                    if (!((FLAG_dataWritingError || FLAG_fullScreenModeError) || (FLAG_frameRateError)))  // don't penalize internet connection or writing errors or framerate errors
                    { 
                        trialScore = -20;
                    }
                }
                else                       // increase the total score
                {
                    trialScore = (int)Mathf.Round(maxMovementTime - totalMovementTime);
                }
                totalScore += trialScore;
                scoreUpdated = true;
                flashTotalScore = true;
            }
        }
    }

    // ********************************************************************** //

    public void CongratulatePlayer()
    {   // Note that we are doing this separately from textMessage because we want it to be a bigger, more dramatic font
        if (!congratulated) // just run this once per trial
        {
            congratulated = true;
            flashCongratulations = true;
        }
    }

    // ********************************************************************** //

    public void FallDeath()
    {   // Disable the player controller, give an error message, save the data and restart the trial
        Debug.Log("AAAAAAAAAAAAH! Player fell off the platform.");

        FLAG_cliffFallError = true;
        StateNext(STATE_ERROR);
    }

    // ********************************************************************** //

    public void StarFound()
    {
        starFound = true; // The player has been at the star for minDwellAtReward seconds
    }

    // ********************************************************************** //

    public void LiftingBoulder() 
    {
        boulderLifted = true; 
    }

    // ********************************************************************** //

    public void DisableRewardByIndex(int index)
    {
        // Disable whichever of the rewards was just hit. Called from RewardHitScript.cs
        rewardsVisible[index] = false;
    }

    // ********************************************************************** //

    public string GetCurrentMapName()
    {
        // Return the name of the currently active scene/map (for saving as part of TrialData)
        return SceneManager.GetActiveScene().name;
    }

    // ********************************************************************** //

    public int GetCurrentMapIndex()
    {
        // Return the index of the currently active scene/map (for saving as part of TrialData)
        return SceneManager.GetActiveScene().buildIndex;
    }

    // ********************************************************************** //


}