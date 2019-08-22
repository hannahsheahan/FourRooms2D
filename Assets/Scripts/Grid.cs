using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    /// Source: Unity tutorial https://www.youtube.com/watch?v=AKKpPmxx07w
    /// Date: 22/08/2019
    /// For our A* computer pathfinding
   
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
                bool Wall = true;

                // check if this is a walkable node (if there is a collision this statement returns false)
                if (Physics.CheckSphere(worldPoint, nodeRadius, wallMask)) 
                {
                    Wall = false;
                }

                grid[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }
    }

    // ********************************************************************** //

    public Node NodeFromWorldPosition(Vector3 a_worldPosition) 
    { 
        float xpoint = ((a_worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float ypoint = ((a_worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y);

        xpoint = Mathf.Clamp01(xpoint);
        ypoint = Mathf.Clamp01(ypoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xpoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * ypoint);

        return grid[x, y];
    }

    // ********************************************************************** //

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null) 
        { 
            foreach(Node node in grid) 
            { 
                if (node.IsWall) // if its not a wall make it white
                {
                    Gizmos.color = Color.white;
                }
                else 
                {
                    Gizmos.color = Color.yellow; // walls are yellow
                }

                if (finalPath != null)
                {
                    if (finalPath.Contains(node))
                    {
                        Gizmos.color = Color.red;    // colour our final path red
                    }
                }

                Gizmos.DrawCube(node.Position, Vector3.one * (nodeDiameter - distance));
            }
        }
    }

    // ********************************************************************** //

    public List<Node> GetNeighbouringNodes(Node a_Node) 
    {
        List<Node> neighbouringNodes = new List<Node>();
        int xCheck;
        int yCheck;

        // right side
        xCheck = a_Node.gridX + 1;
        yCheck = a_Node.gridY;

        if (xCheck >= 0 && xCheck < gridSizeX) 
        { 
            if (yCheck >= 0 && yCheck < gridSizeY) 
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        // left side
        xCheck = a_Node.gridX - 1;
        yCheck = a_Node.gridY;

        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        // top side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY + 1;

        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        // bottom side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY - 1;

        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        return neighbouringNodes;
    }

    // ********************************************************************** //

}
