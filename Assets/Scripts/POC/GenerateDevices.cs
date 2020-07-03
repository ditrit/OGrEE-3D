using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDevices : MonoBehaviour
{
    public void CreateDevice(SDeviceInfos _data)
    {
        GameObject newDevice = Instantiate(GameManager.gm.deviceModel, GameObject.Find(_data.parentName).transform);
        Object obj = newDevice.GetComponent<Object>();

        newDevice.name = _data.name;
        newDevice.transform.localScale = _data.size;
        newDevice.transform.localPosition = _data.pos;

        switch (_data.type)
        {
            case "powerpanel":
                obj.type = EObjectType.Powerpanel;
                break;
            case "airconditionner":
                obj.type = EObjectType.Airconditionner;
                break;
            case "chassis":
                obj.type = EObjectType.Chassis;
                break;
            case "compute":
                obj.type = EObjectType.Device;
                break;
            case "pdu":
                obj.type = EObjectType.Pdu;
                break;
            case "container":
                obj.type = EObjectType.Container;
                break;
        }

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
        }

        obj.model = _data.model;
        obj.vendor = _data.vendor;
    }

    public void CreateDevice(string _deviceJson)
    {
        SDeviceInfos data = JsonUtility.FromJson<SDeviceInfos>(_deviceJson);
        CreateDevice(data);
    }
}
