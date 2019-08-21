using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlStateMessageScript : MonoBehaviour
{
    public Text screenMessage;
    private string controlStateMessage;

    // ********************************************************************** //

    void Update()
    {
        // update the text gameobject's message in-line with the FSM
        screenMessage.fontSize = 30;
        screenMessage.text = controlStateMessage;

        if (GameController.control.State >= GameController.STATE_GO) 
        { 
            switch (GameController.control.controlState)
            {
                case GameController.CONTROL_HUMAN:
                    controlStateMessage = "[Your turn]";
                    screenMessage.color = Color.green;
                    break;
                case GameController.CONTROL_COMPUTER:
                    controlStateMessage = "[Computer turn]";
                    screenMessage.color = Color.red;
                    break;
                default:
                    controlStateMessage = "";
                    break;
            }
        }
    }
    // ********************************************************************** //

}
