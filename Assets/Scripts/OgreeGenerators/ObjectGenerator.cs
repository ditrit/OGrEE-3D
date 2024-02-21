using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerator
{
    ///<summary>
    /// Instantiate a rackModel or a rackTemplate (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_rk">The rack data to apply</param>
    ///<param name="_parent">The parent of the created rack</param>
    ///<returns>The created Rack</returns>
    public Rack CreateRack(SApiObject _rk, Transform _parent)
    {
        if (GameManager.instance.allItems.Contains(_rk.id))
        {
            GameManager.instance.AppendLogLine($"{_rk.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newRack;
        if (!Utils.IsInDict(_rk.attributes, "template"))
        {
            newRack = Object.Instantiate(GameManager.instance.rackModel);

            // Apply scale and move all components to have the rack's pivot at the lower left corner
            Vector2 size = Utils.ParseVector2(_rk.attributes["size"]);
            float height = Utils.ParseDecFrac(_rk.attributes["height"]);
            if (_rk.attributes["heightUnit"] == LengthUnit.U)
                height *= UnitValue.U;
            else if (_rk.attributes["heightUnit"] == LengthUnit.Centimeter)
                height /= 100;
            Vector3 scale = new(size.x / 100, height, size.y / 100);

            newRack.transform.GetChild(0).localScale = scale;
            foreach (Transform child in newRack.transform)
                child.localPosition += scale / 2;
        }
        else
        {
            if (GameManager.instance.objectTemplates.ContainsKey(_rk.attributes["template"]))
            {
                newRack = Object.Instantiate(GameManager.instance.objectTemplates[_rk.attributes["template"]]);
                newRack.GetComponent<ObjectDisplayController>().isTemplate = false;
            }
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template \"{_rk.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
                return null;
            }
        }

        newRack.name = _rk.name;
        newRack.transform.parent = _parent;

        Rack rack = newRack.GetComponent<Rack>();
        rack.UpdateFromSApiObject(_rk);

        if (_parent)
        {
            PlaceInRoom(newRack.transform, _rk);
            newRack.transform.localEulerAngles = Utils.ParseVector3(rack.attributes["rotation"], true);
        }
        else
            newRack.transform.localPosition = Vector3.zero;

        DisplayObjectData dod = newRack.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel(rack.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (rack.attributes.ContainsKey("color"))
            rack.SetColor(rack.attributes["color"]);
        else
            rack.UpdateColorByDomain();

        GameManager.instance.allItems.Add(rack.id, newRack);

        if (Utils.IsInDict(rack.attributes, "template"))
        {
            Device[] components = rack.transform.GetComponentsInChildren<Device>();
            foreach (Device comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    comp.id = $"{rack.id}.{comp.name}";
                    comp.domain = rack.domain;
                    GameManager.instance.allItems.Add(comp.id, comp.gameObject);
                    comp.referent = rack;
                }
            }
        }
        return rack;
    }

    ///<summary>
    /// Instantiate a deviceModel or a deviceTemplate (from GameManager) and apply given data to it.
    ///</summary>
    ///<param name="_dv">The device data to apply</param>
    ///<param name="_parent">The parent of the created device</param>
    ///<returns>The created Device</returns>
    public Device CreateDevice(SApiObject _dv, Transform _parent)
    {
        if (_parent)
        {
            if (!(_parent.GetComponent<Rack>() || _parent.GetComponent<Device>()))
            {
                GameManager.instance.AppendLogLine($"Device must be child of a Rack or another Device", ELogTarget.both, ELogtype.error);
                return null;
            }

            // Check parent for subdevice
            if (_parent.GetComponent<Rack>() == null
                && (string.IsNullOrEmpty(_dv.attributes["slot"]) || string.IsNullOrEmpty(_dv.attributes["template"])))
            {
                GameManager.instance.AppendLogLine("A sub-device needs to be declared with a parent's slot and a template", ELogTarget.both, ELogtype.error);
                return null;
            }

            // Check if parent not hidden in a group
            if (_parent.gameObject.activeSelf == false)
            {
                GameManager.instance.AppendLogLine("The parent object must be active (not hidden in a group)", ELogTarget.both, ELogtype.error);
                return null;
            }
        }

        // Check if unique hierarchyName
        if (GameManager.instance.allItems.Contains(_dv.id))
        {
            GameManager.instance.AppendLogLine($"{_dv.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        // Check template
        if (Utils.IsInDict(_dv.attributes, "template") && !GameManager.instance.objectTemplates.ContainsKey(_dv.attributes["template"]))
        {
            GameManager.instance.AppendLogLine($"Unknown template \"{_dv.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
            return null;
        }

        // Check slot
        Transform primarySlot = null;
        Vector3 slotsScale = new();
        List<Slot> takenSlots = new();
        if (_parent)
        {
            if (Utils.IsInDict(_dv.attributes, "slot"))
            {
                string slots = _dv.attributes["slot"].Trim('[', ']');
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
                    SlotsShape(takenSlots, out Vector3 slotsPivot, out slotsScale);
                    primarySlot = takenSlots[0].transform;
                    foreach (Slot slot in takenSlots)
                    {
                        if (Vector3.Distance(slotsPivot, slot.transform.position) < Vector3.Distance(slotsPivot, primarySlot.position))
                            primarySlot = slot.transform;
                    }
                }
                else
                {
                    GameManager.instance.AppendLogLine($"Slot {_dv.attributes["slot"]} not found in {_parent.name}", ELogTarget.both, ELogtype.error);
                    return null;
                }
            }
        }

        // Generate device
        GameObject newDevice;
        Vector2 size;
        float height;

        if (!Utils.IsInDict(_dv.attributes, "template"))
        {
            newDevice = GenerateBasicDevice(_parent, Utils.ParseDecFrac(_dv.attributes["height"]), primarySlot);
            Vector3 boxSize = newDevice.transform.GetChild(0).localScale;
            size = new(boxSize.x, boxSize.z);
            height = boxSize.y;
        }
        else
        {
            newDevice = GenerateTemplatedDevice(_parent, _dv.attributes["template"]);
            OgreeObject tmp = newDevice.GetComponent<OgreeObject>();
            size = Utils.ParseVector2(tmp.attributes["size"]) / 1000;
            height = Utils.ParseDecFrac(tmp.attributes["height"]) / 1000;
        }

        Device dv = newDevice.GetComponent<Device>();
        dv.UpdateFromSApiObject(_dv);
        dv.takenSlots = takenSlots;

        // Place the device
        if (_parent)
        {
            if (Utils.IsInDict(_dv.attributes, "slot"))
            {
                // parent to slot for applying orientation
                newDevice.transform.parent = primarySlot;
                newDevice.transform.localEulerAngles = Vector3.zero;
                newDevice.transform.localPosition = Vector3.zero;

                float deltaZ = slotsScale.z - size.y;
                switch (_dv.attributes["orientation"])
                {
                    case Orientation.Front:
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ);
                        break;
                    case Orientation.Rear:
                        newDevice.transform.localEulerAngles += new Vector3(0, 180, 0);
                        newDevice.transform.localPosition += new Vector3(size.x, 0, size.y);
                        break;
                    case Orientation.FrontFlipped:
                        newDevice.transform.localEulerAngles += new Vector3(0, 0, 180);
                        newDevice.transform.localPosition += new Vector3(size.x, height, deltaZ);
                        break;
                    case Orientation.RearFlipped:
                        newDevice.transform.localEulerAngles += new Vector3(180, 0, 0);
                        newDevice.transform.localPosition += new Vector3(0, height, size.y);
                        break;
                }
                // align device to right side of the slot if invertOffset == true
                if (_dv.attributes.ContainsKey("invertOffset") && _dv.attributes["invertOffset"] == "true")
                    newDevice.transform.localPosition += new Vector3(slotsScale.x - size.x, 0, 0);
                // parent back to _parent for good hierarchy 
                newDevice.transform.parent = _parent;

                if (!dv.attributes.ContainsKey("color"))
                {
                    // if slot, color
                    Color slotColor = primarySlot.GetChild(0).GetComponent<Renderer>().material.color;
                    dv.color = new(slotColor.r, slotColor.g, slotColor.b);
                    newDevice.GetComponent<ObjectDisplayController>().ChangeColor(slotColor);
                    dv.hasSlotColor = true;
                }
            }
            else
            {
                Vector3 parentShape = _parent.GetChild(0).localScale;
                newDevice.transform.localEulerAngles = Vector3.zero;
                newDevice.transform.localPosition = Vector3.zero;
                if (_dv.attributes.ContainsKey("posU"))
                    newDevice.transform.localPosition += new Vector3(0, (Utils.ParseDecFrac(_dv.attributes["posU"]) - 1) * UnitValue.U, 0);

                float deltaX = parentShape.x - size.x;
                float deltaZ = parentShape.z - size.y;
                newDevice.transform.localPosition += new Vector3(deltaX / 2, 0, deltaZ);
                newDevice.GetComponent<Device>().color = Color.white;
            }
        }
        else
        {
            newDevice.transform.localEulerAngles = Vector3.zero;
            newDevice.transform.localPosition = Vector3.zero;
            newDevice.GetComponent<Device>().color = Color.white;
        }

        // Fill OObject class
        newDevice.name = _dv.name;

        // Set labels
        DisplayObjectData dod = newDevice.GetComponent<DisplayObjectData>();
        if (primarySlot?.GetComponent<Slot>())
            dod.PlaceTexts(primarySlot?.GetComponent<Slot>().labelPos);
        else
            dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel(dv.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (dv.attributes.ContainsKey("color"))
            dv.SetColor(dv.attributes["color"]);
        else if (!dv.hasSlotColor)
            dv.UpdateColorByDomain();

        GameManager.instance.allItems.Add(dv.id, newDevice);

        if (Utils.IsInDict(_dv.attributes, "template"))
        {
            Device[] components = newDevice.transform.GetComponentsInChildren<Device>();
            foreach (Device comp in components)
            {
                if (comp.gameObject != newDevice)
                {
                    comp.id = $"{dv.id}.{comp.name}";
                    comp.domain = dv.domain;
                    GameManager.instance.allItems.Add(comp.id, comp.gameObject);
                    comp.referent = dv.referent;
                }
            }
        }
        return dv;
    }

    ///<summary>
    /// Generate a basic device.
    ///</summary>
    ///<param name="_parent">The parent of the generated device</param>
    ///<param name="_height">The height in mm of the device</param>
    ///<returns>The generated device</returns>
    private GameObject GenerateBasicDevice(Transform _parent, float _height, Transform _slot = null)
    {
        GameObject go = Object.Instantiate(GameManager.instance.labeledBoxModel);
        go.AddComponent<Device>();
        go.transform.parent = _parent;
        Vector3 scale;
        if (_slot)
            scale = new(_slot.GetChild(0).localScale.x, _height / 1000, _slot.GetChild(0).localScale.z);
        else
            scale = new(_parent.GetChild(0).localScale.x, _height / 1000, _parent.GetChild(0).localScale.z);
        go.transform.GetChild(0).localScale = scale;
        go.transform.GetChild(0).GetComponent<Collider>().enabled = true;

        foreach (Transform child in go.transform)
            child.localPosition = scale / 2;

        return go;
    }

    ///<summary>
    /// Generate a templated device.
    ///</summary>
    ///<param name="_parent">The parent of the generated device</param>
    ///<param name="_template">The template to instantiate</param>
    ///<returns>The generated device</returns>
    private GameObject GenerateTemplatedDevice(Transform _parent, string _template)
    {
        if (GameManager.instance.objectTemplates.ContainsKey(_template))
        {
            GameObject go = Object.Instantiate(GameManager.instance.objectTemplates[_template]);
            go.transform.parent = _parent;
            go.GetComponent<ObjectDisplayController>().isTemplate = false;
            return go;
        }
        else
        {
            GameManager.instance.AppendLogLine($"Unknown template \"{_template}\"", ELogTarget.both, ELogtype.error);
            return null;
        }
    }

    /// <summary>
    /// Get <paramref name="_pivot"/> and <paramref name="_scale"/> of all slots combined
    /// </summary>
    /// <param name="_slotsList">The list of slots to look in</param>
    /// <param name="_pivot">The pivot of the combined slots</param>
    /// <param name="_scale">The scale of the combined slots</param>
    private void SlotsShape(List<Slot> _slotsList, out Vector3 _pivot, out Vector3 _scale)
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

        _scale = new Vector3(right - left, top - bottom, front - rear);
        _pivot = new(left, bottom, rear);
    }

    ///<summary>
    /// Generate a group (from GameManager.labeledBoxModel) which contains all the given objects.
    ///</summary>
    ///<param name="_gr">The group data to apply</param>
    ///<param name="_parent">The parent of the created group. Leave null if _gr contains the parendId</param>
    ///<returns>The created rackGroup</returns>
    public Group CreateGroup(SApiObject _gr, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _gr.parentId);
        if (!parent)
        {
            GameManager.instance.AppendLogLine("Parent not found", ELogTarget.both, ELogtype.error);
            return null;
        }
        string parentCategory = parent.GetComponent<OgreeObject>().category;
        if (parentCategory != Category.Room && parentCategory != Category.Rack)
        {
            GameManager.instance.AppendLogLine("A group must be a child of a room or a rack", ELogTarget.both, ELogtype.error);
            return null;
        }

        if (GameManager.instance.allItems.Contains(_gr.id))
        {
            GameManager.instance.AppendLogLine($"{_gr.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        List<Transform> content = new();
        string[] contentNames = _gr.attributes["content"].Split(',');
        foreach (string cn in contentNames)
        {
            GameObject go = Utils.GetObjectById($"{_gr.parentId}.{cn}");
            if (go && go.GetComponent<OgreeObject>())
            {
                if ((parentCategory == Category.Room && (go.GetComponent<Rack>() || go.GetComponent<Corridor>()))
                    || parentCategory == Category.Rack && go.GetComponent<Device>())
                    content.Add(go.transform);
            }
            else
                GameManager.instance.AppendLogLine($"{_gr.parentId}.{cn} doesn't exists.", ELogTarget.both, ELogtype.warning);
        }
        if (content.Count == 0)
            return null;

        GameObject newGr = Object.Instantiate(GameManager.instance.labeledBoxModel);
        newGr.name = _gr.name;
        newGr.transform.parent = parent;

        // According to group type, set pos, rot & scale
        Vector3 pos = Vector3.zero;
        Vector3 scale = Vector3.zero;
        if (parentCategory == Category.Room)
        {
            RackGroupPosScale(content, out pos, out scale);
            newGr.transform.localEulerAngles += new Vector3(0, 180, 0);
        }
        else if (parentCategory == Category.Rack)
        {
            DeviceGroupPosScale(content, out pos, out scale);
            newGr.transform.localEulerAngles = Vector3.zero;
        }
        newGr.transform.position = pos;
        newGr.transform.GetChild(0).localScale = scale;

        // Set Group component
        Group gr = newGr.AddComponent<Group>();
        gr.UpdateFromSApiObject(_gr);
        if (gr.attributes.ContainsKey("color"))
            gr.SetColor(gr.attributes["color"]);
        else
            gr.UpdateColorByDomain();

        // Setup labels
        DisplayObjectData dod = newGr.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        if (parentCategory == Category.Room)
            dod.PlaceTexts(LabelPos.Top);
        else if (parentCategory == Category.Rack)
            dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel(gr.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        GameManager.instance.allItems.Add(gr.id, newGr);

        return gr;
    }

    ///<summary>
    /// For a group of Racks, set _pos and _scale
    ///</summary>
    ///<param name="_content">The list of racks or corridors in the group</param>
    ///<param name="_pos">The position to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private void RackGroupPosScale(List<Transform> _content, out Vector3 _pos, out Vector3 _scale)
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
    ///<param name="_racks">The list of devices in the group</param>
    ///<param name="_pos">The localPosition to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private void DeviceGroupPosScale(List<Transform> _devices, out Vector3 _pos, out Vector3 _scale)
    {
        Transform lowerDv = _devices[0];
        Transform upperDv = _devices[0];
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

    ///<summary>
    /// Generate a corridor (from GameManager.labeledBoxModel) and apply the given data to it.
    ///</summary>
    ///<param name="_co">The corridor data to apply</param>
    ///<param name="_parent">The parent of the created corridor. Leave null if _co contains the parendId</param>
    ///<returns>The created corridor</returns>
    public Corridor CreateCorridor(SApiObject _co, Transform _parent = null)
    {
        if (GameManager.instance.allItems.Contains(_co.id))
        {
            GameManager.instance.AppendLogLine($"{_co.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newCo = Object.Instantiate(GameManager.instance.labeledBoxModel);
        newCo.name = _co.name;
        newCo.transform.parent = _parent;

        // Apply scale and move all components to have the rack's pivot at the lower left corner
        Vector2 size = Utils.ParseVector2(_co.attributes["size"]);
        float height = Utils.ParseDecFrac(_co.attributes["height"]);
        Vector3 scale = 0.01f * new Vector3(size.x, height, size.y);

        newCo.transform.GetChild(0).localScale = scale;
        foreach (Transform child in newCo.transform)
            child.localPosition += scale / 2;

        Corridor co = newCo.AddComponent<Corridor>();
        co.UpdateFromSApiObject(_co);

        // Apply position & rotation
        if (_parent)
        {
            PlaceInRoom(newCo.transform, _co);
            newCo.transform.localEulerAngles = Utils.ParseVector3(co.attributes["rotation"], true);
        }
        else
            newCo.transform.localPosition = Vector3.zero;

        // Set color according to attribute["temperature"]
        newCo.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.instance.alphaMat;
        Material mat = newCo.transform.GetChild(0).GetComponent<Renderer>().material;
        mat.color = new(mat.color.r, mat.color.g, mat.color.b, 0.5f);
        if (_co.attributes["temperature"] == "cold")
            co.SetColor("000099");
        else
            co.SetColor("990000");

        DisplayObjectData dod = newCo.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        dod.PlaceTexts(LabelPos.Top);
        dod.SetLabel(co.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        GameManager.instance.allItems.Add(co.id, newCo);

        return co;
    }

    ///<summary>
    /// Generate a sensor (from GameManager.sensorExtModel/sensorIntModel) with defined temperature.
    ///</summary>
    ///<param name="_se">The sensor data to apply</param>
    ///<param name="_parent">The parent of the created sensor. Leave null if _se contains the parendId</param>
    ///<returns>The created sensor</returns>
    public Sensor CreateSensor(SApiObject _se, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _se.parentId);
        if (!parent)
        {
            GameManager.instance.AppendLogLine($"Parent not found", ELogTarget.both, ELogtype.error);
            return null;
        }
        OgreeObject parentOgree = parent.GetComponent<OgreeObject>();
        Vector3 parentSize = _parent.GetChild(0).localScale;

        GameObject newSensor = Object.Instantiate(GameManager.instance.sensorExtModel, _parent);
        newSensor.name = "sensor";

        Vector3 shapeSize = newSensor.transform.GetChild(0).localScale;
        newSensor.transform.localPosition = new(shapeSize.x / 2, parentSize.y - shapeSize.y / 2, parentSize.z);
        if (parentOgree is Rack)
        {
            float uXSize = UnitValue.OU;
            if (parentOgree.attributes.ContainsKey("heightUnit") && parentOgree.attributes["heightUnit"] == LengthUnit.U)
                uXSize = UnitValue.U;
            newSensor.transform.localPosition += uXSize * Vector3.right;
        }

        Sensor sensor = newSensor.GetComponent<Sensor>();
        sensor.fromTemplate = false;
        DisplayObjectData dod = newSensor.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.Front);
        dod.SetLabel($"{Utils.FloatToRefinedStr(sensor.temperature)} {sensor.temperatureUnit}");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        return sensor;
    }

    ///<summary>
    /// Move the given Transform to given position in tiles in a room.
    ///</summary>
    ///<param name="_obj">The object to move</param>
    ///<param name="_apiObj">The SApiObject with posXY and posU data</param>
    private void PlaceInRoom(Transform _obj, SApiObject _apiObj)
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
        {
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
        }

        Vector3 pos;
        if ((_apiObj.category == Category.Rack || _apiObj.category == Category.Corridor || _apiObj.category == Category.Generic) && _apiObj.attributes.ContainsKey("posXYZ"))
            pos = Utils.ParseVector3(_apiObj.attributes["posXYZ"], true);
        else
        {
            Vector2 tmp = Utils.ParseVector2(_apiObj.attributes["posXY"]);
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
    }

    ///<summary>
    /// Get a posXYUnit regarding given object's attributes.
    ///</summary>
    ///<param name="_obj">The object to parse</param>
    ///<returns>The posXYUnit, <see cref="UnitValue.Tile"/> by default</returns>
    private float GetUnitFromAttributes(SApiObject _obj)
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

    ///<summary>
    /// Instantiate a genericCubeModel, a genericSphereModel, a genericCylinderModel or a rackTemplate (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_go">The generic object data to apply</param>
    ///<param name="_parent">The parent of the created generic object</param>
    ///<returns>The created generic object</returns>
    public GenericObject CreateGeneric(SApiObject _go, Transform _parent)
    {
        if (GameManager.instance.allItems.Contains(_go.id))
        {
            GameManager.instance.AppendLogLine($"{_go.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newGeneric;
        if (string.IsNullOrEmpty(_go.attributes["template"]))
        {
            newGeneric = _go.attributes["shape"] switch
            {
                "cube" => Object.Instantiate(GameManager.instance.genericCubeModel),
                "sphere" => Object.Instantiate(GameManager.instance.genericSphereModel),
                "cylinder" => Object.Instantiate(GameManager.instance.genericCylinderModel),
                _ => null
            };
            if (!newGeneric)
            {
                GameManager.instance.AppendLogLine($"Incorrect generic shape {_go.attributes["shape"]}", ELogTarget.both, ELogtype.error);
                return null;
            }
            Vector2 size = Utils.ParseVector2(_go.attributes["size"]);
            newGeneric.transform.GetChild(0).localScale = new(size.x, Utils.ParseDecFrac(_go.attributes["height"]), size.y);

            newGeneric.transform.GetChild(0).localScale /= 100;
            if (_go.attributes["sizeUnit"] == LengthUnit.Millimeter)
                newGeneric.transform.GetChild(0).localScale /= 10;
            foreach (Transform child in newGeneric.transform)
                child.localPosition += newGeneric.transform.GetChild(0).localScale / 2;
        }
        else
        {
            if (GameManager.instance.objectTemplates.ContainsKey(_go.attributes["template"]))
            {
                newGeneric = Object.Instantiate(GameManager.instance.objectTemplates[_go.attributes["template"]]);
                newGeneric.GetComponent<ObjectDisplayController>().isTemplate = false;
            }
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template \"{_go.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
                return null;
            }
        }

        newGeneric.name = _go.name;
        newGeneric.transform.parent = _parent;

        GenericObject genericObject = newGeneric.GetComponent<GenericObject>();
        genericObject.UpdateFromSApiObject(_go);

        if (_parent)
        {
            PlaceInRoom(newGeneric.transform, _go);
            newGeneric.transform.localEulerAngles = Utils.ParseVector3(genericObject.attributes["rotation"], true);
        }
        else
            newGeneric.transform.localPosition = Vector3.zero;

        DisplayObjectData dod = newGeneric.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel(genericObject.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (genericObject.attributes.ContainsKey("color"))
            genericObject.SetColor(genericObject.attributes["color"]);
        else
            genericObject.UpdateColorByDomain();

        GameManager.instance.allItems.Add(genericObject.id, newGeneric);
        return genericObject;
    }
}
