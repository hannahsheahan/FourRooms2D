using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    /// Source: Unity tutorial https://www.youtube.com/watch?v=AKKpPmxx07w
    /// Date: 22/08/2019
    /// For our A* computer pathfinding

    // grid variables
    public int gridX;  // x position in node array
    public int gridY;
    public bool IsWall;      // if the node is obstructed by a wall
    public Vector3 Position;

    // A* variables
    public Node Parent;      // for A*, store what node you just moved from
    public int gCost;
    public int hCost;
    public int FCost { get { return gCost + hCost; } }

    // ********************************************************************** //

    public Node(bool a_IsWall, Vector3 a_pos, int a_gridX, int a_gridY) 
    {
        IsWall = a_IsWall;
        Position = a_pos;
        gridX = a_gridX;
        gridY = a_gridY;
    }
    // ********************************************************************** //

}
