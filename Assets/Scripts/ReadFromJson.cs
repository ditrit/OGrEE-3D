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

    #region Rack
    [System.Serializable]
    private struct SRackFromJson
    {
        public string name;
        public string slug;
        public string vendor;
        public string model;
        public string type;
        public int[] sizeWDHmm;
        public SColor[] colors;
        public SRackSlot[] components;
        public SRackSlot[] slots;
    }

    [System.Serializable]
    private struct SRackSlot
    {
        public string location;
        public string family;
        // public string installed;// to del
        public string elemOrient;
        public int[] elemPos;
        public int[] elemSize;
        public string mandatory;
        public string labelPos;
        public string color;
    }

    [System.Serializable]
    public struct SColor
    {
        public string name;
        public string value;
    }

    #endregion

    #region Device
    [System.Serializable]
    private struct SDevice
    {
        public string slug;
        public string description;
        public string vendor;
        public string model;
        public string type;
        public string side;
        public string fulldepth;
        public float[] sizeWDHmm;
        public string fbxModel;
        public SColor[] colors;
        public SDeviceSlot[] components;
        public SDeviceSlot[] slots;
    }

    [System.Serializable]
    private struct SDeviceSlot
    {
        public string location;
        public string type;
        public string factor; // ?
        // public string position; // to del
        public string elemOrient;
        public int[] elemPos;
        public int[] elemSize;
        public string mandatory;
        public string labelPos;
        public string color;
    }
    #endregion


    ///<summary>
    /// Create a rack from _json data and add it to GameManager.rackTemplates.
    ///</summary>
    ///<param name="_json">Json to parse</param>
    public void CreateRackTemplate(string _json)
    {
        SRackFromJson rackData = JsonUtility.FromJson<SRackFromJson>(_json);
        if (rackData.type != "rack")
        {
            GameManager.gm.AppendLogLine($"{rackData.slug} is a {rackData.type}, not a rack.", "red");
            return;
        }
        if (GameManager.gm.rackTemplates.ContainsKey(rackData.slug))
        {
            GameManager.gm.AppendLogLine($"{rackData.slug} already exists.", "yellow");
            return;
        }

        Vector3 tmp = new Vector3(rackData.sizeWDHmm[0], rackData.sizeWDHmm[1], rackData.sizeWDHmm[2]) / 10;
        SApiObject rk = new SApiObject();
        rk.description = new List<string>();
        rk.attributes = new Dictionary<string, string>();
        rk.name = rackData.slug;
        rk.category = "rack";
        rk.attributes["posXY"] = JsonUtility.ToJson(Vector2.zero);
        rk.attributes["posXYUnit"] = "Tile";
        rk.attributes["size"] = JsonUtility.ToJson(new Vector2(tmp.x, tmp.y));
        rk.attributes["sizeUnit"] = "cm";
        rk.attributes["height"] = ((int)tmp.z).ToString();
        rk.attributes["heightUnit"] = "cm";
        rk.attributes["template"] = "";
        rk.attributes["orientation"] = "front";
        rk.attributes["vendor"] = rackData.vendor;
        rk.attributes["model"] = rackData.model;
        Rack rack = ObjectGenerator.instance.CreateRack(rk, GameManager.gm.templatePlaceholder);

        rack.transform.localPosition = Vector3.zero;
        Dictionary<string, string> customColors = new Dictionary<string, string>();
        if (rackData.colors != null)
        {
            foreach (SColor color in rackData.colors)
                customColors.Add(color.name, color.value);
        }
        if (rackData.components != null)
        {
            foreach (SRackSlot comp in rackData.components)
            {
                SDeviceSlot compData = new SDeviceSlot();
                compData.location = comp.location;
                compData.type = comp.family;
                compData.elemOrient = comp.elemOrient;
                compData.elemPos = comp.elemPos;
                compData.elemSize = comp.elemSize;
                compData.mandatory = comp.mandatory;
                compData.labelPos = comp.labelPos;
                compData.color = comp.color;
                PopulateSlot(false, compData, rack, customColors);
            }
        }
        if (rackData.slots != null)
        {
            foreach (SRackSlot comp in rackData.slots)
            {
                SDeviceSlot slotData = new SDeviceSlot();
                slotData.location = comp.location;
                slotData.type = comp.family;
                // slotData.factor = comp.factor;
                // slotData.position = comp.installed; // not used
                slotData.elemOrient = comp.elemOrient;
                slotData.elemPos = comp.elemPos;
                slotData.elemSize = comp.elemSize;
                slotData.mandatory = comp.mandatory;
                slotData.labelPos = comp.labelPos;
                slotData.color = comp.color;
                PopulateSlot(true, slotData, rack, customColors);
            }
        }

        // Count the right height in U 
        Slot[] slots = rack.GetComponentsInChildren<Slot>();
        if (slots.Length > 0)
        {
            int height = 0;
            foreach (Slot s in slots)
            {
                if (s.orient == "horizontal")
                    height++;
            }
            rack.attributes["height"] = height.ToString();
            rack.attributes["heightUnit"] = "U";
        }

#if !DEBUG
        Renderer[] renderers = rack.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif

        GameManager.gm.allItems.Remove(rack.hierarchyName);
        GameManager.gm.rackTemplates.Add(rack.name, rack.gameObject);
    }

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
    /// Create a chassis from _json data and add it to GameManager.chassisTemplates.
    ///</summary>
    ///<param name="_json">Json to parse</param>
    public void CreateDeviceTemplate(string _json)
    {
        SDevice data = JsonUtility.FromJson<SDevice>(_json);
        if (data.type == "rack")
        {
            GameManager.gm.AppendLogLine($"{data.slug} is a rack, not a device.", "red");
            return;
        }
        if (GameManager.gm.rackTemplates.ContainsKey(data.slug))
        {
            GameManager.gm.AppendLogLine($"{data.slug} already exists.", "yellow");
            return;
        }

        if (GameManager.gm.devicesTemplates.ContainsKey(data.slug))
            return;

        SApiObject dv = new SApiObject();
        dv.description = new List<string>();
        dv.attributes = new Dictionary<string, string>();
        dv.name = data.slug;
        dv.category = "device";
        dv.attributes["posU"] = "0";
        dv.attributes["sizeU"] = (data.sizeWDHmm[2] / 10).ToString();
        dv.attributes["size"] = JsonUtility.ToJson(new Vector2(data.sizeWDHmm[0], data.sizeWDHmm[1]));
        dv.attributes["sizeUnit"] = "mm";
        dv.attributes["height"] = data.sizeWDHmm[2].ToString();
        dv.attributes["heightUnit"] = "mm";
        dv.attributes["template"] = "";
        dv.attributes["slot"] = "";

        dv.description.Add(data.description);
        dv.attributes["deviceType"] = data.type;
        dv.attributes["vendor"] = data.vendor;
        dv.attributes["model"] = data.model;
        dv.attributes["orientation"] = data.side;
        if (data.fulldepth == "yes")
        {
            dv.attributes["fulldepth"] = "yes";
            dv.attributes["orientation"] = "front";
        }
        else if (data.fulldepth == "no")
            dv.attributes["fulldepth"] = "no";

        OObject device = ObjectGenerator.instance.CreateDevice(dv, GameManager.gm.templatePlaceholder.GetChild(0));
        if (string.IsNullOrEmpty(data.fbxModel))
            device.transform.GetChild(0).localScale = new Vector3(data.sizeWDHmm[0], data.sizeWDHmm[2], data.sizeWDHmm[1]) / 1000;
        else
            ModelGenerator.instance.ReplaceBox(device.gameObject, data.fbxModel);
        device.transform.localPosition = Vector3.zero;

        Dictionary<string, string> customColors = new Dictionary<string, string>();
        if (data.colors != null)
        {
            foreach (SColor color in data.colors)
                customColors.Add(color.name, color.value);
        }
        if (data.components != null)
        {
            foreach (SDeviceSlot compData in data.components)
                PopulateSlot(false, compData, device, customColors);
        }
        if (data.slots != null)
        {
            foreach (SDeviceSlot slotData in data.slots)
                PopulateSlot(true, slotData, device, customColors);
        }

#if !DEBUG
        Renderer[] renderers = device.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif

        GameManager.gm.allItems.Remove(device.hierarchyName);
        GameManager.gm.devicesTemplates.Add(device.name, device.gameObject);
    }

    ///<summary>
    /// Create a child Slot object, with alpha=0.25.
    ///</summary>
    ///<param name="_data">Data for slot creation</param>
    ///<param name="_parent">The parent of the Slot</param>
    private void PopulateSlot(bool isSlot, SDeviceSlot _data, OgreeObject _parent,
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

        if (isSlot)
        {
            Slot s = go.AddComponent<Slot>();
            s.orient = _data.elemOrient;
            s.mandatory = _data.mandatory;
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
            obj.UpdateHierarchyName();
        }

        DisplayObjectData dod = go.GetComponent<DisplayObjectData>();
        // dod.Setup();
        dod.PlaceTexts(_data.labelPos);
        dod.SetLabel("name");

        go.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.gm.defaultMat;
        Material mat = go.transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        if (_data.color != null && _data.color.StartsWith("@"))
            ColorUtility.TryParseHtmlString($"#{_customColors[_data.color.Substring(1)]}", out myColor);
        else
            ColorUtility.TryParseHtmlString($"#{_data.color}", out myColor);
        if (isSlot)
        {
            if (_data.mandatory == "yes")
                mat.color = new Color(myColor.r, myColor.g, myColor.b, 0.5f);
            else if (_data.mandatory == "no")
                mat.color = new Color(myColor.r, myColor.g, myColor.b, 0.2f);
        }
        else
            mat.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
    }

}
