using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : Building
{
    public SMargin reserved;
    public SMargin technical;

    [Header("Floor layers")]
    public Transform usableZone;
    public Transform reservedZone;
    public Transform technicalZone;
    public Transform tilesGrid;

    [Header("Room references")]
    public string temperatureUnit;
    public bool tileName = false;
    public bool tileColor = false;

    public bool barChart = false;
    public List<Group> openedGroups = new();
    public List<Separator> separators = new();
    public bool sepNamesDisplayed = false;
    public bool genNamesDisplayed = false;
    public GameObject childrenOrigin;

    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if (HasAttributeChanged(_src, "separators"))
        {
            Dictionary<string, SSeparator> newSeparators = ((JObject)_src.attributes["separators"]).ToObject<Dictionary<string, SSeparator>>();
            // Delete old separators
            for (int i = 0; i < separators.Count; i++)
            {
                Separator sep = separators[i];
                if (!newSeparators.ContainsKey(sep.name))
                {
                    Destroy(sep.gameObject);
                    separators.Remove(sep);
                }
            }
            // Add new separators
            foreach (KeyValuePair<string, SSeparator> sep in newSeparators)
                if (!HasSeparator(sep.Key))
                    BuildSeparator(new SSeparator(sep.Key, sep.Value));
            UpdateColorByDomain(_src.domain);
        }

        if (HasAttributeChanged(_src, "pillars"))
        {
            foreach (Transform wall in walls)
                if (wall.name.Contains("Pillar"))
                    Destroy(wall.gameObject);
            Dictionary<string, SPillar> pillars = ((JObject)_src.attributes["pillars"]).ToObject<Dictionary<string, SPillar>>();
            foreach (KeyValuePair<string, SPillar> pillar in pillars)
                BuildPillar(new SPillar(pillar.Key, pillar.Value));
            UpdateColorByDomain(_src.domain);
        }

        if ((!Utils.HasKeyAndValue(_src.attributes, "template") || (Utils.HasKeyAndValue(_src.attributes, "template") && GameManager.instance.roomTemplates[(string)_src.attributes["template"]] is SRoomFromJson template && template.vertices == null))
            && HasAttributeChanged(_src, "reserved") && HasAttributeChanged(_src, "technical"))
        {
            SMargin reserved = new(((JArray)_src.attributes["reserved"]).ToObject<List<float>>().ToVector4());
            SMargin technical = new(((JArray)_src.attributes["technical"]).ToObject<List<float>>().ToVector4());
            SetAreas(reserved, technical);
        }

        base.UpdateFromSApiObject(_src);
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
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Can't modify areas", id), ELogTarget.both, ELogtype.error);
            return;
        }
        tilesGrid.gameObject.SetActive(true);

        reserved = new(_resDim);
        technical = new(_techDim);

        // Reset  ->  techzone is always full size of a room
        usableZone.localScale = technicalZone.localScale;
        usableZone.localPosition = new(technicalZone.localPosition.x, usableZone.localPosition.y, technicalZone.localPosition.z);
        reservedZone.localScale = technicalZone.localScale;
        reservedZone.localPosition = new(technicalZone.localPosition.x, reservedZone.localPosition.y, technicalZone.localPosition.z);

        // If tileOffset in template, apply it
        if (attributes.HasKeyAndValue("template") && GameManager.instance.roomTemplates.ContainsKey((string)attributes["template"]))
        {
            SRoomFromJson template = GameManager.instance.roomTemplates[(string)attributes["template"]];
            if (template.tileOffset != null && template.tileOffset.Count != 0)
            {
                // Find the part to substract to land on a whole tile
                float sizeX = technicalZone.localScale.x * 10 - template.tileOffset[0];
                float sizeY = technicalZone.localScale.z * 10 - template.tileOffset[1];
                Vector3 delta = new(sizeX % UnitValue.Tile, 0, sizeY % UnitValue.Tile);

                ApplyTileOffset(usableZone, template.tileOffset, delta);
                ApplyTileOffset(reservedZone, template.tileOffset, delta);
                tilesGrid.GetComponent<Renderer>().material.mainTextureOffset += new Vector2(template.tileOffset[0], template.tileOffset[1]) / UnitValue.Tile;
            }
        }

        // Reduce zones
        ReduceZone(reservedZone, _techDim);
        ReduceZone(usableZone, _techDim);
        ReduceZone(usableZone, _resDim);
    }

    ///<summary>
    /// Apply given _offset to a _zone
    ///</summary>
    ///<param name="_zone">The zone to modify</param>
    ///<param name="_offset">The offset to apply</param>
    private void ApplyTileOffset(Transform _zone, List<float> _offset, Vector3 _delta)
    {
        // Move the _zone
        _zone.localPosition += new Vector3(_offset[0], 0, _offset[1]);
        // _zone overflow X
        if (_zone.localScale.x * 10 + Mathf.Abs(_offset[0]) > technicalZone.localScale.x * 10)
        {
            _zone.localScale -= new Vector3(Mathf.Abs(_offset[0]), 0, 0) / 10;
            _zone.localPosition -= new Vector3(_offset[0], 0, 0) / 2;
        }
        // _zone overflow Y
        if (_zone.localScale.z * 10 + Mathf.Abs(_offset[1]) > technicalZone.localScale.z * 10)
        {
            _zone.localScale -= new Vector3(0, 0, Mathf.Abs(_offset[1])) / 10;
            _zone.localPosition -= new Vector3(0, 0, _offset[1]) / 2;
        }
        // Rescale and move to have the whole tile
        _zone.localScale -= _delta / 10;
        _zone.localPosition -= _delta / 2;
    }

    ///<summary>
    /// Toggle tiles name.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesName(bool _value)
    {
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
        if (isSquare)
        {
            GameObject root = transform.Find("tilesNameRoot")?.gameObject;
            if (_value && !root)
                BuildTilesName();
            else if (!_value && root)
                root.CleanDestroy("Logs", "Hide tiles name for", name);
        }
        else
        {
            Transform root = transform.Find("Floor");
            if (root)
            {
                foreach (Transform tile in root)
                    tile.GetChild(0).GetComponent<MeshRenderer>().enabled = _value;
                nameText.enabled = !_value;
            }
        }
        tileName = _value;
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
    }

    ///<summary>
    /// Toggle tiles name.
    ///</summary>
    public void ToggleTilesName()
    {
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
        if (isSquare)
        {
            GameObject root = transform.Find("tilesNameRoot")?.gameObject;
            if (root)
                root.CleanDestroy("Logs", "Hide tiles name for", name);
            else
                BuildTilesName();
        }
        else
        {
            Transform root = transform.Find("Floor");
            if (root)
            {
                foreach (Transform tile in root)
                    tile.GetChild(0).GetComponent<MeshRenderer>().enabled ^= true; //toggle bool : bool1 ^= true <=> bool1 = !bool1           
                nameText.enabled = !nameText.enabled;
            }
        }
        tileName = !tileName;
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
    }

    ///<summary>
    /// Create the root for tiles name, then all tiles name
    ///</summary>
    private void BuildTilesName()
    {
        GameObject root = new("tilesNameRoot");
        root.transform.parent = transform;
        root.transform.localPosition = usableZone.localPosition;
        root.transform.localPosition += new Vector3(UnitValue.Tile, 0.003f, UnitValue.Tile) / 2;
        root.transform.localEulerAngles = Vector3.zero;
        LoopThroughTiles("name", root.transform);
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Display tiles name for", name), ELogTarget.logger, ELogtype.success);
    }

    ///<summary>
    /// Toggle tiles colors and textures.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleTilesColor(bool _value)
    {
        if (!GameManager.instance.roomTemplates.ContainsKey((string)attributes["template"]))
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "There is no template for", name), ELogTarget.logger, ELogtype.warning);
            return;
        }

        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
        if (isSquare)
        {
            GameObject root = transform.Find("tilesColorRoot")?.gameObject;
            if (_value && !root)
                BuildTilesColor();
            else if (!_value && root)
                root.CleanDestroy("Logs", "Hide tiles color for", name);
        }
        else
        {
            Transform root = transform.Find("Floor");
            if (root)
            {
                List<SColor> customColors = new();
                if (attributes.ContainsKey("colors"))
                    customColors = (List<SColor>)attributes["colors"];
                foreach (Transform tileObj in root)
                {
                    Tile tile = tileObj.GetComponent<Tile>();
                    if ((_value && tile.modified) || (!_value && !tile.modified))
                        continue;

                    if (!_value)
                    {
                        tile.GetComponent<Renderer>().material = new(tile.defaultMat);
                        tile.modified = false;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(tile.texture))
                        tile.SetTexture(id);
                    if (!string.IsNullOrEmpty(tile.color))
                        tile.SetColor(customColors);

                    tile.modified = true;
                }
            }
        }
        tileColor = _value;
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
    }

    ///<summary>
    /// Toggle tiles colors and textures.
    ///</summary>
    public void ToggleTilesColor()
    {
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Loading));
        if (isSquare)
        {
            GameObject root = transform.Find("tilesColorRoot")?.gameObject;
            if (root)
                root.CleanDestroy("Logs", "Hide tiles color for", name);
            else
                BuildTilesColor();
        }
        else
        {
            Transform root = transform.Find("Floor");
            if (root)
            {
                List<SColor> customColors = new();
                if (attributes.ContainsKey("colors"))
                    customColors = (List<SColor>)attributes["colors"];
                foreach (Transform tileObj in root)
                {
                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile.modified)
                    {
                        tile.GetComponent<Renderer>().material = new Material(tile.defaultMat);
                        tile.modified = false;
                        continue;
                    }

                    if (!string.IsNullOrEmpty(tile.texture))
                        tile.SetTexture(id);
                    if (!string.IsNullOrEmpty(tile.color))
                        tile.SetColor(customColors);

                    tile.modified = true;
                }
            }
        }
        tileColor = !tileColor;
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
    }

    ///<summary>
    /// Create the root for tiles color, then all tiles color
    ///</summary>
    private void BuildTilesColor()
    {
        GameObject root = new("tilesColorRoot");
        root.transform.parent = transform;
        root.transform.localPosition = usableZone.localPosition;
        root.transform.localPosition += new Vector3(UnitValue.Tile, 0.002f, UnitValue.Tile) / 2;
        root.transform.localEulerAngles = Vector3.zero;
        LoopThroughTiles("color", root.transform);
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Display tiles color for", name), ELogTarget.logger, ELogtype.success);
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
        switch (attributes["axisOrientation"])
        {
            case AxisOrientation.Default:
                // Lower Left   
                orient = new(1, 1);
                offsetX = (int)-reserved.left;
                offsetY = (int)-reserved.back;
                break;

            case AxisOrientation.XMinus:
                // Lower Right
                orient = new(-1, 1);
                offsetX = (int)-reserved.right;
                offsetY = (int)-reserved.back;
                _root.transform.localPosition -= new Vector3(UnitValue.Tile, 0, 0);
                break;

            case AxisOrientation.YMinus:
                // Upper Left
                orient = new(1, -1);
                offsetX = (int)-reserved.left;
                offsetY = (int)-reserved.front;
                _root.transform.localPosition -= new Vector3(0, 0, UnitValue.Tile);
                break;

            case AxisOrientation.BothMinus:
                // Upper Right
                orient = new(-1, -1);
                offsetX = (int)-reserved.right;
                offsetY = (int)-reserved.front;
                _root.transform.localPosition -= new Vector3(UnitValue.Tile, 0, UnitValue.Tile);
                break;
        }

        Vector2 size = (Vector2)attributes["size"];
        float x = size.x / UnitValue.Tile - technical.right - technical.left + offsetX;
        float y = size.y / UnitValue.Tile - technical.front - technical.back + offsetY;
        Vector3 origin = usableZone.localScale / 0.2f;
        _root.transform.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);

        for (int j = offsetY; j < y; j++)
        {
            for (int i = offsetX; i < x; i++)
            {
                Vector2 pos = new Vector2(i, j) * orient * UnitValue.Tile;

                string tileID = $"{i}/{j}";
                if (_mode == "name")
                    GenerateTileName(_root, pos, tileID);
                else if (_mode == "color")
                    GenerateTileColor(_root, pos, tileID);
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
        STile tileData = new();
        if (attributes.ContainsKey("tiles"))
        {
            List<STile> tiles = (List<STile>)attributes["tiles"];
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
        STile tileData = new();
        if (attributes.ContainsKey("tiles"))
        {
            List<STile> tiles = (List<STile>)attributes["tiles"];
            foreach (STile tile in tiles)
            {
                if (tile.location.Trim() == _id)
                    tileData = tile;
            }
        }

        List<SColor> customColors = new();
        if (attributes.ContainsKey("colors"))
            customColors = (List<SColor>)attributes["colors"];

        if (!string.IsNullOrEmpty(tileData.location))
        {
            if (!string.IsNullOrEmpty(tileData.texture) || !string.IsNullOrEmpty(tileData.color))
            {
                GameObject tile = Instantiate(GameManager.instance.tileModel);
                tile.name = $"Color_{_id}";
                tile.transform.parent = _root;
                tile.transform.localPosition = new(_pos.x, 0, _pos.y);
                tile.transform.localEulerAngles = new(0, 180, 0);
                if (!string.IsNullOrEmpty(tileData.texture))
                {
                    Renderer rend = tile.GetComponent<Renderer>();
                    if (GameManager.instance.textures.ContainsKey(tileData.texture))
                    {
                        rend.material = new(GameManager.instance.perfMat)
                        {
                            mainTexture = GameManager.instance.textures[tileData.texture]
                        };
                    }
                    else
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Unknow tile texture", new List<string>() { id, tileData.texture }), ELogTarget.logger, ELogtype.warning);
                }
                if (!string.IsNullOrEmpty(tileData.color))
                {
                    Material mat = tile.GetComponent<Renderer>().material;
                    Color customColor = new();
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
        _zone.localScale -= UnitValue.Tile * 0.1f * new Vector3(_dim.right + _dim.left, 0, _dim.front + _dim.back);
        _zone.localPosition -= UnitValue.Tile * 0.5f * new Vector3(_dim.right - _dim.left, 0, _dim.front - _dim.back);
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
            usableZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("usableZone"));

        if (site && site.attributes.ContainsKey("reservedColor"))
            reservedZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["reservedColor"]}");
        else
            reservedZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("reservedZone"));

        if (site && site.attributes.ContainsKey("technicalColor"))
            technicalZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["technicalColor"]}");
        else
            technicalZone.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("technicalZone"));
    }

    ///<summary>
    /// Place the given separator in the room.
    ///</summary>
    ///<param name="_sep">The separator to draw</param>
    public void BuildSeparator(SSeparator _sep)
    {
        Vector2 startPos = new(_sep.startPosXYm[0], _sep.startPosXYm[1]);
        Vector2 endPos = new(_sep.endPosXYm[0], _sep.endPosXYm[1]);

        if (startPos.x < endPos.x)
            (startPos, endPos) = (endPos, startPos);

        float length = Vector2.Distance(startPos, endPos);
        float height = walls.GetChild(0).localScale.y;
        float angle = Vector3.SignedAngle(Vector3.right, endPos - startPos, Vector3.up);

        GameObject separator = Instantiate(GameManager.instance.separatorModel);
        separator.name = _sep.name;
        separator.transform.parent = walls;

        // Set textured box
        separator.transform.GetChild(0).localScale = new(length, height, 0.001f);
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
            separator.transform.localPosition = new(roomScale.x, 0, roomScale.z);
        }
        else
            separator.transform.localPosition = Vector3.zero;

        // Apply wanted transform
        separator.transform.localPosition += new Vector3(startPos.x, 0, startPos.y);
        separator.transform.localEulerAngles = new(0, -angle, 0);

        Separator sep = separator.GetComponent<Separator>();
        sep.Initialize();
        sep.ToggleTexts(sepNamesDisplayed);
        separators.Add(sep);
    }

    ///<summary>
    /// Place the given pillar in the room.
    ///</summary>
    ///<param name="_pil">The pillar to draw</param>
    public void BuildPillar(SPillar _pil)
    {
        float height = walls.GetChild(0).localScale.y;

        GameObject pillar = Instantiate(GameManager.instance.pillarModel);
        pillar.name += $"_{_pil.name}";
        pillar.transform.parent = walls;

        pillar.transform.localScale = new(_pil.sizeXY[0], height, _pil.sizeXY[1]);

        // Place the pillar in the right place
        if (technicalZone)
        {
            Vector3 roomScale = technicalZone.localScale * -5;
            pillar.transform.localPosition = new(roomScale.x, 0, roomScale.z);
        }
        else
            pillar.transform.localPosition = Vector3.zero;

        pillar.transform.localPosition += new Vector3(_pil.centerXY[0], height / 2, _pil.centerXY[1]);
        pillar.transform.localEulerAngles = new(0, _pil.rotation, 0);
    }

    /// <summary>
    /// Toggle texts of each separator in <see cref="separators"/>.
    /// </summary>
    public void ToggleSeparatorText()
    {
        sepNamesDisplayed ^= true;
        foreach (Separator sep in separators)
            sep.ToggleTexts(sepNamesDisplayed);
    }

    /// <summary>
    /// Toggle texts of each generic object in the room>.
    /// </summary>
    public void ToggleGenericText()
    {
        genNamesDisplayed ^= true;
        foreach (Transform child in transform)
            if (child.TryGetComponent(out GenericObject _))
                child.GetComponent<DisplayObjectData>().ToggleLabel(genNamesDisplayed);
    }

    /// <summary>
    /// Toggle walls, separators and pillars Renderer & Collider according to <see cref="displayWalls"/>.
    /// </summary>
    public override void ToggleWalls()
    {
        base.ToggleWalls();
        foreach (Separator sep in separators)
            sep.ToggleTexts(displayWalls && sepNamesDisplayed);
    }

    /// <summary>
    /// Tell if a separator is in this room.
    /// </summary>
    /// <param name="_name">The name to search for</param>
    /// <returns>True if a separator has wanted name</returns>
    public bool HasSeparator(string _name)
    {
        foreach (Separator sep in separators)
        {
            if (sep.name == _name)
                return true;
        }
        return false;
    }

    public void ComputeChildrenOrigin()
    {
        childrenOrigin = Instantiate(GameManager.instance.childOriginModel, gameObject.transform);
        switch (attributes["axisOrientation"])
        {
            case AxisOrientation.XMinus:
                childrenOrigin.transform.GetChild(0).GetChild(0).localPosition *= -1;
                if (isSquare)
                    childrenOrigin.transform.position += technicalZone.localScale.x * 10 * transform.TransformDirection(Vector3.right);
                break;
            case AxisOrientation.YMinus:
                childrenOrigin.transform.GetChild(0).GetChild(1).localPosition *= -1;
                if (isSquare)
                    childrenOrigin.transform.position += technicalZone.localScale.z * 10 * transform.TransformDirection(Vector3.forward);
                break;
            case AxisOrientation.BothMinus:
                childrenOrigin.transform.GetChild(0).GetChild(0).localPosition *= -1;
                childrenOrigin.transform.GetChild(0).GetChild(1).localPosition *= -1;
                if (isSquare)
                    childrenOrigin.transform.position += 10 * (technicalZone.localScale.z * transform.TransformDirection(Vector3.forward) + technicalZone.localScale.x * transform.TransformDirection(Vector3.right));
                break;
            default:
                break;
        }
        childrenOrigin.transform.localScale *= 7;
        childrenOrigin.SetActive(false);
    }
}
