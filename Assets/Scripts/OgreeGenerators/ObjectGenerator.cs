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
    ///<param name="_rk">THe rack data to apply</param>
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

        string hierarchyName = $"{parent.GetComponent<HierarchyName>()?.fullname}.{_rk.name}";
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
            Destroy(newRack.GetComponent<HierarchyName>());
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

        switch (rack.attributes["orientation"])
        {
            case "front":
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                if (orient.y == 1)
                    newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                else
                    newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, 0);
                break;
            case "rear":
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (orient.y == 1)
                    newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z);
                else
                    newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, 0);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
            case "left":
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                if (orient.x == 1)
                    newRack.transform.localPosition += new Vector3(-boxOrigin.z, boxOrigin.y, boxOrigin.x);
                else
                    newRack.transform.localPosition += new Vector3(0, boxOrigin.y, boxOrigin.x);
                newRack.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, 0);
                break;
            case "right":
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                if (orient.x == 1)
                    newRack.transform.localPosition += new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x);
                else
                    newRack.transform.localPosition += new Vector3(0, boxOrigin.y, -boxOrigin.x);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
        }

        newRack.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
        newRack.GetComponent<DisplayObjectData>().SetLabel("name");

        rack.UpdateColor();
        GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = newRack.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            HierarchyName[] components = rack.transform.GetComponentsInChildren<HierarchyName>();
            foreach (HierarchyName comp in components)
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
            && (!_dv.attributes.ContainsKey("slot") || !_dv.attributes.ContainsKey("template")))
        {
            GameManager.gm.AppendLogLine("A sub-device needs to be declared with a parent's slot and a template", "yellow");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<HierarchyName>()?.fullname}.{_dv.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newDevice;
        if (!_dv.attributes.ContainsKey("slot"))
        {
            //+chassis:[name]@[posU]@[sizeU]
            if (!_dv.attributes.ContainsKey("template"))
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
            if (!_dv.attributes.ContainsKey("template"))
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
                if (!_dv.attributes.ContainsKey("template"))
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
                    case "Front":
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                        break;
                    case "Rear":
                        newDevice.transform.localPosition -= new Vector3(0, 0, deltaZ / 2);
                        newDevice.transform.localEulerAngles += new Vector3(0, 180, 0);
                        break;
                    case "FrontFlipped":
                        newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                        newDevice.transform.localEulerAngles += new Vector3(0, 0, 180);
                        break;
                    case "RearFlipped":
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
        if (!_dv.attributes.ContainsKey("template"))
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



        newDevice.GetComponent<DisplayObjectData>().SetLabel("name");

        string hn = newDevice.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newDevice);

        if (_dv.attributes.ContainsKey("template"))
        {
            HierarchyName[] components = newDevice.transform.GetComponentsInChildren<HierarchyName>();
            foreach (HierarchyName comp in components)
            {
                if (comp.gameObject != newDevice.gameObject)
                {
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return newDevice.GetComponent<Object>();
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
            Destroy(go.GetComponent<HierarchyName>());
            return go;
        }
        else
        {
            GameManager.gm.AppendLogLine($"Unknown template \"{_template}\"", "yellow");
            return null;
        }
    }

    ///<summary>
    /// Generate a rackGroup (from GameManager.labeledBoxModel) which contains all the given racks.
    ///</summary>
    ///<param name="_name">The name of the rackGroup</param>
    ///<param name="_parent">The parent of the generated rackGroup</param>
    ///<param name="_racksList">The well formatted list of racks to contains (r1,r2,..,rN)</param>
    ///<returns>The created rackGroup</returns>
    public RackGroup CreateRackGroup(SApiObject _rg, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _rg.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        List<Transform> racks = new List<Transform>();
        string[] rackNames = _rg.attributes["racksList"].Split(',');
        foreach (string rn in rackNames)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{parent.GetComponent<HierarchyName>().fullname}.{rn}");
            if (go && (go.GetComponent<OgreeObject>()?.category == "rack" || go.GetComponent<OgreeObject>()?.category == "corridor"))
                racks.Add(go.transform);
            else
                GameManager.gm.AppendLogLine($"{parent.GetComponent<HierarchyName>().fullname}.{rn} doesn't exists.", "yellow");
        }
        if (racks.Count == 0)
            return null;

        Transform lowerLeft = racks[0];
        Transform upperRight = racks[0];
        float maxHeight = 0;
        float maxLength = 0;
        foreach (Transform r in racks)
        {
            if (r.GetComponent<OgreeObject>().category == "rack")
            {
                Vector2 rackPos = JsonUtility.FromJson<Vector2>(r.GetComponent<OgreeObject>().attributes["posXY"]);
                Vector2 lowerLeftPos = JsonUtility.FromJson<Vector2>(lowerLeft.GetComponent<OgreeObject>().attributes["posXY"]);
                Vector2 upperRightPos = JsonUtility.FromJson<Vector2>(upperRight.GetComponent<OgreeObject>().attributes["posXY"]);

                if (rackPos.x <= lowerLeftPos.x && rackPos.y <= lowerLeftPos.y)
                    lowerLeft = r;
                if (rackPos.y > upperRightPos.y || (rackPos.x >= upperRightPos.x && rackPos.y >= upperRightPos.y))
                    upperRight = r;

                if (r.transform.GetChild(0).localScale.y > maxHeight)
                    maxHeight = r.transform.GetChild(0).localScale.y;
                if (r.transform.GetChild(0).localScale.z > maxLength)
                    maxLength = r.transform.GetChild(0).localScale.z;
            }
        }
        // racks = racks.OrderBy(t => t.GetChild(0).localScale.y).ToList();
        // maxHeight = racks[racks.Count - 1].GetChild(0).localScale.y;
        // racks = racks.OrderBy(t => t.GetChild(0).localScale.z).ToList();
        // maxLength = racks[racks.Count - 1].GetChild(0).localScale.z;

        GameObject newRg = Instantiate(GameManager.gm.labeledBoxModel);
        newRg.name = _rg.name;
        newRg.transform.parent = parent;

        float x = upperRight.localPosition.x - lowerLeft.localPosition.x;
        float z = upperRight.localPosition.z - lowerLeft.localPosition.z;
        if (lowerLeft.GetComponent<Rack>().attributes["orientation"] == "front"
            || lowerLeft.GetComponent<Rack>().attributes["orientation"] == "rear")
        {
            x += (upperRight.GetChild(0).localScale.x + lowerLeft.GetChild(0).localScale.x) / 2;
            z -= (upperRight.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2;
            z += maxLength * 2;
        }
        else
        {
            z += (upperRight.GetChild(0).localScale.x + lowerLeft.GetChild(0).localScale.x) / 2;
            x -= (upperRight.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2;
            x += maxLength * 2;
        }
        newRg.transform.GetChild(0).localScale = new Vector3(x, maxHeight, z);
        newRg.transform.localEulerAngles = new Vector3(0, 180, 0);
        newRg.transform.localPosition = new Vector3(lowerLeft.localPosition.x, maxHeight / 2, lowerLeft.localPosition.z);

        float xOffset;
        float zOffset;
        if (lowerLeft.GetComponent<Rack>().attributes["orientation"] == "front"
            || lowerLeft.GetComponent<Rack>().attributes["orientation"] == "rear")
        {
            xOffset = (newRg.transform.GetChild(0).localScale.x - lowerLeft.GetChild(0).localScale.x) / 2;
            zOffset = (newRg.transform.GetChild(0).localScale.z + lowerLeft.GetChild(0).localScale.z) / 2 - maxLength;
        }
        else
        {
            xOffset = (newRg.transform.GetChild(0).localScale.x + lowerLeft.GetChild(0).localScale.z) / 2 - maxLength;
            zOffset = (newRg.transform.GetChild(0).localScale.z - lowerLeft.GetChild(0).localScale.x) / 2;
        }
        newRg.transform.localPosition += new Vector3(xOffset, 0, zOffset);

        RackGroup rg = newRg.AddComponent<RackGroup>();
        rg.name = newRg.name;
        rg.parentId = _rg.parentId;
        if (string.IsNullOrEmpty(rg.parentId))
            rg.parentId = parent.GetComponent<Room>().id;
        rg.category = "rackGroup";
        rg.description = _rg.description;
        rg.domain = _rg.domain;
        if (string.IsNullOrEmpty(rg.domain))
            rg.domain = racks[0].GetComponent<Rack>().domain;
        rg.attributes = _rg.attributes;
        rg.DisplayRacks(false);

        newRg.GetComponent<DisplayObjectData>().PlaceTexts("top");
        newRg.GetComponent<DisplayObjectData>().SetLabel("name");

        string hn = newRg.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newRg);

        return rg;
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

        string roomHierarchyName = parent.GetComponent<HierarchyName>().fullname;
        string[] rackNames = _co.attributes["racksList"].Split(',');
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

        co.SetAttribute("alpha", "50");
        if (_co.attributes["temperature"] == "cold")
            co.SetAttribute("color", "000099");
        else
            co.SetAttribute("color", "990000");

        newCo.GetComponent<DisplayObjectData>().PlaceTexts("top");
        newCo.GetComponent<DisplayObjectData>().SetLabel("name");

        string hn = newCo.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newCo);

        return co;
    }

}
