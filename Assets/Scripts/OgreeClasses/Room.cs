using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Room : Building
{
    // ITROOM : technical <> null + reserved <> null + WDunit = tile 
    // ROOM (AC + power): technical = 0 + reserved = 0 + WDunit = cm / inch / tile

    public ECardinalOrient orientation;

    public Customer tenant;
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
    /// Set usable/reserved/technical areas.
    ///</summary>
    ///<param name="_resDim">The dimensions of the reserved zone</param>
    ///<param name="_techDim">The dimensions of the technical zone</param>
    public void SetAreas(SMargin _resDim, SMargin _techDim)
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
                string tileID = $"{i}/{j}";
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
                    if (GameManager.gm.textures.ContainsKey(tileData.type))
                    {
                        rend.material = new Material(GameManager.gm.perfMat);
                        rend.material.mainTexture = GameManager.gm.textures[tileData.type];
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
            case "tenant":
                if (GameManager.gm.allItems.ContainsKey(_value))
                {
                    GameObject go = (GameObject)GameManager.gm.allItems[_value];
                    tenant = go.GetComponent<Customer>();
                }
                else
                    GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exists. Please create it before assign it.", "yellow");
                break;
            case "floor":
                floor = _value;
                break;
            case "areas":
                ParseAreas(_value);
                break;
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
        usableZone.GetComponent<Renderer>().material.color = ParseColor(dc.usableColor);
        reservedZone.GetComponent<Renderer>().material.color = ParseColor(dc.reservedColor);
        technicalZone.GetComponent<Renderer>().material.color = ParseColor(dc.technicalColor);
    }

    ///<summary>
    /// Parse a "areas" command and call SetZones().
    ///</summary>
    ///<param name="_input">String with zones data to parse</param>
    private void ParseAreas(string _input)
    {
        string patern = "^\\[([0-9.]+,){3}[0-9.]+\\]@\\[([0-9.]+,){3}[0-9.]+\\]$";
        if (Regex.IsMatch(_input, patern))
        {
            _input = _input.Replace("[", "");
            _input = _input.Replace("]", "");
            string[] data = _input.Split('@', ',');

            SMargin resDim = new SMargin(float.Parse(data[0]), float.Parse(data[1]),
                                        float.Parse(data[2]), float.Parse(data[3]));
            SMargin techDim = new SMargin(float.Parse(data[4]), float.Parse(data[5]),
                                        float.Parse(data[6]), float.Parse(data[7]));
            SetAreas(resDim, techDim);
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    private Color ParseColor(string _hex)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString($"#{_hex}", out newColor);
        return newColor;
    }

}
