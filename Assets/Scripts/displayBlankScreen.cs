using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class displayBlankScreen : MonoBehaviour
{
    public Image blankImage;

    // ********************************************************************** //

    void Start()
    {
        //Fetch the Image from the GameObject
        blankImage = GetComponent<Image>();
        blankImage.color = Color.black;
        blankImage.enabled = false;
    }

    // ********************************************************************** //

    void Update()
    {
        if (GameController.control.blankScreen)
        {
            blankImage.color = Color.black;
            blankImage.enabled = true;
        }
        else if (GameController.control.darkTintScreen)
        {
            blankImage.color = new Color(0f,0f,0f,0.65f);
            blankImage.enabled = true;
        }
        else
        {
            blankImage.enabled = false;
        }
    }

    // ********************************************************************** //

}
