using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GridCell : MonoBehaviour
{
    public GameObject gridCellPrefab;
    [Tooltip("Number of Cells for a side")]
    [SerializeField] private float gridSize = 10;
    [SerializeField] private float boxscale = 1.5f;
    private string currentGridLocation;

    private void Start()
    {
        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
    }

    ///<summary>
    /// Destroy grid when changing selection.
    ///</summary>
    ///<param name="_e">Event raised when selecting something</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        Destroy(GetComponent<Rack>().gridForULocation);
    }

    ///<summary>
    /// Destroy grid when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void OnEditModeIn(EditModeInEvent _e)
    {
        Destroy(GetComponent<Rack>().gridForULocation);
    }

    ///<summary>
    /// Handle removing of current grid and creation of new grid.
    ///</summary>
    ///<param name="_height">World position y to place the grid</param>
    ///<param name="_location">Name of the ULocation where the new grid should be created</param>
    public void ToggleGrid(float _height, string _location)
    {
        GameObject previousGrid = GetComponent<Rack>().gridForULocation;
        if (previousGrid)
            Destroy(previousGrid.gameObject);

        if (_location != currentGridLocation)
        {
            GetComponent<Rack>().gridForULocation = InstantiateGrid(_height);
            currentGridLocation = _location;
        }
        else
            currentGridLocation = "";
    }

    ///<summary>
    /// Instantiate a new grid and adjust its scale, texture and height.
    ///</summary>
    ///<param name="_height">World position y to place the grid</param>
    ///<returns>The created grid</returns>
    private GameObject InstantiateGrid(float _height)
    {
        GameObject grid = Instantiate(gridCellPrefab, transform);
        grid.name = "GridForULocation";

        Vector3 newPosition = grid.transform.position;
        if (GetComponent<Rack>().attributes["heightUnit"] == "U")
            newPosition.y = _height - GameManager.gm.uSize / 2;
        else if (GetComponent<Rack>().attributes["heightUnit"] == "OU")
            newPosition.y = _height - GameManager.gm.ouSize / 2;
        grid.transform.position = newPosition;

        Vector2 size = new Vector2(transform.GetChild(0).localScale.x, transform.GetChild(0).localScale.z) / 10;
        size *= boxscale;
        grid.transform.localScale = new Vector3(size.x, 1, size.y);

        Vector2 gridRatio;
        if (size.x < size.y)
            gridRatio = new Vector2(size.x / size.y, 1) * gridSize;
        else
            gridRatio = new Vector2(size.y / size.x, 1) * gridSize;
        grid.GetComponent<Renderer>().material.mainTextureScale = gridRatio;

        return grid;
    }

}