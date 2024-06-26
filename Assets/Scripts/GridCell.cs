using UnityEngine;

public class GridCell : MonoBehaviour
{
    public GameObject gridCellPrefab;
    [Tooltip("Number of Cells for a side")]
    [SerializeField] private float gridSize = 10;
    [SerializeField] private float boxscale = 1.5f;
    private string currentGridLocation;

    private void Start()
    {
        EventManager.instance.EditModeIn.Add(OnEditModeIn);
        EventManager.instance.OnSelectItem.Add(OnSelectItem);
    }

    private void OnDestroy()
    {
        EventManager.instance.EditModeIn.Remove(OnEditModeIn);
        EventManager.instance.OnSelectItem.Remove(OnSelectItem);
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
    ///<param name="_clickedU">The transform of the ULocation where the new grid should be created</param>
    public void ToggleGrid(Transform _clickedU)
    {
        GameObject previousGrid = GetComponent<Rack>().gridForULocation;
        if (previousGrid)
            Destroy(previousGrid);

        if (_clickedU.name != currentGridLocation)
        {
            GetComponent<Rack>().gridForULocation = InstantiateGrid(_clickedU.localPosition.y);
            currentGridLocation = _clickedU.name;
        }
        else
            currentGridLocation = "";
    }

    ///<summary>
    /// Instantiate a new grid and adjust its scale, texture and height.
    ///</summary>
    ///<param name="_height">The local height of the related U helper</param>
    ///<returns>The created grid</returns>
    private GameObject InstantiateGrid(float _height)
    {
        GameObject grid = Instantiate(gridCellPrefab, transform);
        grid.name = "GridForULocation";

        Vector3 newPosition = transform.GetChild(0).localScale / 2;
        if ((string)GetComponent<Rack>().attributes["heightUnit"] == LengthUnit.OU)
            newPosition.y = _height - UnitValue.OU / 2;
        else
            newPosition.y = _height - UnitValue.U / 2;
        grid.transform.localPosition = newPosition;

        Vector2 size = new Vector2(transform.GetChild(0).localScale.x, transform.GetChild(0).localScale.z) / 10;
        size *= boxscale;
        grid.transform.localScale = new(size.x, 1, size.y);

        Vector2 gridRatio;
        if (size.x < size.y)
            gridRatio = new Vector2(size.x / size.y, 1) * gridSize;
        else
            gridRatio = new Vector2(size.y / size.x, 1) * gridSize;
        grid.GetComponent<Renderer>().material.mainTextureScale = gridRatio;

        return grid;
    }
}