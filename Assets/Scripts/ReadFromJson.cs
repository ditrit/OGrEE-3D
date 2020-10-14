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
        public string type;
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
        public SRackSlot[] components;
    }

    [System.Serializable]
    private struct SRackSlot
    {
        public string location;
        public string family;
        public string role;
        public string installed;
        public string elemOrient;
        public int[] elemPos;
        public int[] elemSize;
        public string mandatory;
        public string labelPos;
        public string color;
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
        public string role;
        public string side;
        public string fulllength;
        public float[] sizeWDHmm;
        public SDeviceSlot[] components;
    }

    [System.Serializable]
    private struct SDeviceSlot
    {
        public string location;
        public string type;
        public string role;
        public string factor;
        public string position;
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

        SRackInfos infos = new SRackInfos();
        infos.name = rackData.slug;
        infos.parent = GameManager.gm.templatePlaceholder;
        infos.orient = "front";
        infos.size = new Vector3(rackData.sizeWDHmm[0], rackData.sizeWDHmm[2], rackData.sizeWDHmm[1]) / 10;
        Rack rack = ObjectGenerator.instance.CreateRack(infos, false);

        rack.transform.localPosition = Vector3.zero;
        rack.vendor = rackData.vendor;
        rack.model = rackData.model;
        foreach (SRackSlot comp in rackData.components)
        {
            SDeviceSlot slotData = new SDeviceSlot();
            slotData.location = comp.location;
            slotData.type = comp.family;
            slotData.role = comp.role;
            // slotData.factor = comp.factor;
            slotData.position = comp.installed; // not used
            slotData.elemOrient = comp.elemOrient;
            slotData.elemPos = comp.elemPos;
            slotData.elemSize = comp.elemSize;
            slotData.mandatory = comp.mandatory;
            slotData.labelPos = comp.labelPos;
            slotData.color = comp.color;

            PopulateSlot(slotData, rack.transform);
        }

        // Count the right height in U 
        Slot[] slots = rack.GetComponentsInChildren<Slot>();
        rack.height = 0;
        foreach (Slot s in slots)
        {
            if (s.orient == "horizontal")
                rack.height++;
        }

#if !DEBUG
        Renderer[] renderers = rack.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif

        GameManager.gm.allItems.Remove(rack.GetComponent<HierarchyName>().fullname);
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

        SDeviceInfos infos = new SDeviceInfos();
        infos.name = data.slug;
        infos.parent = GameManager.gm.templatePlaceholder.GetChild(0);
        infos.posU = 0;
        infos.sizeU = data.sizeWDHmm[2] / 10;

        Object device = ObjectGenerator.instance.CreateDevice(infos, false);
        device.transform.GetChild(0).localScale = new Vector3(data.sizeWDHmm[0], data.sizeWDHmm[2], data.sizeWDHmm[1]) / 1000;
        device.transform.localPosition = Vector3.zero;

        switch (data.type)
        {
            case "chassis":
                device.family = EObjFamily.chassis;
                break;
            case "blade":
                device.family = EObjFamily.device;
                break;
        }
        device.vendor = data.vendor;
        device.model = data.model;
        device.description = data.description;
        switch (data.side)
        {
            case "front":
                device.orient = EObjOrient.Frontward;
                break;
            case "rear":
                device.orient = EObjOrient.Backward;
                break;
        }
        if (data.fulllength == "yes")
            device.extras.Add("fulllength", "yes");

        foreach (SDeviceSlot comp in data.components)
            PopulateSlot(comp, device.transform);

#if !DEBUG
        Renderer[] renderers = device.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif

        GameManager.gm.allItems.Remove(device.GetComponent<HierarchyName>().fullname);
        GameManager.gm.devicesTemplates.Add(device.name, device.gameObject);
    }

    ///<summary>
    /// Create a child Slot object, with alpha=0.25.
    ///</summary>
    ///<param name="_data">Data for slot creation</param>
    ///<param name="_parent">The parent of the Slot</param>
    private void PopulateSlot(SDeviceSlot _data, Transform _parent)
    {
        // GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject go = MonoBehaviour.Instantiate(GameManager.gm.deviceModel);
        MonoBehaviour.Destroy(go.GetComponent<Object>());

        go.name = _data.location;
        go.transform.parent = _parent;
        go.transform.GetChild(0).localScale = new Vector3(_data.elemSize[0], _data.elemSize[2], _data.elemSize[1]) / 1000;
        go.transform.localPosition = go.transform.parent.GetChild(0).localScale / -2;
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

        Slot s = go.AddComponent<Slot>();
        s.installed = _data.position;
        s.orient = _data.elemOrient;
        s.mandatory = _data.mandatory;
        s.labelPos = _data.labelPos;

        DisplayObjectData dod = go.GetComponent<DisplayObjectData>();
        dod.Setup();
        dod.PlaceTexts(s.labelPos);
        dod.UpdateLabels(go.name);

        go.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.gm.defaultMat;
        Material mat = go.transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString($"#{_data.color}", out myColor);
        if (_data.mandatory == "yes")
            mat.color = new Color(myColor.r, myColor.g, myColor.b, 1f);
        else if (_data.mandatory == "no")
            mat.color = new Color(myColor.r, myColor.g, myColor.b, 0.25f);
    }

}
