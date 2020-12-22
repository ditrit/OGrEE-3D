using System.Collections;
using System.Collections.Generic;
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
    /// Instantiate a rackModel or a rackPreset (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the rack</param>
    ///<returns>The created Rack</returns>
    public Rack CreateRack(SRackInfos _data)
    {
        if (_data.parent.GetComponent<Room>() == null)
        {
            GameManager.gm.AppendLogLine("Rack must be child of a Room", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newRack;
        if (string.IsNullOrEmpty(_data.template))
            newRack = Instantiate(GameManager.gm.rackModel);
        else
        {
            if (GameManager.gm.rackTemplates.ContainsKey(_data.template))
                newRack = Instantiate(GameManager.gm.rackTemplates[_data.template]);
            else
            {
                GameManager.gm.AppendLogLine($"Unknown template \"{_data.template}\"", "yellow");
                return null;
            }
            Renderer[] renderers = newRack.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = true;
            Destroy(newRack.GetComponent<HierarchyName>());
        }

        newRack.name = _data.name;
        newRack.transform.parent = _data.parent;

        if (string.IsNullOrEmpty(_data.template))
            newRack.transform.GetChild(0).localScale = _data.size / 100;

        Vector3 origin = newRack.transform.parent.GetChild(0).localScale / -0.2f;
        Vector3 boxOrigin = newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.position = newRack.transform.parent.GetChild(0).position;
        newRack.transform.localPosition += new Vector3(origin.x, 0, origin.z);
        newRack.transform.localPosition += new Vector3(_data.pos.x, 0, _data.pos.y) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.name = newRack.name;
        rack.parentId = _data.parent.GetComponent<OgreeObject>().id;
        rack.category = "rack";
        rack.domain = _data.parent.GetComponent<OgreeObject>().domain;
        
        rack.attributes["posXY"] = JsonUtility.ToJson(_data.pos);
        rack.attributes["posXYUnit"] =  "Tile";
        if (string.IsNullOrEmpty(_data.template))
        {
            rack.attributes["size"] = JsonUtility.ToJson(new Vector2(_data.size.x, _data.size.z));
            rack.attributes["sizeUnit"] = "cm";
            rack.attributes["height"] = _data.height.ToString();
            rack.attributes["heightUnit"] = "U" ;
        }
        switch (_data.orient)
        {
            case "front":
                rack.attributes["orientation"] = "Front";
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                newRack.transform.localPosition += boxOrigin;
                break;
            case "rear":
                rack.attributes["orientation"] = "Rear";
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
            case "left":
                rack.attributes["orientation"] = "Left";
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                newRack.transform.localPosition += new Vector3(-boxOrigin.z, boxOrigin.y, boxOrigin.x);
                newRack.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, 0);
                break;
            case "right":
                rack.attributes["orientation"] = "Right";
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
        }

        newRack.GetComponent<DisplayRackData>().PlaceTexts();
        newRack.GetComponent<DisplayRackData>().FillTexts();

        rack.UpdateColor();
        GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = newRack.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(_data.template))
        {
            Object[] components = rack.transform.GetComponentsInChildren<Object>();
            foreach (Object comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    string compHn = comp.GetComponent<HierarchyName>().UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return rack;
    }

    ///<summary>
    /// Instantiate a deviceModel or a deviceTemplate (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the chassis</param>
    ///<returns>The created Chassis</returns>
    public Object CreateDevice(SDeviceInfos _data)
    {
        if (_data.parent.GetComponent<Object>() == null)
        {
            GameManager.gm.AppendLogLine("Device must be child of a Rack or another Device", "yellow");
            return null;
        }

        if (_data.parent.GetComponent<Rack>() == null
            && (string.IsNullOrEmpty(_data.slot) || string.IsNullOrEmpty(_data.template)))
        {
            GameManager.gm.AppendLogLine("A device needs to be declared with a parent's slot and a template", "yellow");
            return null;
        }

        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newDevice;
        if (string.IsNullOrEmpty(_data.slot))
        {
            //+chassis:[name]@[posU]@[sizeU]
            if (string.IsNullOrEmpty(_data.template))
            {
                newDevice = Instantiate(GameManager.gm.labeledBoxModel);
                newDevice.AddComponent<Object>();
                newDevice.transform.parent = _data.parent;
                newDevice.transform.GetChild(0).localScale = new Vector3(_data.parent.GetChild(0).localScale.x,
                                                                _data.sizeU * GameManager.gm.uSize,
                                                                _data.parent.GetChild(0).localScale.z);
            }
            //+chassis:[name]@[posU]@[template]
            else
            {
                if (GameManager.gm.devicesTemplates.ContainsKey(_data.template))
                    newDevice = Instantiate(GameManager.gm.devicesTemplates[_data.template]);
                else
                {
                    GameManager.gm.AppendLogLine($"Unknown template \"{_data.template}\"", "yellow");
                    return null;
                }
                newDevice.transform.parent = _data.parent;
                Renderer[] renderers = newDevice.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                    r.enabled = true;
                Destroy(newDevice.GetComponent<HierarchyName>());
            }
            newDevice.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
            newDevice.transform.localEulerAngles = Vector3.zero;
            newDevice.transform.localPosition = new Vector3(0, (-_data.parent.GetChild(0).localScale.y + newDevice.transform.GetChild(0).localScale.y) / 2, 0);
            newDevice.transform.localPosition += new Vector3(0, (_data.posU - 1) * GameManager.gm.uSize, 0);

            float deltaZ = _data.parent.GetChild(0).localScale.z - newDevice.transform.GetChild(0).localScale.z;
            newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
        }
        else
        {
            List<Slot> takenSlots = new List<Slot>();
            int i = 0;
            float max;
            if (string.IsNullOrEmpty(_data.template))
                max = _data.sizeU;
            else
            {
                if (GameManager.gm.devicesTemplates.ContainsKey(_data.template))
                    max = GameManager.gm.devicesTemplates[_data.template].transform.GetChild(0).localScale.y / GameManager.gm.uSize;
                else
                {
                    GameManager.gm.AppendLogLine($"Unknown template \"{_data.template}\"", "yellow");
                    return null;
                }
            }
            foreach (Transform child in _data.parent)
            {
                if ((child.name == _data.slot || (i > 0 && i < max)) && child.GetComponent<Slot>())
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
                if (string.IsNullOrEmpty(_data.template))
                {
                    newDevice = Instantiate(GameManager.gm.labeledBoxModel);
                    newDevice.AddComponent<Object>();
                    newDevice.transform.parent = _data.parent;
                    newDevice.transform.GetChild(0).localScale = new Vector3(slot.GetChild(0).localScale.x,
                                                                                _data.sizeU * slot.GetChild(0).localScale.y,
                                                                                slot.GetChild(0).localScale.z);

                }
                //+chassis:[name]@[slot]@[template]
                else
                {
                    if (GameManager.gm.devicesTemplates.ContainsKey(_data.template))
                        newDevice = Instantiate(GameManager.gm.devicesTemplates[_data.template]);
                    else
                    {
                        GameManager.gm.AppendLogLine($"Unknown template \"{_data.template}\"", "yellow");
                        return null;
                    }
                    newDevice.transform.parent = _data.parent;
                    Renderer[] renderers = newDevice.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                        r.enabled = true;
                    Destroy(newDevice.GetComponent<HierarchyName>());
                }
                newDevice.GetComponent<DisplayObjectData>().PlaceTexts(slot.GetComponent<Slot>().labelPos);
                newDevice.transform.localPosition = slot.localPosition;
                newDevice.transform.localEulerAngles = slot.localEulerAngles;
                if (newDevice.transform.GetChild(0).localScale.y > slot.GetChild(0).localScale.y)
                    newDevice.transform.localPosition += new Vector3(0, newDevice.transform.GetChild(0).localScale.y / 2 - GameManager.gm.uSize / 2, 0);

                Object ob = newDevice.GetComponent<Object>();
                switch (_data.side)
                {
                    case "front":
                        ob.attributes["orientation"] = "Front";
                        break;
                    case "rear":
                        ob.attributes["orientation"] = "Rear";
                        break;
                    case "frontflipped":
                        ob.attributes["orientation"] = "FrontFlipped";
                        break;
                    case "rearflipped":
                        ob.attributes["orientation"] = "RearFlipped";
                        break;
                    default:
                        ob.attributes["orientation"] = "Front";
                        break;
                }
                if (ob.attributes.ContainsKey("fulldepth") && ob.attributes["fulldepth"] == "yes")
                    ob.attributes["orientation"] = "Front";

                float deltaZ = slot.GetChild(0).localScale.z - newDevice.transform.GetChild(0).localScale.z;
                switch (ob.attributes["orientation"])
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

        newDevice.name = _data.name;
        Object obj = newDevice.GetComponent<Object>();
        obj.name = newDevice.name;
        obj.parentId = _data.parent.GetComponent<OgreeObject>().id;
        obj.category = "device";
        obj.domain = _data.parent.GetComponent<OgreeObject>().domain;

        obj.attributes["size"] = JsonUtility.ToJson(new Vector2(newDevice.transform.GetChild(0).localScale.x,
                                newDevice.transform.GetChild(0).localScale.z) * 1000);
        obj.attributes["sizeUnit"] = "mm";
        obj.attributes["height"] = (newDevice.transform.GetChild(0).localScale.y * 1000).ToString();
        obj.attributes["heightUnit"] = "mm";
        if (string.IsNullOrEmpty(_data.slot))
        {
            obj.attributes["posZ"] = _data.posU.ToString();
            obj.attributes["posZUnit"] = "U";
        }
        else
            obj.attributes["slot"] = _data.slot;


        newDevice.GetComponent<DisplayObjectData>().UpdateLabels(newDevice.name);

        string hn = newDevice.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newDevice);

        if (!string.IsNullOrEmpty(_data.template))
        {
            Object[] components = newDevice.transform.GetComponentsInChildren<Object>();
            foreach (Object comp in components)
            {
                if (comp.gameObject != newDevice.gameObject)
                {
                    // Debug.Log($"[{newDevice.name}] => {comp.name}");
                    string compHn = comp.GetComponent<HierarchyName>().UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return newDevice.GetComponent<Object>();
    }

}
