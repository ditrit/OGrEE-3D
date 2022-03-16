using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

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
    ///<param name="_parent">The parent of the created rack. Leave null if _rk contains the parendId</param>
    ///<param name="_copyAttr">If false, do not copy all attributes</param>
    ///<returns>The created Rack</returns>
    public Rack CreateRack(SApiObject _rk, Transform _parent = null, bool _copyAttr = true)
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
            if (GameManager.gm.objectTemplates.ContainsKey(_rk.attributes["template"]))
                newRack = Instantiate(GameManager.gm.objectTemplates[_rk.attributes["template"]]);
            else
            {
                GameManager.gm.AppendLogLine($"Unknown template \"{_rk.attributes["template"]}\"", "yellow");
                return null;
            }
            Renderer[] renderers = newRack.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            newRack.transform.GetChild(0).GetComponent<Collider>().enabled = true;
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

        Vector2 orient;
        PlaceInRoom(newRack.transform, _rk, out orient);

        Rack rack = newRack.GetComponent<Rack>();
        rack.UpdateFromSApiObject(_rk, _copyAttr);
        if (!_copyAttr)
        {
            rack.attributes["template"] = _rk.attributes["template"];
            rack.attributes["posXY"] = _rk.attributes["posXY"];
            rack.attributes["posXYUnit"] = _rk.attributes["posXYUnit"];
            rack.attributes["orientation"] = _rk.attributes["orientation"];
        }

        // Correct position according to rack size & rack orientation
        Vector3 boxOrigin;
        Transform box = newRack.transform.GetChild(0);
        if (box.childCount == 0)
            boxOrigin = box.localScale / 2;
        else
            boxOrigin = box.GetComponent<BoxCollider>().size / 2;
        float floorUnit = GetUnitFromRoom(parent.GetComponent<Room>());
        Vector3 fixPos = Vector3.zero;
        switch (rack.attributes["orientation"])
        {
            case "front":
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z + floorUnit);
                break;
            case "rear":
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z + floorUnit);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                break;
            case "left":
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(-boxOrigin.z + floorUnit, boxOrigin.y, boxOrigin.x);
                else
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, boxOrigin.x);
                break;
            case "right":
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x + floorUnit);
                else
                    fixPos = new Vector3(-boxOrigin.z + floorUnit, boxOrigin.y, -boxOrigin.x + floorUnit);
                break;
        }
        newRack.transform.localPosition += fixPos;

        newRack.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
        newRack.GetComponent<DisplayObjectData>().SetLabel("#name");

        rack.UpdateColorByTenant();
        GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = rack.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            OObject[] components = rack.transform.GetComponentsInChildren<OObject>();
            foreach (OObject comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    comp.domain = rack.domain;
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }
        newRack.transform.GetChild(0).GetComponent<BoxCollider>().enabled = true;
        newRack.transform.GetChild(0).gameObject.AddComponent<NearInteractionTouchableVolume>();
        newRack.transform.GetChild(0).gameObject.AddComponent<HandInteractionHandler>();
        return rack;
    }

    ///<summary>
    /// Instantiate a deviceModel or a deviceTemplate (from GameManager) and apply given data to it.
    ///</summary>
    ///<param name="_dv">The device data to apply</param>
    ///<param name="_parent">The parent of the created device. Leave null if _dv contains the parendId</param>
    ///<param name="_copyAttr">If false, do not copy all attributes</param>
    ///<returns>The created Device</returns>
    public OObject CreateDevice(SApiObject _dv, Transform _parent = null, bool _copyAttr = true)
    {
        // Check parent & parent category
        Transform parent = Utils.FindParent(_parent, _dv.parentId);
        if (!parent || parent.GetComponent<OObject>() == null)
        {
            GameManager.gm.AppendLogLine($"Device must be child of a Rack or another Device", "red");
            return null;
        }

        // Check parent for subdevice
        if (parent.GetComponent<Rack>() == null
            && (string.IsNullOrEmpty(_dv.attributes["slot"]) || string.IsNullOrEmpty(_dv.attributes["template"])))
        {
            GameManager.gm.AppendLogLine("A sub-device needs to be declared with a parent's slot and a template", "red");
            return null;
        }

        // Check if parent not hidden in a group
        if (parent.gameObject.activeSelf == false)
        {
            GameManager.gm.AppendLogLine("The parent object must be active (not hidden in a group)", "red");
            return null;
        }

        // Check if unique hierarchyName
        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_dv.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        // Check template
        if (!string.IsNullOrEmpty(_dv.attributes["template"]) && !GameManager.gm.objectTemplates.ContainsKey(_dv.attributes["template"]))
        {
            GameManager.gm.AppendLogLine($"Unknown template \"{_dv.attributes["template"]}\"", "red");
            return null;
        }

        // Check slot
        Transform slot = null;
        if (!string.IsNullOrEmpty(_dv.attributes["slot"]))
        {
            List<Slot> takenSlots = new List<Slot>();
            int i = 0;
            float max;
            if (string.IsNullOrEmpty(_dv.attributes["template"]))
                max = float.Parse(_dv.attributes["sizeU"]);
            else
                max = float.Parse(GameManager.gm.objectTemplates[_dv.attributes["template"]].GetComponent<OgreeObject>().attributes["height"]) / 1000 / GameManager.gm.uSize;
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
                slot = takenSlots[0].transform;
            }
            else
            {
                GameManager.gm.AppendLogLine($"Slot {_dv.attributes["slot"]} not found in {parent.name}", "red");
                return null;
            }
        }

        // Generate device
        GameObject newDevice;
        Vector2 size;
        float height;

        if (string.IsNullOrEmpty(_dv.attributes["template"]))
        {
            newDevice = GenerateBasicDevice(parent, float.Parse(_dv.attributes["height"]), slot);
            Vector3 boxSize = newDevice.transform.GetChild(0).localScale;
            size = new Vector2(boxSize.x, boxSize.z);
            height = boxSize.y;
        }
        else
        {
            newDevice = GenerateTemplatedDevice(parent, _dv.attributes["template"]);
            OgreeObject tmp = newDevice.GetComponent<OgreeObject>();
            size = JsonUtility.FromJson<Vector2>(tmp.attributes["size"]) / 1000;
            height = float.Parse(tmp.attributes["height"]) / 1000;
        }

        // Place the device
        if (!string.IsNullOrEmpty(_dv.attributes["slot"]))
        {
            newDevice.transform.localEulerAngles = slot.localEulerAngles;
            newDevice.transform.localPosition = slot.localPosition;

            if (height > slot.GetChild(0).localScale.y)
                newDevice.transform.localPosition += new Vector3(0, height / 2 - GameManager.gm.uSize / 2, 0);

            float deltaZ = slot.GetChild(0).localScale.z - size.y;
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

            // if slot, color
            Material mat = newDevice.transform.GetChild(0).GetComponent<Renderer>().material;
            Color slotColor = slot.GetChild(0).GetComponent<Renderer>().material.color;
            mat.color = new Color(slotColor.r, slotColor.g, slotColor.b);
            newDevice.GetComponent<OObject>().color = mat.color;
        }
        else
        {
            newDevice.transform.localEulerAngles = Vector3.zero;
            newDevice.transform.localPosition = new Vector3(0, (-parent.GetChild(0).localScale.y + height) / 2, 0);
            if (_dv.attributes.ContainsKey("posU"))
                newDevice.transform.localPosition += new Vector3(0, (float.Parse(_dv.attributes["posU"]) - 1) * GameManager.gm.uSize, 0);

            float deltaZ = parent.GetChild(0).localScale.z - size.y;
            newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
            newDevice.GetComponent<OObject>().color = Color.white;
        }

        // Fill OObject class
        newDevice.name = _dv.name;
        OObject dv = newDevice.GetComponent<OObject>();
        dv.UpdateFromSApiObject(_dv, _copyAttr);
        if (!_copyAttr)
        {
            dv.attributes["template"] = _dv.attributes["template"];
            dv.attributes["orientation"] = _dv.attributes["orientation"];
            if (_dv.attributes.ContainsKey("posU"))
                dv.attributes["posU"] = _dv.attributes["posU"];
            if (_dv.attributes.ContainsKey("slot"))
                dv.attributes["slot"] = _dv.attributes["slot"];
        }

        // Set labels
        if (string.IsNullOrEmpty(dv.attributes["slot"]))
            newDevice.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
        else
            newDevice.GetComponent<DisplayObjectData>().PlaceTexts(slot?.GetComponent<Slot>().labelPos);
        newDevice.GetComponent<DisplayObjectData>().SetLabel("#name");

        string hn = dv.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newDevice);

        if (!string.IsNullOrEmpty(_dv.attributes["template"]))
        {
            OObject[] components = newDevice.transform.GetComponentsInChildren<OObject>();
            foreach (OObject comp in components)
            {
                if (comp.gameObject != newDevice.gameObject)
                {
                    comp.domain = dv.domain;
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
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
        GameObject go = Instantiate(GameManager.gm.labeledBoxModel);
        go.AddComponent<OObject>();
        go.transform.parent = _parent;
        Vector3 scale;
        if (_slot)
            scale = new Vector3(_slot.GetChild(0).localScale.x, _height / 1000, _slot.GetChild(0).localScale.z);
        else
            scale = new Vector3(_parent.GetChild(0).localScale.x, _height / 1000, _parent.GetChild(0).localScale.z);
        go.transform.GetChild(0).localScale = scale;
        go.transform.GetChild(0).GetComponent<Collider>().enabled = true;
        //Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator = go.transform.GetChild(0).gameObject.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
        //objectManipulator.HostTransform = go.transform;
        //go.transform.GetChild(0).gameObject.AddComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>();
        //go.transform.GetChild(0).gameObject.AddComponent<BoundsControl>();
        go.transform.GetChild(0).gameObject.AddComponent<NearInteractionTouchableVolume>();
        go.transform.GetChild(0).gameObject.AddComponent<HandInteractionHandler>();
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
        if (GameManager.gm.objectTemplates.ContainsKey(_template))
        {
            GameObject go = Instantiate(GameManager.gm.objectTemplates[_template]);
            go.transform.parent = _parent;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            go.transform.GetChild(0).GetComponent<Collider>().enabled = true;
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
    ///<param name="_parent">The parent of the created group. Leave null if _gr contains the parendId</param>
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

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_gr.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
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
        newGr.transform.parent = parent;

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
        gr.UpdateFromSApiObject(_gr);
        gr.UpdateColorByTenant();
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
    ///<param name="_co">The corridor data to apply</param>
    ///<param name="_parent">The parent of the created corridor. Leave null if _co contains the parendId</param>
    ///<returns>The created corridor</returns>
    public OObject CreateCorridor(SApiObject _co, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _co.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_co.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
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

        OObject co = newCo.AddComponent<OObject>();
        co.UpdateFromSApiObject(_co);

        newCo.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.gm.alphaMat;
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
            GameManager.gm.AppendLogLine($"Parent not found", "red");
            return null;
        }
        string parentCategory = parent.GetComponent<OgreeObject>().category;
        if (_se.attributes["formFactor"] == "ext"
            && (parentCategory != "rack" && parentCategory != "device"))
        {
            GameManager.gm.AppendLogLine("An external sensor must be child of a rack or a device", "red");
            return null;
        }
        if (_se.attributes["formFactor"] == "int"
            && (parentCategory != "room" && parentCategory != "rack" && parentCategory != "device"))
        {
            GameManager.gm.AppendLogLine("An internal sensor must be child of a room, a rack or a device", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_se.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newSensor;
        if (_se.attributes["formFactor"] == "ext") //Dimensions : 80 x 26 x 18 mm
        {
            newSensor = Instantiate(GameManager.gm.sensorExtModel, _parent);
            newSensor.name = "sensor";

            Vector3 parentSize = _parent.GetChild(0).localScale;
            Vector3 boxSize = newSensor.transform.GetChild(0).localScale;
            newSensor.transform.localPosition = new Vector3(-parentSize.x, parentSize.y, parentSize.z) / 2;
            newSensor.transform.localPosition += new Vector3(boxSize.x, -boxSize.y, 0) / 2;
        }
        else
        {
            newSensor = Instantiate(GameManager.gm.sensorIntModel, _parent);
            newSensor.name = _se.name;
            if (parentCategory == "room")
            {
                Vector2 orient;
                PlaceInRoom(newSensor.transform, _se, out orient);

                // Adjust position
                float floorUnit = GetUnitFromRoom(parent.GetComponent<Room>());
                newSensor.transform.localPosition += new Vector3(floorUnit * orient.x, 0, floorUnit * orient.y) / 2;
                newSensor.transform.localEulerAngles = new Vector3(0, 180, 0);

                float posU = float.Parse(_se.attributes["posU"]);
                if (posU == 0)
                {
                    newSensor.transform.localPosition += Vector3.up;
                }
                else
                {
                    newSensor.transform.localScale = Vector3.one * GameManager.gm.uSize * 5;
                    newSensor.transform.localPosition += Vector3.up * (posU * GameManager.gm.uSize);
                }
            }
            else
            {
                newSensor.transform.localPosition = parent.GetChild(0).localScale / -2;
                // Assuming given pos is in mm
                Vector2 posXY = JsonUtility.FromJson<Vector2>(_se.attributes["posXY"]);
                Vector3 newPos = new Vector3(posXY.x, float.Parse(_se.attributes["posU"]), posXY.y) / 1000;
                newSensor.transform.localPosition += newPos;

                newSensor.transform.GetChild(0).localScale = Vector3.one * 0.05f;
                newSensor.transform.localEulerAngles = Vector3.zero;
            }

        }

        Sensor sensor = newSensor.GetComponent<Sensor>();
        sensor.UpdateFromSApiObject(_se);

        sensor.UpdateSensorColor();
        newSensor.GetComponent<DisplayObjectData>().PlaceTexts("front");
        newSensor.GetComponent<DisplayObjectData>().SetLabel("#temperature");



        string hn = sensor.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newSensor);

        return sensor;
    }

    ///<summary>
    /// Move the given Transform to given position in tiles in a room.
    ///</summary>
    ///<param name="_obj">The object to move</param>
    ///<param name="_apiObj">The SApiObject with posXY and posU data</param>
    private void PlaceInRoom(Transform _obj, SApiObject _apiObj, out Vector2 _orient)
    {
        float floorUnit = GetUnitFromRoom(_obj.parent.GetComponent<Room>());

        Vector2 pos = JsonUtility.FromJson<Vector2>(_apiObj.attributes["posXY"]);
        Vector3 origin = _obj.parent.GetChild(0).localScale / 0.2f;
        _obj.position = _obj.parent.GetChild(0).position;

        _orient = new Vector2();
        if (_obj.parent.GetComponent<Room>().attributes.ContainsKey("orientation"))
        {
            if (Regex.IsMatch(_obj.parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Left corner of the room
                _orient = new Vector2(1, 1);
            }
            else if (Regex.IsMatch(_obj.parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Right corner of the room
                _orient = new Vector2(-1, 1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, 0);
            }
            else if (Regex.IsMatch(_obj.parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Right corner of the room
                _orient = new Vector2(-1, -1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, _obj.GetChild(0).localScale.z);
            }
            else if (Regex.IsMatch(_obj.parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Left corner of the room
                _orient = new Vector2(1, -1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(0, 0, _obj.GetChild(0).localScale.z);
            }
        }
        // Go to the right corner of the room & apply pos
        _obj.localPosition += new Vector3(origin.x * -_orient.x, 0, origin.z * -_orient.y);
        _obj.localPosition += new Vector3(pos.x * _orient.x, 0, pos.y * _orient.y) * floorUnit;
    }

    ///<summary>
    /// Get a floorUnit regarding given room attributes.
    ///</summary>
    ///<param name="_r">The room to parse</param>
    ///<returns>The floor unit</returns>
    private float GetUnitFromRoom(Room _r)
    {
        if (!_r.attributes.ContainsKey("floorUnit"))
            return GameManager.gm.tileSize;
        switch (_r.attributes["floorUnit"])
        {
            case "m":
                return 1.0f;
            case "f":
                return 3.28084f;
            default:
                return GameManager.gm.tileSize;
        }
    }
}