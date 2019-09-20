using System;
using UnityEngine;
using UnityEngine.UI;

public class TrialCountdownScript : MonoBehaviour {
   
    public Text CountdownTime;
    public Text FrozenCountdownTime;
    private float timeLeft;
    private int secondsLeft;
    private int frozenSecondsLeft;
    private int subtractTime;
    private float actualSecondsLeft;
    private float actualSubtractTime;

    // ********************************************************************** //

    void Update () {

        timeLeft = GameController.control.maxMovementTime - GameController.control.currentMovementTime;
        secondsLeft = (int)Mathf.Round(timeLeft);
        if (GameController.control.displayTimeLeft) 
        {
            if (secondsLeft >= 0)
            {
                CountdownTime.text = (secondsLeft).ToString();
            }
        }

        // display the frozen countdown
        //if (GameController.control.State == GameController.STATE_HALLFREEZE)
        if (GameController.control.displayMessage == "traversingHallway")
        {
            frozenSecondsLeft = secondsLeft - subtractTime;
            actualSecondsLeft = timeLeft - actualSubtractTime;

            // Debug.Log("sec left: " + frozenSecondsLeft);
            // make sure it freezes for AT LEAST hallwayFreezeTime
            if ((actualSecondsLeft >= 0f) && (actualSecondsLeft <= GameController.control.hallwayFreezeTime[GameController.control.hallwaysTraversed] -1))
            {
               // FrozenCountdownTime.text = (frozenSecondsLeft).ToString();
               // Debug.Log("printed sec left: " + frozenSecondsLeft);
            }
            else
            {
               // FrozenCountdownTime.text = ((int)Mathf.Round(GameController.control.hallwayFreezeTime[GameController.control.hallwaysTraversed])).ToString();
               // Debug.Log("printed sec left: " + frozenSecondsLeft);
            }
        }
        else
        {
            FrozenCountdownTime.text = "";

            try 
            { 
                subtractTime = (int)Mathf.Round(timeLeft - GameController.control.hallwayFreezeTime[GameController.control.hallwaysTraversed]);
                actualSubtractTime = (timeLeft - GameController.control.hallwayFreezeTime[GameController.control.hallwaysTraversed]);
            }
            catch (NullReferenceException e)
            {
                // don't worry 'bout it
                Debug.Log("Null reference exception in TrialCountdownScript. Not a big deal.");
            }
        }

    }
    // ********************************************************************** //

}