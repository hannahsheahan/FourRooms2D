using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresentRevealScript : MonoBehaviour {
    /// <summary>
    /// Author: Hannah Sheahan, sheahan.hannah@gmail.com
    /// 
    /// Notes:  this method works a little differently to the reward display script 
    /// HideOrDisplayReward.cs, simply because presents will be spawned active from
    /// the start of every scene and only disabled, whereas rewards need to be turned on AND off.
    /// If the presents needed to be turned on, then this script could not be attached
    /// directly to the present object because it would never be activated/ran 
    /// (hence for the rewards, we use an empty parent gameobject to run HideOrDisplayReward.cs)
    /// 
    /// Edit: updated for 2D environment by HRS (14/05/2019)
    /// </summary>

    public GameObject present;
    public int presentIndex;

    // ********************************************************************** //

    void Start()
    {
        // Load the present spawn location from the configured datafile
        transform.position = GameController.control.presentPositions[presentIndex];
    }

    // ********************************************************************** //

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            GameController.control.OpenBoxQuestion(true);
        }
    }

    // ********************************************************************** //

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameController.control.OpenBox();
                GameController.control.OpenBoxQuestion(false);
                present.SetActive(false);
                GameController.control.giftWrapState[presentIndex] = 0; // effectively a bool, but shorter to write as string to file 
                GameController.control.RecordGiftStates();              // save a timestamp and the gift states
            }
        }
    }

    // ********************************************************************** //

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            GameController.control.OpenBoxQuestion(false);
        }
    }
}
