using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    /// Source: Unity tutorial https://www.youtube.com/watch?v=AKKpPmxx07w
    /// Date: 22/08/2019
    /// For our A* computer pathfinding
   
    public Transform startPosition;
    public LayerMask wallMask;       // for finding obstructions on map
    public Vector2 gridWorldSize;
    public float nodeRadius;         // node size
    public float distance;           // distance between nodes


    Node[,] grid;
    public List<Node> finalPath;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    // ********************************************************************** //

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();

    }

    // ********************************************************************** //

    void CreateGrid() 
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;
    
        for (int x = 0; x < gridSizeX; x++)
        { 
            for (int y = 0; y < gridSizeY; y++) 
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool Wall = false;

                // check if this is a walkable node
                if (Physics.CheckSphere(worldPoint, nodeRadius, wallMask)) 
                {
                    Wall = true;
                    Debug.Log("something should be turning yellow");
                }

                grid[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }
    }

    // ********************************************************************** //

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null) 
        { 
            foreach(Node node in grid) 
            { 
                if (!node.IsWall) // if its not a wall make it white
                {
                    Gizmos.color = Color.white;
                }
                else 
                {
                    Gizmos.color = Color.yellow; // walls are yellow
                }

                if (finalPath != null) 
                {
                    Gizmos.color = Color.red;    // colour our final path red
                }

                Gizmos.DrawCube(node.Position, Vector3.one * (nodeDiameter - distance));
            }
        }
    }
    // ********************************************************************** //


}
