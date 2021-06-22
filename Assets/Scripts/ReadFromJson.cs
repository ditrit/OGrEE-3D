using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReadFromJson
{
    #region Room
    [System.Serializable]
    public struct SRoomFromJson
    {
        public string slug;
        public string orientation;
        public float[] sizeWDHm;
        public int[] technicalArea;
        public int[] reservedArea;
        public SSeparator[] separators;
        public SColor[] colors;
        public STiles[] tiles;
        public SAisles[] aisles;
    }

    [System.Serializable]
    public struct SSeparator
    {
        public string name;
        public float[] pos1XYm;
        public float[] pos2XYm;
    }

    [System.Serializable]
    public struct STiles
    {
        public string location;
        public string name;
        public string label;
        public string texture;
        public string color;
    }

    [System.Serializable]
    public struct SAisles
    {
        public string name;
        public string locationY; // should be posY
        public string orientation;
    }
    #endregion

    #region Object
    [System.Serializable]
    private struct STemplate
    {
        public string slug;
        public string description;
        public string category;
        public float[] sizeWDHmm;
        public string fbxModel;
        public Dictionary<string, string> attributes;
        public SColor[] colors;
        public STemplateChild[] components;
        public STemplateChild[] slots;
    }

    [System.Serializable]
    private struct STemplateChild
    {
        public string location;
        public string type;
        public string elemOrient;
        public float[] elemPos;
        public float[] elemSize;
        public string labelPos;
        public string color;
        public Dictionary<string, string> attributes;
    }

    [System.Serializable]
    public struct SColor
    {
        public string name;
        public string value;
    }

    #endregion

    ///<summary>
    /// Store room data in GameManager.roomTemplates.
    ///</summary>
    ///<param name="_json">Json to parse</param>
    public void CreateRoomTemplate(string _json)
    {
        SRoomFromJson roomData = JsonUtility.FromJson<SRoomFromJson>(_json);
        if (GameManager.gm.roomTemplates.ContainsKey(roomData.slug))
            return;

        GameManager.gm.roomTemplates.Add(roomData.slug, roomData);
    }

    ///<summary>
    /// Create a rack or a device from received json and add it to correct GameManager list
    ///</summary>
    ///<param name="_json">Json to parse</param>
    public void CreateObjectTemplate(string _json)
    {
        STemplate data = JsonConvert.DeserializeObject<STemplate>(_json);
        if (data.category != "rack" && data.category != "device")
        {
            GameManager.gm.AppendLogLine($"Unknown category for {data.slug} template.", "red");
            return;
        }
        if (GameManager.gm.objectTemplates.ContainsKey(data.slug))
        {
            GameManager.gm.AppendLogLine($"{data.slug} already exists.", "yellow");
            return;
        }

        // Build SApiObject
        SApiObject obj = new SApiObject();
        obj.description = new List<string>();
        obj.attributes = new Dictionary<string, string>();

        obj.name = data.slug;
        obj.category = data.category;
        obj.description.Add(data.description);
        if (obj.category == "rack")
        {
            Vector3 tmp = new Vector3(data.sizeWDHmm[0], data.sizeWDHmm[1], data.sizeWDHmm[2]) / 10;
            obj.attributes["posXY"] = JsonUtility.ToJson(Vector2.zero);
            obj.attributes["posXYUnit"] = "Tile";
            obj.attributes["size"] = JsonUtility.ToJson(new Vector2(tmp.x, tmp.y));
            obj.attributes["sizeUnit"] = "cm";
            obj.attributes["height"] = ((int)tmp.z).ToString();
            obj.attributes["heightUnit"] = "cm";
            obj.attributes["orientation"] = "front";
        }
        else if (obj.category == "device")
        {
            if (data.attributes.ContainsKey("type")
                && (data.attributes["type"] == "chassis" || data.attributes["type"] == "server"))
            {
                int sizeU = Mathf.CeilToInt((data.sizeWDHmm[2] / 1000) / GameManager.gm.uSize);
                obj.attributes["sizeU"] = sizeU.ToString();
            }
            obj.attributes["size"] = JsonUtility.ToJson(new Vector2(data.sizeWDHmm[0], data.sizeWDHmm[1]));
            obj.attributes["sizeUnit"] = "mm";
            obj.attributes["height"] = data.sizeWDHmm[2].ToString();
            obj.attributes["heightUnit"] = "mm";
            obj.attributes["slot"] = "";
        }
        obj.attributes["template"] = "";
        obj.attributes["fbxModel"] = (!string.IsNullOrEmpty(data.fbxModel)).ToString();
        if (data.attributes != null)
        {
            foreach (KeyValuePair<string, string> kvp in data.attributes)
                obj.attributes[kvp.Key] = kvp.Value;
        }

        // Generate the 3D object
        OgreeObject newObject;
        if (obj.category == "rack")
        {
            newObject = ObjectGenerator.instance.CreateRack(obj, GameManager.gm.templatePlaceholder);
            if (!string.IsNullOrEmpty(data.fbxModel))
                ModelLoader.instance.ReplaceBox(newObject.gameObject, data.fbxModel);
        }
        else// if (obj.category == "device")
        {
            newObject = ObjectGenerator.instance.CreateDevice(obj, GameManager.gm.templatePlaceholder.GetChild(0));
            if (string.IsNullOrEmpty(data.fbxModel))
                newObject.transform.GetChild(0).localScale = new Vector3(data.sizeWDHmm[0], data.sizeWDHmm[2], data.sizeWDHmm[1]) / 1000;
            else
                ModelLoader.instance.ReplaceBox(newObject.gameObject, data.fbxModel);
        }
        newObject.transform.localPosition = Vector3.zero;

        // Retrieve custom colors
        Dictionary<string, string> customColors = new Dictionary<string, string>();
        if (data.colors != null)
        {
            foreach (SColor color in data.colors)
                customColors.Add(color.name, color.value);
        }

        // Generate components & slots
        if (data.components != null)
        {
            foreach (STemplateChild compData in data.components)
                PopulateSlot(false, compData, newObject, customColors);
        }
        if (data.slots != null)
        {
            foreach (STemplateChild slotData in data.slots)
                PopulateSlot(true, slotData, newObject, customColors);
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
#if !DEBUG
        Renderer[] renderers = newObject.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif
        GameManager.gm.allItems.Remove(newObject.hierarchyName);
        GameManager.gm.objectTemplates.Add(newObject.name, newObject.gameObject);
    }

    ///<summary>
    /// Create a child Slot or Component object.
    ///</summary>
    ///<param name="_isSlot">Device if it's a slot or a component to create</param>
    ///<param name="_data">Data for Slot/Component creation</param>
    ///<param name="_parent">The parent of the Slot/Component</param>
    ///<param name="_customColors">Custom colors to use</param>
    private void PopulateSlot(bool _isSlot, STemplateChild _data, OgreeObject _parent,
                                Dictionary<string, string> _customColors)
    {
        GameObject go = MonoBehaviour.Instantiate(GameManager.gm.labeledBoxModel);

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
            // obj.id // ??
            obj.parentId = _parent.id;
            obj.category = "device";
            obj.domain = _parent.domain;
            obj.description = new List<string>();
            obj.attributes = new Dictionary<string, string>();
            obj.attributes["deviceType"] = _data.type;
            if (_data.attributes != null)
            {
                foreach (KeyValuePair<string, string> kvp in _data.attributes)
                    obj.attributes[kvp.Key] = kvp.Value;
            }
            obj.UpdateHierarchyName();
        }

        DisplayObjectData dod = go.GetComponent<DisplayObjectData>();
        // dod.Setup();
        dod.PlaceTexts(_data.labelPos);
        dod.SetLabel("#name");

        go.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.gm.defaultMat;
        Material mat = go.transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        if (_data.color != null && _data.color.StartsWith("@"))
            ColorUtility.TryParseHtmlString($"#{_customColors[_data.color.Substring(1)]}", out myColor);
        else
            ColorUtility.TryParseHtmlString($"#{_data.color}", out myColor);
        if (_isSlot)
            mat.color = new Color(myColor.r, myColor.g, myColor.b, 0.33f);
        else
            mat.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
    }

}
