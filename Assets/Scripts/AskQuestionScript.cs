using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AskQuestionScript : MonoBehaviour {

    /// <summary>
    /// This script determines which question to ask the player on this trial.
    /// This is a simplified, general purpose codebase for creating different behavioural experiments in Unity.
    /// Author: Hannah Sheahan, sheahan.hannah@gmail.com
    /// Date: December 2018
    /// </summary>

    public Text QuestionText;
    private QuestionData debriefQuestion;

    // ********************************************************************** //

    void Update()
    {
        // Note: can modulate when this is visible using a GameController.cs controlled flag if you want

        // ***HRS will have to put a flag or something in here that is triggered in game controller to determine whether to actually do the question asking stuff
        if (GameController.control.State >= GameController.STATE_STARTTRIAL)  // display the Q the whole trial
        //if (GameController.control.displayCue)
        {
            if ((GameController.control.State != GameController.STATE_ERROR) && (GameController.control.State != GameController.STATE_PAUSE))
            {
                debriefQuestion = GameController.control.questionData;
                QuestionText.text = debriefQuestion.questionText;
            }
            else
            {
                QuestionText.text = "";
            }
        }
    }
    // ********************************************************************** //
}
