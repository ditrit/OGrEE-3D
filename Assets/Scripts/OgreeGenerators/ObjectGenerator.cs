using System.Collections;
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
        if (GameManager.instance.allItems.Contains(_rk.hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{_rk.hierarchyName} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newRack;
        if (string.IsNullOrEmpty(_rk.attributes["template"]))
            newRack = Object.Instantiate(GameManager.instance.rackModel);
        else
        {
            if (GameManager.instance.objectTemplates.ContainsKey(_rk.attributes["template"]))
                newRack = Object.Instantiate(GameManager.instance.objectTemplates[_rk.attributes["template"]]);
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template \"{_rk.attributes["template"]}\"", ELogTarget.both, ELogtype.error);
                return null;
            }
            Renderer[] renderers = newRack.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            newRack.transform.GetChild(0).GetComponent<Collider>().enabled = true;
        }

        newRack.name = _rk.name;
        newRack.transform.parent = _parent;

        if (string.IsNullOrEmpty(_rk.attributes["template"]))
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(_rk.attributes["size"]);
            float height = Utils.ParseDecFrac(_rk.attributes["height"]);
            if (_rk.attributes["heightUnit"] == "U")
                height *= GameManager.instance.uSize;
            else if (_rk.attributes["heightUnit"] == "cm")
                height /= 100;
            newRack.transform.GetChild(0).localScale = new Vector3(size.x / 100, height, size.y / 100);
        }

        Rack rack = newRack.GetComponent<Rack>();
        rack.UpdateFromSApiObject(_rk);

        if (_parent)
        {
            PlaceInRoom(newRack.transform, _rk, out Vector2 orient);

            // Correct position according to rack size & rack orientation
            Vector3 boxOrigin;
            Transform box = newRack.transform.GetChild(0);
            if (box.childCount == 0)
                boxOrigin = box.localScale / 2;
            else
                boxOrigin = box.GetComponent<BoxCollider>().size / 2;
            float floorUnit = GetUnitFromRoom(_parent.GetComponent<Room>());
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
        }
        else
            newRack.transform.localPosition = Vector3.zero;

        DisplayObjectData dod = newRack.GetComponent<DisplayObjectData>();
        dod.PlaceTexts("frontrear");
        dod.SetLabel("#name");
        dod.hasFloatingLabel = true;
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        if (rack.attributes.ContainsKey("color"))
            rack.SetColor(rack.attributes["color"]);
        else
            rack.UpdateColorByTenant();

        GameManager.instance.allItems.Add(rack.hierarchyName, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            OObject[] components = rack.transform.GetComponentsInChildren<OObject>();
            foreach (OObject comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    comp.domain = rack.domain;
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.instance.allItems.Add(compHn, comp.gameObject);
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
    public OObject CreateDevice(SApiObject _dv, Transform _parent)
    {
        if (_parent)
        {
            if (_parent.GetComponent<OObject>() == null)
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
        if (GameManager.instance.allItems.Contains(_dv.hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{_dv.hierarchyName} already exists.", ELogTarget.both, ELogtype.warning);
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
                    max = Utils.ParseDecFrac(GameManager.instance.objectTemplates[_dv.attributes["template"]].GetComponent<OgreeObject>().attributes["height"]) / 1000 / GameManager.instance.uSize;
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

        // Place the device
        if (_parent)
        {
            if (!string.IsNullOrEmpty(_dv.attributes["slot"]))
            {
                newDevice.transform.localEulerAngles = slot.localEulerAngles;
                newDevice.transform.localPosition = slot.localPosition;

                if (height > slot.GetChild(0).localScale.y)
                    newDevice.transform.localPosition += new Vector3(0, height / 2 - GameManager.instance.uSize / 2, 0);

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
                newDevice.transform.localPosition = new Vector3(0, (-_parent.GetChild(0).localScale.y + height) / 2, 0);
                if (_dv.attributes.ContainsKey("posU"))
                    newDevice.transform.localPosition += new Vector3(0, (Utils.ParseDecFrac(_dv.attributes["posU"]) - 1) * GameManager.instance.uSize, 0);

                float deltaZ = _parent.GetChild(0).localScale.z - size.y;
                newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                newDevice.GetComponent<OObject>().color = Color.white;
            }
        }
        else
        {
            newDevice.transform.localEulerAngles = Vector3.zero;
            newDevice.transform.localPosition = Vector3.zero;
            newDevice.GetComponent<OObject>().color = Color.white;
        }

        // Fill OObject class
        newDevice.name = _dv.name;
        OObject dv = newDevice.GetComponent<OObject>();
        dv.UpdateFromSApiObject(_dv);

        // Set labels
        DisplayObjectData dod = newDevice.GetComponent<DisplayObjectData>();
        if (string.IsNullOrEmpty(dv.attributes["slot"]))
            dod.PlaceTexts("frontrear");
        else
            dod.PlaceTexts(slot?.GetComponent<Slot>().labelPos);
        dod.SetLabel("#name");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        GameManager.instance.allItems.Add(dv.hierarchyName, newDevice);

        if (!string.IsNullOrEmpty(_dv.attributes["template"]))
        {
            OObject[] components = newDevice.transform.GetComponentsInChildren<OObject>();
            foreach (OObject comp in components)
            {
                if (comp.gameObject != newDevice)
                {
                    comp.domain = dv.domain;
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.instance.allItems.Add(compHn, comp.gameObject);
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
        go.AddComponent<OObject>();
        go.transform.parent = _parent;
        Vector3 scale;
        if (_slot)
            scale = new Vector3(_slot.GetChild(0).localScale.x, _height / 1000, _slot.GetChild(0).localScale.z);
        else
            scale = new Vector3(_parent.GetChild(0).localScale.x, _height / 1000, _parent.GetChild(0).localScale.z);
        go.transform.GetChild(0).localScale = scale;
        go.transform.GetChild(0).GetComponent<Collider>().enabled = true;
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
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            go.transform.GetChild(0).GetComponent<Collider>().enabled = true;
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
        if (parentCategory != "room" && parentCategory != "rack")
        {
            GameManager.instance.AppendLogLine("A group must be a child of a room or a rack", ELogTarget.both, ELogtype.error);
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_gr.name}";
        if (GameManager.instance.allItems.Contains(hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{hierarchyName} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        List<Transform> content = new List<Transform>();
        string[] contentNames = _gr.attributes["content"].Split(',');
        foreach (string cn in contentNames)
        {
            GameObject go = GameManager.instance.FindByAbsPath($"{parent.GetComponent<OgreeObject>().hierarchyName}.{cn}");
            if (go && go.GetComponent<OgreeObject>())
            {
                if ((parentCategory == "room" && (go.GetComponent<OgreeObject>().category == "rack" || go.GetComponent<OgreeObject>().category == "corridor"))
                    || parentCategory == "rack" && go.GetComponent<OgreeObject>().category == "device")
                    content.Add(go.transform);
            }
            else
                GameManager.instance.AppendLogLine($"{parent.GetComponent<OgreeObject>().hierarchyName}.{cn} doesn't exists.", ELogTarget.both, ELogtype.warning);
        }
        if (content.Count == 0)
            return null;

        GameObject newGr = Object.Instantiate(GameManager.instance.labeledBoxModel);
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
        gr.hierarchyName = hierarchyName;
        gr.UpdateFromSApiObject(_gr);
        gr.UpdateColorByTenant();
        gr.DisplayContent(false);

        // Setup labels
        DisplayObjectData dod = newGr.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        if (parentCategory == "room")
            dod.PlaceTexts("top");
        else if (parentCategory == "rack")
            dod.PlaceTexts("frontrear");
        dod.SetLabel("#name");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        GameManager.instance.allItems.Add(hierarchyName, newGr);

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
            GameManager.instance.AppendLogLine($"Parent room not found", ELogTarget.both, ELogtype.error);
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_co.name}";
        if (GameManager.instance.allItems.Contains(hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{hierarchyName} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        string roomHierarchyName = parent.GetComponent<OgreeObject>().hierarchyName;
        string[] rackNames = _co.attributes["content"].Split(',');
        Transform cornerA = GameManager.instance.FindByAbsPath($"{roomHierarchyName}.{rackNames[0]}")?.transform;
        Transform cornerB = GameManager.instance.FindByAbsPath($"{roomHierarchyName}.{rackNames[1]}")?.transform;
        if (cornerA == null || cornerB == null)
        {
            GameManager.instance.AppendLogLine($"{rackNames[0]} or {rackNames[1]} doesn't exist", ELogTarget.both, ELogtype.error);
            return null;
        }

        bool horizontalCorridor = (cornerA.GetComponent<Rack>().attributes["orientation"] == "front" || cornerA.GetComponent<Rack>().attributes["orientation"] == "rear");

        Vector2 orient = Vector2.one;
        if (cornerA.localPosition.x > cornerB.localPosition.x)
            orient.x = -1;
        if (cornerA.localPosition.z > cornerB.localPosition.z)
            orient.y = -1;

        float maxHeight = cornerA.GetChild(0).localScale.y;
        if (cornerB.GetChild(0).localScale.y > maxHeight)
            maxHeight = cornerB.GetChild(0).localScale.y;

        GameObject newCo = Object.Instantiate(GameManager.instance.labeledBoxModel);
        newCo.name = _co.name;
        newCo.transform.parent = parent;

        // Scale
        float x = (cornerB.localPosition.x - cornerA.localPosition.x) * orient.x;
        float z = (cornerB.localPosition.z - cornerA.localPosition.z) * orient.y;
        if (horizontalCorridor)
        {
            x += (cornerB.GetChild(0).localScale.x + cornerA.GetChild(0).localScale.x) / 2;
            z -= (cornerB.GetChild(0).localScale.z + cornerA.GetChild(0).localScale.z) / 2;
        }
        else
        {
            x -= (cornerB.GetChild(0).localScale.z + cornerA.GetChild(0).localScale.z) / 2;
            z += (cornerB.GetChild(0).localScale.x + cornerA.GetChild(0).localScale.x) / 2;
        }
        newCo.transform.GetChild(0).localScale = new Vector3(Mathf.Abs(x), maxHeight, Mathf.Abs(z));

        // localPosition
        newCo.transform.localEulerAngles = new Vector3(0, 180, 0);
        newCo.transform.localPosition = new Vector3(cornerA.localPosition.x, maxHeight / 2, cornerA.localPosition.z);
        float xOffset;
        float zOffset;
        if (horizontalCorridor)
        {
            xOffset = (newCo.transform.GetChild(0).localScale.x - cornerA.GetChild(0).localScale.x) / 2;
            zOffset = (newCo.transform.GetChild(0).localScale.z + cornerA.GetChild(0).localScale.z) / 2;
        }
        else
        {
            xOffset = (newCo.transform.GetChild(0).localScale.x + cornerA.GetChild(0).localScale.z) / 2;
            zOffset = (newCo.transform.GetChild(0).localScale.z - cornerA.GetChild(0).localScale.x) / 2;
        }
        newCo.transform.localPosition += new Vector3(xOffset * orient.x, 0, zOffset * orient.y);

        OObject co = newCo.AddComponent<OObject>();
        co.hierarchyName = hierarchyName;
        co.UpdateFromSApiObject(_co);

        newCo.transform.GetChild(0).GetComponent<Renderer>().material = GameManager.instance.alphaMat;
        Material mat = newCo.transform.GetChild(0).GetComponent<Renderer>().material;
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.5f);
        if (_co.attributes["temperature"] == "cold")
            co.SetColor("000099");
        else
            co.SetColor("990000");

        DisplayObjectData dod = newCo.GetComponent<DisplayObjectData>();
        dod.hasFloatingLabel = true;
        dod.PlaceTexts("top");
        dod.SetLabel("#name");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);

        GameManager.instance.allItems.Add(hierarchyName, newCo);

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
        string parentCategory = parentOgree.category;
        if (_se.attributes["formFactor"] == "ext"
            && (parentCategory != "rack" && parentCategory != "device"))
        {
            GameManager.instance.AppendLogLine("An external sensor must be child of a rack or a device", ELogTarget.both, ELogtype.error);
            return null;
        }
        if (_se.attributes["formFactor"] == "int"
            && (parentCategory != "room" && parentCategory != "rack" && parentCategory != "device"))
        {
            GameManager.instance.AppendLogLine("An internal sensor must be child of a room, a rack or a device", ELogTarget.both, ELogtype.error);
            return null;
        }

        GameObject newSensor;
        if (_se.attributes["formFactor"] == "ext") //Dimensions : 80 x 26 x 18 mm
        {
            newSensor = Object.Instantiate(GameManager.instance.sensorExtModel, _parent);
            newSensor.name = "sensor";

            Vector3 parentSize = _parent.GetChild(0).localScale;
            Vector3 boxSize = newSensor.transform.GetChild(0).localScale;
            newSensor.transform.localPosition = new Vector3(-parentSize.x, parentSize.y, parentSize.z) / 2;
            float uXSize = GameManager.instance.ouSize;
            if (parentOgree.attributes.ContainsKey("heightUnit") && parentOgree.attributes["heightUnit"] == "U")
                uXSize = GameManager.instance.uSize;
            newSensor.transform.localPosition += new Vector3(boxSize.x + uXSize, -boxSize.y, 0) / 2;
        }
        else
        {
            newSensor = Object.Instantiate(GameManager.instance.sensorIntModel, _parent);
            newSensor.name = _se.name;
            if (parentCategory == "room")
            {
                PlaceInRoom(newSensor.transform, _se, out Vector2 orient);

                // Adjust position
                float floorUnit = GetUnitFromRoom(parent.GetComponent<Room>());
                newSensor.transform.localPosition += new Vector3(floorUnit * orient.x, 0, floorUnit * orient.y) / 2;
                newSensor.transform.localEulerAngles = new Vector3(0, 180, 0);

                float posU = Utils.ParseDecFrac(_se.attributes["posU"]);
                if (posU == 0)
                {
                    newSensor.transform.localPosition += Vector3.up;
                }
                else
                {
                    newSensor.transform.localScale = 5 * GameManager.instance.uSize * Vector3.one;
                    newSensor.transform.localPosition += Vector3.up * (posU * GameManager.instance.uSize);
                }
            }
            else
            {
                newSensor.transform.localPosition = parent.GetChild(0).localScale / -2;
                // Assuming given pos is in mm
                Vector2 posXY = JsonUtility.FromJson<Vector2>(_se.attributes["posXY"]);
                Vector3 newPos = new Vector3(posXY.x, Utils.ParseDecFrac(_se.attributes["posU"]), posXY.y) / 1000;
                newSensor.transform.localPosition += newPos;

                newSensor.transform.GetChild(0).localScale = Vector3.one * 0.05f;
                newSensor.transform.localEulerAngles = Vector3.zero;
            }

        }

        Sensor sensor = newSensor.GetComponent<Sensor>();
        sensor.fromTemplate = false;
        DisplayObjectData dod = newSensor.GetComponent<DisplayObjectData>();
        dod.PlaceTexts("front");
        dod.SetLabel("#temperature");
        dod.SwitchLabel((ELabelMode)UiManager.instance.labelsDropdown.value);
        newSensor.transform.GetChild(0).GetComponent<Collider>().enabled = false;

        return sensor;
    }

    ///<summary>
    /// Move the given Transform to given position in tiles in a room.
    ///</summary>
    ///<param name="_obj">The object to move</param>
    ///<param name="_apiObj">The SApiObject with posXY and posU data</param>
    private void PlaceInRoom(Transform _obj, SApiObject _apiObj, out Vector2 _orient)
    {
        Room parentRoom = _obj.parent.GetComponent<Room>();
        float floorUnit = GetUnitFromRoom(parentRoom);
        Vector3 origin = _obj.parent.GetChild(0).localScale / 0.2f;
        _obj.position = _obj.parent.GetChild(0).position;

        _orient = new Vector2();
        if (parentRoom.attributes.ContainsKey("axisOrientation"))
        {
            if (parentRoom.attributes["axisOrientation"] == "+x+y")
            {
                // Lower Left corner of the room
                _orient = new Vector2(1, 1);
            }
            else if (parentRoom.attributes["axisOrientation"] == "-x+y")
            {
                // Lower Right corner of the room
                _orient = new Vector2(-1, 1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, 0);
            }
            else if (parentRoom.attributes["axisOrientation"] == "-x-y")
            {
                // Upper Right corner of the room
                _orient = new Vector2(-1, -1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(_obj.GetChild(0).localScale.x, 0, _obj.GetChild(0).localScale.z);
            }
            else if (parentRoom.attributes["axisOrientation"] == "+x-y")
            {
                // Upper Left corner of the room
                _orient = new Vector2(1, -1);
                if (_apiObj.category == "rack")
                    _obj.localPosition -= new Vector3(0, 0, _obj.GetChild(0).localScale.z);
            }
        }

        Vector3 pos;
        if (_apiObj.category == "rack" && _apiObj.attributes.ContainsKey("posXYZ"))
            pos = JsonUtility.FromJson<Vector3>(_apiObj.attributes["posXYZ"]);
        else
        {
            Vector2 tmp = JsonUtility.FromJson<Vector2>(_apiObj.attributes["posXY"]);
            pos = new Vector3(tmp.x, tmp.y, 0);
        }

        Transform floor = _obj.parent.Find("Floor");
        if (!parentRoom.isSquare && _apiObj.category == "rack" && parentRoom.attributes["floorUnit"] == "t" && floor)
        {
            int trunkedX = (int)pos.x;
            int trunkedY = (int)pos.y;
            foreach (Transform tileObj in floor)
            {
                Tile tile = tileObj.GetComponent<Tile>();
                if (tile.coord.x == trunkedX && tile.coord.y == trunkedY)
                {
                    _obj.localPosition += new Vector3(tileObj.localPosition.x - 5 * tileObj.localScale.x, pos.z / 100, tileObj.localPosition.z - 5 * tileObj.localScale.z);
                    _obj.localPosition += GameManager.instance.tileSize * new Vector3(pos.x - trunkedX, 0, pos.y - trunkedY);
                    return;
                }
            }
        }

        // Go to the right corner of the room & apply pos
        if (parentRoom.isSquare)
            _obj.localPosition += new Vector3(origin.x * -_orient.x, 0, origin.z * -_orient.y);

        _obj.localPosition += new Vector3(pos.x * _orient.x * floorUnit, pos.z / 100, pos.y * _orient.y * floorUnit);
    }

    ///<summary>
    /// Get a floorUnit regarding given room attributes.
    ///</summary>
    ///<param name="_r">The room to parse</param>
    ///<returns>The floor unit</returns>
    private float GetUnitFromRoom(Room _r)
    {
        if (!_r.attributes.ContainsKey("floorUnit"))
            return GameManager.instance.tileSize;
        return _r.attributes["floorUnit"] switch
        {
            "m" => 1.0f,
            "f" => 3.28084f,
            _ => GameManager.instance.tileSize,
        };
    }
}
