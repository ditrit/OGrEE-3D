using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class Room : Building
{
    public bool isSquare = true;
    public SMargin reserved;
    public SMargin technical;

    [Header("Floor layers")]
    public Transform usableZone;
    public Transform reservedZone;
    public Transform technicalZone;
    public Transform tilesEdges;
    public string temperatureUnit;

    ///<summary>
    /// Set usable/reserved/technical areas.
    ///</summary>
    ///<param name="_resDim">The dimensions of the reserved zone</param>
    ///<param name="_techDim">The dimensions of the technical zone</param>
    public void SetAreas(SMargin _resDim, SMargin _techDim)
    {
        if (transform.GetComponentInChildren<Rack>())
        {
            GameManager.instance.AppendLogLine("Can't modify areas if room has a rack drawn in it.", true, ELogtype.error);
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

        // Save areas in attributes
        attributes["reserved"] = JsonUtility.ToJson(_resDim);
        attributes["technical"] = JsonUtility.ToJson(_techDim);
    }

    ///<summary>
    /// Toggle tiles name.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesName(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine("tilesName value has to be true or false", true, ELogtype.warning);
            return;
        }

        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        GameObject root = transform.Find("tilesNameRoot")?.gameObject;
        if (_value == "true")
        {
            if (!root)
            {
                root = new GameObject("tilesNameRoot");
                root.transform.parent = transform;
                root.transform.localPosition = usableZone.localPosition;
                root.transform.localPosition += new Vector3(GameManager.instance.tileSize, 0.003f, GameManager.instance.tileSize) / 2;
                root.transform.localEulerAngles = Vector3.zero;
                LoopThroughTiles("name", root.transform);
            }
        }
        else
        {
            if (root)
                Destroy(root);
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }

    public void ToggleTilesName()
    {
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        GameObject root = transform.Find("tilesNameRoot")?.gameObject;
        if (root)
        {
            root.SetActive(false); //for UI
            Destroy(root);
        }
        else
        {
            root = new GameObject("tilesNameRoot");
            root.transform.parent = transform;
            root.transform.localPosition = usableZone.localPosition;
            root.transform.localPosition += new Vector3(GameManager.instance.tileSize, 0.003f, GameManager.instance.tileSize) / 2;
            root.transform.localEulerAngles = Vector3.zero;
            LoopThroughTiles("name", root.transform);
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }


    ///<summary>
    /// Toggle tiles colors and textures.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesColor(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine("tilesColor value has to be true or false", true, ELogtype.warning);
            return;
        }
        if (!GameManager.instance.roomTemplates.ContainsKey(attributes["template"]))
        {
            GameManager.instance.AppendLogLine($"There is no template for {name}", false, ELogtype.warning);
            return;
        }

        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        GameObject root = transform.Find("tilesColorRoot")?.gameObject;
        if (_value == "true")
        {
            if (!root)
            {
                root = new GameObject("tilesColorRoot");
                root.transform.parent = transform;
                root.transform.localPosition = usableZone.localPosition;
                root.transform.localPosition += new Vector3(GameManager.instance.tileSize, 0.002f, GameManager.instance.tileSize) / 2;
                root.transform.localEulerAngles = Vector3.zero;
                LoopThroughTiles("color", root.transform);
            }
        }
        else
        {
            if (root)
                Destroy(root);
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }
    public void ToggleTilesColor()
    {
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
        GameObject root = transform.Find("tilesColorRoot")?.gameObject;
        if (root)
        {
            root.SetActive(false); // for UI
            Destroy(root);
        }
        else
        {
            root = new GameObject("tilesColorRoot");
            root.transform.parent = transform;
            root.transform.localPosition = usableZone.localPosition;
            root.transform.localPosition += new Vector3(GameManager.instance.tileSize, 0.002f, GameManager.instance.tileSize) / 2;
            root.transform.localEulerAngles = Vector3.zero;
            LoopThroughTiles("color", root.transform);
        }
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }

    ///<summary>
    /// Loop through every tile placement and populate name or plane according to _mode.
    ///</summary>
    ///<param name="_mode">"name" or "color" mode</param>
    ///<param name="_root">Root for choosen mode</param>
    private void LoopThroughTiles(string _mode, Transform _root)
    {
        Vector2 orient = Vector2.one;
        int offsetX = 0;
        int offsetY = 0;
        if (attributes["axisOrientation"] == "+x+y")
        {
            // Lower Left   
            orient = new Vector2(1, 1);
            offsetX = (int)-reserved.left;
            offsetY = (int)-reserved.bottom;
        }
        else if (attributes["axisOrientation"] == "-x+y")
        {
            // Lower Right
            orient = new Vector2(-1, 1);
            offsetX = (int)-reserved.right;
            offsetY = (int)-reserved.bottom;
            _root.transform.localPosition -= new Vector3(GameManager.instance.tileSize, 0, 0);
        }
        else if (attributes["axisOrientation"] == "-x-y")
        {
            // Upper Right
            orient = new Vector2(-1, -1);
            offsetX = (int)-reserved.right;
            offsetY = (int)-reserved.top;
            _root.transform.localPosition -= new Vector3(GameManager.instance.tileSize, 0, GameManager.instance.tileSize);
        }
        else if (attributes["axisOrientation"] == "+x-y")
        {
            // Upper Left
            orient = new Vector2(1, -1);
            offsetX = (int)-reserved.left;
            offsetY = (int)-reserved.top;
            _root.transform.localPosition -= new Vector3(0, 0, GameManager.instance.tileSize);
        }

        if (isSquare)
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(attributes["size"]);
            float x = size.x / GameManager.instance.tileSize - technical.right - technical.left + offsetX;
            float y = size.y / GameManager.instance.tileSize - technical.top - technical.bottom + offsetY;
            Vector3 origin = usableZone.localScale / 0.2f;
            _root.transform.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);

            for (int j = offsetY; j < y; j++)
            {
                for (int i = offsetX; i < x; i++)
                {
                    Vector2 pos = new Vector2(i, j) * orient * GameManager.instance.tileSize;

                    string tileID = $"{i}/{j}";
                    if (_mode == "name")
                        GenerateTileName(_root, pos, tileID);
                    else if (_mode == "color")
                        GenerateTileColor(_root, pos, tileID);
                }
            }
        }
        else
        {
            List<STile> tiles = JsonConvert.DeserializeObject<List<STile>>(attributes["tiles"]);
            foreach (STile tile in tiles)
            {
                string[] splittedLoc = tile.location.Split('/');
                float tileX = Utils.ParseDecFrac(splittedLoc[0]);
                float tileY = Utils.ParseDecFrac(splittedLoc[1]);
                Vector2 pos = new Vector2(tileX, tileY) * orient * GameManager.instance.tileSize;
                if (_mode == "name")
                    GenerateTileName(_root, pos, tile.location);
                else if (_mode == "color")
                    GenerateTileColor(_root, pos, tile.location);
            }
        }
    }

    ///<summary>
    /// Instantiate a tileNameModel from gm and assign a name to it.
    ///</summary>
    ///<param name="_root">The root to parent the tile</param>
    ///<param name="_pos">The position of the current tile</param>
    ///<param name="_id">The id of the current tile</param>
    private void GenerateTileName(Transform _root, Vector2 _pos, string _id)
    {
        // Select the right tile from attributes["tiles"]
        STile tileData = new STile();
        if (attributes.ContainsKey("tiles"))
        {
            List<STile> tiles = JsonConvert.DeserializeObject<List<STile>>(attributes["tiles"]);
            foreach (STile tile in tiles)
            {
                if (tile.location.Trim() == _id)
                    tileData = tile;
            }
        }

        GameObject tileText = Instantiate(GameManager.instance.tileNameModel);
        tileText.name = $"Text_{_id}";
        tileText.transform.SetParent(_root);
        tileText.transform.localPosition = new Vector3(_pos.x, 0, _pos.y);
        tileText.transform.localEulerAngles = new Vector3(90, 0, 0);

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
    private void GenerateTileColor(Transform _root, Vector2 _pos, string _id)
    {
        // Select the right tile from attributes["tiles"]
        STile tileData = new STile();
        if (attributes.ContainsKey("tiles"))
        {
            List<STile> tiles = JsonConvert.DeserializeObject<List<STile>>(attributes["tiles"]);
            foreach (STile tile in tiles)
            {
                if (tile.location.Trim() == _id)
                    tileData = tile;
            }
        }

        List<SColor> customColors = new List<SColor>();
        if (attributes.ContainsKey("customColors"))
            customColors = JsonConvert.DeserializeObject<List<SColor>>(attributes["customColors"]);

        if (!string.IsNullOrEmpty(tileData.location))
        {
            if (!string.IsNullOrEmpty(tileData.texture) || !string.IsNullOrEmpty(tileData.color))
            {
                GameObject tile = Instantiate(GameManager.instance.tileModel);
                tile.name = $"Color_{_id}";
                tile.transform.parent = _root;
                tile.transform.localPosition = new Vector3(_pos.x, 0, _pos.y);
                tile.transform.localEulerAngles = new Vector3(0, 180, 0);
                if (!string.IsNullOrEmpty(tileData.texture))
                {
                    Renderer rend = tile.GetComponent<Renderer>();
                    if (GameManager.instance.textures.ContainsKey(tileData.texture))
                    {
                        rend.material = new Material(GameManager.instance.perfMat)
                        {
                            mainTexture = GameManager.instance.textures[tileData.texture]
                        };
                    }
                    else
                        GameManager.instance.AppendLogLine($"Unknow texture: {tileData.texture}", false, ELogtype.warning);
                }
                if (!string.IsNullOrEmpty(tileData.color))
                {
                    Material mat = tile.GetComponent<Renderer>().material;
                    Color customColor = new Color();
                    if (tileData.color.StartsWith("@"))
                    {
                        foreach (SColor color in customColors)
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
        _zone.localScale -= new Vector3(0, 0, _dim.top) * GameManager.instance.tileSize / 10;
        _zone.localPosition -= new Vector3(0, 0, _dim.top) * GameManager.instance.tileSize / 2;

        _zone.localScale -= new Vector3(0, 0, _dim.bottom) * GameManager.instance.tileSize / 10;
        _zone.localPosition += new Vector3(0, 0, _dim.bottom) * GameManager.instance.tileSize / 2;

        _zone.localScale -= new Vector3(_dim.right, 0, 0) * GameManager.instance.tileSize / 10;
        _zone.localPosition -= new Vector3(_dim.right, 0, 0) * GameManager.instance.tileSize / 2;

        _zone.localScale -= new Vector3(_dim.left, 0, 0) * GameManager.instance.tileSize / 10;
        _zone.localPosition += new Vector3(_dim.left, 0, 0) * GameManager.instance.tileSize / 2;
    }

    ///<summary>
    /// Set usable/reserved/technical zones color according to parented Site
    ///</summary>
    public void UpdateZonesColor()
    {
        OgreeObject site = null;
        if (transform.parent && transform.parent.parent)
            site = transform.parent.parent.GetComponentInParent<OgreeObject>();

        if (site && site.attributes.ContainsKey("usableColor"))
            usableZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["usableColor"]}");
        else
            usableZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configLoader.GetColor("usableZone"));

        if (site && site.attributes.ContainsKey("reservedColor"))
            reservedZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["reservedColor"]}");
        else
            reservedZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configLoader.GetColor("reservedZone"));

        if (site && site.attributes.ContainsKey("technicalColor"))
            technicalZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["technicalColor"]}");
        else
            technicalZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configLoader.GetColor("technicalZone"));
    }

    ///<summary>
    /// Place the given separator in the room.
    ///</summary>
    ///<param name="_sep">The separator to draw</param>
    public void BuildSeparator(SSeparator _sep)
    {
        Vector2 startPos = new Vector2(_sep.startPosXYm[0], _sep.startPosXYm[1]);
        Vector2 endPos = new Vector2(_sep.endPosXYm[0], _sep.endPosXYm[1]);

        float length = Vector2.Distance(startPos, endPos);
        float height = Utils.ParseDecFrac(attributes["height"]);
        float angle = Vector3.SignedAngle(Vector3.right, endPos - startPos, Vector3.up);

        GameObject separator = Instantiate(GameManager.instance.separatorModel);
        separator.transform.parent = walls;

        // Set textured box
        separator.transform.GetChild(0).localScale = new Vector3(length, height, 0.001f);
        separator.transform.GetChild(0).localPosition = new Vector3(length, height, 0) / 2;
        Renderer rend = separator.transform.GetChild(0).GetComponent<Renderer>();
        if (_sep.type == "wireframe")
            rend.material.mainTextureScale = new Vector2(length, height) * 1.5f;
        else
            rend.material = GameManager.instance.defaultMat;

        // Place the separator in the right place
        if (technicalZone)
        {
            Vector3 roomScale = technicalZone.localScale * -5;
            separator.transform.localPosition = new Vector3(roomScale.x, 0, roomScale.z);
        }
        else
            separator.transform.localPosition = Vector3.zero;

        // Apply wanted transform
        separator.transform.localPosition += new Vector3(startPos.x, 0, startPos.y);
        separator.transform.localEulerAngles = new Vector3(0, -angle, 0);
    }

    ///<summary>
    /// Place the given pillar in the room.
    ///</summary>
    ///<param name="_pil">The pillar to draw</param>
    public void BuildPillar(SPillar _pil)
    {
        float height = Utils.ParseDecFrac(attributes["height"]);

        GameObject pillar = Instantiate(GameManager.instance.pillarModel);
        pillar.transform.parent = walls;

        pillar.transform.localScale = new Vector3(_pil.sizeXY[0], height, _pil.sizeXY[1]);

        // Place the pillar in the right place
        if (technicalZone)
        {
            Vector3 roomScale = technicalZone.localScale * -5;
            pillar.transform.localPosition = new Vector3(roomScale.x, 0, roomScale.z);
        }
        else
            pillar.transform.localPosition = Vector3.zero;

        pillar.transform.localPosition += new Vector3(_pil.centerXY[0], height / 2, _pil.centerXY[1]);
        pillar.transform.localEulerAngles = new Vector3(0, _pil.rotation, 0);
    }
}
