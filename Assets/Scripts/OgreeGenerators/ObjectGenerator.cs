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
            newRack = Instantiate(GameManager.gm.rackTemplates[_data.template]);
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
    /// Instantiate a chassisModel or a chassisTemplate (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the chassis</param>
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    ///<returns>The created Chassis</returns>
    public Object CreateChassis(SChassisInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Rack>() == null)
        {
            GameManager.gm.AppendLogLine("Chassis must be child of a Rack", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.GetHierarchyName()}.{_data.name}";
        // Debug.Log("Create " + hierarchyName);
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newChassis;
        if (string.IsNullOrEmpty(_data.slot))
        {
            //+chassis:[name]@[posU]@[sizeU]
            if (string.IsNullOrEmpty(_data.template))
            {
                newChassis = Instantiate(GameManager.gm.chassisModel);
                newChassis.transform.parent = _data.parent;
                newChassis.transform.GetChild(0).localScale = new Vector3(_data.parent.GetChild(0).localScale.x,
                                                                _data.sizeU * GameManager.gm.uSize,
                                                                _data.parent.GetChild(0).localScale.z);
            }
            //+chassis:[name]@[posU]@[template]
            else
            {
                newChassis = Instantiate(GameManager.gm.chassisTemplates[_data.template]);
                newChassis.transform.parent = _data.parent;
                Renderer[] renderers = newChassis.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                    r.enabled = true;
                Destroy(newChassis.GetComponent<HierarchyName>());
            }
            newChassis.GetComponent<DisplayObjectData>().PlaceTexts("frontrear");
            newChassis.transform.localEulerAngles = Vector3.zero;
            newChassis.transform.localPosition = new Vector3(0, (-_data.parent.GetChild(0).localScale.y + newChassis.transform.GetChild(0).localScale.y) / 2, 0);
            newChassis.transform.localPosition += new Vector3(0, (_data.posU - 1) * GameManager.gm.uSize, 0);

            float deltaZ = _data.parent.GetChild(0).localScale.z - newChassis.transform.GetChild(0).localScale.z;
            newChassis.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
        }
        else
        {
            List<Slot> takenSlots = new List<Slot>();
            int i = 0;
            float max;
            if (string.IsNullOrEmpty(_data.template))
                max = _data.sizeU;
            else
                max = GameManager.gm.chassisTemplates[_data.template].transform.GetChild(0).localScale.y / GameManager.gm.uSize;
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
                    newChassis = Instantiate(GameManager.gm.chassisModel);
                    newChassis.transform.parent = _data.parent;
                    newChassis.transform.GetChild(0).localScale = new Vector3(slot.transform.localScale.x, _data.sizeU * slot.localScale.y, slot.localScale.z);

                }
                //+chassis:[name]@[slot]@[template]
                else
                {
                    newChassis = Instantiate(GameManager.gm.chassisTemplates[_data.template]);
                    newChassis.transform.parent = _data.parent;
                    Renderer[] renderers = newChassis.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                        r.enabled = true;
                    Destroy(newChassis.GetComponent<HierarchyName>());
                }
                newChassis.GetComponent<DisplayObjectData>().PlaceTexts(slot.GetComponent<Slot>().labelPos);
                newChassis.transform.localPosition = slot.localPosition;
                if (newChassis.transform.GetChild(0).localScale.y > slot.localScale.y)
                    newChassis.transform.localPosition += new Vector3(0, newChassis.transform.GetChild(0).localScale.y / 2 - GameManager.gm.uSize / 2, 0);

                float deltaZ = slot.localScale.z - newChassis.transform.GetChild(0).localScale.z;
                if (newChassis.GetComponent<Object>().orient == EObjOrient.Frontward
                    || newChassis.GetComponent<Object>().extras["fulllenght"] == "yes")
                    newChassis.transform.localPosition += new Vector3(0, 0, deltaZ / 2);
                else if (newChassis.GetComponent<Object>().orient == EObjOrient.Backward)
                    newChassis.transform.localPosition -= new Vector3(0, 0, deltaZ / 2);
            }
            else
            {
                GameManager.gm.AppendLogLine("Slot doesn't exist", "red");
                return null;
            }
        }
        newChassis.transform.localEulerAngles = Vector3.zero;

        newChassis.name = _data.name;
        newChassis.GetComponent<DisplayObjectData>().UpdateLabels(newChassis.name);

        newChassis.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(hierarchyName, newChassis);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newChassis);

        return newChassis.GetComponent<Object>();
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

    public void CreateDevice()
    {

    }

    public void CreateComponent()
    {

    }

}
