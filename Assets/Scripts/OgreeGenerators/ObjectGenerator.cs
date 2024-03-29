﻿using System.Collections.Generic;
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
        if (!_rk.attributes.HasKeyAndValue("template"))
        {
            newRack = Object.Instantiate(GameManager.instance.rackModel);

            // Apply scale and move all components to have the rack's pivot at the lower left corner
            Vector2 size = Utils.ParseVector2(_rk.attributes["size"]);
            float height = Utils.ParseDecFrac(_rk.attributes["height"]);
            switch (_rk.attributes["heightUnit"])
            {
                case LengthUnit.U:
                    height *= UnitValue.U;
                    break;
                case LengthUnit.Centimeter:
                    height /= 100;
                    break;
                case LengthUnit.Millimeter:
                    height /= 1000;
                    break;
                default:
                    GameManager.instance.AppendLogLine($"Unknown {_rk.attributes["heightUnit"]} unit at {_rk.name} creation.", ELogTarget.both, ELogtype.error);
                    return null;
            }
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
            Utils.PlaceInRoom(newRack.transform, _rk);
        else
        {
            newRack.transform.localPosition = Vector3.zero;
            newRack.transform.localEulerAngles = Vector3.zero;
        }

        DisplayObjectData dod = newRack.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(LabelPos.FrontRear);
        dod.SetLabel(rack.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (rack.attributes.ContainsKey("color"))
            rack.SetColor(rack.attributes["color"]);
        else
            rack.UpdateColorByDomain();

        GameManager.instance.allItems.Add(rack.id, newRack);

        if (rack.attributes.HasKeyAndValue("template"))
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
    ///<param name="_deviceData">The device data to apply</param>
    ///<param name="_parent">The parent of the created device</param>
    ///<returns>The created Device</returns>
    public Device CreateDevice(SApiObject _deviceData, Transform _parent)
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
                && (string.IsNullOrEmpty(_deviceData.attributes["slot"]) || string.IsNullOrEmpty(_deviceData.attributes["template"])))
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
        if (GameManager.instance.allItems.Contains(_deviceData.id))
        {
            GameManager.instance.AppendLogLine($"{_deviceData.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        // Check template
        if (_deviceData.attributes.HasKeyAndValue("template") && !GameManager.instance.objectTemplates.ContainsKey(_deviceData.attributes["template"]))
        {
            GameManager.instance.AppendLogLine($"Unknown template \"{_deviceData.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
            return null;
        }

        // Generate device
        GameObject newDevice = _deviceData.attributes.HasKeyAndValue("template") ? GenerateTemplatedDevice(_parent, _deviceData.attributes["template"]) : GenerateBasicDevice(_parent);
        Device device = newDevice.GetComponent<Device>();
        newDevice.name = _deviceData.name;
        Utils.PlaceDevice(_parent, device, _deviceData);
        device.UpdateFromSApiObject(_deviceData);

        DisplayObjectData dod = newDevice.GetComponent<DisplayObjectData>();
        dod.SetLabel(device.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        device.GetComponent<Device>().color = Color.white;
        if (device.attributes.ContainsKey("color"))
            device.SetColor(device.attributes["color"]);
        else if (!device.hasSlotColor)
            device.UpdateColorByDomain();

        GameManager.instance.allItems.Add(device.id, newDevice);

        if (_deviceData.attributes.HasKeyAndValue("template"))
            foreach (Device comp in newDevice.transform.GetComponentsInChildren<Device>())
                if (comp.gameObject != newDevice)
                {
                    comp.id = $"{device.id}.{comp.name}";
                    comp.domain = device.domain;
                    GameManager.instance.allItems.Add(comp.id, comp.gameObject);
                    comp.referent = device.referent;
                }

        return device;
    }

    ///<summary>
    /// Generate a basic device.
    ///</summary>
    ///<param name="_parent">The parent of the generated device</param>
    ///<returns>The generated device</returns>
    private GameObject GenerateBasicDevice(Transform _parent)
    {
        GameObject go = Object.Instantiate(GameManager.instance.labeledBoxModel);
        go.AddComponent<Device>();
        go.transform.parent = _parent;
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
        GameManager.instance.AppendLogLine($"Unknown template \"{_template}\"", ELogTarget.both, ELogtype.error);
        return null;
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

        Group gr = newGr.AddComponent<Group>();
        Utils.ShapeGroup(content, gr, parentCategory);
        gr.UpdateFromSApiObject(_gr);

        // Set Group component
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
            Utils.PlaceInRoom(newCo.transform, _co);
        else
        {
            newCo.transform.localPosition = Vector3.zero;
            newCo.transform.localEulerAngles = Vector3.zero;
        }

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
        if (!_go.attributes.HasKeyAndValue("template"))
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
            Utils.PlaceInRoom(newGeneric.transform, _go);
        else
        {
            newGeneric.transform.localPosition = Vector3.zero;
            newGeneric.transform.localEulerAngles = Vector3.zero;
        }

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
