using UnityEngine;

//(FlatTutorials, 2015)

[System.Serializable]
public class Cell 
{
    public bool visited;
    public GameObject north; //each wall surrounding the cell
    public GameObject east;
    public GameObject south;
    public GameObject west; 
    public Vector3 point; //the position of the cell in world space

    //for the a* algorithm. 
    public Cell parent; //this holding the previous cell in the path 
    public float hCost; //heuristic distance from end 
    public float gCost; //distance from start

    public float FCost
    {
        get
        {
            return hCost + gCost;
        }
    }

    public Cell()
    {
        north = null;
        east = null;
        west = null;
        south = null;
    }
}
