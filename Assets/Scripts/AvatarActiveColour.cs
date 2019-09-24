using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarActiveColour : MonoBehaviour
{
    private SpriteRenderer avatarRenderer;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        avatarRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        /*   // super cute, but probably not what we want for science comparisons of state? but useful if we have no sound
        if (playerController.enabled) 
        {
            avatarRenderer.color = Color.white;
        }
        else 
        {
            avatarRenderer.color = new Color(1f, 1f, 1f, 0.5f);  // make our little dude go transparent if not active!
        }
        */


        // instead just make him look deactivated when crossing bridges
        if (GameController.control.displayMessage == "traversingHallway")
        {
            //avatarRenderer.color = new Color(1f,1f, 1f, .6f);  // make our little dude go transparent and 'inactive' looking on the bridge
            //avatarRenderer.color = new Color(.2f,0.2f, 0.2f, 1f);  // make our little dude go transparent and 'inactive' looking on the bridge

        }
        else
        {
            avatarRenderer.color = Color.white;
        }


    }
}
