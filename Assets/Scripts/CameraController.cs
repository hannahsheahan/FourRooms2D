using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Control camera position and zoom as the agent moves around the maze
// Author: Hannah Sheahan, sheahan.hannah@gmail.com
// Date: 16/05/2019
// Issues: N/A
// Notes: N/A

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 cameraPosition;
    private float cameraZoom;
    private float zPosition;
    private string currentPlayerRoom = "update";    // Note: set this different to previousPlayerRoom so we will update camera position on spawn
    private string previousPlayerRoom = "";
    private float targetaspect;

    // ********************************************************************** //

    void Start()
    {
        targetaspect = 9.0f / 9.0f;  // set the desired aspect ratio (we want a square camera viewport and to pillarbox otherwise)

        mainCamera = GetComponent<Camera>();
        zPosition = -10f;
        cameraPosition = new Vector3(0f, 0f, zPosition);
        cameraZoom = 5f;

        SetCamera();
      
    }


    // ********************************************************************** //

    void Update()
    {
        // Only update the camera position etc if we have to
        currentPlayerRoom = GameController.control.playerRoom;

        if (currentPlayerRoom != previousPlayerRoom)
        {
            Debug.Log("Transitioning from: " + previousPlayerRoom + "->" + currentPlayerRoom);
            cameraZoom = 3f;
            switch (currentPlayerRoom)
            {
                case "blue":
                    cameraPosition = new Vector3(-2.5f, -2.5f, zPosition);
                    break;

                case "yellow":
                    cameraPosition = new Vector3(-2.5f, 2.5f, zPosition);
                    break;

                case "red":
                    cameraPosition = new Vector3(2.5f, -2.5f, zPosition);
                    break;

                case "green":
                    cameraPosition = new Vector3(2.5f, 2.5f, zPosition);
                    break;

                case "hallway":
                    //cameraPosition = new Vector3(0f, 0f, zPosition); // don't update the camera position for hallways
                    //cameraZoom = 5f;
                    break;

                default:
                    cameraPosition = new Vector3(0f, 0f, zPosition); 
                    cameraZoom = 5f;
                    break;
            }
            // update the camera position and zoom accordingly
            SetCamera();
        }

        previousPlayerRoom = currentPlayerRoom;
    }

    // ********************************************************************** //

    private void SetCamera() 
    {
        transform.position = cameraPosition;
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = cameraZoom;

        PillarBoxCamera();  // rescale the viewing window to crop appropriately for different screen sizes
    
    }

    // ********************************************************************** //

    private void PillarBoxCamera() 
    {

        // This function will resize the display of the game to a specific aspect ratio, regardless of what the screen resolution/ratio is.
        // This way we get consistent play across different playforms/monitors etc by pillarboxing/letterboxing the viewing window.
        // downloaded on 20/05/2019 from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html , downloaded by Hannah Sheahan, sheahan.hannah@gmail.com

        float windowaspect = (float)Screen.width / (float)Screen.height;   // determine the game window's current aspect ratio
        float scaleheight = windowaspect / targetaspect;  // current viewport height should be scaled by this amount

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = mainCamera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            mainCamera.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;
            Rect rect = mainCamera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            mainCamera.rect = rect;
        }
    }

    // ********************************************************************** //

}
