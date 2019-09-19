//Attach this script to an Image GameObject and set its Source Image to the Sprite you would like.
//Press the space key to change the Sprite. Remember to assign a second Sprite in this script's section of the Inspector.

using UnityEngine;
using UnityEngine.UI;

public class DisplayRewardCue : MonoBehaviour
{
    public Image rewardImage;
    //Set this in the Inspector
    public Sprite wineImage;
    public Sprite cheeseImage;
    public Sprite martiniImage;
    public Sprite bananaImage;
    public Sprite watermelonImage;
    public Sprite peanutImage;
    public Sprite pineappleImage;
    public Sprite cherryImage;
    public Sprite mushroomImage;
    private string cue;
    private Vector3 originalRewardScale;

    // ********************************************************************** //

    void Start()
    {
        //Fetch the Image from the GameObject
        rewardImage = GetComponent<Image>();
        rewardImage.enabled = false;
        originalRewardScale = transform.localScale;
    }

    // ********************************************************************** //

    void Update()
    {
        if ( GameController.control.displayCue  || GameController.control.showCanvasReward )
        {
            cue = GameController.control.rewardType;
            switch (cue)
            {
                case "wine":
                    rewardImage.sprite = wineImage;
                    break;
                case "cheese":
                    rewardImage.sprite = cheeseImage;
                    break;
                case "martini":
                    rewardImage.sprite = martiniImage;
                    break;
                case "banana":
                    rewardImage.sprite = bananaImage;
                    break;
                case "watermelon":
                    rewardImage.sprite = watermelonImage;
                    break;
                case "peanut":
                    rewardImage.sprite = peanutImage;
                    break;
                case "pineapple":
                    rewardImage.sprite = pineappleImage;
                    break;
                case "cherries":
                    rewardImage.sprite = cherryImage;
                    break;
                case "mushroom":
                    rewardImage.sprite = mushroomImage;
                    break;

            }

            if (GameController.control.showCanvasReward) 
            {
                transform.localScale = 1.3f * originalRewardScale;
            }
            else 
            {
                transform.localScale = originalRewardScale;
            }



            rewardImage.enabled = true;
        }
        else
        {
            rewardImage.enabled = false;
        }
    }

    // ********************************************************************** //

            
           
        

}