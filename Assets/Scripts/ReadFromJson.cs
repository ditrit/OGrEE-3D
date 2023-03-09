using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

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

    /// <summary>
    /// Check if given JSON is a room template and call <see cref="CreateRoomTemplate(SRoomFromJson)"/> if the JSON is valid
    /// </summary>
    /// <param name="_json">the JSON to deserialize</param>
    public void CreateRoomTemplateJson(string _json)
    {
        SRoomFromJson roomData;
        try
        {
            roomData = JsonConvert.DeserializeObject<SRoomFromJson>(_json);
        }
        catch (Exception e)
        {
            GameManager.instance.AppendLogLine($"Error on Json deserialization: {e.Message}.", true, ELogtype.error);
            return;
        }
        CreateRoomTemplate(roomData);
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
    /// Create a rack or a device from received JSON and add it to correct <see cref="GameManager"/> list
    ///</summary>
    ///<param name="_json">JSON to parse</param>
    public async Task CreateObjTemplateJson(string _json)
    {
        STemplate data;
        try
        {
            data = JsonConvert.DeserializeObject<STemplate>(_json);
        }
        catch (Exception e)
        {
            GameManager.instance.AppendLogLine($"Error on Json deserialization: {e.Message}.", true, ELogtype.error);
            return;
        }
        await CreateObjectTemplate(data);
    }

    ///<summary>
    /// Create a rack or a device from received data and add it to correct GameManager list
    ///</summary>
    ///<param name="_data">The data template</param>
    public async Task CreateObjectTemplate(STemplate _data)
    {
        if (_data.category != "rack" && _data.category != "device")
        {
            GameManager.instance.AppendLogLine($"Unknown category for {_data.slug} template.", true, ELogtype.error);
            return;
        }
        if (GameManager.instance.objectTemplates.ContainsKey(_data.slug))
        {
            GameManager.instance.AppendLogLine($"{_data.slug} already exists.", false, ELogtype.warning);
            return;
        }

        // Build SApiObject
        SApiObject obj = new SApiObject
        {
            description = new List<string>(),
            attributes = new Dictionary<string, string>(),

            name = _data.slug,
            category = _data.category
        };
        obj.description.Add(_data.description);
        if (obj.category == "rack")
        {
            Vector3 tmp = new Vector3(_data.sizeWDHmm[0], _data.sizeWDHmm[1], _data.sizeWDHmm[2]) / 10;
            obj.attributes["posXY"] = JsonUtility.ToJson(Vector2.zero);
            obj.attributes["posXYUnit"] = "tile";
            obj.attributes["size"] = JsonUtility.ToJson(new Vector2(tmp.x, tmp.y));
            obj.attributes["sizeUnit"] = "cm";
            obj.attributes["height"] = ((int)tmp.z).ToString();
            obj.attributes["heightUnit"] = "cm";
            obj.attributes["orientation"] = "front";
        }
        else if (obj.category == "device")
        {
            if (_data.attributes.ContainsKey("type")
                && (_data.attributes["type"] == "chassis" || _data.attributes["type"] == "server"))
            {
                int sizeU = Mathf.CeilToInt((_data.sizeWDHmm[2] / 1000) / GameManager.instance.uSize);
                obj.attributes["sizeU"] = sizeU.ToString();
            }
            obj.attributes["size"] = JsonUtility.ToJson(new Vector2(_data.sizeWDHmm[0], _data.sizeWDHmm[1]));
            obj.attributes["sizeUnit"] = "mm";
            obj.attributes["height"] = _data.sizeWDHmm[2].ToString();
            obj.attributes["heightUnit"] = "mm";
            obj.attributes["slot"] = "";
        }
        obj.attributes["template"] = "";
        obj.attributes["fbxModel"] = (!string.IsNullOrEmpty(_data.fbxModel)).ToString();
        if (_data.attributes != null)
        {
            foreach (KeyValuePair<string, string> kvp in _data.attributes)
                obj.attributes[kvp.Key] = kvp.Value;
        }

        // Generate the 3D object
        OgreeObject newObject;
        if (obj.category == "rack")
        {
            newObject = await OgreeGenerator.instance.CreateItemFromSApiObject(obj, GameManager.instance.templatePlaceholder);
            if (!string.IsNullOrEmpty(_data.fbxModel))
                await ModelLoader.instance.ReplaceBox(newObject.gameObject, _data.fbxModel);
        }
        else// if (obj.category == "device")
        {
            newObject = await OgreeGenerator.instance.CreateItemFromSApiObject(obj, GameManager.instance.templatePlaceholder.GetChild(0));
            newObject.transform.GetChild(0).localScale = new Vector3(_data.sizeWDHmm[0], _data.sizeWDHmm[2], _data.sizeWDHmm[1]) / 1000;
            if (!string.IsNullOrEmpty(_data.fbxModel))
                await ModelLoader.instance.ReplaceBox(newObject.gameObject, _data.fbxModel);
        }
        newObject.transform.localPosition = Vector3.zero;

        newObject.GetComponent<OObject>().color = newObject.transform.GetChild(0).GetComponent<Renderer>().material.color;

        // Retrieve custom colors
        Dictionary<string, string> customColors = new Dictionary<string, string>();
        if (_data.colors != null)
        {
            foreach (SColor color in _data.colors)
                customColors.Add(color.name, color.value);
        }

        // Generate components & slots
        if (_data.components != null)
        {
            foreach (STemplateChild compData in _data.components)
                PopulateSlot(false, compData, newObject, customColors);
        }
        if (_data.slots != null)
        {
            foreach (STemplateChild slotData in _data.slots)
                PopulateSlot(true, slotData, newObject, customColors);
        }

        if (_data.sensors != null)
        {
            foreach (STemplateSensor sensor in _data.sensors)
                GenerateSensorTemplate(sensor, newObject.transform);
        }

        // For rack, update height counting
        if (newObject.category == "rack")
        {
            Slot[] slots = newObject.GetComponentsInChildren<Slot>();
            if (slots.Length > 0)
            {
                int height = 0;
                foreach (Slot s in slots)
                {
                    if (s.orient == "horizontal")
                        height++;
                }
                newObject.attributes["height"] = height.ToString();
                newObject.attributes["heightUnit"] = "U";
            }
        }

        // Toggle renderers & put newObj in GameManager.objectTemplates
#if PROD
        Renderer[] renderers = newObject.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
        newObject.transform.GetChild(0).GetComponent<Collider>().enabled = false;
#endif
        GameManager.instance.allItems.Remove(newObject.hierarchyName);
        GameManager.instance.objectTemplates.Add(newObject.name, newObject.gameObject);
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

        Vector2 parentSizeXZ = JsonUtility.FromJson<Vector2>(_parent.attributes["size"]);
        Vector3 parentSize = new Vector3(parentSizeXZ.x, float.Parse(_parent.attributes["height"]), parentSizeXZ.y);
        if (_parent.attributes["sizeUnit"] == "mm")
            parentSize /= 1000;
        else if (_parent.attributes["sizeUnit"] == "cm")
            parentSize /= 100;

        go.name = _data.location;
        go.transform.parent = _parent.transform;
        go.transform.GetChild(0).localScale = new Vector3(_data.elemSize[0], _data.elemSize[2], _data.elemSize[1]) / 1000;
        go.transform.localPosition = parentSize / -2;
        go.transform.localPosition += new Vector3(_data.elemPos[0], _data.elemPos[2], _data.elemPos[1]) / 1000;
        if (_data.elemOrient == "vertical")
        {
            go.transform.localEulerAngles = new Vector3(0, 0, 90);
            go.transform.localPosition += new Vector3(go.transform.GetChild(0).localScale.y,
                                                      go.transform.GetChild(0).localScale.x,
                                                      go.transform.GetChild(0).localScale.z) / 2;
        }
        else
        {
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localPosition += go.transform.GetChild(0).localScale / 2;
        }

        if (_isSlot)
        {
            Slot s = go.AddComponent<Slot>();
            s.orient = _data.elemOrient;
            if (_data.attributes != null && _data.attributes.ContainsKey("factor"))
                s.formFactor = _data.attributes["factor"];
            s.labelPos = _data.labelPos;

            go.transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        else
        {
            OObject obj = go.AddComponent<OObject>();
            obj.name = go.name;
            obj.parentId = _parent.id;
            obj.category = "device";
            obj.domain = _parent.domain;
            obj.description = new List<string>();
            obj.attributes = new Dictionary<string, string>
            {
                ["deviceType"] = _data.type
            };
            if (_data.attributes != null)
            {
                foreach (KeyValuePair<string, string> kvp in _data.attributes)
                    obj.attributes[kvp.Key] = kvp.Value;
            }
            obj.UpdateHierarchyName();
            obj.SetBaseTransform();
        }

        DisplayObjectData dod = go.GetComponent<DisplayObjectData>();
        dod.PlaceTexts(_data.labelPos);
        if (_isSlot)
            dod.SetLabelFont("color@888888");
        dod.SetLabel("#name");

        go.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.instance.defaultMat;
        Renderer rend = go.transform.GetChild(0).GetComponent<Renderer>();
        Color myColor;
        if (_data.color != null && _data.color.StartsWith("@"))
            ColorUtility.TryParseHtmlString($"#{_customColors[_data.color.Substring(1)]}", out myColor);
        else
            ColorUtility.TryParseHtmlString($"#{_data.color}", out myColor);
        if (_isSlot)
        {
            rend.material = GameManager.instance.alphaMat;
            rend.material.color = new Color(myColor.r, myColor.g, myColor.b, 0.33f);
        }
        else
        {
            rend.material.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
            go.GetComponent<OObject>().color = rend.material.color;
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
        newSensor.transform.localPosition = new Vector3(0, 0, 0);
        Vector3 offset = 0.5f * (_parent.GetChild(0).localScale - newSensor.transform.GetChild(0).localScale);
        switch (_sensor.elemPos[0])
        {
            case "left":
                newSensor.transform.localPosition += (offset.x) * Vector3.left;
                break;
            case "center":
                break;
            case "right":
                newSensor.transform.localPosition += (offset.x) * Vector3.right;
                break;
            default:
                try
                {
                    Vector3 pos = newSensor.transform.localPosition;
                    pos[0] = _parent.GetChild(0).localScale[0] / -2;
                    pos[0] += Utils.ParseDecFrac(_sensor.elemPos[0]) / 1000;
                    newSensor.transform.localPosition = pos;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine($"Wrong width pos value for sensor {_sensor.location} in template {_parent.name}", true, ELogtype.error);
                }
                break;
        }
        switch (_sensor.elemPos[1])
        {
            case "front":
                newSensor.transform.localPosition += (offset.z) * Vector3.forward;
                break;
            case "center":
                break;
            case "rear":
                newSensor.transform.localPosition += (offset.z) * Vector3.back;
                break;
            default:
                try
                {
                    Vector3 pos = newSensor.transform.localPosition;
                    pos[2] = _parent.GetChild(0).localScale[2] / -2;
                    pos[2] += Utils.ParseDecFrac(_sensor.elemPos[1]) / 1000;
                    newSensor.transform.localPosition = pos;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine($"Wrong depth pos value for sensor {_sensor.location} in template {_parent.name}", true, ELogtype.error);
                }
                break;
        }
        switch (_sensor.elemPos[2])
        {
            case "lower":
                newSensor.transform.localPosition += (offset.y) * Vector3.down;
                break;
            case "center":
                break;
            case "upper":
                newSensor.transform.localPosition += (offset.y) * Vector3.up;
                break;
            default:
                try
                {
                    Vector3 pos = newSensor.transform.localPosition;
                    pos[1] = _parent.GetChild(0).localScale[1] / -2;
                    pos[1] += Utils.ParseDecFrac(_sensor.elemPos[2]) / 1000;
                    newSensor.transform.localPosition = pos;
                }
                catch (FormatException)
                {
                    GameManager.instance.AppendLogLine($"Wrong height pos value for sensor {_sensor.location} in template {_parent.name}", true, ELogtype.error);
                }
                break;
        }
        Sensor sensor = newSensor.GetComponent<Sensor>();

        sensor.fromTemplate = true;
        newSensor.GetComponent<DisplayObjectData>().PlaceTexts(_sensor.elemPos[1]);
        newSensor.GetComponent<DisplayObjectData>().SetLabel("#temperature");
        newSensor.transform.GetChild(0).GetComponent<Collider>().enabled = false;
    }
}
