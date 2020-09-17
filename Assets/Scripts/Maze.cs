using System.Collections.Generic;
using UnityEngine;

//(FlatTutorials, 2015)

public class Maze : MonoBehaviour
{
    public GameObject wall;
    public int xSize = 3;
    public int zSize = 3;
    private float wallLength = 1f;
    private Vector3 initialPosition;
    public GameObject wallHolder;
    public GameObject floor;

    [SerializeField]
    public Cell[] cells;
    public List<GameObject> activeWalls = new List<GameObject>();
    public List<Vector3> centrePoints;
    public float yStart;

    private int currentCell;
    private int totalCells;
    private int visitedCells;
    private bool startedBuilding;
    private int currentNeighbour;
    private List<int> lastCells;
    private int backingUp = 0;
    private int wallToBreak;
 
    public void CreateWalls()
    {
        wallHolder = new GameObject
        {
            name = "Maze",
        };
        initialPosition = new Vector3((-xSize / 2) + (wallLength / 2), -.35f, (-zSize / 2) + (wallLength / 2));
        Vector3 pos = initialPosition;
        GameObject tempWall;

        //for x axis
        for(int i = 0; i < zSize; i++)
        {
            for (int j = 0; j <= xSize; j++)
            {
                pos = new Vector3(initialPosition.x + (j * wallLength) - (wallLength / 2), yStart, initialPosition.z + (i * wallLength) - (wallLength / 2));
                tempWall = Instantiate(wall, pos, Quaternion.identity) as GameObject;
                activeWalls.Add(tempWall);
                tempWall.transform.parent = wallHolder.transform;
            }
        }
        //for z axis
        for (int i = 0; i <= zSize; i++)
        {
            for (int j = 0; j < xSize; j++)
            {
                pos = new Vector3(initialPosition.x + (j * wallLength), yStart, initialPosition.z + (i * wallLength) - (wallLength));
                tempWall = Instantiate(wall, pos, Quaternion.Euler(0, 90, 0)) as GameObject;
                activeWalls.Add(tempWall);
                tempWall.transform.parent = wallHolder.transform;
            }
        }
        CreateCells();
    }

    void CreateCells()
    {
        lastCells = new List<int>();
        lastCells.Clear();
        totalCells = xSize * zSize;
        GameObject[] allWalls;
        int children = wallHolder.transform.childCount;
        allWalls = new GameObject[children];
        cells = new Cell[xSize * zSize];
        centrePoints = new List<Vector3>();
        centrePoints.Clear();
        int eastWest = 0;
        int northSouth = 0;
        int rowIndex = 0;

        //Get all the children
        for(int i = 0; i < children; i++)
        {
            allWalls[i] = wallHolder.transform.GetChild(i).gameObject;
        }

        //assigns walls to cells
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = new Cell
            {
                west = allWalls[eastWest], //simply adding one each time as the vertical walls are spawned first
                south = allWalls[northSouth + (xSize + 1) * zSize] //specific algorithm for south & north(horizontal walls) as they are spawned in after the vertical walls 
            };
            
            if (rowIndex == xSize)
            {
                eastWest += 2;//onto next line
                rowIndex = 0; //restart row count
            }
            else
            {
                eastWest++;
            }
            if (rowIndex == 0 && eastWest != 0) //allows the pathfinding system to get the correct neighbours 
            {
                cells[i].west = allWalls[eastWest - 1];
            }
            rowIndex++;
            northSouth++;
            cells[i].east = allWalls[eastWest];
            cells[i].north = allWalls[northSouth + (xSize + 1) * zSize + (xSize - 1)];
            cells[i].point = new Vector3((cells[i].north.transform.position.x + cells[i].south.transform.position.x) / 2, yStart, (cells[i].north.transform.position.z + cells[i].south.transform.position.z) / 2);
            //establishing the centre of each cell using the walls as reference
            centrePoints.Add(cells[i].point);
        }
        
        CreateMaze();
    }

    void CreateMaze()
    {       
        while (visitedCells < totalCells) //looping round all cells 
        {
            if (startedBuilding)
            {
                GetNeighbours();
                if(!cells[currentNeighbour].visited && cells[currentCell].visited)
                {
                    BreakWall();
                    cells[currentNeighbour].visited = true;
                    visitedCells++;
                    lastCells.Add(currentCell);
                    currentCell = currentNeighbour;
                    if(lastCells.Count > 0)
                    {
                        backingUp = lastCells.Count - 1;
                    }
                }
            }
            else
            {
                currentCell = Random.Range(0, totalCells);
                cells[currentCell].visited = true;
                visitedCells++;
                startedBuilding = true;
            }
        }       
    }

    void BreakWall()
    {
        switch (wallToBreak)  
        {
            case 1:
                cells[currentCell].north.SetActive(false); 
                activeWalls.Remove(cells[currentCell].north); //removing from active walls so the pathfinding can calculate neighbours 
                break;
            case 2:
                cells[currentCell].east.SetActive(false);
                activeWalls.Remove(cells[currentCell].east);

                break;
            case 3:
                cells[currentCell].west.SetActive(false);
                activeWalls.Remove(cells[currentCell].west);

                break;
            case 4:
                cells[currentCell].south.SetActive(false);
                activeWalls.Remove(cells[currentCell].south);
                break;
        }
    }

    void GetNeighbours() //finding the cell neighbours 
    {
        int length = 0;
        int[] neighbours = new int[4];
        int[] connectingWall = new int[4];
        int check = 0;
        check = (currentCell + 1) / xSize;
        check -= 1;
        check *= xSize;
        check += xSize;

        //east
        if(currentCell + 1 < totalCells && (currentCell + 1) != check)
        {
            if(!cells[currentCell + 1].visited)
            {
                neighbours[length] = currentCell + 1;
                connectingWall[length] = 2;
                length++;
            }
        }
        //west
        if (currentCell - 1 >= 0 && currentCell != check)
        {
            if (!cells[currentCell - 1].visited)
            {
                neighbours[length] = currentCell - 1;
                connectingWall[length] = 3;
                length++;
            }
        }       
        //north
        if (currentCell + xSize < totalCells)
        {
            if (!cells[currentCell + xSize].visited)
            {
                neighbours[length] = currentCell + xSize;
                connectingWall[length] = 1;
                length++;
            }
        }
        //south
        if (currentCell - xSize >= 0)
        {
            if (!cells[currentCell - xSize].visited)
            {
                neighbours[length] = currentCell - xSize;
                connectingWall[length] = 4;
                length++;
            }
        }       
        if(length != 0)
        {
            int rand = Random.Range(0, length);
            currentNeighbour = neighbours[rand];
            wallToBreak = connectingWall[rand];
        }
        else
        {
            if(backingUp > 0)
            {
                currentCell = lastCells[backingUp];
                backingUp--;
            }
        }
    }
}
