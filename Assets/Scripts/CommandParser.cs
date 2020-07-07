using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class CommandParser : MonoBehaviour
{
    public List<string> debugLines = new List<string>();

    private GenerateItRoom gItRoom;
    private GenerateRacks gRacks;
    private GenerateDevices gDevices;

    private void OnEnable()
    {
        gItRoom = GetComponent<GenerateItRoom>();
        gRacks = GetComponent<GenerateRacks>();
        gDevices = GetComponent<GenerateDevices>();
    }

    private void Start()
    {
        // create:itroom:[name]:[v2pos(tile)]:[v2margin(tile)]
        // create:itroom:ALPHA:30;60:3;3

        // create:rack:[name]:[v3size(cm;cm;u)]:[v2pos(tile)]:[orient]:[row]:[comment]
        // create:rack:B05:60;100;42:1;1:rear:B:test
        // create:rack:B06:60;100;40:2;1:rear:B:Lindemann

        // create:device:[type]:[name]:[parent]:[v3size(cm;cm;u)]:[v3pos(cm:cm:u)]:[orient]:[comment]
        // create:device:chassis:ch001:B05:60;100;2:0;0;0:front:First chassis created!
        // create:device:chassis:ch002:B05:60;100;2:0;0;2:front:Second chassis created!

        // update:[name]:[field]=[value]
        // update:B05:comment=This is a test.
        // update:ch001:serial=ABC-0042-XYZ

        // delete:[name]:[keepChildren(true|false)]
        // delete:B05:true

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
        // Debug.Log($"[ParseCreate] {_input}");
        if (_input.StartsWith("itroom"))
            CreateItRoom(_input);
        else if (_input.StartsWith("rack"))
            CreateRack(_input);
        else if (_input.StartsWith("device"))
            CreateDevice(_input);
        else
            Debug.LogWarning("Unknowed object to create\nitroom / rack");
    }

    private void CreateItRoom(string _input)
    {
        string regex = "itroom:[^\\s:]+:[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            gItRoom.CreateItRoom(data[1], new Vector2(float.Parse(data[2]), float.Parse(data[3])),
                                    new Vector2(float.Parse(data[4]), float.Parse(data[5])));

            gRacks.margin = new Vector2(float.Parse(data[4]), float.Parse(data[5]));
            gRacks.root = GameObject.Find(data[1]).transform;
        }
        else
            Debug.LogWarning("[CreateItRoom] Syntax Error\ncreate:itroom:[name]:[v2pos(tile)]:[v2margin(tile)]");
    }

    private void CreateRack(string _input)
    {
        string regex = "rack:[^\\s:]+:[0-9.]+;[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+:(front|rear):[A-Z]:[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            SRackInfos infos = new SRackInfos();
            infos.name = data[1];
            infos.orient = data[7];
            infos.pos = new Vector2(float.Parse(data[5]), float.Parse(data[6]));
            infos.size = new Vector2(float.Parse(data[2]), float.Parse(data[3]));
            infos.height = int.Parse(data[4]);
            infos.comment = data[9];
            infos.row = data[8];
            gRacks.CreateRack(infos);
        }
        else
            Debug.LogWarning("[CreateRack] Syntax Error\ncreate:rack:[name]:[v3size(cm;cm;u)]:[v2pos(tile)]:[orient]:[row]:[comment]");
    }

    private void CreateDevice(string _input)
    {
        string regex = "device:[^\\s:]+:[^\\s:]+:[^\\s:]+:[0-9.]+;[0-9.]+;[0-9.]+:[0-9.]+;[0-9.]+;[0-9.]+:(front|rear):[^:]*$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(':', ';');
            SDeviceInfos infos = new SDeviceInfos();
            infos.type = data[1];
            infos.name = data[2];
            infos.parentName = data[3];
            infos.size = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));
            infos.pos = new Vector3(float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]));
            infos.orient = data[10];
            infos.comment = data[11];
            gDevices.CreateDevice(infos);
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
            Object objToUpdate = GameObject.Find(data[0]).GetComponent<Object>();
            if (objToUpdate)
                objToUpdate.UpdateField(data[1], data[2]);
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
}
