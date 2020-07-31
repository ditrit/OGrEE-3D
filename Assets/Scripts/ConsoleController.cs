﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/*

+customer:[name]
+customer:DEMO

+datacenter:[name]@[orientation]
+datacenter:BETA@N

+building:[name]@[pos]@[size]
+building:/DEMO.BETA.A@[0,80,0]@[20,30,4]
+building:/DEMO.BETA.B@[0,20,0]@[20,30,4]
+bd:C@[30,0,0]@[60,120,5]

+room:[name]@[pos]@[size]@[orientation]
+room:R1@[0,15,0]@[60,60,5]@W
+room:/DEMO.BETA.C.R2@[0,75,0]@[60,60,5]@W
+ro:/DEMO.BETA.C.Office@[60,0,0]@[20,75,4]@N

+zones:[reserved N,S,E,W]@[technical N,S,E,W]
+zones:[2,1,3,3]@[4,4,4,4]

+rack:[name]@[pos]@[size]@orient
+rack:[name]@[pos]@[preset]@orient

*/

public class ConsoleController
{

    // Used to communicate with ConsoleView
    public delegate void LogChangedHandler(string[] log);
    public event LogChangedHandler logChanged;

    /// <summary>
    /// How many log lines should be retained?
    /// Note that strings submitted to AppendLogLine with embedded newlines will be counted as a single line.
    /// </summary>
    const int scrollbackSize = 20;
    Queue<string> scrollback = new Queue<string>(scrollbackSize);
    public string[] log { get; private set; } //Copy of scrollback as an array for easier use by ConsoleView

    // private Dictionary<string, System.Action> createMethods;

    // public ConsoleController()
    // {
    //     createMethods = new Dictionary<string, System.Action>();
    //     createMethods.Add("customer", CreateCustomer());
    // }

    /// <summary>
    /// Collecting log output by Eliot Lash
    /// </summary>
    public void AppendLogLine(string line, string color = "white")
    {
        Debug.Log(line);
        line = $"<color={color}>{line}</color>";

        if (scrollback.Count >= ConsoleController.scrollbackSize)
        {
            scrollback.Dequeue();
        }
        scrollback.Enqueue(line);

        log = scrollback.ToArray();
        if (logChanged != null)
        {
            logChanged(log);
        }
    }

    public void RunCommandString(string _input)
    {
        if (string.IsNullOrEmpty(_input))
            return;

        AppendLogLine("$ " + _input);
        if (_input[0] == '+')
            ParseCreate(_input.Substring(1));
        else if (_input == "..")
            SelectParent();
        else if (_input[0] == '=')
            SelectItem(_input.Substring(1));
        else
            AppendLogLine("Unknowned command", "red");
    }

    #region HierarchyMethods()

    private void SelectParent()
    {
        if (!GameManager.gm.currentItem)
            return;
        else if (GameManager.gm.currentItem.GetComponent<Customer>())
            GameManager.gm.SetCurrentItem(null);
        else
        {
            GameObject parent = GameManager.gm.currentItem.transform.parent.gameObject;
            if (parent)
                GameManager.gm.SetCurrentItem(parent);
        }

    }

    private void SelectItem(string _input)
    {
        HierarchyName[] allObjects = GameObject.FindObjectsOfType<HierarchyName>();
        foreach (HierarchyName obj in allObjects)
        {
            if (obj.fullname == _input)
            {
                GameManager.gm.SetCurrentItem(obj.gameObject);
                return;
            }
        }
        AppendLogLine("Error: Object does not exist");
    }

    #endregion

    #region CreateMethods

    private void ParseCreate(string _input)
    {
        string[] str = _input.Split(new char[] { ':' }, 2);

        if (str[0] == "customer" || str[0] == "cu")
            CreateCustomer(str[1]);
        else if (str[0] == "datacenter" || str[0] == "dc")
            CreateDataCenter(str[1]);
        else if (str[0] == "building" || str[0] == "bd")
            CreateBuilding(str[1]);
        else if (str[0] == "room" || str[0] == "ro")
            CreateRoom(str[1]);
        else if (str[0] == "zones")
            SetRoomZones(str[1]);
        else if (str[0] == "rack" || str[0] == "rk")
            CreateRack(str[1]);
        else
            AppendLogLine("Unknowned command", "red");

        // createMethods[str[0]](str[1]);

    }

    private void CreateCustomer(string _input)
    {
        string regex = "^[^.]+$";
        if (Regex.IsMatch(_input, regex))
        {
            if (_input.StartsWith("/"))
            {
                _input = _input.Substring(1);
                CustomerGenerator.instance.CreateCustomer(_input, false);
            }
            else
                CustomerGenerator.instance.CreateCustomer(_input, true);
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    private void CreateDataCenter(string _input)
    {
        string regex = "^[^:]+@(N|S|W|E)$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split('@');

            SDataCenterInfos infos = new SDataCenterInfos();
            infos.orient = data[1];
            // infos.address = data[2];
            // infos.zipcode = data[3];
            // infos.city = data[4];
            // infos.country = data[5];
            // infos.description = data[6];

            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                string[] path = data[0].Split('.');
                infos.name = path[1];
                GameObject tmp = GameObject.Find(path[0]);
                if (tmp)
                {
                    infos.parent = tmp.transform;
                    CustomerGenerator.instance.CreateDatacenter(infos, false);
                }
                else
                    AppendLogLine("Error: customer doesn't exist", "red");
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItem.transform;
                CustomerGenerator.instance.CreateDatacenter(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    private void CreateBuilding(string _input)
    {
        string regex = "^[^@]+@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split('@');

            SBuildingInfos infos = new SBuildingInfos();
            infos.pos = ParseVector3(data[1]);
            infos.size = ParseVector3(data[2]);

            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                IsolateParent(data[0], out infos.parent, out infos.name);
                if (infos.parent)
                    BuildingGenerator.instance.CreateBuilding(infos, false);
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItem.transform;
                BuildingGenerator.instance.CreateBuilding(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");

    }

    private void CreateRoom(string _input)
    {
        string regex = "^[^@]+@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(N|S|W|E)$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split('@');

            SRoomInfos infos = new SRoomInfos();
            infos.pos = ParseVector3(data[1]);
            infos.size = ParseVector3(data[2]);
            infos.orient = data[3];

            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                string[] path = data[0].Split('.');
                string parentPath = "";
                for (int i = 0; i < path.Length - 1; i++)
                    parentPath += $"{path[i]}.";
                parentPath = parentPath.Remove(parentPath.Length - 1);
                GameObject tmp = GameManager.gm.FindAbsPath(parentPath);
                if (tmp)
                {
                    infos.name = path[path.Length - 1];
                    infos.parent = tmp.transform;
                    BuildingGenerator.instance.CreateRoom(infos, false);
                }
                else
                    AppendLogLine("Error: path doesn't exist", "red");
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItem.transform;
                BuildingGenerator.instance.CreateRoom(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");

    }

    private void CreateRack(string _input)
    {
        string regex = "^[^:]+@\\[[0-9]+,[0-9]+\\]@\\[[0-9.]+,[0-9.]+,[0-9]+\\]@(front|rear|left|right)$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split('@');

            SRackInfos infos = new SRackInfos();
            infos.pos = ParseVector2(data[1]);
            data[2] = data[2].Trim('[', ']');
            string[] vector = data[2].Split(',');
            infos.size = new Vector2(float.Parse(vector[0]), float.Parse(vector[1]));
            infos.height = int.Parse(vector[2]);
            infos.orient = data[3];

            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                IsolateParent(data[0], out infos.parent, out infos.name);
                if (infos.parent)
                    ObjectGenerator.instance.CreateRack(infos, false);
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItem.transform;
                ObjectGenerator.instance.CreateRack(infos, true);
            }

        }
        else
            AppendLogLine("Syntax error", "red");
    }

    #endregion

    #region SetMethods

    private void SetRoomZones(string _input)
    {
        string regex = "^(\\/[^:]+@)*\\[([0-9.]+,){3}[0-9.]+\\]@\\[([0-9.]+,){3}[0-9.]+\\]$";
        if (Regex.IsMatch(_input, regex))
        {
            _input = _input.Replace("[", "");
            _input = _input.Replace("]", "");
            string[] data = _input.Split('@', ',');
            if (data.Length == 8) // No path -> On current object
            {
                if (GameManager.gm.currentItem.GetComponent<Room>())
                {
                    SMargin resDim = new SMargin(float.Parse(data[0]), float.Parse(data[1]),
                                                float.Parse(data[2]), float.Parse(data[3]));
                    SMargin techDim = new SMargin(float.Parse(data[4]), float.Parse(data[5]),
                                                float.Parse(data[6]), float.Parse(data[7]));
                    GameManager.gm.currentItem.GetComponent<Room>().SetZones(resDim, techDim);
                }
                else
                    AppendLogLine("Current object must be a room");

            }
            else // There is an object path
            {
                GameObject room = GameManager.gm.FindAbsPath(data[0].Substring(1));
                if (room)
                {
                    SMargin resDim = new SMargin(float.Parse(data[1]), float.Parse(data[2]),
                                                float.Parse(data[3]), float.Parse(data[4]));
                    SMargin techDim = new SMargin(float.Parse(data[5]), float.Parse(data[6]),
                                                float.Parse(data[7]), float.Parse(data[8]));
                    room.GetComponent<Room>().SetZones(resDim, techDim);
                }
                else
                    AppendLogLine("Error: path doesn't exist");
            }
        }
        else
            AppendLogLine("Syntax error");
    }

    #endregion

    #region Utils

    private Vector2 ParseVector2(string _input)
    {
        Vector2 res = new Vector2();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = float.Parse(parts[0]);
        res.y = float.Parse(parts[1]);
        return res;
    }

    private Vector3 ParseVector3(string _input, bool _YUp = true)
    {
        Vector3 res = new Vector3();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = float.Parse(parts[0]);
        if (_YUp)
        {
            res.y = float.Parse(parts[2]);
            res.z = float.Parse(parts[1]);
        }
        else
        {
            res.y = float.Parse(parts[1]);
            res.z = float.Parse(parts[2]);
        }
        return res;
    }

    private void IsolateParent(string _input, out Transform parent, out string name)
    {
        string[] path = _input.Split('.');
        string parentPath = "";
        for (int i = 0; i < path.Length - 1; i++)
            parentPath += $"{path[i]}.";
        parentPath = parentPath.Remove(parentPath.Length - 1);
        GameObject tmp = GameManager.gm.FindAbsPath(parentPath);
        if (tmp)
        {
            name = path[path.Length - 1];
            parent = tmp.transform;
        }
        else
        {
            parent = null;
            name = "";
            AppendLogLine("Error: path doesn't exist", "red");
        }
    }

    #endregion

}
