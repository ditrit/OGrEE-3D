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

    public void CreateRack(SRackInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Room>() == null)
        {
            Debug.LogWarning("Rack must be child of a Room");
            return;
        }

        GameObject newRack = Instantiate(GameManager.gm.rackModel);
        newRack.name = _data.name;
        newRack.transform.parent = _data.parent;
        
        newRack.transform.GetChild(0).localScale = new Vector3(_data.size.x / 100, _data.height * GameManager.gm.uSize, _data.size.y / 100);

        Vector3 origin = newRack.transform.parent.GetChild(0).localScale / -0.2f;
        newRack.transform.position = newRack.transform.parent.GetChild(0).position;
        newRack.transform.localPosition += new Vector3(origin.x, 0, origin.z);
        newRack.transform.localPosition += newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.localPosition += new Vector3(_data.pos.x - 1, 0, _data.pos.y - 1) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.description = _data.comment;
        rack.posXY = _data.pos;
        rack.posXYUnit = EUnit.tile;
        rack.size = new Vector2(_data.size.x, _data.size.y);
        rack.sizeUnit = EUnit.cm;
        rack.height = _data.height;
        rack.heightUnit = EUnit.U;
        switch (_data.orient)
        {
            case "front":
                rack.orient = EObjOrient.Frontward;
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "rear":
                rack.orient = EObjOrient.Backward;
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "left":
                rack.orient = EObjOrient.Left;
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case "right":
                rack.orient = EObjOrient.Right;
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
        }

        Filters.instance.AddIfUnknowned(Filters.instance.racks, newRack);
        Filters.instance.AddIfUnknowned(Filters.instance.rackRowsList, newRack.name[0].ToString());
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRackRows, Filters.instance.rackRowsList);

        newRack.GetComponent<DisplayRackData>().FillTexts();

        newRack.AddComponent<HierarchyName>();
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newRack);
    }

    public void CreateChassis(SDeviceInfos _data)
    {
        GameObject newDevice = Instantiate(GameManager.gm.deviceModel, GameObject.Find(_data.parentName).transform);
        newDevice.name = _data.name;
        Vector3 size = new Vector3(_data.size.x / 100, _data.size.z * GameManager.gm.uSize, _data.size.y / 100);
        newDevice.transform.localScale = size;
        Vector3 origin = new Vector3(0, -newDevice.transform.parent.GetChild(0).localScale.y + newDevice.transform.localScale.y, 0) / 2;
        newDevice.transform.localPosition = origin;
        Vector3 pos = new Vector3(_data.pos.x, _data.pos.z * 0.0445f, _data.pos.y);
        newDevice.transform.localPosition += pos;

        Object obj = newDevice.GetComponent<Object>();
        obj.family = EObjFamily.chassis;
        switch (_data.orient)
        {
            case "front":
                obj.orient = EObjOrient.Frontward;
                newDevice.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "rear":
                obj.orient = EObjOrient.Backward;
                newDevice.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "left":
                obj.orient = EObjOrient.Left;
                newDevice.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case "right":
                obj.orient = EObjOrient.Right;
                newDevice.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
        }
        obj.model = _data.model;
        obj.serial = _data.serial;
        obj.vendor = _data.vendor;
        obj.description = _data.comment;

        newDevice.AddComponent<HierarchyName>();
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
