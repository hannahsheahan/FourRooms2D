﻿using System.Collections;
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

    // ********************************************************************** //

    void Start()
    {
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
            cameraZoom = 2.5f;
            switch (currentPlayerRoom)
            {
                case "blue":
                    cameraPosition = new Vector3(-2.5f, -2.5f, zPosition);
                    Debug.Log("Camera positioned on the blue room.");
                    break;

                case "yellow":
                    cameraPosition = new Vector3(-2.5f, 2.5f, zPosition);
                    Debug.Log("Camera positioned on the yellow room.");
                    break;

                case "red":
                    cameraPosition = new Vector3(2.5f, -2.5f, zPosition);
                    Debug.Log("Camera positioned on the red room.");
                    break;

                case "green":
                    cameraPosition = new Vector3(2.5f, 2.5f, zPosition);
                    Debug.Log("Camera positioned on the green room.");
                    break;

                case "hallway":
                    cameraPosition = new Vector3(0f, 0f, zPosition); // for debugging!
                    cameraZoom = 5f;
                    Debug.Log("Camera positioned on a hallway.");
                    break;

                default:
                    cameraPosition = new Vector3(0f, 0f, zPosition); 
                    cameraZoom = 5f;
                    Debug.Log("Default: Camera positioned in centre of environment.");
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
    }

    // ********************************************************************** //

}