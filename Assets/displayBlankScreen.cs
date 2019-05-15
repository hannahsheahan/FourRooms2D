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
            blankImage.enabled = true;
        }
        else
        {
            blankImage.enabled = false;
        }
    }

    // ********************************************************************** //

}
