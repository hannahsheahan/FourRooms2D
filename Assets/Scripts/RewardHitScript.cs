using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

// Updated for 2D collision detection by HRS (14/05/2019)

public class RewardHitScript : MonoBehaviour
{
    private Timer rewardTimer;
    private bool rewardHit = false;
    private bool rewardUncovered = false;
    public int rewardIndex;
    private Vector3 rewardPosition;
    private Vector3[] presentPositions;
    private int presentCoveringIndex = 0;

    // ********************************************************************** //

    void Start()
    {
        rewardTimer = new Timer();
        rewardTimer.Reset();
        rewardPosition = GameController.control.rewardSpawnLocations[rewardIndex];
        presentPositions = GameController.control.presentPositions;

        for (int i=0; i < presentPositions.Length; i++) 
        { 
            if (presentPositions[i] == rewardPosition) 
            {
                presentCoveringIndex = i;
            }
        }
        Debug.Log("the present covering reward " + rewardIndex + "is present no. " + presentCoveringIndex);
    }

    // ********************************************************************** //

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player") 
        {   
            rewardTimer.Reset(); // record entry time
            rewardHit = true;
        }
    }

    // ********************************************************************** //

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            rewardHit = false;
        }
        else 
        {
            Debug.Log("boulder removal detected!");
        }
    }

    // ********************************************************************** //

    void Update()
    {
        rewardUncovered = (GameController.control.giftWrapState[presentCoveringIndex] == 0) ? true : false;  // has the covering boulder been removed?

        if ((rewardTimer.ElapsedSeconds() > GameController.control.minDwellAtReward) && (rewardHit && rewardUncovered))
        {
            GameController.control.StarFound();
            rewardHit = false;
            GameController.control.DisableRewardByIndex(rewardIndex);
        }
    }
    // ********************************************************************** //

}
