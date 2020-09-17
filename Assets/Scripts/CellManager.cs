using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellManager : MonoBehaviour
{
    public GameObject debug;
    private List<Cell> currentCells = new List<Cell>();
    private Cell[] allMazeCells;
    private List<GameObject> pathPoints = new List<GameObject>();
    public bool showPath = true;

    public void InitialiseCells(Cell[] allCells)
    {
        allMazeCells = allCells; // initialsing the maze cells
        currentCells.Clear();
        foreach (Cell cell in allMazeCells)
            currentCells.Add(cell);
    }

    public void ShowCurrentPath(bool show) //function to show the path to the player
    {
        foreach (GameObject g in pathPoints)
            g.SetActive(show);
    }

    public Cell[] AllCells()
    {
        return allMazeCells; //returning all maze cells
    }

    public Vector3 GetMazeCentre(Vector3 yPos, int x, int z) //returning the centre of the maze for the camera position
    {
        Vector3 centre = new Vector3();
        float y = yPos.y;
        if (x > z)
            y += x - 1f;
        else
            y += z - 1f;

        if (x % 2 != 0 && z % 2 != 0)
        {
            centre = allMazeCells[(allMazeCells.Length - 1) / 2].point;
        }
        else if (x % 2 == 0 && z % 2 == 0)
        {
            centre = allMazeCells[((allMazeCells.Length - 1) / 2) - (x / 2)].point + new Vector3(.5f, 0, .5f);
        }
        else if (z % 2 != 0 && x % 2 == 0)
        {
            centre = allMazeCells[(allMazeCells.Length - 1) / 2].point + new Vector3(.5f, 0, 0);
        }
        else if (z % 2 == 0 && x % 2 != 0)
        {
            centre = allMazeCells[(allMazeCells.Length / 2) - 1 - (x / 2)].point + new Vector3(0f, 0, .5f);
        }
        return new Vector3(centre.x, y, centre.z);
    }

    public void GetPath(Cell finish, Vector3 holePos, List<GameObject> walls, List<Vector3> path, int xVal) //(Lague, 2014)
    {
        Cell start = GetFurthestCell(holePos);//player pos     
        List<Cell> openList = new List<Cell>();
        HashSet<Cell> closedList = new HashSet<Cell>();
        openList.Add(start);
        path.Clear();

        while (openList.Count > 0) // a* pathfinding algorithm in use to find the path to the end of the maze
        {
            Cell current = openList[0];
            for (int i = 0; i < openList.Count; i++)
            {
                if (openList[i].FCost < current.FCost || openList[i].FCost == current.FCost && openList[i].hCost < current.hCost)
                {
                    current = openList[i];
                }
            }
            openList.Remove(current);
            closedList.Add(current);

            if (current == finish)
            {
                RetracePath(start, finish, path);
                return;
            }

            foreach (Cell neighbour in GetNeighbours(current, walls, xVal))
            {
                if (closedList.Contains(neighbour))
                {
                    continue;
                }

                float movementCost = current.gCost + GetDistance(current, neighbour);
                if (movementCost < neighbour.gCost || !openList.Contains(neighbour))
                {
                    neighbour.gCost = movementCost;
                    neighbour.hCost = GetDistance(neighbour, finish);
                    neighbour.parent = current;

                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Cell start, Cell end, List<Vector3> path) //retracing the path back to the start
    {
        Cell current = end;
        GameObject g = Instantiate(debug, start.point, Quaternion.identity);
        while (current != start)
        {
            path.Add(current.point);
            current = current.parent;
        }
        for (int i = 0; i < path.Count; i++)
        {
            GameObject line = Instantiate(debug, path[i], Quaternion.identity);
            pathPoints.Add(line); //for the show path mechanic, will show the route to finish the maze
        }
        pathPoints.Add(g);
        for (int i = 1; i < pathPoints.Count; i++)
        {
            pathPoints[i].transform.LookAt(pathPoints[i - 1].transform); //connecting the lines
        }
        for (int i = 0; i < pathPoints.Count; i++)
        {
            pathPoints[i].transform.position += pathPoints[i].transform.forward / 2; //connecting the lines
        }
        Destroy(pathPoints[0]); //removing the object over the hole
        pathPoints.Remove(pathPoints[0]);
        ShowCurrentPath(false);
        path.Remove(path[0]);
        path.Reverse();
    }

    public void ClearPaths()
    {
        foreach (GameObject g in pathPoints)
            Destroy(g);
        pathPoints.Clear();
    }

    float GetDistance(Cell cellA, Cell cellB) //retriving the distance between two cells
    {
        float distX = Mathf.Abs(cellA.point.x - cellB.point.x);
        float distZ = Mathf.Abs(cellA.point.z - cellB.point.z);

        if (distX > distZ)
        {
            return 14 * distZ + 10 * (distX - distZ);
        }
        return 14 * distX + 10 * (distZ - distX);
    }

    List<Cell> GetNeighbours(Cell current, List<GameObject> currentWalls, int x)
    {
        List<Cell> possibelNeighbours = new List<Cell>();
        int index = Array.IndexOf(allMazeCells, current);
        if (!currentWalls.Contains(current.north))
        {
            possibelNeighbours.Add(allMazeCells[index + x]); //up on the maze
        }
        if (!currentWalls.Contains(current.south))
        {
            possibelNeighbours.Add(allMazeCells[index - x]); //down on the maze
        }
        if (!currentWalls.Contains(current.east))
        {
            possibelNeighbours.Add(allMazeCells[index + 1]); //right on the maze
        }
        if (!currentWalls.Contains(current.west))
        {
            possibelNeighbours.Add(allMazeCells[index - 1]); //left on the maze
        }
        return possibelNeighbours;
    }

    public Cell GetFurthestCell(Vector3 from) //fuction to find the cell furthest away 
    {
        Cell theCell = new Cell();
        float dist = 0;
        foreach (Cell c in currentCells)
        {
            if (c.point != from)
            {
                if (Vector3.Distance(c.point, from) > dist)
                {
                    dist = Vector3.Distance(c.point, from);
                    theCell = c;
                }
            }
        }
        return theCell;
    }

    public Cell RandomFurthest(Vector3 from, List<Vector3> cellPoints)
    {
        List<Cell> furthestPos = new List<Cell>();
        int i = 0;
        while (i < 5) //the five furthest away
        {
            Cell cell = GetFurthestCell(from);
            currentCells.Remove(cell);
            cellPoints.Remove(cell.point);
            furthestPos.Add(cell);
            i++;
        }

        Cell c = new Cell();
        int rand = UnityEngine.Random.Range(0, furthestPos.Count);
        c = furthestPos[rand];
        return c;
    }

    public Cell GetRandomCell()
    {
        int rand = UnityEngine.Random.Range(0, allMazeCells.Length);
        Cell cell = allMazeCells[rand];
        return cell;
    }
}
