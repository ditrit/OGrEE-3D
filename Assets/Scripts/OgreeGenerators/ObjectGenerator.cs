using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// using System.Linq;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    public static ObjectGenerator instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Instantiate a rackModel or a rackTemplate (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_rk">The rack data to apply</param>
    ///<param name="_parent">The parent of the created rack. Leave null if _bd contains the parendId</param>
    ///<returns>The created Rack</returns>
    public Rack CreateRack(SApiObject _rk, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _rk.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_rk.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newRack;
        if (string.IsNullOrEmpty(_rk.attributes["template"]))
            newRack = Instantiate(GameManager.gm.rackModel);
        else
        {
            if (GameManager.gm.rackTemplates.ContainsKey(_rk.attributes["template"]))
                newRack = Instantiate(GameManager.gm.rackTemplates[_rk.attributes["template"]]);
            else
            {
                GameManager.gm.AppendLogLine($"Unknown template \"{_rk.attributes["template"]}\"", "yellow");
                return null;
            }
            Renderer[] renderers = newRack.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
        }

        newRack.name = _rk.name;
        newRack.transform.parent = parent;

        if (string.IsNullOrEmpty(_rk.attributes["template"]))
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(_rk.attributes["size"]);
            float height = float.Parse(_rk.attributes["height"]);
            if (_rk.attributes["heightUnit"] == "U")
                height *= GameManager.gm.uSize;
            else if (_rk.attributes["heightUnit"] == "cm")
                height /= 100;
            newRack.transform.GetChild(0).localScale = new Vector3(size.x / 100, height, size.y / 100);
        }

        Vector2 pos = JsonUtility.FromJson<Vector2>(_rk.attributes["posXY"]);
        Vector3 origin = newRack.transform.parent.GetChild(0).localScale / 0.2f;
        Vector3 boxOrigin = newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.position = newRack.transform.parent.GetChild(0).position;

        Vector2 orient = Vector2.one;
        if (parent.GetComponent<Room>().attributes.ContainsKey("orientation"))
        {
            if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Left corner of the room
                orient = new Vector2(1, 1);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Right corner of the room
                orient = new Vector2(-1, 1);
                newRack.transform.localPosition -= new Vector3(GameManager.gm.tileSize, 0, 0);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Right corner of the room
                orient = new Vector2(-1, -1);
                newRack.transform.localPosition -= new Vector3(GameManager.gm.tileSize, 0, GameManager.gm.tileSize);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Left corner of the room
                orient = new Vector2(1, -1);
                newRack.transform.localPosition -= new Vector3(0, 0, GameManager.gm.tileSize);
            }
        }
        newRack.transform.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);
        newRack.transform.localPosition += new Vector3(pos.x * orient.x, 0, pos.y * orient.y) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.name = newRack.name;
        rack.id = _rk.id;
        rack.parentId = _rk.parentId;
        if (string.IsNullOrEmpty(rack.parentId))
            rack.parentId = parent.GetComponent<OgreeObject>().id;
        rack.category = "rack";
        rack.description = _rk.description;
        rack.domain = _rk.domain;
        if (string.IsNullOrEmpty(rack.domain))
            rack.domain = parent.GetComponent<OgreeObject>().domain;
        if (string.IsNullOrEmpty(_rk.attributes["template"]))
            rack.attributes = _rk.attributes;
        else
        {
            rack.attributes["template"] = _rk.attributes["template"];
            rack.attributes["posXY"] = _rk.attributes["posXY"];
            rack.attributes["posXYUnit"] = _rk.attributes["posXYUnit"];
            rack.attributes["orientation"] = _rk.attributes["orientation"];
        }

        Vector3 fixPos = Vector3.zero;
        switch (rack.attributes["orientation"])
        {
            case "front":
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z + GameManager.gm.tileSize);
                break;
            case "rear":
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z + GameManager.gm.tileSize);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                break;
            case "left":
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(-boxOrigin.z + GameManager.gm.tileSize, boxOrigin.y, boxOrigin.x);
                else
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, boxOrigin.x);
                break;
            case "right":
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x + GameManager.gm.tileSize);
                else
                    fixPos = new Vector3(-boxOrigin.z + GameManager.gm.tileSize, boxOrigin.y, -boxOrigin.x + GameManager.gm.tileSize);
                break;
        }
        newRack.transform.localPosition += fixPos;

        newRack.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
        newRack.GetComponent<DisplayObjectData>().SetLabel("#name");

        rack.UpdateColor();
        GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = rack.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            Object[] components = rack.transform.GetComponentsInChildren<Object>();
            foreach (Object comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return rack;
    }

    ///<summary>
    /// Instantiate a deviceModel or a deviceTemplate (from GameManager) and apply given data to it.
    ///</summary>
    ///<param name="_dv">The device data to apply</param>
    ///<param name="_parent">The parent of the created device. Leave null if _bd contains the parendId</param>
    ///<returns>The created Device</returns>
    public Object CreateDevice(SApiObject _dv, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _dv.parentId);
        if (!parent || parent.GetComponent<Object>() == null)
        {
            GameManager.gm.AppendLogLine($"Device must be child of a Rack or another Device", "red");
            return null;
        }

        if (parent.GetComponent<Rack>() == null
            && (!_dv.attributes.ContainsKey("slot") || string.IsNullOrEmpty(_dv.attributes["template"])))
        {
            GameManager.gm.AppendLogLine("A sub-device needs to be declared with a parent's slot and a template", "red");
            return null;
        }

        if (parent.gameObject.activeSelf == false)
        {
            GameManager.gm.AppendLogLine("The parent rack must be active (not hidden in a rackGroup)", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_dv.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "red");
            return null;
        }

        GameObject newDevice;
        if (!_dv.attributes.ContainsKey("slot"))
        {
            //+chassis:[name]@[posU]@[sizeU]
            if (string.IsNullOrEmpty(_dv.attributes["template"]))
                newDevice = GenerateBasicDevice(parent, float.Parse(_dv.attributes["sizeU"]));
            //+chassis:[name]@[posU]@[template]
            else
            {
                newDevice = GenerateTemplatedDevice(parent, _dv.attributes["template"]);
                if (newDevice == null)
                    return null;
            }
            newDevice.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
            newDevice.transform.localEulerAngles = Vector3.zero;
            newDevice.transform.localPosition = new Vector3(0, (-parent.GetChild(0).localScale.y + newDevice.transform.GetChild(0).localScale.y) / 2, 0);
            newDevice.transform.localPosition += new Vector3(0, (float.Parse(_dv.attributes["posU"]) - 1) * GameManager.gm.uSize, 0);

            float deltaZ = parent.GetChild(0).localScale.z - newDevice.transform.GetChild(0).localScale.z;
            newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
        }
        else
        {
            List<Slot> takenSlots = new List<Slot>();
            int i = 0;
            float max;
            if (string.IsNullOrEmpty(_dv.attributes["template"]))
                max = float.Parse(_dv.attributes["sizeU"]);
            else
            {
                if (GameManager.gm.devicesTemplates.ContainsKey(_dv.attributes["template"]))
                    max = GameManager.gm.devicesTemplates[_dv.attributes["template"]].transform.GetChild(0).localScale.y / GameManager.gm.uSize;
                else
                {
                    GameManager.gm.AppendLogLine($"Unknown template \"{_dv.attributes["template"]}\"", "yellow");
                    return null;
                }
            }
            foreach (Transform child in parent)
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

                Transform slot = takenSlots[0].transform;
                //+chassis:[name]@[slot]@[sizeU]
                if (string.IsNullOrEmpty(_dv.attributes["template"]))
                    newDevice = GenerateBasicDevice(parent, float.Parse(_dv.attributes["sizeU"]), takenSlots[0].transform);
                //+chassis:[name]@[slot]@[template]
                else
                {
                    newDevice = GenerateTemplatedDevice(parent, _dv.attributes["template"]);
                    if (newDevice == null)
                        return null;
                }
                newDevice.GetComponent<DisplayObjectData>().PlaceTexts(slot.GetComponent<Slot>().labelPos);
                newDevice.transform.localPosition = slot.localPosition;
                newDevice.transform.localEulerAngles = slot.localEulerAngles;
                if (newDevice.transform.GetChild(0).localScale.y > slot.GetChild(0).localScale.y)
                    newDevice.transform.localPosition += new Vector3(0, newDevice.transform.GetChild(0).localScale.y / 2 - GameManager.gm.uSize / 2, 0);

                float deltaZ = slot.GetChild(0).localScale.z - newDevice.transform.GetChild(0).localScale.z;
                switch (_dv.attributes["orientation"])
                {
                    case "front":
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                        break;
                    case "rear":
                        newDevice.transform.localPosition -= new Vector3(0, 0, deltaZ / 2);
                        newDevice.transform.localEulerAngles += new Vector3(0, 180, 0);
                        break;
                    case "frontflipped":
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                        newDevice.transform.localEulerAngles += new Vector3(0, 0, 180);
                        break;
                    case "rearflipped":
                        newDevice.transform.localPosition -= new Vector3(0, 0, deltaZ / 2);
                        newDevice.transform.localEulerAngles += new Vector3(180, 0, 0);
                        break;
                }

                // Assign default color = slot color
                Material mat = newDevice.transform.GetChild(0).GetComponent<Renderer>().material;
                Color slotColor = slot.GetChild(0).GetComponent<Renderer>().material.color;
                mat.color = new Color(slotColor.r, slotColor.g, slotColor.b);
            }
            else
            {
                GameManager.gm.AppendLogLine("Slot doesn't exist", "red");
                return null;
            }
        }

        newDevice.name = _dv.name;
        Object obj = newDevice.GetComponent<Object>();
        obj.name = _dv.name;
        obj.id = _dv.id;
        obj.parentId = _dv.parentId;
        if (string.IsNullOrEmpty(obj.parentId))
            obj.parentId = parent.GetComponent<OgreeObject>().id;
        obj.category = "device";
        obj.description = _dv.description;
        obj.domain = _dv.domain;
        if (string.IsNullOrEmpty(obj.domain))
            obj.domain = parent.GetComponent<OgreeObject>().domain;
        if (string.IsNullOrEmpty(_dv.attributes["template"]))
            obj.attributes = _dv.attributes;
        else
        {
            obj.attributes["template"] = _dv.attributes["template"];
            obj.attributes["orientation"] = _dv.attributes["orientation"];
            if (_dv.attributes.ContainsKey("posU"))
                obj.attributes["posU"] = _dv.attributes["posU"];
            if (_dv.attributes.ContainsKey("slot"))
                obj.attributes["slot"] = _dv.attributes["slot"];
        }

        newDevice.GetComponent<DisplayObjectData>().SetLabel("#name");

        string hn = obj.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newDevice);

        if (_dv.attributes.ContainsKey("template"))
        {
            Object[] components = newDevice.transform.GetComponentsInChildren<Object>();
            foreach (Object comp in components)
            {
                if (comp.gameObject != newDevice.gameObject)
                {
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return obj;
    }

    ///<summary>
    /// Generate a basic device.
    ///</summary>
    ///<param name="_parent">The parent of the generated device</param>
    ///<param name="_sizeU">The size in U of the device</param>
    ///<returns>The generated device</returns>
    private GameObject GenerateBasicDevice(Transform _parent, float _sizeU, Transform _slot = null)
    {
        GameObject go = Instantiate(GameManager.gm.labeledBoxModel);
        go.AddComponent<Object>();
        go.transform.parent = _parent;
        Vector3 scale;
        if (_slot)
            scale = new Vector3(_slot.GetChild(0).localScale.x, _sizeU * _slot.GetChild(0).localScale.y, _slot.GetChild(0).localScale.z);
        else
            scale = new Vector3(_parent.GetChild(0).localScale.x, _sizeU * GameManager.gm.uSize, _parent.GetChild(0).localScale.z);
        go.transform.GetChild(0).localScale = scale;
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
        if (GameManager.gm.devicesTemplates.ContainsKey(_template))
        {
            GameObject go = Instantiate(GameManager.gm.devicesTemplates[_template]);
            go.transform.parent = _parent;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            return go;
        }
        else
        {
            GameManager.gm.AppendLogLine($"Unknown template \"{_template}\"", "yellow");
            return null;
        }
    }

    ///<summary>
    /// Generate a group (from GameManager.labeledBoxModel) which contains all the given objects.
    ///</summary>
    ///<param name="_gr">The group data to apply</param>
    ///<param name="_parent">The parent of the generated group</param>
    ///<returns>The created rackGroup</returns>
    public Group CreateGroup(SApiObject _gr, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _gr.parentId);
        if (!parent)
        {
            GameManager.gm.AppendLogLine("Parent not found", "red");
            return null;
        }
        string parentCategory = parent.GetComponent<OgreeObject>().category;
        if (parentCategory != "room" && parentCategory != "rack")
        {
            GameManager.gm.AppendLogLine("A group must be a child of a room or a rack", "red");
            return null;
        }

        List<Transform> content = new List<Transform>();
        string[] contentNames = _gr.attributes["content"].Split(',');
        foreach (string cn in contentNames)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{parent.GetComponent<OgreeObject>().hierarchyName}.{cn}");
            if (go && go.GetComponent<OgreeObject>())
            {
                if ((parentCategory == "room" && (go.GetComponent<OgreeObject>().category == "rack" || go.GetComponent<OgreeObject>().category == "corridor"))
                    || parentCategory == "rack" && go.GetComponent<OgreeObject>().category == "device")
                    content.Add(go.transform);
            }
            else
                GameManager.gm.AppendLogLine($"{parent.GetComponent<OgreeObject>().hierarchyName}.{cn} doesn't exists.", "yellow");
        }
        if (content.Count == 0)
            return null;

        GameObject newGr = Instantiate(GameManager.gm.labeledBoxModel);
        newGr.name = _gr.name;
        newGr.transform.parent = _parent;

        // According to group type, set pos, rot & scale
        Vector3 pos = Vector3.zero;
        Vector3 scale = Vector3.zero;
        if (parentCategory == "room")
        {
            RackGroupPosScale(content, out pos, out scale);
            newGr.transform.localEulerAngles = new Vector3(0, 180, 0);
        }
        else if (parentCategory == "rack")
        {
            DeviceGroupPosScale(content, out pos, out scale);
            newGr.transform.localEulerAngles = Vector3.zero;
        }
        newGr.transform.localPosition = pos;
        newGr.transform.GetChild(0).localScale = scale;

        // Set Group component
        Group gr = newGr.AddComponent<Group>();
        gr.name = newGr.name;
        gr.parentId = _gr.parentId;
        if (string.IsNullOrEmpty(gr.parentId))
            gr.parentId = parent.GetComponent<OgreeObject>().id;
        gr.category = "group";
        gr.domain = _gr.domain;
        if (string.IsNullOrEmpty(gr.domain))
            gr.domain = content[0].GetComponent<OgreeObject>().domain;
        gr.description = _gr.description;
        gr.attributes = _gr.attributes;
        gr.DisplayContent(false);

        if (parentCategory == "room")
            newGr.GetComponent<DisplayObjectData>().PlaceTexts("top");
        else if (parentCategory == "rack")
            newGr.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
        newGr.GetComponent<DisplayObjectData>().SetLabel("#name");

        string hn = gr.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newGr);

        return gr;
    }

    ///<summary>
    /// For a group of Racks, set _pos and _scale
    ///</summary>
    ///<param name="_racks">The list of racks in the group</param>
    ///<param name="_pos">The localPosition to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private void RackGroupPosScale(List<Transform> _racks, out Vector3 _pos, out Vector3 _scale)
    {
        Transform rackAtLowerLeft = _racks[0];
        Transform rackAtRight = _racks[0];
        Transform rackAtTop = _racks[0];
        float maxHeight = 0;
        float maxLength = 0;
        foreach (Transform rk in _racks)
        {
            if (rk.GetComponent<OgreeObject>().category == "rack")
            {
                if (rk.localPosition.x <= rackAtLowerLeft.localPosition.x && rk.localPosition.z <= rackAtLowerLeft.localPosition.z)
                    rackAtLowerLeft = rk;
                if (rk.localPosition.x > rackAtRight.localPosition.x)
                    rackAtRight = rk;
                if (rk.localPosition.z > rackAtTop.localPosition.z)
                    rackAtTop = rk;

                if (rk.GetChild(0).localScale.y > maxHeight)
                    maxHeight = rk.GetChild(0).localScale.y;
                if (rk.GetChild(0).localScale.z > maxLength)
                    maxLength = rk.GetChild(0).localScale.z;
            }
        }

        float witdh = rackAtRight.localPosition.x - rackAtLowerLeft.localPosition.x;
        float length = rackAtTop.localPosition.z - rackAtLowerLeft.localPosition.z;
        if (rackAtLowerLeft.GetComponent<Rack>().attributes["orientation"] == "front"
            || rackAtLowerLeft.GetComponent<Rack>().attributes["orientation"] == "rear")
        {
            witdh += (rackAtRight.GetChild(0).localScale.x + rackAtLowerLeft.GetChild(0).localScale.x) / 2;
            length -= (rackAtTop.GetChild(0).localScale.z + rackAtLowerLeft.GetChild(0).localScale.z) / 2;
            length += maxLength * 2;
        }
        else
        {
            length += (rackAtRight.GetChild(0).localScale.x + rackAtLowerLeft.GetChild(0).localScale.x) / 2;
            witdh -= (rackAtTop.GetChild(0).localScale.z + rackAtLowerLeft.GetChild(0).localScale.z) / 2;
            witdh += maxLength * 2;
        }
        _scale = new Vector3(witdh, maxHeight, length);

        float xOffset;
        float zOffset;
        if (rackAtLowerLeft.GetComponent<Rack>().attributes["orientation"] == "front"
            || rackAtLowerLeft.GetComponent<Rack>().attributes["orientation"] == "rear")
        {
            xOffset = (_scale.x - rackAtLowerLeft.GetChild(0).localScale.x) / 2;
            zOffset = (_scale.z + rackAtLowerLeft.GetChild(0).localScale.z) / 2 - maxLength;
        }
        else
        {
            xOffset = (_scale.x + rackAtLowerLeft.GetChild(0).localScale.z) / 2 - maxLength;
            zOffset = (_scale.z - rackAtLowerLeft.GetChild(0).localScale.x) / 2;
        }
        _pos = new Vector3(rackAtLowerLeft.localPosition.x, maxHeight / 2, rackAtLowerLeft.localPosition.z);
        _pos += new Vector3(xOffset, 0, zOffset);
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

        _pos = lowerDv.localPosition;
        _pos += new Vector3(0, (_scale.y - lowerDv.GetChild(0).localScale.y) / 2, 0);

    }

    ///<summary>
    /// Generate a corridor (from GameManager.labeledBoxModel) with defined corners and color.
    ///</summary>
    ///<param name="_name">The name of the corridor</param>
    ///<param name="_parent">The parent of the generated corridor</param>
    ///<param name="_cornerRacks">The well formatted list of racks/corners (r1,r2)</param>
    ///<param name="_temp">"cold" or "warm" value</param>
    ///<returns>The created corridor</returns>
    public Object CreateCorridor(SApiObject _co, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _co.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        string roomHierarchyName = parent.GetComponent<OgreeObject>().hierarchyName;
        string[] rackNames = _co.attributes["content"].Split(',');
        Transform lowerLeft = GameManager.gm.FindByAbsPath($"{roomHierarchyName}.{rackNames[0]}")?.transform;
        Transform upperRight = GameManager.gm.FindByAbsPath($"{roomHierarchyName}.{rackNames[1]}")?.transform;

        if (lowerLeft == null || upperRight == null)
        {
            GameManager.gm.AppendLogLine($"{rackNames[0]} or {rackNames[1]} doesn't exist", "red");
            return null;
        }

        float maxHeight = lowerLeft.GetChild(0).localScale.y;
        if (upperRight.GetChild(0).localScale.y > maxHeight)
            maxHeight = upperRight.GetChild(0).localScale.y;

        GameObject newCo = Instantiate(GameManager.gm.labeledBoxModel);
        newCo.name = _co.name;
        newCo.transform.parent = parent;

        float x = upperRight.localPosition.x - lowerLeft.localPosition.x;
        float z = upperRight.localPosition.z - lowerLeft.localPosition.z;
        if (lowerLeft.GetComponent<Rack>().attributes["orientation"] == "front"
            || lowerLeft.GetComponent<Rack>().attributes["orientation"] == "rear")
        {
            x += (upperRight.GetChild(0).localScale.x + lowerLeft.GetChild(0).localScale.x) / 2;
            z -= (upperRight.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2;
        }
        else
        {
            x += (upperRight.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2;
            z -= (upperRight.GetChild(0).localScale.x + lowerLeft.GetChild(0).localScale.x) / 2;
        }
        newCo.transform.GetChild(0).localScale = new Vector3(x, maxHeight, z);

        newCo.transform.localEulerAngles = new Vector3(0, 180, 0);
        newCo.transform.localPosition = new Vector3(lowerLeft.localPosition.x, maxHeight / 2, lowerLeft.localPosition.z);
        float xOffset = (newCo.transform.GetChild(0).localScale.x - lowerLeft.GetChild(0).localScale.x) / 2;
        float zOffset = (newCo.transform.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2;
        newCo.transform.localPosition += new Vector3(xOffset, 0, zOffset);

        Object co = newCo.AddComponent<Object>();
        co.name = newCo.name;
        co.parentId = _co.parentId;
        if (string.IsNullOrEmpty(co.parentId))
            co.parentId = parent.GetComponent<Room>().id;
        co.category = "corridor";
        co.domain = _co.domain;
        if (string.IsNullOrEmpty(co.domain))
            co.domain = lowerLeft.GetComponent<Rack>().domain;
        co.attributes = _co.attributes;

        Material mat = newCo.transform.GetChild(0).GetComponent<Renderer>().material;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.5f);
        if (_co.attributes["temperature"] == "cold")
            co.SetAttribute("color", "000099");
        else
            co.SetAttribute("color", "990000");

        newCo.GetComponent<DisplayObjectData>().PlaceTexts("top");
        newCo.GetComponent<DisplayObjectData>().SetLabel("#name");

        string hn = co.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newCo);

        return co;
    }

}
