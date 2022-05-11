using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Grid : MonoBehaviour

{
    public GameObject gridCellPrefab;
    [Header("Number of Cells for a side")]
    public float gridSize;
    public float boxscale = 1.5f;
    private bool isFirstGrid = true;
    string currentGridName;

    private void Start()
    {
        //CreateGrid();
    }

    ///<summary>
    /// Handle removing of current grid and creation of new grid.
    ///</summary>
    ///<param name="_height">World position y to place the grid</param>
    ///<param name="_name">Name of the ULocation where the new grid should be created</param>
    public void CreateGrid(float _height, string _name)
    {   
        if (!isFirstGrid)
        {
            GameObject previousGrid = transform.Find("Grid").gameObject;

            if (_name == currentGridName)
            {
                if (previousGrid)
                    Destroy(previousGrid);  
                    isFirstGrid = true;
            }
            else
            {
                if (previousGrid)
                {
                    Destroy(previousGrid); 
                    InstantiateGrid( _height);       
                    currentGridName = _name;  
                }
            }
        }
        else
        {
            InstantiateGrid(_height);
            currentGridName = _name;
            isFirstGrid = false;
        }
    }

    ///<summary>
    /// Instantiate a new grid and adjust its scale, texture and height.
    ///</summary>
    ///<param name="_height">World position y to place the grid</param>
    private void InstantiateGrid(float _height)
    {
        GameObject grid = Instantiate(gridCellPrefab, transform);
        Vector2 size = new Vector2(transform.GetChild(0).localScale.x, transform.GetChild(0).localScale.z)/10; 
        size *= boxscale;
        grid.transform.localScale = new Vector3 (size.x, 1, size.y); 
        grid.transform.GetChild(0).GetComponent<Renderer>().material.mainTextureScale = size * 10 * gridSize / boxscale; //multiplier par 10
        grid.transform.GetChild(1).GetComponent<Renderer>().material.mainTextureScale = size * 10 * gridSize / boxscale; //multiplier par 10
        Vector3 newPosition = new Vector3();
        newPosition = grid.transform.position;
        newPosition.y = _height - GameManager.gm.uSize/2; //retirer pour placer en bas du U au lieu du milieu 22.25 /1000
        grid.transform.position = newPosition;
        grid.name = "Grid";
    }
}