using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class CommandParser : MonoBehaviour
{
    public List<string> debugLines = new List<string>();

    // private GenerateDataCenter gDataCenter;
    // private GenerateItRoom gItRoom;
    // private GenerateRacks gRacks;
    // private GenerateDevices gDevices;

    // create:datacenter:[name]:[address]:[zipcode]:[city]:[country]:[desc]
    // create:datacenter:ALPHA:15 all Léon Gambetta:92110:Clichy:France:site de test

    // create:itroom:[name]:[parent]:[v2size(tile)]:[v2margin(tile)]:[v2pos(tile)]:[orient]
    // create:itroom:C8:ALPHA:30;60:3;3:0;0:front
    // create:itroom:C9:ALPHA:20;25:1;2:35;0:left

    // create:rack:[name]:[parent]:[v3size(cm;cm;u)]:[v2pos(tile)]:[orient]:[row]:[comment]
    // create:rack:B05:C8:60;100;42:1;1:rear:B:test
    // create:rack:B06:C8:60;100;40:2;1:rear:B:Lindemann

    // create:device:[type]:[name]:[parent]:[v3size(cm;cm;u)]:[v3pos(cm:cm:u)]:[orient]:[comment]
    // create:device:chassis:ch001:B05:60;100;2:0;0;0:front:First chassis created!
    // create:device:chassis:ch002:B05:60;100;2:0;0;2:front:Second chassis created!

    // update:[name]:[field]=[value]
    // update:B05:comment=This is a test.
    // update:ch001:serial=ABC-0042-XYZ

    // delete:[name]:[keepChildren(true|false)]
    // delete:B05:true

    // private void OnEnable()
    // {
        // gDataCenter = GetComponent<GenerateDataCenter>();
        // gItRoom = GetComponent<GenerateItRoom>();
        // gRacks = GetComponent<GenerateRacks>();
        // gDevices = GetComponent<GenerateDevices>();
    // }

    private void Start()
    {
        foreach (string str in debugLines)
        {
            if (!string.IsNullOrEmpty(str))
                ParseCmd(str);
        }
    }


    public void ParseCmd(string _input)
    {
        Debug.Log(_input);
        string[] str = _input.Split(new char[] { ':' }, 2);

        switch (str[0])
        {
            case "create":
                ParseCreate(str[1]);
                break;
            case "update":
                UpdateObject(str[1]);
                break;
            case "delete":
                DeleteObject(str[1]);
                break;
            default:
                Debug.LogWarning("Unknowned command");
                break;
        }
    }

    private void ParseCreate(string _input)
    {
        if (_input.StartsWith("datacenter"))
            CreateDataCenter(_input);
        else if (_input.StartsWith("itroom"))
            CreateItRoom(_input);
        else if (_input.StartsWith("rack"))
            CreateRack(_input);
        else if (_input.StartsWith("device"))
            CreateDevice(_input);
        else
            Debug.LogWarning("Unknowed object to create\nitroom / rack");
    }

    private void CreateDataCenter(string _input)
    {
        string regex = "datacenter:[^\\s:]+:[^:]+:[0-9]+:[a-zA-Z -]+:[a-zA-Z]+:[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':');
            if (AlreadyExists(data[1]))
                return;

            SDataCenterInfos infos = new SDataCenterInfos();
            infos.name = data[1];
            infos.address = data[2];
            infos.zipcode = data[3];
            infos.city = data[4];
            infos.country = data[5];
            infos.description = data[6];
            // gDataCenter.GenerateDC(infos);
            CustomerGenerator.instance.CreateDatacenter(infos);
        }
        else
            Debug.LogWarning("[CreateDataCenter] Syntax Error\ncreate:itroom:[name]:[parent]:[v2pos(tile)]:[v2margin(tile)]");
    }

    private void CreateItRoom(string _input)
    {
        string regex = "itroom:[^\\s:]+:[^\\s:]+:[0-9]+;[0-9]+:[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+:(front|rear|left|right)$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            if (AlreadyExists(data[1]))
                return;

            SRoomInfos infos = new SRoomInfos();
            infos.name = data[1];
            infos.parentName = data[2];
            infos.size = new Vector2(float.Parse(data[3]), float.Parse(data[4]));
            infos.margin = new Vector2(float.Parse(data[5]), float.Parse(data[6]));
            infos.pos = new Vector2(float.Parse(data[7]), float.Parse(data[8]));
            infos.orient = data[9];
            // gItRoom.CreateItRoom(infos);
            BuildingGenerator.instance.CreateRoom(infos);

            // gRacks.margin = infos.margin;
        }
        else
            Debug.LogWarning("[CreateItRoom] Syntax Error\ncreate:itroom:[name]:[parent]:[v2size(tile)]:[v2margin(tile)]:[v2pos(tile)]:[orient]");
    }

    private void CreateRack(string _input)
    {
        string regex = "rack:[^\\s:]+:[^\\s:]+:[0-9.]+;[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+:(front|rear):[A-Z]:[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            if (AlreadyExists(data[1]))
                return;

            SRackInfos infos = new SRackInfos();
            infos.name = data[1];
            infos.parentName = data[2];
            infos.orient = data[8];
            infos.pos = new Vector2(float.Parse(data[6]), float.Parse(data[7]));
            infos.size = new Vector2(float.Parse(data[3]), float.Parse(data[4]));
            infos.height = int.Parse(data[5]);
            infos.comment = data[10];
            infos.row = data[9];
            // gRacks.CreateRack(infos);
            ObjectGenerator.instance.CreateRack(infos);
        }
        else
            Debug.LogWarning("[CreateRack] Syntax Error\ncreate:rack:[name]:[parent]:[v3size(cm;cm;u)]:[v2pos(tile)]:[orient]:[row]:[comment]");
    }

    private void CreateDevice(string _input)
    {
        string regex = "device:[^\\s:]+:[^\\s:]+:[^\\s:]+:[0-9.]+;[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+;[0-9.]+:(front|rear):[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            if (AlreadyExists(data[2]))
                return;

            SDeviceInfos infos = new SDeviceInfos();
            infos.type = data[1];
            infos.name = data[2];
            infos.parentName = data[3];
            infos.size = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));
            infos.pos = new Vector3(float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]));
            infos.orient = data[10];
            infos.comment = data[11];
            // gDevices.CreateDevice(infos);
            ObjectGenerator.instance.CreateChassis(infos);
        }
        else
            Debug.LogWarning(("[CreateDevice] Syntex Error\ncreate:device:[type]:[name]:[parent]:[v3size(cm;cm;u)]:[v3pos(cm:cm:u)]:[orient]:[comment]"));
    }

    private void UpdateObject(string _input)
    {
        string regex = "[^\\s:]+:[^\\s:]+=[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', '=');
            GameObject objToUpdate = GameObject.Find(data[0]);
            if (objToUpdate)
                objToUpdate.GetComponent<Object>().UpdateField(data[1], data[2]);
            else
                Debug.LogWarning($"{data[0]} doesn't exists.");
        }
        else
            Debug.LogWarning("[UpdateObject] Syntax Error\nupdate:[name]:[field]=[value]");
    }

    private void DeleteObject(string _input)
    {
        string regex = "[^\\s:]+:(true|false)$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':');
            GameObject objToDel = GameObject.Find(data[0]);
            if (objToDel)
            {
                if (data[1] == "true")
                {
                    Transform tmp = objToDel.transform.parent;
                    foreach (Transform child in objToDel.transform)
                    {
                        if (child.GetComponent<Object>())
                            child.parent = tmp;
                    }
                }
                Destroy(objToDel);
            }
            else
                Debug.LogWarning("Unkowned object to delete");
        }
        else
            Debug.LogWarning("[DeleteObject] Syntax Error\ndelete:[name]:[keepChildren(true|false)]");
    }

    private bool AlreadyExists(string _name)
    {
        GameObject go = GameObject.Find(_name);
        if (go)
        {
            Debug.LogWarning($"{_name} already exists.");
            return true;
        }
        return false;
    }
}
