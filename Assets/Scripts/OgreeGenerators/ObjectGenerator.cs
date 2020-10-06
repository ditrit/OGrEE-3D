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
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    ///<returns>The created Rack</returns>
    public Rack CreateRack(SRackInfos _data, bool _changeHierarchy)
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
            newRack.transform.GetChild(0).localScale = new Vector3(_data.size.x / 100, _data.height * GameManager.gm.uSize, _data.size.y / 100);

        Vector3 origin = newRack.transform.parent.GetChild(0).localScale / -0.2f;
        Vector3 boxOrigin = newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.position = newRack.transform.parent.GetChild(0).position;
        newRack.transform.localPosition += new Vector3(origin.x, 0, origin.z);
        newRack.transform.localPosition += new Vector3(_data.pos.x - 1, 0, _data.pos.y - 1) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.posXY = _data.pos;
        rack.posXYUnit = EUnit.tile;
        if (string.IsNullOrEmpty(_data.template))
        {
            rack.size = new Vector2(_data.size.x, _data.size.y);
            rack.sizeUnit = EUnit.cm;
            rack.height = _data.height;
            rack.heightUnit = EUnit.U;
        }
        switch (_data.orient)
        {
            case "front":
                rack.orient = EObjOrient.Frontward;
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                newRack.transform.localPosition += boxOrigin;
                break;
            case "rear":
                rack.orient = EObjOrient.Backward;
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
            case "left":
                rack.orient = EObjOrient.Left;
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                newRack.transform.localPosition += new Vector3(-boxOrigin.z, boxOrigin.y, boxOrigin.x);
                newRack.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, 0);
                break;
            case "right":
                rack.orient = EObjOrient.Right;
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
        }

        newRack.GetComponent<DisplayRackData>().PlaceTexts();
        newRack.GetComponent<DisplayRackData>().FillTexts();

        newRack.AddComponent<HierarchyName>();

        rack.tenant = _data.parent.GetComponent<Room>().tenant;
        rack.UpdateColor();
        GameManager.gm.SetRackMaterial(newRack.transform);

        GameManager.gm.allItems.Add(hierarchyName, newRack);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newRack);

        return rack;
    }

    ///<summary>
    /// Instantiate a deviceModel or a deviceTemplate (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the chassis</param>
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    ///<returns>The created Chassis</returns>
    public Object CreateDevice(SDeviceInfos _data, bool _changeHierarchy)
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

        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.GetHierarchyName()}.{_data.name}";
        // Debug.Log("Create " + hierarchyName);
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
                newDevice = Instantiate(GameManager.gm.deviceModel);
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
                if (child.name == _data.slot || (i > 0 && i < max))
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
                    newDevice = Instantiate(GameManager.gm.deviceModel);
                    newDevice.transform.parent = _data.parent;
                    newDevice.transform.GetChild(0).localScale = new Vector3(slot.transform.localScale.x, _data.sizeU * slot.localScale.y, slot.localScale.z);

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
                if (newDevice.transform.GetChild(0).localScale.y > slot.localScale.y)
                    newDevice.transform.localPosition += new Vector3(0, newDevice.transform.GetChild(0).localScale.y / 2 - GameManager.gm.uSize / 2, 0);

                float deltaZ = slot.localScale.z - newDevice.transform.GetChild(0).localScale.z;
                if (newDevice.GetComponent<Object>().orient == EObjOrient.Frontward
                    || newDevice.GetComponent<Object>().extras["fulllenght"] == "yes")
                    newDevice.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                else if (newDevice.GetComponent<Object>().orient == EObjOrient.Backward)
                    newDevice.transform.localPosition -= new Vector3(0, 0, deltaZ / 2);

                // Assign default color = slot color
                Material mat = newDevice.transform.GetChild(0).GetComponent<Renderer>().material;
                Color slotColor = slot.GetComponent<Renderer>().material.color;
                mat.color = new Color(slotColor.r, slotColor.g, slotColor.b);
            }
            else
            {
                GameManager.gm.AppendLogLine("Slot doesn't exist", "red");
                return null;
            }
        }
        newDevice.transform.localEulerAngles = Vector3.zero;

        newDevice.name = _data.name;
        Object obj = newDevice.GetComponent<Object>();
        obj.size = new Vector2(newDevice.transform.GetChild(0).localScale.x,
                                newDevice.transform.GetChild(0).localScale.z) * 1000;
        obj.sizeUnit = EUnit.mm;
        obj.height = newDevice.transform.GetChild(0).localScale.y * 1000;
        obj.heightUnit = EUnit.mm;
        if (string.IsNullOrEmpty(_data.slot))
        {
            obj.posZ = _data.posU;
            obj.posZUnit = EUnit.U;
        }
        else
            obj.extras.Add("slot", _data.slot);

        obj.tenant = _data.parent.GetComponent<Object>().tenant;

        newDevice.GetComponent<DisplayObjectData>().UpdateLabels(newDevice.name);

        newDevice.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(hierarchyName, newDevice);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newDevice);

        return newDevice.GetComponent<Object>();
    }

    public void CreateAirconditionner()
    {

    }

    public void CreatePowerpanel()
    {

    }

    public void CreatePdu()
    {

    }

}
