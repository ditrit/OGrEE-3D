using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : Building
{
    // ITROOM : technical <> null + reserved <> null + WDunit = tile 
    // ROOM (AC + power): technical = 0 + reserved = 0 + WDunit = cm / inch / tile

    public EOrientation orientation;

    public Tenant tenant;
    public SMargin reserved;
    public SMargin technical;
    public float floorHeight;
    public EUnit floorUnit;

    [Header("References")]
    public Transform usableZone;
    public Transform reservedZone;
    public Transform technicalZone;
    public Transform tilesEdges;
    public Transform walls;
    public TextMeshPro nameText;


    public void SetZones(SMargin _resDim, SMargin _techDim)
    {
        reserved = new SMargin(_resDim);
        technical = new SMargin(_techDim);

        // Reset  ->  techzone is always full size of a room
        usableZone.localScale = technicalZone.localScale;
        usableZone.localPosition = new Vector3(technicalZone.localPosition.x,
                                                usableZone.localPosition.y, technicalZone.localPosition.z);
        reservedZone.localScale = technicalZone.localScale;
        reservedZone.localPosition = new Vector3(technicalZone.localPosition.x,
                                                reservedZone.localPosition.y, technicalZone.localPosition.z);

        // Reduce zones
        ReduceZone(reservedZone, _techDim);
        ReduceZone(usableZone, _techDim);
        ReduceZone(usableZone, _resDim);
    }

    ///<summary>
    /// If a root is finded, delete it. Else instantiate one TileText per usable tile in the room. 
    ///</summary>
    public void ToggleTilesName()
    {
        GameObject root = transform.Find("tilesRoot")?.gameObject;
        if (root)
            Destroy(root);
        else
        {
            root = new GameObject("tilesRoot");
            root.transform.parent = transform;
            root.transform.localPosition = usableZone.localPosition;
            root.transform.localEulerAngles = Vector3.zero;

            float x = size.x / GameManager.gm.tileSize - reserved.right - reserved.left - technical.right - technical.left;
            float y = size.y / GameManager.gm.tileSize - reserved.top - reserved.bottom - technical.top - technical.bottom;
            // Debug.Log($"{name}: x={x} / y={y}");

            Vector3 origin = usableZone.localScale / -0.2f;
            root.transform.localPosition += new Vector3(origin.x, 0.001f, origin.z);
            root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, GameManager.gm.tileSize) / 2;
            for (int j = 0; j < y; j++)
            {
                for (int i = 0; i < x; i++)
                {
                    GameObject tileText = Instantiate(GameManager.gm.tileNameModel);
                    tileText.transform.SetParent(root.transform);
                    tileText.transform.localPosition = new Vector3(i, +  0, j) * GameManager.gm.tileSize;
                    tileText.transform.localEulerAngles = new Vector3(90, 0, 0);
                    tileText.GetComponent<TextMeshPro>().text = $"{i + 1}/{j + 1}";
                }
            }
        }
    }

    private void ReduceZone(Transform _zone, SMargin _dim)
    {
        _zone.localScale -= new Vector3(0, 0, _dim.top) * GameManager.gm.tileSize / 10;
        _zone.localPosition -= new Vector3(0, 0, _dim.top) * GameManager.gm.tileSize / 2;

        _zone.localScale -= new Vector3(0, 0, _dim.bottom) * GameManager.gm.tileSize / 10;
        _zone.localPosition += new Vector3(0, 0, _dim.bottom) * GameManager.gm.tileSize / 2;

        _zone.localScale -= new Vector3(_dim.right, 0, 0) * GameManager.gm.tileSize / 10;
        _zone.localPosition -= new Vector3(_dim.right, 0, 0) * GameManager.gm.tileSize / 2;

        _zone.localScale -= new Vector3(_dim.left, 0, 0) * GameManager.gm.tileSize / 10;
        _zone.localPosition += new Vector3(_dim.left, 0, 0) * GameManager.gm.tileSize / 2;
    }

}
