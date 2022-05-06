using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour

{
    public int width, height;
    public float gridSpaceSize;
    [SerializeField] private GameObject gridCellPrefab;
    private GameObject[,] gameGrid;
 
    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        gameGrid = new GameObject[height,width];

        if (gridCellPrefab == null)
        {
            Debug.Log("Error");
            return;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                gameGrid[x,y] = Instantiate(gridCellPrefab, new Vector3(x * gridSpaceSize, 0, y * gridSpaceSize), Quaternion.identity);
                gameGrid[x,y].SetActive(true);
                gameGrid[x,y].transform.parent = transform;
                gameGrid[x,y].gameObject.name = "Grid Space ( x: " + x.ToString() + ", Y: " + y.ToString() + ")";
            }
        }
    }
}