using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDevices : MonoBehaviour
{
    public void CreateDevice(SDeviceInfos _data)
    {
        GameObject newDevice = Instantiate(GameManager.gm.deviceModel, GameObject.Find(_data.parentName).transform);
        newDevice.name = _data.name;
        Vector3 size = new Vector3(_data.size.x / 100, _data.size.z * 0.0445f, _data.size.y / 100);
        newDevice.transform.localScale = size;
        Vector3 origin = new Vector3(0, -newDevice.transform.parent.GetChild(0).localScale.y + newDevice.transform.localScale.y, 0) / 2;
        newDevice.transform.localPosition = origin;
        Vector3 pos = new Vector3(_data.pos.x, _data.pos.z * 0.0445f, _data.pos.y);
        newDevice.transform.localPosition += pos;

        Object obj = newDevice.GetComponent<Object>();
        // switch (_data.type)
        // {
        //     case "powerpanel":
        //         obj.type = EObjectType.Powerpanel;
        //         break;
        //     case "airconditionner":
        //         obj.type = EObjectType.Airconditionner;
        //         break;
        //     case "chassis":
        //         obj.type = EObjectType.Chassis;
        //         break;
        //     case "compute":
        //         obj.type = EObjectType.Device;
        //         break;
        //     case "pdu":
        //         obj.type = EObjectType.Pdu;
        //         break;
        //     case "container":
        //         obj.type = EObjectType.Container;
        //         break;
        // }
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
    }

    public void CreateDevice(string _deviceJson)
    {
        SDeviceInfos data = JsonUtility.FromJson<SDeviceInfos>(_deviceJson);
        CreateDevice(data);
    }
}
