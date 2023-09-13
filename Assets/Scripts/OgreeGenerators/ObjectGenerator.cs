﻿using System.Collections;
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
        if (string.IsNullOrEmpty(_rk.attributes["template"]))
        {
            newRack = Object.Instantiate(GameManager.instance.rackModel);

            // Apply scale and move all components to have the rack's pivot at the lower left corner
            Vector2 size = JsonUtility.FromJson<Vector2>(_rk.attributes["size"]);
            float height = Utils.ParseDecFrac(_rk.attributes["height"]);
            if (_rk.attributes["heightUnit"] == LengthUnit.U)
                height *= UnitValue.U;
            else if (_rk.attributes["heightUnit"] == LengthUnit.Centimeter)
                height /= 100;
            Vector3 scale = new Vector3(size.x / 100, height, size.y / 100);

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
            newRack.transform.localEulerAngles = JsonUtility.FromJson<Vector3>(rack.attributes["rotation"]).ZAxisUp();
        }
        else
            newRack.transform.localPosition = Vector3.zero;

        DisplayObjectData dod = newRack.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel("#name");
        dod.hasFloatingLabel = true;
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (rack.attributes.ContainsKey("color"))
            rack.SetColor(rack.attributes["color"]);
        else
            rack.UpdateColorByDomain();

        GameManager.instance.allItems.Add(rack.id, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
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
        if (!string.IsNullOrEmpty(_dv.attributes["template"]) && !GameManager.instance.objectTemplates.ContainsKey(_dv.attributes["template"]))
        {
            GameManager.instance.AppendLogLine($"Unknown template \"{_dv.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
            return null;
        }

        // Check slot
        Transform slot = null;
        if (_parent)
        {
            if (!string.IsNullOrEmpty(_dv.attributes["slot"]))
            {
                List<Slot> takenSlots = new List<Slot>();
                int i = 0;
                float max;
                if (string.IsNullOrEmpty(_dv.attributes["template"]))
                    max = Utils.ParseDecFrac(_dv.attributes["sizeU"]);
                else
                    max = Utils.ParseDecFrac(GameManager.instance.objectTemplates[_dv.attributes["template"]].GetComponent<OgreeObject>().attributes["height"]) / 1000 / UnitValue.U;
                foreach (Transform child in _parent)
                {
                    if ((child.name == _dv.attributes["slot"] || (i > 0 && i < max)) && child.GetComponent<Slot>())
                    {
                        takenSlots.Add(child.GetComponent<Slot>());
                        i++;
                    }
                }

                if (takenSlots.Count > 0)
                {
                    foreach (Slot s in takenSlots)
                        s.SlotTaken(true);
                    slot = takenSlots[0].transform;
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

        if (string.IsNullOrEmpty(_dv.attributes["template"]))
        {
            newDevice = GenerateBasicDevice(_parent, Utils.ParseDecFrac(_dv.attributes["height"]), slot);
            Vector3 boxSize = newDevice.transform.GetChild(0).localScale;
            size = new Vector2(boxSize.x, boxSize.z);
            height = boxSize.y;
        }
        else
        {
            newDevice = GenerateTemplatedDevice(_parent, _dv.attributes["template"]);
            OgreeObject tmp = newDevice.GetComponent<OgreeObject>();
            size = JsonUtility.FromJson<Vector2>(tmp.attributes["size"]) / 1000;
            height = Utils.ParseDecFrac(tmp.attributes["height"]) / 1000;
        }

        Device dv = newDevice.GetComponent<Device>();
        dv.UpdateFromSApiObject(_dv);
        // Place the device
        if (_parent)
        {
            if (!string.IsNullOrEmpty(_dv.attributes["slot"]))
            {
                Vector3 slotScale = slot.GetChild(0).localScale;
                newDevice.transform.localEulerAngles = slot.localEulerAngles;
                newDevice.transform.localPosition = slot.localPosition;

                float deltaZ = slotScale.z - size.y;
                switch (_dv.attributes["orientation"])
                {
                    case Orientation.Front:
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ);
                        break;
                    case Orientation.Rear:
                        newDevice.transform.localEulerAngles += new Vector3(0, 180, 0);
                        if (slot.GetComponent<Slot>().orient == "horizontal")
                            newDevice.transform.localPosition += new Vector3(size.x, 0, size.y);
                        else
                            newDevice.transform.localPosition += new Vector3(-height, 0, size.y);
                        break;
                    case Orientation.FrontFlipped:
                        newDevice.transform.localEulerAngles += new Vector3(0, 0, 180);
                        if (slot.GetComponent<Slot>().orient == "horizontal")
                            newDevice.transform.localPosition += new Vector3(size.x, height, deltaZ);
                        else
                            newDevice.transform.localPosition += new Vector3(-height, size.x, deltaZ);
                        break;
                    case Orientation.RearFlipped:
                        newDevice.transform.localEulerAngles += new Vector3(180, 0, 0);
                        if (slot.GetComponent<Slot>().orient == "horizontal")
                            newDevice.transform.localPosition += new Vector3(0, height, size.y);
                        else
                            newDevice.transform.localPosition += new Vector3(0, size.x, size.y);
                        break;
                }
                if (!dv.attributes.ContainsKey("color"))
                {
                    // if slot, color
                    Color slotColor = slot.GetChild(0).GetComponent<Renderer>().material.color;
                    dv.color = new Color(slotColor.r, slotColor.g, slotColor.b);
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
        if (slot?.GetComponent<Slot>())
            dod.PlaceTexts(slot?.GetComponent<Slot>().labelPos);
        else
            dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel("#name");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (dv.attributes.ContainsKey("color"))
            dv.SetColor(dv.attributes["color"]);
        else if (!dv.hasSlotColor)
            dv.UpdateColorByDomain();

        GameManager.instance.allItems.Add(dv.id, newDevice);

        if (!string.IsNullOrEmpty(_dv.attributes["template"]))
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
            scale = new Vector3(_slot.GetChild(0).localScale.x, _height / 1000, _slot.GetChild(0).localScale.z);
        else
            scale = new Vector3(_parent.GetChild(0).localScale.x, _height / 1000, _parent.GetChild(0).localScale.z);
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

        List<Transform> content = new List<Transform>();
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
        gr.UpdateColorByDomain();

        // Setup labels
        DisplayObjectData dod = newGr.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        if (parentCategory == Category.Room)
            dod.PlaceTexts(LabelPos.Top);
        else if (parentCategory == Category.Rack)
            dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel("#name");
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

        _scale = new Vector3(maxWidth, height, maxLength);

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
        Vector2 size = JsonUtility.FromJson<Vector2>(_co.attributes["size"]);
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
            newCo.transform.localEulerAngles = JsonUtility.FromJson<Vector3>(co.attributes["rotation"]).ZAxisUp();
        }
        else
            newCo.transform.localPosition = Vector3.zero;

        // Set color according to attribute["temperature"]
        newCo.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.instance.alphaMat;
        Material mat = newCo.transform.GetChild(0).GetComponent<Renderer>().material;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.5f);
        if (_co.attributes["temperature"] == "cold")
            co.SetColor("000099");
        else
            co.SetColor("990000");

        DisplayObjectData dod = newCo.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        dod.PlaceTexts(LabelPos.Top);
        dod.SetLabel("#name");
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
        newSensor.transform.localPosition = new Vector3(shapeSize.x / 2, parentSize.y - shapeSize.y / 2, parentSize.z);
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
        dod.SetLabel("#temperature");
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

        Vector2 orient = new Vector2();
        if (parentRoom.attributes.ContainsKey("axisOrientation"))
        {
            switch (parentRoom.attributes["axisOrientation"])
            {
                case AxisOrientation.Default:
                    // Lower Left corner of the room
                    orient = new Vector2(1, 1);
                    break;

                case AxisOrientation.XMinus:
                    // Lower Right corner of the room
                    orient = new Vector2(-1, 1);
                    if (_apiObj.category == Category.Rack)
                        _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, 0);
                    break;

                case AxisOrientation.YMinus:
                    // Upper Left corner of the room
                    orient = new Vector2(1, -1);
                    if (_apiObj.category == Category.Rack)
                        _obj.localPosition -= new Vector3(0, 0, _obj.GetChild(0).localScale.z);
                    break;

                case AxisOrientation.BothMinus:
                    // Upper Right corner of the room
                    orient = new Vector2(-1, -1);
                    if (_apiObj.category == Category.Rack)
                        _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, _obj.GetChild(0).localScale.z);
                    break;
            }
        }

        Vector3 pos;
        if ((_apiObj.category == Category.Rack || _apiObj.category == Category.Corridor) && _apiObj.attributes.ContainsKey("posXYZ"))
            pos = JsonUtility.FromJson<Vector3>(_apiObj.attributes["posXYZ"]).ZAxisUp();
        else
        {
            Vector2 tmp = JsonUtility.FromJson<Vector2>(_apiObj.attributes["posXY"]);
            pos = new Vector3(tmp.x, 0, tmp.y);
        }

        Transform floor = _obj.parent.Find("Floor");
        if (!parentRoom.isSquare && _apiObj.category == Category.Rack && parentRoom.attributes["floorUnit"] == LengthUnit.Tile && floor)
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
}
