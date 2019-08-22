using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    /// Source: Unity tutorial https://www.youtube.com/watch?v=AKKpPmxx07w
    /// Date: 22/08/2019
    /// For our A* computer pathfinding
    /// Edited by HRS into plain class

    Grid grid;
    List<Node> bestPath = new List<Node>();

    // ********************************************************************** //

    public Pathfinder(Grid a_grid)
    {
        grid = a_grid; 
    }

    // ********************************************************************** //

    public List<Node> FindPath(Vector3 a_startPos, Vector3 a_targetPos) 
    {
        Node startNode = grid.NodeFromWorldPosition(a_startPos);
        Node targetNode = grid.NodeFromWorldPosition(a_targetPos);

        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        OpenList.Add(startNode);

        // implement A* pathfinding
        while(OpenList.Count > 0) 
        {
            Node currentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)  // start from second node
            { 
                if(OpenList[i].FCost < currentNode.FCost || OpenList[i].FCost == currentNode.FCost && OpenList[i].hCost < currentNode.hCost) 
                {
                    currentNode = OpenList[i];
                }
            }
            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);

            if (currentNode == targetNode) 
            {
                GetFinalPath(startNode, targetNode);
                break;
            }

            foreach (Node neighbourNode in grid.GetNeighbouringNodes(currentNode)) 
            { 
                // ignore this node if we cant move there or have already visited
                if (!neighbourNode.IsWall || ClosedList.Contains(neighbourNode)) 
                {
                    continue;
                }

                int moveCost = currentNode.gCost + GetManhattenDistance(currentNode, neighbourNode);
           
                // move to the neighbour node if its better
                if (moveCost < neighbourNode.FCost || !OpenList.Contains(neighbourNode)) 
                {
                    neighbourNode.gCost = moveCost;  // set g-cost to f-cost
                    neighbourNode.hCost = GetManhattenDistance(neighbourNode, targetNode);
                    neighbourNode.Parent = currentNode;
                }

                if (!OpenList.Contains(neighbourNode)) 
                {
                    OpenList.Add(neighbourNode);
                }
            }
        }
        return bestPath;
    }
    // ********************************************************************** //

    void GetFinalPath(Node a_startingNode, Node a_endNode) 
    {
        List<Node> finalPath = new List<Node>();
        Node currentNode = a_endNode;

        // we will loop from target backwards to start node
        while(currentNode != a_startingNode) 
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        finalPath.Reverse();
        grid.finalPath = finalPath;
        bestPath = finalPath;
    }

    // ********************************************************************** //

    int GetManhattenDistance(Node a_nodaA, Node a_nodeB) 
    {
        int ix = Mathf.Abs(a_nodaA.gridX - a_nodeB.gridX);
        int iy = Mathf.Abs(a_nodaA.gridY - a_nodeB.gridY);

        return ix + iy;
    }

    // ********************************************************************** //

}
