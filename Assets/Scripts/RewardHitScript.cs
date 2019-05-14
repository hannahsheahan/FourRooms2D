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
    private Timer starTimer;
    private bool starHit = false;
    public int rewardIndex;

    // ********************************************************************** //

    void Start()
    {
        starTimer = new Timer();
        starTimer.Reset();
    }

    // ********************************************************************** //

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something just hit me! I'm a reward.");
        if (other.tag == "Player") 
        {
            Debug.Log("The player just hit me! I'm a reward.");

            starTimer.Reset(); // record entry time
            starHit = true;
        }
    }

    // ********************************************************************** //

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            starHit = false;
        }
    }

    // ********************************************************************** //

    void Update()
    {
        if ((starTimer.ElapsedSeconds() > GameController.control.minDwellAtReward) && (starHit))
        {
            GameController.control.StarFound();
            starHit = false;
            GameController.control.DisableRewardByIndex(rewardIndex);
        }
    }
    // ********************************************************************** //

}
