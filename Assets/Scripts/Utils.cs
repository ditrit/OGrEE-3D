using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class Utils
{
    ///<summary>
    /// Parse a string with format "[x,y]" into a Vector2.
    ///</summary>
    ///<param name="_input">String with format "[x,y]"</param>
    ///<returns>The parsed Vector2</returns>
    public static Vector2 ParseVector2(string _input)
    {
        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        return new(ParseDecFrac(parts[0]), ParseDecFrac(parts[1]));
    }

    ///<summary>
    /// Parse a string with format "[x,y,z]" into a Vector3. The vector can be given in Y axis or Z axis up.
    ///</summary>
    ///<param name="_input">String with format "[x,y,z]"</param>
    ///<param name="_ZUp">Is the coordinates given are in Z axis up or Y axis up ? </param>
    ///<returns>The parsed Vector3</returns>
    public static Vector3 ParseVector3(string _input, bool _ZUp = true)
    {
        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        if (_ZUp)
            return new(ParseDecFrac(parts[0]), ParseDecFrac(parts[2]), ParseDecFrac(parts[1]));
        else
            return new(ParseDecFrac(parts[0]), ParseDecFrac(parts[1]), ParseDecFrac(parts[2]));
    }

    ///<summary>
    /// Parse a string with format "[x,y,z,w]" into a Vector4.
    ///</summary>
    ///<param name="_input">String with format "[x,y,z,w]"</param>
    ///<returns>The parsed Vector4</returns>
    public static Vector4 ParseVector4(string _input)
    {
        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        return new(ParseDecFrac(parts[0]), ParseDecFrac(parts[1]), ParseDecFrac(parts[2]), ParseDecFrac(parts[3]));
    }

    ///<summary>
    /// Parse a string into a float. Can be decimal, a fraction and/or negative.
    ///</summary>
    ///<param name="_input">The string which contains the float</param>
    ///<returns>The parsed float</returns>
    public static float ParseDecFrac(string _input)
    {
        _input = _input.Replace(" ", "");
        _input = _input.Replace(",", ".");
        if (_input.Contains("/"))
        {
            string[] div = _input.Split('/');
            float a = float.Parse(div[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            float b = float.Parse(div[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            return a / b;
        }
        else
            return float.Parse(_input, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
    }

    ///<summary>
    /// Tries to return given <see cref="Transform"/>, otherwise look for given parent Id
    ///</summary>
    ///<param name="_parent">The Transform to check</param>
    ///<param name="_parentId">The ID to search</param>
    ///<returns>A valid Transform or null</returns>
    public static Transform FindParent(Transform _parent, string _parentId)
    {
        Transform parent = null;
        if (_parent)
            parent = _parent;
        else
        {
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                GameObject go = (GameObject)de.Value;
                if (go && go.GetComponent<OgreeObject>().id == _parentId)
                    parent = go.transform;
            }
        }
        return parent;
    }

    ///<summary>
    /// Cast a raycast from main camera and returns hit object
    ///</summary>
    ///<returns>Hit GameObject or null</returns>
    public static GameObject RaycastFromCameraToMouse()
    {
        Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit);
        if (hit.collider)
            return hit.collider.transform.parent.gameObject;
        else
            return null;
    }

    ///<summary>
    /// Get an object from <see cref="GameManager.allItems"/> by it's id.
    ///</summary>
    ///<param name="_id">The id to search</param>
    ///<returns>The asked object</returns>
    public static GameObject GetObjectById(string _id)
    {
        if (!string.IsNullOrEmpty(_id))
        {
            if (GameManager.instance.allItems.Contains(_id))
                return (GameObject)GameManager.instance.allItems[_id];
            else
                return null;
        }
        return null;
    }

    ///<summary>
    /// Get a list of objects from GameManager.allItems by their id.
    ///</summary>
    ///<param name="_idList">The array of ids to search</param>
    ///<returns>the asked list of objects</returns>
    public static List<GameObject> GetObjectsById(List<string> _idList)
    {
        List<GameObject> objects = new();
        foreach (string objId in _idList)
        {
            if (GameManager.instance.allItems.Contains(objId))
                objects.Add((GameObject)GameManager.instance.allItems[objId]);
        }
        return objects;
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_str">The hexadecimal value, without '#'</param>
    ///<returns>The wanted color</returns>
    public static Color ParseHtmlColor(string _str)
    {
        ColorUtility.TryParseHtmlString(_str, out Color newColor);
        return newColor;
    }

    ///<summary>
    /// Get a lightest version of the inverted color.
    ///</summary>
    ///<param name="_color">The base color</param>
    ///<returns>The new color</returns>
    public static Color InvertColor(Color _color)
    {
        float max = _color.maxColorComponent;
        return new(max - _color.r / 3, max - _color.g / 3, max - _color.b / 3, _color.a);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_physicalList">The list of physical objects to complete</param>
    ///<param name="_logicalList">The list of logical objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    ///<param name="_leafIds">The list of leaf IDs to complete</param>
    public static void ParseNestedObjects(List<SApiObject> _physicalList, List<SApiObject> _logicalList, SApiObject _src, List<string> _leafIds)
    {
        if (_src.category == Category.Group)
            _logicalList.Add(_src);
        else
            _physicalList.Add(_src);
        if (_src.children != null)
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_physicalList, _logicalList, obj, _leafIds);
        }
        else
            _leafIds.Add(_src.id);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_list">The list of objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    ///<param name="_leafIds">The list of leaf IDs to complete</param>
    public static void ParseNestedObjects(List<SApiObject> _list, SApiObject _src, List<string> _leafIds)
    {
        _list.Add(_src);
        if (_src.children != null)
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_list, obj, _leafIds);
        }
        else
            _leafIds.Add(_src.id);
    }

    ///<summary>
    /// Check is the given OgreeObject has been moved, rotated or rescaled.
    ///</summary>
    ///<param name="_obj">The object to check</param>
    ///<returns>True or false</returns>
    public static bool IsObjectMoved(OgreeObject _obj)
    {
        if (_obj.originalLocalPosition != _obj.transform.localPosition)
            return true;
        if (_obj.originalLocalRotation != _obj.transform.localRotation)
            return true;
        if (_obj.originalLocalScale != _obj.transform.localScale)
            return true;
        return false;
    }

    /// <summary>
    /// Compute the signed volume of a pyramid from a Mesh
    /// </summary>
    /// <param name="_p1">First corner of the pyramid</param>
    /// <param name="_p2">Second corner of the pyramid</param>
    /// <param name="_p3">Third corner of the pyramid</param>
    /// <returns>signed volume of the pyramid</returns>
    public static float SignedVolumeOfPyramid(Vector3 _p1, Vector3 _p2, Vector3 _p3)
    {
        float v321 = _p3.x * _p2.y * _p1.z;
        float v231 = _p2.x * _p3.y * _p1.z;
        float v312 = _p3.x * _p1.y * _p2.z;
        float v132 = _p1.x * _p3.y * _p2.z;
        float v213 = _p2.x * _p1.y * _p3.z;
        float v123 = _p1.x * _p2.y * _p3.z;

        return (-v321 + v231 + v312 - v132 - v213 + v123) / 6;
    }

    /// <summary>
    /// Compute the volume of a mesh by adding the volume of each of its pyramids
    /// </summary>
    /// <param name="_meshFilter">The MeshFilter of the object whose volume is needed</param>
    /// <returns>The volume of the mesh</returns>
    public static float VolumeOfMesh(MeshFilter _meshFilter)
    {
        Mesh mesh = _meshFilter.sharedMesh;
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfPyramid(p1, p2, p3);
        }
        volume *= _meshFilter.transform.localScale.x * _meshFilter.transform.localScale.y * _meshFilter.transform.localScale.z;
        return Mathf.Abs(volume);
    }

    ///<summary>
    /// Map a value from a given range to another range and clamp it.
    ///</summary>
    ///<param name="_input">The value to map</param>
    ///<param name="_inMin">The minimal value of the input range</param>
    ///<param name="_inMax">The maximal value of the input range</param>
    ///<param name="_outMin">The minimal value of the output range</param>
    ///<param name="_outMax">The maximal value of the output range</param>
    ///<returns>The maped and clamped value</returns>
    public static float MapAndClamp(float _input, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        return Mathf.Clamp((_input - _inMin) * (_outMax - _outMin) / (_inMax - _inMin) + _outMin, Mathf.Min(_outMin, _outMax), Mathf.Max(_outMin, _outMax));
    }

    /// <summary>
    /// Returns the referent of an OObject if it's a rack, returns null otherwise
    /// </summary>
    /// <param name="_item">The Oobject whose referent is returned</param>
    /// <returns>the Rack which is the referent of <paramref name="_item"/> or null</returns>
    public static Rack GetRackReferent(Item _item)
    {
        try
        {
            return (Rack)_item.referent;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    ///<summary>
    /// Wait end of frame to raise a ImportFinishedEvent in order to fill FocusHandler lists for the group content.
    ///</summary>
    public static IEnumerator ImportFinished()
    {
        yield return new WaitForEndOfFrame();
        EventManager.instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Loop through parents of given object and set their currentLod.
    ///</summary>
    ///<param name="_leaf">The object to start the loop</param>
    public static void RebuildLods(Transform _leaf)
    {
        Transform parent = _leaf.parent;
        while (parent)
        {
            OgreeObject leafObj = _leaf.GetComponent<OgreeObject>();
            OgreeObject parentObj = parent.GetComponent<OgreeObject>();

            if (leafObj.currentLod >= parentObj.currentLod)
                parentObj.currentLod = leafObj.currentLod + 1;

            _leaf = parent;
            parent = _leaf.parent;
        }
    }

    ///<summary>
    /// Disable _target, destroy it and display given _msg to logger
    ///</summary>
    ///<param name="_target">The object to destroy</param>
    ///<param name="_msg">The message to display (ELogTarget.logger, ELogtype.success)</param>
    public static void CleanDestroy(this GameObject _target, string _msg)
    {
        _target.SetActive(false); //for UI
        Object.Destroy(_target);
        GameManager.instance.AppendLogLine(_msg, ELogTarget.logger, ELogtype.success);
    }

    ///<summary>
    /// Convert a float to a string with "0.##" format
    ///</summary>
    ///<param name="_input">The float to convert</param>
    ///<returns>The converted float</returns>
    public static string FloatToRefinedStr(float _input)
    {
        return _input.ToString("0.##", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Switch y and z value.
    /// </summary>
    /// <param name="_v">The vector to modify</param>
    /// <returns>A Z-Up oriented vector</returns>
    public static Vector3 ZAxisUp(this Vector3 _v)
    {
        return new(_v.x, _v.z, _v.y);
    }

    /// <summary>
    /// Set the alpha of a <paramref name="_color"/>
    /// </summary>
    /// <param name="_color">The color to modify</param>
    /// <param name="_alpha">The alpha to apply</param>
    /// <returns>This color with given <paramref name="_alpha"/></returns>
    public static Color WithAlpha(this Color _color, float _alpha)
    {
        _color.a = _alpha;
        return _color;
    }

    /// <summary>
    /// Check if a <paramref name="_key"/> exists in <paramref name="_dict"/> and if it is neither null or empty 
    /// </summary>
    /// <param name="_dict">The dictionnary to look in</param>
    /// <param name="_key">The key to look for</param>
    /// <returns>True if the key is in the dictionnary and it has a non empty value, otherweise false</returns>
    public static bool HasKeyAndValue(this Dictionary<string, string> _dict, string _key)
    {
        return _dict.ContainsKey(_key) && !string.IsNullOrEmpty(_dict[_key]);
    }

    ///<summary>
    /// Move the given Transform to its position in a room according to the API data.
    ///</summary>
    ///<param name="_obj">The object to move</param>
    ///<param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public static void PlaceInRoom(Transform _obj, SApiObject _apiObj)
    {
        Room parentRoom = _obj.parent.GetComponent<Room>();
        float posXYUnit = GetUnitFromAttributes(_apiObj);
        Vector3 origin;
        if (posXYUnit != UnitValue.Tile && parentRoom.technicalZone) // technicalZone is null for a nonSquareRoom
        {
            _obj.position = parentRoom.technicalZone.position;
            origin = parentRoom.technicalZone.localScale / 0.2f;
        }
        else
        {
            _obj.position = parentRoom.usableZone.position;
            origin = parentRoom.usableZone.localScale / 0.2f;
        }

        Vector2 orient = new();
        if (parentRoom.attributes.ContainsKey("axisOrientation"))
            switch (parentRoom.attributes["axisOrientation"])
            {
                case AxisOrientation.Default:
                    // Lower Left corner of the room
                    orient = new(1, 1);
                    break;

                case AxisOrientation.XMinus:
                    // Lower Right corner of the room
                    orient = new(-1, 1);
                    break;

                case AxisOrientation.YMinus:
                    // Upper Left corner of the room
                    orient = new(1, -1);
                    break;

                case AxisOrientation.BothMinus:
                    // Upper Right corner of the room
                    orient = new(-1, -1);
                    break;
            }

        Vector3 pos;
        if ((_apiObj.category == Category.Rack || _apiObj.category == Category.Corridor || _apiObj.category == Category.Generic) && _apiObj.attributes.ContainsKey("posXYZ"))
            pos = ParseVector3(_apiObj.attributes["posXYZ"], true);
        else
        {
            Vector2 tmp = ParseVector2(_apiObj.attributes["posXY"]);
            pos = new(tmp.x, 0, tmp.y);
        }

        Transform floor = _obj.parent.Find("Floor");
        if (!parentRoom.isSquare && posXYUnit == UnitValue.Tile && floor)
        {
            int trunkedX = (int)pos.x;
            int trunkedZ = (int)pos.z;
            foreach (Transform tileObj in floor)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile.coord.x == trunkedX && tile.coord.y == trunkedZ)
                {
                    _obj.localPosition += new Vector3(tileObj.localPosition.x - 5 * tileObj.localScale.x, pos.y / 100, tileObj.localPosition.z - 5 * tileObj.localScale.z);
                    _obj.localPosition += UnitValue.Tile * new Vector3(orient.x * (pos.x - trunkedX), 0, orient.y * (pos.z - trunkedZ));
                    return;
                }
            }
        }

        // Go to the right corner of the room & apply pos
        if (parentRoom.isSquare)
            _obj.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);

        _obj.localPosition += new Vector3(pos.x * orient.x * posXYUnit, pos.y / 100, pos.z * orient.y * posXYUnit);
        _obj.transform.localEulerAngles = ParseVector3(_apiObj.attributes["rotation"], true);
    }

    ///<summary>
    /// Get a posXYUnit regarding given object's attributes.
    ///</summary>
    ///<param name="_obj">The object to parse</param>
    ///<returns>The posXYUnit, <see cref="UnitValue.Tile"/> by default</returns>
    public static float GetUnitFromAttributes(SApiObject _obj)
    {
        if (!_obj.attributes.ContainsKey("posXYUnit"))
            return UnitValue.Tile;
        return _obj.attributes["posXYUnit"] switch
        {
            LengthUnit.Meter => 1.0f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => UnitValue.Tile,
        };
    }

    /// <summary>
    /// Move the given device to its position in a rack according to the API data.
    /// </summary>
    /// <param name="_parent">The parent object of the device</param>
    /// <param name="_device">The device to be moved</param>
    /// <param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public static void PlaceDevice(Transform _parent, Device _device, SApiObject _apiObj)
    {
        // Check slot
        Transform primarySlot = null;
        Vector3 slotsScale = new();
        List<Slot> takenSlots = new();
        if (_parent && _apiObj.attributes.HasKeyAndValue("slot"))
        {
            string slots = _apiObj.attributes["slot"].Trim('[', ']');
            string[] slotsArray = slots.Split(",");

            foreach (Transform child in _parent)
            {
                if (child.TryGetComponent(out Slot slot))
                {
                    foreach (string slotName in slotsArray)
                    {
                        if (child.name == slotName)
                        {
                            takenSlots.Add(slot);
                            slot.SlotTaken(true);
                        }
                    }
                }
            }
            if (takenSlots.Count > 0)
            {
                SlotsShape(_parent, takenSlots, out Vector3 slotsPivot, out slotsScale);
                primarySlot = takenSlots[0].transform;
                foreach (Slot slot in takenSlots)
                {
                    if (Vector3.Distance(slotsPivot, slot.transform.position) < Vector3.Distance(slotsPivot, primarySlot.position))
                        primarySlot = slot.transform;
                }
            }
            else
            {
                GameManager.instance.AppendLogLine($"One or more slots from {_apiObj.attributes["slot"]} not found in {_parent.name}", ELogTarget.both, ELogtype.error);
                return;
            }
        }
        foreach (Slot s in _device.takenSlots)
            if (!takenSlots.Contains(s))
                s.SlotTaken(false);
        _device.takenSlots = takenSlots;

        Vector2 size;
        float height;
        if (!_apiObj.attributes.HasKeyAndValue("template"))//Rescale according to slot or parent if basic object
        {
            Vector3 scale;
            if (takenSlots.Count > 0)
                scale = new(takenSlots[0].transform.GetChild(0).localScale.x, ParseDecFrac(_apiObj.attributes["height"]) / 1000, takenSlots[0].transform.GetChild(0).localScale.z);
            else
                scale = new(_parent.GetChild(0).localScale.x, ParseDecFrac(_apiObj.attributes["height"]) / 1000, _parent.GetChild(0).localScale.z);
            _device.transform.GetChild(0).localScale = scale;
            _device.transform.GetChild(0).GetComponent<Collider>().enabled = true;

            foreach (Transform child in _device.transform)
                child.localPosition = scale / 2;
            size = new(scale.x, scale.z);
            height = scale.y;
        }
        else
        {
            size = ParseVector2(_apiObj.attributes["size"]) / 1000;
            height = ParseDecFrac(_apiObj.attributes["height"]) / 1000;
        }

        // Place the device
        if (_parent)
        {
            if (_apiObj.attributes.HasKeyAndValue("slot"))
            {
                // parent to slot for applying orientation
                _device.transform.parent = primarySlot;
                _device.transform.localEulerAngles = Vector3.zero;
                _device.transform.localPosition = Vector3.zero;

                float deltaZ = slotsScale.z - size.y;
                switch (_apiObj.attributes["orientation"])
                {
                    case Orientation.Front:
                        _device.transform.localPosition += new Vector3(0, 0, deltaZ);
                        break;
                    case Orientation.Rear:
                        _device.transform.localEulerAngles += new Vector3(0, 180, 0);
                        _device.transform.localPosition += new Vector3(size.x, 0, size.y);
                        break;
                    case Orientation.FrontFlipped:
                        _device.transform.localEulerAngles += new Vector3(0, 0, 180);
                        _device.transform.localPosition += new Vector3(size.x, height, deltaZ);
                        break;
                    case Orientation.RearFlipped:
                        _device.transform.localEulerAngles += new Vector3(180, 0, 0);
                        _device.transform.localPosition += new Vector3(0, height, size.y);
                        break;
                }
                // align device to right side of the slot if invertOffset == true
                if (_apiObj.attributes.ContainsKey("invertOffset") && _apiObj.attributes["invertOffset"] == "true")
                    _device.transform.localPosition += new Vector3(slotsScale.x - size.x, 0, 0);
                // parent back to _parent for good hierarchy 
                _device.transform.parent = _parent;

                if (!_apiObj.attributes.ContainsKey("color"))
                {
                    // if slot, color
                    Color slotColor = primarySlot.GetChild(0).GetComponent<Renderer>().material.color;
                    _device.color = new(slotColor.r, slotColor.g, slotColor.b);
                    _device.GetComponent<ObjectDisplayController>().ChangeColor(slotColor);
                    _device.hasSlotColor = true;
                }
            }
            else
            {
                Vector3 parentShape = _parent.GetChild(0).localScale;
                _device.transform.localEulerAngles = Vector3.zero;
                _device.transform.localPosition = Vector3.zero;
                if (_apiObj.attributes.ContainsKey("posU"))
                    _device.transform.localPosition += new Vector3(0, (ParseDecFrac(_apiObj.attributes["posU"]) - 1) * UnitValue.U, 0);

                float deltaX = parentShape.x - size.x;
                float deltaZ = parentShape.z - size.y;
                _device.transform.localPosition += new Vector3(deltaX / 2, 0, deltaZ);
            }
        }
        else
        {
            _device.transform.localPosition = Vector3.zero;
            _device.transform.localEulerAngles = Vector3.zero;
        }
        // Set labels
        DisplayObjectData dod = _device.GetComponent<DisplayObjectData>();
        if (primarySlot)
            dod.PlaceTexts(primarySlot.GetComponent<Slot>().labelPos);
        else
            dod.PlaceTexts(LabelPos.FrontRear);
    }

    /// <summary>
    /// Get <paramref name="_pivot"/> and <paramref name="_scale"/> of all slots combined
    /// </summary>
    /// <param name="_parent">The parent of the device</param>
    /// <param name="_slotsList">The list of slots to look in</param>
    /// <param name="_pivot">The pivot of the combined slots</param>
    /// <param name="_scale">The scale of the combined slots</param>
    private static void SlotsShape(Transform _parent, List<Slot> _slotsList, out Vector3 _pivot, out Vector3 _scale)
    {
        Quaternion parentRot = _parent.rotation;
        _parent.rotation = Quaternion.identity;

        // x axis
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        // y axis
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        // z axis
        float rear = float.PositiveInfinity;
        float front = float.NegativeInfinity;

        foreach (Slot slot in _slotsList)
        {
            Bounds bounds = slot.transform.GetChild(0).GetComponent<Renderer>().bounds;
            left = Mathf.Min(bounds.min.x, left);
            right = Mathf.Max(bounds.max.x, right);
            bottom = Mathf.Min(bounds.min.y, bottom);
            top = Mathf.Max(bounds.max.y, top);
            rear = Mathf.Min(bounds.min.z, rear);
            front = Mathf.Max(bounds.max.z, front);
        }

        _scale = new(right - left, top - bottom, front - rear);
        _pivot = new(left, bottom, rear);

        _parent.rotation = parentRot;
    }

    /// <summary>
    /// Reshape a group according to its content
    /// </summary>
    /// <param name="_content">The content of the group</param>
    /// <param name="_group">The group</param>
    /// <param name="_parentCategory">The category of the parent of the group (rack or room)</param>
    public static void ShapeGroup(IEnumerable<Transform> _content, Group _group, string _parentCategory)
    {

        // According to group type, set pos, rot & scale
        Vector3 pos = Vector3.zero;
        Vector3 scale = Vector3.zero;
        if (_parentCategory == Category.Room)
        {
            RackGroupPosScale(_content, out pos, out scale);
            _group.transform.localEulerAngles += new Vector3(0, 180, 0);
        }
        else if (_parentCategory == Category.Rack)
        {
            DeviceGroupPosScale(_content, out pos, out scale);
            _group.transform.localEulerAngles = Vector3.zero;
        }
        _group.transform.position = pos;
        _group.transform.GetChild(0).localScale = scale;
    }

    ///<summary>
    /// For a group of Racks, set _pos and _scale
    ///</summary>
    ///<param name="_content">The list of racks or corridors in the group</param>
    ///<param name="_pos">The position to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private static void RackGroupPosScale(IEnumerable<Transform> _content, out Vector3 _pos, out Vector3 _scale)
    {
        // x axis
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        // y axis
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        // z axis
        float rear = float.PositiveInfinity;
        float front = float.NegativeInfinity;

        foreach (Transform obj in _content)
        {
            Bounds bounds = obj.GetChild(0).GetComponent<Renderer>().bounds;
            left = Mathf.Min(bounds.min.x, left);
            right = Mathf.Max(bounds.max.x, right);
            bottom = Mathf.Min(bounds.min.y, bottom);
            top = Mathf.Max(bounds.max.y, top);
            rear = Mathf.Min(bounds.min.z, rear);
            front = Mathf.Max(bounds.max.z, front);
        }

        _scale = new Vector3(right - left, top - bottom, front - rear);
        _pos = 0.5f * new Vector3(left + right, bottom + top, rear + front);
    }

    ///<summary>
    /// For a group of devices, set _pos and _scale
    ///</summary>
    ///<param name="_devices">The list of devices in the group</param>
    ///<param name="_pos">The localPosition to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private static void DeviceGroupPosScale(IEnumerable<Transform> _devices, out Vector3 _pos, out Vector3 _scale)
    {
        Transform lowerDv = _devices.First();
        Transform upperDv = _devices.First();
        float maxWidth = 0;
        float maxLength = 0;
        foreach (Transform dv in _devices)
        {
            if (dv.localPosition.y < lowerDv.localPosition.y)
                lowerDv = dv;
            if (dv.localPosition.y > upperDv.localPosition.y)
                upperDv = dv;

            if (dv.GetChild(0).localScale.x > maxWidth)
                maxWidth = dv.GetChild(0).localScale.x;
            if (dv.GetChild(0).localScale.z > maxLength)
                maxLength = dv.GetChild(0).localScale.z;
        }
        float height = upperDv.localPosition.y - lowerDv.localPosition.y;
        height += (upperDv.GetChild(0).localScale.y + lowerDv.GetChild(0).localScale.y) / 2;

        _scale = new(maxWidth, height, maxLength);

        _pos = lowerDv.position;
        _pos += new Vector3(0, (_scale.y - lowerDv.GetChild(0).localScale.y) / 2, 0);
    }

    /// <summary>
    /// Move the given building/room to its position in a site/building according to the API data.
    /// </summary>
    /// <param name="building">The building/room to be moved</param>
    /// <param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public static void PlaceBuilding(Transform building, SApiObject _apiObj)
    {
        Vector2 posXY = ParseVector2(_apiObj.attributes["posXY"]);
        posXY *= _apiObj.attributes["posXYUnit"] switch
        {
            LengthUnit.Centimeter => 0.01f,
            LengthUnit.Millimeter => 0.001f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => 1
        };
        building.transform.localPosition = new(posXY.x, 0, posXY.y);
        building.transform.localEulerAngles = new(0, Utils.ParseDecFrac(_apiObj.attributes["rotation"]), 0);

    }
}
