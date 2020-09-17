using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorStars : MonoBehaviour
{
    public GameObject star;
    GameManager gameManager;
    CellManager cManager;
    Cell[] mazeCells;
    static List<GameObject> allStars = new List<GameObject>();

    void Start()
    {
        if (allStars.Count > 0)
        {
            foreach (GameObject g in allStars)
                Destroy(g);
            allStars.Clear();
        }
        gameManager = FindObjectOfType<GameManager>();
        cManager = FindObjectOfType<CellManager>();
        mazeCells = cManager.AllCells();
        SpawnStars();
    }
    
    void SpawnStars()
    {
        float xVal = gameManager.mazeX;
        for(int x = 0; x < mazeCells.Length; x++)
        {
            Vector3 yPos = mazeCells[x].point + new Vector3(0, Random.Range(xVal, xVal + 4), 0);
            GameObject starObj = Instantiate(star, yPos , Quaternion.identity);
            allStars.Add(starObj);
        }
    }
}
