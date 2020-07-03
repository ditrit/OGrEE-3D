using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFromJSON : MonoBehaviour
{
    public NetboxRack.SNbRack rack;
    public NetboxDevice.SNbDevices devices;

    private TextAsset file;
    private GenerateRacks gr;
    private GenerateDevices gd;
    private RackFilter rf;

    private void Awake()
    {
        gr = GetComponent<GenerateRacks>();
        gd = GetComponent<GenerateDevices>();
        rf = GetComponent<RackFilter>();
    }

    private void Start()
    {
        file = Resources.Load<TextAsset>("EDF.NOE.C8.BO5.rack");
        if (file)
            rack = JsonUtility.FromJson<NetboxRack.SNbRack>(file.ToString());
        Resources.UnloadAsset(file);

        file = Resources.Load<TextAsset>("EDF.NOE.C8.BO5.devices");
        if (file)
            devices = JsonUtility.FromJson<NetboxDevice.SNbDevices>(file.ToString());
        Resources.UnloadAsset(file);

        CreateRack(rack);
        CreateRackDevices(devices);
    }

    public void CreateRack(NetboxRack.SNbRack _rack)
    {
        rf.racks.Clear();
        rf.DefaultList(rf.rackRows, "All");

        SRackInfos data = new SRackInfos();
        data.name = _rack.name;
        data.orient = "";//
        data.pos = Vector3.zero;//
        data.size = new Vector2(_rack.outer_width, _rack.outer_depth) / 10; // mm to cm
        data.height = _rack.u_height;
        data.comment = _rack.comments;
        data.row = data.name[0].ToString();// works only if named like [A-Z][digits]

        gr.CreateRack(data);

        rf.AddIfUnknowned(rf.rackRows, data.row);
        rf.UpdateDropdownFromList(rf.dropdownRackRows, rf.rackRows);
    }

    public void CreateRackDevices(NetboxDevice.SNbDevices _devices)
    {
        foreach (NetboxDevice.SNbDevice device in _devices.results)
        {
            if (device.name != "host-installer")
            {
                SDeviceInfos data = new SDeviceInfos();
                data.name = device.name;
                data.parentName = device.rack.name; // device.parent_device.name;
                data.pos = Vector3.zero; //
                data.size = Vector3.one * 0.01f; //
                data.type = device.device_role.slug;
                data.orient = device.face.value;
                data.model = device.device_type.model;
                data.serial = device.serial;
                data.vendor = device.device_type.manufacturer.name;
                data.comment = device.comments;

                gd.CreateDevice(data);
            }
        }
    }

}
