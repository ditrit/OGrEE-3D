using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReadFromJson
{
    ///<summary>
    /// Store <paramref name="_data"/> in <see cref="GameManager.buildingTemplates"/>.
    ///</summary>
    ///<param name="_data">Building data to store</param>
    public void CreateBuildingTemplate(SBuildingFromJson _data)
    {
        if (GameManager.instance.buildingTemplates.ContainsKey(_data.slug))
            return;

        GameManager.instance.buildingTemplates.Add(_data.slug, _data);
    }

    ///<summary>
    /// Store <paramref name="_data"/> in <see cref="GameManager.roomTemplates"/>.
    ///</summary>
    ///<param name="_data">Room data to store</param>
    public void CreateRoomTemplate(SRoomFromJson _data)
    {
        if (GameManager.instance.roomTemplates.ContainsKey(_data.slug))
            return;

        GameManager.instance.roomTemplates.Add(_data.slug, _data);
    }

    ///<summary>
    /// Create a rack or a device from received data and add it to correct GameManager list
    ///</summary>
    ///<param name="_data">The data template</param>
    public async Task CreateObjectTemplate(STemplate _data)
    {
        if (_data.category != Category.Rack && _data.category != Category.Device && _data.category != Category.Generic)
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Unknown category in template", _data.slug), ELogTarget.both, ELogtype.error);
            return;
        }
        if (GameManager.instance.objectTemplates.ContainsKey(_data.slug))
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Object already exists", _data.slug), ELogTarget.logger, ELogtype.warning);
            return;
        }

        // Build SApiObject
        SApiObject obj = new()
        {
            attributes = new(),
            tags = new(),

            name = _data.slug,
            id = _data.slug,
            category = _data.category,
            description = _data.description,
            domain = ""
        };
        if (obj.category == Category.Rack)
        {
            Vector3 tmp = new Vector3(_data.sizeWDHmm[0], _data.sizeWDHmm[1], _data.sizeWDHmm[2]) / 10;
            obj.attributes["posXYZ"] = "[0,0,0]";
            obj.attributes["posXYUnit"] = LengthUnit.Tile;
            obj.attributes["rotation"] = "[0,0,0]";
            obj.attributes["size"] = $"[{tmp.x},{tmp.y}]";
            obj.attributes["sizeUnit"] = LengthUnit.Centimeter;
            obj.attributes["height"] = tmp.z.ToString();
            obj.attributes["heightUnit"] = LengthUnit.Centimeter;
        }
        else if (obj.category == Category.Device)
        {
            obj.attributes["size"] = $"[{_data.sizeWDHmm[0]},{_data.sizeWDHmm[1]}]";
            obj.attributes["sizeUnit"] = LengthUnit.Millimeter;
            obj.attributes["height"] = _data.sizeWDHmm[2].ToString();
            obj.attributes["heightUnit"] = LengthUnit.Millimeter;
        }
        else if (obj.category == Category.Generic)
        {
            obj.attributes["posXYZ"] = "[0,0,0]";
            obj.attributes["posXYUnit"] = LengthUnit.Tile;
            obj.attributes["rotation"] = "[0,0,0]";
            obj.attributes["shape"] = _data.shape;
            obj.attributes["size"] = $"[{_data.sizeWDHmm[0]},{_data.sizeWDHmm[1]}]";
            obj.attributes["sizeUnit"] = LengthUnit.Millimeter;
            obj.attributes["height"] = _data.sizeWDHmm[2].ToString();
            obj.attributes["heightUnit"] = LengthUnit.Millimeter;
        }
        obj.attributes["fbxModel"] = (!string.IsNullOrEmpty(_data.fbxModel)).ToString();
        if (_data.attributes != null)
            foreach (KeyValuePair<string, string> kvp in _data.attributes)
                obj.attributes[kvp.Key] = kvp.Value;

        // Generate the 3D object
        Item newItem;
        if (obj.category == Category.Rack || obj.category == Category.Generic)
        {
            newItem = (Item)await OgreeGenerator.instance.CreateItemFromSApiObject(obj, GameManager.instance.templatePlaceholder);
            if (!string.IsNullOrEmpty(_data.fbxModel))
            {
                bool precompiled = false;
                foreach (GameObject fbxModel in GameManager.instance.fbxModels)
                    if (fbxModel.name == _data.slug)
                    {
                        Vector3 boxScale = newItem.transform.GetChild(0).localScale;
                        GameObject model = UnityEngine.Object.Instantiate(fbxModel);
                        model.transform.parent = newItem.transform;
                        model.transform.localPosition = newItem.transform.GetChild(0).localPosition;
                        model.transform.SetAsFirstSibling();
                        UnityEngine.Object.Destroy(newItem.transform.GetChild(1).gameObject);
                        precompiled = true;
                        break;
                    }
#if TRILIB
                if (!precompiled)
                    await ModelLoader.instance.ReplaceBox(newItem.gameObject, _data.fbxModel);
#endif
            }
        }
        else// if (obj.category == Category.Device)
        {
            newItem = (Item)await OgreeGenerator.instance.CreateItemFromSApiObject(obj, GameManager.instance.templatePlaceholder.GetChild(0));
            Vector3 accurateScale = new Vector3(_data.sizeWDHmm[0], _data.sizeWDHmm[2], _data.sizeWDHmm[1]) / 1000;
            newItem.transform.GetChild(0).localScale = accurateScale;
            foreach (Transform comp in newItem.transform)
                comp.localPosition = accurateScale / 2;
#if TRILIB
            if (!string.IsNullOrEmpty(_data.fbxModel))
                await ModelLoader.instance.ReplaceBox(newItem.gameObject, _data.fbxModel);
#endif
        }
        newItem.GetComponent<ObjectDisplayController>().isTemplate = true;

        newItem.color = newItem.transform.GetChild(0).GetComponent<Renderer>().material.color;

        // Retrieve custom colors
        Dictionary<string, string> customColors = new();
        if (_data.colors != null)
            foreach (SColor color in _data.colors)
                customColors.Add(color.name, color.value);

        // Generate components & slots
        if (_data.components != null)
            foreach (STemplateChild compData in _data.components)
                PopulateSlot(false, compData, newItem, customColors);
        if (_data.slots != null)
            foreach (STemplateChild slotData in _data.slots)
                PopulateSlot(true, slotData, newItem, customColors);

        if (_data.sensors != null)
            foreach (STemplateSensor sensor in _data.sensors)
                GenerateSensorTemplate(sensor, newItem.transform);

        // Toggle renderers & put newObj in GameManager.objectTemplates
#if PROD
        Renderer[] renderers = newItem.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
        newItem.transform.GetChild(0).GetComponent<Collider>().enabled = false;
        newItem.referent = null;
#endif
        GameManager.instance.allItems.Remove(newItem.id);
        GameManager.instance.objectTemplates.Add(newItem.name, newItem.gameObject);
    }

    ///<summary>
    /// Create a child Slot or Component object.
    ///</summary>
    ///<param name="_isSlot">Device if it's a slot or a component to create</param>
    ///<param name="_data">Data for Slot/Component creation</param>
    ///<param name="_parent">The parent of the Slot/Component</param>
    ///<param name="_customColors">Custom colors to use</param>
    private void PopulateSlot(bool _isSlot, STemplateChild _data, OgreeObject _parent, Dictionary<string, string> _customColors)
    {
        GameObject go = UnityEngine.Object.Instantiate(GameManager.instance.labeledBoxModel);

        go.name = _data.location;
        go.transform.parent = _parent.transform;
        go.transform.GetChild(0).localScale = new Vector3(_data.elemSize[0], _data.elemSize[2], _data.elemSize[1]) / 1000;
        go.transform.localPosition = new Vector3(_data.elemPos[0], _data.elemPos[2], _data.elemPos[1]) / 1000;
        go.transform.localEulerAngles = new(_data.elemOrient[0], _data.elemOrient[2], _data.elemOrient[1]);
        go.transform.GetChild(0).localPosition += go.transform.GetChild(0).localScale / 2;
        if (_isSlot)
        {
            Slot s = go.AddComponent<Slot>();
            s.orient = _data.elemOrient;
            if (_data.attributes != null && _data.attributes.ContainsKey("factor"))
                s.formFactor = _data.attributes["factor"];
            s.labelPos = _data.labelPos;
            s.isU = _data.type == "u";
            go.transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        else
        {
            Device obj = go.AddComponent<Device>();
            obj.name = go.name;
            obj.id = $"{_parent.id}.{obj.name}";
            obj.parentId = _parent.id;
            obj.category = Category.Device;
            obj.domain = _parent.domain;
            obj.attributes = new()
            {
                ["deviceType"] = _data.type
            };
            if (_data.attributes != null)
                foreach (KeyValuePair<string, string> kvp in _data.attributes)
                    obj.attributes[kvp.Key] = kvp.Value;
            obj.isComponent = true;
            obj.SetBaseTransform();
        }

        DisplayObjectData dod = go.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(_data.labelPos);
        if (_isSlot)
            dod.SetLabelFont("color@888888");
        dod.SetLabel(go.name);
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        go.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.instance.defaultMat;
        Renderer rend = go.transform.GetChild(0).GetComponent<Renderer>();
        Color myColor;
        if (_data.color != null && _data.color.StartsWith("@"))
            ColorUtility.TryParseHtmlString($"#{_customColors[_data.color[1..]]}", out myColor);
        else
            ColorUtility.TryParseHtmlString($"#{_data.color}", out myColor);
        if (_isSlot)
        {
            rend.material = GameManager.instance.alphaMat;
            rend.material.color = new(myColor.r, myColor.g, myColor.b, 0.33f);
        }
        else
        {
            rend.material.color = new(myColor.r, myColor.g, myColor.b, 1f);
            go.GetComponent<Item>().color = rend.material.color;
        }
    }

    ///<summary>
    /// Generate a sensor from a rack or device template.
    ///</summary>
    ///<param name="_sensor">The sensor data to apply</param>
    ///<param name="_parent">The parent of the created sensor</param>
    private void GenerateSensorTemplate(STemplateSensor _sensor, Transform _parent)
    {
        GameObject newSensor;
        if (_sensor.elemSize.Length == 1)
        {
            newSensor = UnityEngine.Object.Instantiate(GameManager.instance.sensorIntModel, _parent);
            newSensor.transform.GetChild(0).localScale = 0.001f * _sensor.elemSize[0] * Vector3.one;
        }
        else
        {
            newSensor = UnityEngine.Object.Instantiate(GameManager.instance.sensorExtModel, _parent);
            newSensor.transform.GetChild(0).localScale = 0.001f * new Vector3(_sensor.elemSize[0], _sensor.elemSize[1], _sensor.elemSize[2]);
        }
        newSensor.name = _sensor.location;
        newSensor.transform.localPosition = Vector3.zero;
        Vector3 parentScale = _parent.GetChild(0).localScale;
        switch (_sensor.elemPos[0])
        {
            case SensorPos.Left:
                newSensor.transform.localPosition += parentScale.x * Vector3.right;
                break;
            case SensorPos.Center:
                newSensor.transform.localPosition += 0.5f * parentScale.x * Vector3.right;
                break;
            case SensorPos.Right:
                break;
            default:
                try
                {
                    newSensor.transform.localPosition += Utils.ParseDecFrac(_sensor.elemPos[0]) / 1000 * Vector3.right;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Wrong pos value for sensor in template", new List<string>() { "x", _sensor.location, _parent.name }), ELogTarget.both, ELogtype.error);
                }
                break;
        }
        switch (_sensor.elemPos[1])
        {
            case SensorPos.Front:
                newSensor.transform.localPosition += parentScale.z * Vector3.forward;
                break;
            case SensorPos.Center:
                newSensor.transform.localPosition += 0.5f * parentScale.z * Vector3.forward;
                break;
            case SensorPos.Rear:
                break;
            default:
                try
                {
                    newSensor.transform.localPosition += Utils.ParseDecFrac(_sensor.elemPos[1]) / 1000 * Vector3.forward;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Wrong pos value for sensor in template", new List<string>() { "y", _sensor.location, _parent.name }), ELogTarget.both, ELogtype.error);
                }
                break;
        }
        switch (_sensor.elemPos[2])
        {
            case SensorPos.Lower:
                break;
            case SensorPos.Center:
                newSensor.transform.localPosition += 0.5f * parentScale.y * Vector3.up;
                break;
            case SensorPos.Upper:
                newSensor.transform.localPosition += parentScale.y * Vector3.up;
                break;
            default:
                try
                {
                    newSensor.transform.localPosition += Utils.ParseDecFrac(_sensor.elemPos[2]) / 1000 * Vector3.up;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Wrong pos value for sensor in template", new List<string>() { "z", _sensor.location, _parent.name }), ELogTarget.both, ELogtype.error);
                }
                break;
        }
        Sensor sensor = newSensor.GetComponent<Sensor>();

        sensor.fromTemplate = true;
        DisplayObjectData dod = newSensor.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(_sensor.elemPos[1]);
        dod.SetLabel($"{sensor.temperature:0.##} {sensor.temperatureUnit}");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);
    }
}
