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
    /// Toggle tiles name.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesName(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.gm.AppendLogLine("tilesName value has to be true or false", "yellow");
            return;
        }
        else
        {
            GameObject root = transform.Find("tilesNameRoot")?.gameObject;
            if (_value == "true")
            {
                if (!root)
                {
                    root = new GameObject("tilesNameRoot");
                    root.transform.parent = transform;
                    root.transform.localPosition = usableZone.localPosition;
                    root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.002f, GameManager.gm.tileSize) / 2;
                    root.transform.localEulerAngles = Vector3.zero;
                    LoopThroughTiles("name", root.transform);
                }
            }
            else
            {
                if (root)
                    Destroy(root);
            }
        }
    }
    public void ToggleTilesName()
    {
        GameObject root = transform.Find("tilesNameRoot")?.gameObject;
        if (root)
            Destroy(root);
        else
        {
            root = new GameObject("tilesNameRoot");
            root.transform.parent = transform;
            root.transform.localPosition = usableZone.localPosition;
            root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.002f, GameManager.gm.tileSize) / 2;
            root.transform.localEulerAngles = Vector3.zero;
            LoopThroughTiles("name", root.transform);
        }
    }


    ///<summary>
    /// Toggle tiles colors and textures.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesColor(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.gm.AppendLogLine("tilesColor value has to be true or false", "yellow");
            return;
        }
        if (!GameManager.gm.roomTemplates.ContainsKey(template))
        {
            GameManager.gm.AppendLogLine($"There is no template for {name}", "yellow");
            return;
        }

        GameObject root = transform.Find("tilesColorRoot")?.gameObject;
        if (_value == "true")
        {
            if (!root)
            {
                root = new GameObject("tilesColorRoot");
                root.transform.parent = transform;
                root.transform.localPosition = usableZone.localPosition;
                root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.001f, GameManager.gm.tileSize) / 2;
                root.transform.localEulerAngles = Vector3.zero;
                LoopThroughTiles("color", root.transform);
            }
        }
        else
        {
            if (root)
                Destroy(root);
        }
    }
    public void ToggleTilesColor()
    {
        GameObject root = transform.Find("tilesColorRoot")?.gameObject;
        if (root)
            Destroy(root);
        else
        {
            root = new GameObject("tilesColorRoot");
            root.transform.parent = transform;
            root.transform.localPosition = usableZone.localPosition;
            root.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.001f, GameManager.gm.tileSize) / 2;
            root.transform.localEulerAngles = Vector3.zero;
            LoopThroughTiles("color", root.transform);
        }
    }

    ///<summary>
    /// Loop through every tile placement and populate name or plane according to _mode.
    ///</summary>
    ///<param name="_mode">"name" or "color" mode</param>
    ///<param name="_root">Root for choosen mode</param>
    private void LoopThroughTiles(string _mode, Transform _root)
    {
        float x = size.x / GameManager.gm.tileSize - reserved.left - technical.right - technical.left;
        float y = size.y / GameManager.gm.tileSize - reserved.bottom - technical.top - technical.bottom;

        Vector3 origin = usableZone.localScale / -0.2f;
        _root.transform.localPosition += new Vector3(origin.x, 0, origin.z);
        for (int j = (int)-reserved.bottom; j < y; j++)
        {
            for (int i = (int)-reserved.left; i < x; i++)
            {
                string tileID = "";
                if (i >= 0 && j >= 0)
                    tileID = $"{i + 1}/{j + 1}";
                else if (i >= 0)
                    tileID = $"{i + 1}/{j}";
                else if (j >= 0)
                    tileID = $"{i}/{j + 1}";
                else
                    tileID = $"{i}/{j}";

                if (_mode == "name")
                {
                    if (GameManager.gm.roomTemplates.ContainsKey(template))
                    {
                        GenerateTileName(_root, new Vector2(i, j) * GameManager.gm.tileSize,
                                         tileID, GameManager.gm.roomTemplates[template]);
                    }
                    else
                    {
                        GenerateTileName(_root, new Vector2(i, j) * GameManager.gm.tileSize,
                                         tileID, new ReadFromJson.SRoomFromJson());
                    }
                }
                else if (_mode == "color")
                {
                    GenerateTileColor(_root, new Vector2(i, j) * GameManager.gm.tileSize,
                                      tileID, GameManager.gm.roomTemplates[template]);
                }
            }
        }
    }

    ///<summary>
    /// Instantiate a tileNameModel from gm and assign a name to it.
    ///</summary>
    ///<param name="_root">The root to parent the tile</param>
    ///<param name="_pos">The position of the current tile</param>
    ///<param name="_id">The id of the current tile</param>
    ///<param name="_data">The room data from json template. Empty one if no templated one</param>
    private void GenerateTileName(Transform _root, Vector2 _pos, string _id, ReadFromJson.SRoomFromJson _data)
    {
        GameObject tileText = Instantiate(GameManager.gm.tileNameModel);
        tileText.name = $"Text_{_id}";
        tileText.transform.SetParent(_root);
        tileText.transform.localPosition = new Vector3(_pos.x, 0, _pos.y);
        tileText.transform.localEulerAngles = new Vector3(90, 0, 0);

        // Select the right tile from _data.tiles
        ReadFromJson.STiles tileData = new ReadFromJson.STiles();
        if (!string.IsNullOrEmpty(_data.slug))
        {
            foreach (ReadFromJson.STiles tile in _data.tiles)
            {
                if (tile.location.Trim() == _id)
                    tileData = tile;
            }
        }
        if (!string.IsNullOrEmpty(tileData.location) && !string.IsNullOrEmpty(tileData.label))
            tileText.GetComponent<TextMeshPro>().text = tileData.label;
        else
            tileText.GetComponent<TextMeshPro>().text = _id;
    }

    ///<summary>
    /// Instantiate a plane as a tile and assign a texture and/or a color to it.
    ///</summary>
    ///<param name="_root">The root to parent the tile</param>
    ///<param name="_pos"> The position of the current tile</param>
    ///<param name="_id">The id of the current tile</param>
    ///<param name="_data">The room data from json template</param>
    private void GenerateTileColor(Transform _root, Vector2 _pos, string _id, ReadFromJson.SRoomFromJson _data)
    {
        // Select the right tile from _data.tiles
        ReadFromJson.STiles tileData = new ReadFromJson.STiles();
        foreach (ReadFromJson.STiles tile in _data.tiles)
        {
            if (tile.location.Trim() == _id)
                tileData = tile;
        }
        if (!string.IsNullOrEmpty(tileData.location))
        {
            if (!string.IsNullOrEmpty(tileData.type) && tileData.type != "plain"
                || !string.IsNullOrEmpty(tileData.color))
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.name = $"Color_{_id}";
                tile.transform.parent = _root;
                tile.transform.localScale = Vector3.one * GameManager.gm.tileSize / 10;
                tile.transform.localPosition = new Vector3(_pos.x, 0, _pos.y);
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
            // case "tiles": // DEPRECIATED
            //     ToggleZones(_value);
            //     break;
            case "tilesName":
                ToggleTilesName(_value);
                break;
            case "tilesColor":
                ToggleTilesColor(_value);
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
    /// DEPRECIATED
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
