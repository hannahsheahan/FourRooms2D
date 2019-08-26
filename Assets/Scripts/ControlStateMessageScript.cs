using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlStateMessageScript : MonoBehaviour
{
    public Text screenMessage;
    private string controlStateMessage;
    private string controlState;

    // ********************************************************************** //

    void Update()
    {
        // update the text gameobject's message in-line with the FSM
        screenMessage.fontSize = 30;
        screenMessage.text = controlStateMessage;
        controlState = GameController.control.controlState;

        if( (GameController.control.State >= GameController.STATE_GO) && (GameController.control.State < GameController.STATE_STAR2FOUND) )
        {
            if ((GameController.control.State == GameController.STATE_SHOWREWARD))
            {
                {
                    controlStateMessage = "";
                }
            }
            else 
            { 
                switch (controlState)
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

    }

    // ********************************************************************** //

}
