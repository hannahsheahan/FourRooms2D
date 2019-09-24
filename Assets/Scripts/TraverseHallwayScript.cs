using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Updated for 2D version by HRS (14/05/2019)
public class TraverseHallwayScript : MonoBehaviour {

    //private bool doorlocked = true;
    public int hallway;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entering collider");
        // if (doorlocked)  // 'unlock' the door
        // {
        // run GameController function that freezes people for X seconds (can add this as a state later to make analysis easier later).

        //GameController.control.HallwayFreeze(hallway);
        Debug.Log("Traversing hallway, player is frozen in place.");
       // }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // if (doorlocked)  // 'unlock' the door
        // {
        // run GameController function that freezes people for X seconds (can add this as a state later to make analysis easier later).
        //GameController.control.HallwayFreeze();
        Debug.Log("Exiting collider.");
        // }
    }

    // Every time you traverse a hallway it takes time
    /*
    private void OnTriggerExit2D(Collider2D other)
    {
        doorlocked = false;  // You've opened this door once so can go back through whenever you want
    }
    */
}
