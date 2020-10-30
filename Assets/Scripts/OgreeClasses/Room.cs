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
    public string floor;

    public string template;

    [Header("RO References")]
    public Transform usableZone;
    public Transform reservedZone;
    public Transform technicalZone;
    public Transform tilesEdges;
    public TextMeshPro nameText;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Filters.instance.roomsList.Remove(name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRooms, Filters.instance.roomsList);
        GameManager.gm.allItems.Remove(GetComponent<HierarchyName>().fullname);
    }

    ///<summary>
    /// Set usable/reserved/technical zones.
    ///</summary>
    ///<param name="_resDim">The dimensions of the reserved zone</param>
    ///<param name="_techDim">The dimensions of the technical zone</param>
    public void SetZones(SMargin _resDim, SMargin _techDim)
    {
        if (transform.GetComponentInChildren<Rack>())
        {
            GameManager.gm.AppendLogLine("Can't modify areas if room has a rack in it.", "yellow");
            return;
        }
        tilesEdges.gameObject.SetActive(true);

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
    /// If a root is finded, delete it. Otherwise instantiate one TileText per usable tile in the room. 
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

            float x = size.x / GameManager.gm.tileSize - reserved.left - technical.right - technical.left;
            float y = size.y / GameManager.gm.tileSize - reserved.bottom - technical.top - technical.bottom;

            Vector3 origin = usableZone.localScale / -0.2f;
            root.transform.localPosition += new Vector3(origin.x, 0.001f, origin.z);
            root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, GameManager.gm.tileSize) / 2;
            for (int j = (int)-reserved.bottom; j < y; j++)
            {
                for (int i = (int)-reserved.left; i < x; i++)
                {
                    GameObject tileText = Instantiate(GameManager.gm.tileNameModel);
                    if (i >= 0 && j >= 0)
                        tileText.name = $"{i + 1}/{j + 1}";
                    else if (i >= 0)
                        tileText.name = $"{i + 1}/{j}";
                    else if (j >= 0)
                        tileText.name = $"{i}/{j + 1}";
                    else
                        tileText.name = $"{i}/{j}";
                    tileText.transform.SetParent(root.transform);
                    tileText.transform.localPosition = new Vector3(i, 0, j) * GameManager.gm.tileSize;
                    tileText.transform.localEulerAngles = new Vector3(90, 0, 0);
                    if (GameManager.gm.roomTemplates.ContainsKey(template))
                        CustomTiles(tileText, GameManager.gm.roomTemplates[template], tileText.name);
                    else
                        tileText.GetComponent<TextMeshPro>().text = tileText.name;
                }
            }
        }
    }

    ///<summary>
    ///
    ///</summary>
    ///<param name=""></param>
    ///<param name=""></param>
    ///<param name=""></param>
    private void CustomTiles(GameObject _tileText, ReadFromJson.SRoomFromJson _data, string _loc)
    {
        // Select the right tile from _data.tiles
        ReadFromJson.STiles tileData = new ReadFromJson.STiles();
        foreach (ReadFromJson.STiles tile in _data.tiles)
        {
            if (tile.location.Trim() == _loc)
                tileData = tile;
        }
        if (!string.IsNullOrEmpty(tileData.location))
        {
            _tileText.GetComponent<TextMeshPro>().text = tileData.label;
            if (!string.IsNullOrEmpty(tileData.type) && tileData.type != "plain"
                || !string.IsNullOrEmpty(tileData.color))
            {
                _tileText.transform.localPosition += new Vector3(0, 0.002f, 0);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.transform.parent = _tileText.transform;
                tile.transform.localScale = Vector3.one * GameManager.gm.tileSize / 10;
                tile.transform.localPosition = new Vector3(0, 0, 0.002f);
                tile.transform.localEulerAngles = new Vector3(-90, 0, 0);
                if (!string.IsNullOrEmpty(tileData.type))
                {
                    Renderer rend = tile.GetComponent<Renderer>();
                    if (tileData.type == "perf22")
                    {
                        rend.material = new Material(GameManager.gm.perfMat);
                        rend.material.mainTexture = Resources.Load<Texture>("Textures/TilePerf22");
                    }
                    if (tileData.type == "perf29")
                    {
                        rend.material = new Material(GameManager.gm.perfMat);
                        rend.material.mainTexture = Resources.Load<Texture>("Textures/TilePerf29");

                    }
                }
                if (!string.IsNullOrEmpty(tileData.color))
                {
                    Material mat = tile.GetComponent<Renderer>().material;
                    Color customColor = new Color();
                    if (tileData.color.StartsWith("@"))
                    {
                        foreach (ReadFromJson.SColor color in _data.colors)
                        {
                            if (color.name == tileData.color.Substring(1))
                                ColorUtility.TryParseHtmlString($"#{color.value}", out customColor);
                        }
                    }
                    else
                        ColorUtility.TryParseHtmlString($"#{tileData.color}", out customColor);
                    mat.color = customColor;
                }
            }
        }
        else
            _tileText.GetComponent<TextMeshPro>().text = _loc;

    }

    ///<summary>
    /// Remove tiles from a zone.
    ///</summary>
    ///<param name ="_zone">The zone to reduce</param>
    ///<param name="_dim">The dimensions of the reduction</param>
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

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        switch (_param)
        {
            case "description":
                description = _value;
                break;
            // case "nbfloors":
            //     nbFloors = _value;
            //     break;
            case "tenant":
                if (GameManager.gm.tenants.ContainsKey(_value))
                    tenant = GameManager.gm.tenants[_value];
                else
                    GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exists. Please create it before assign it.", "yellow");
                break;
            case "floor":
                floor = _value;
                break;
            case "tiles":
                ToggleZones(_value);
                break;
            default:
                GameManager.gm.AppendLogLine($"[Room] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

    ///<summary>
    /// Set usable/reserved/technical zones color according to parented Datacenter
    ///</summary>
    public void UpdateZonesColor()
    {
        Datacenter dc = transform.parent.GetComponentInParent<Datacenter>();
        usableZone.GetComponent<Renderer>().material.color = dc.usableColor;
        reservedZone.GetComponent<Renderer>().material.color = dc.reservedColor;
        technicalZone.GetComponent<Renderer>().material.color = dc.technicalColor;
    }

    ///<summary>
    /// Display or hide zones and tiles edges (floor color will stay the technical one).
    ///</summary>
    ///<param name="_value">True of false value</param>
    private void ToggleZones(string _value)
    {
        if (_value == "true")
        {
            usableZone.GetComponent<Renderer>().enabled = true;
            reservedZone.GetComponent<Renderer>().enabled = true;
            tilesEdges.GetComponent<Renderer>().enabled = true;
        }
        else if (_value == "false")
        {
            usableZone.GetComponent<Renderer>().enabled = false;
            reservedZone.GetComponent<Renderer>().enabled = false;
            tilesEdges.GetComponent<Renderer>().enabled = false;
        }
        else
            GameManager.gm.AppendLogLine("tiles value must be true of false", "yellow");
    }

}
