﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ConsoleController : MonoBehaviour
{
    // Used to communicate with ConsoleView
    public delegate void LogChangedHandler(string[] log);
    public event LogChangedHandler logChanged;

    /// <summary>
    /// How many log lines should be retained?
    /// Note that strings submitted to AppendLogLine with embedded newlines will be counted as a single line.
    /// </summary>
    const int scrollbackSize = 150;
    Queue<string> scrollback = new Queue<string>(scrollbackSize);
    public string[] log { get; private set; } //Copy of scrollback as an array for easier use by ConsoleView

    public ReadFromJson rfJson = new ReadFromJson();
    public Dictionary<string, string> variables = new Dictionary<string, string>();

    [SerializeField] private bool isReady = true;
    public float timerValue = 1f;
    public bool timer = false;


    ///<summary>
    /// Collecting log output by Eliot Lash.
    ///</summary>
    ///<param name="_line">The line to display</param>
    ///<param name="_color">The color of the line, white by default</param>
    public void AppendLogLine(string _line, string _color = "white")
    {
        if (_color == "yellow")
            Debug.LogWarning(_line);
        else if (_color == "red")
            Debug.LogError(_line);
        else
            Debug.Log(_line);

        _line = $"<color={_color}>{_line}</color>";

        if (scrollback.Count >= ConsoleController.scrollbackSize)
        {
            scrollback.Dequeue();
        }
        scrollback.Enqueue(_line);

        log = scrollback.ToArray();
        if (logChanged != null)
        {
            logChanged(log);
        }
    }

    ///<summary>
    /// Execute a command line. Look for the first char to call the corresponding method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    ///<param name="_saveCmd">If ".cmds", save it in GameManager ? true by default</param>
    public void RunCommandString(string _input, bool _saveCmd = true)
    {
        if (string.IsNullOrEmpty(_input.Trim()) || _input.StartsWith("//"))
            return;

        StartCoroutine(WaitAndRunCmdStr(_input.Trim(), _saveCmd));
    }

    ///<summary>
    /// Wait until ConsoleController.isReady for jumping to next command string.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    ///<param name="_saveCmd">If ".cmds", save it in GameManager ?</param>
    private IEnumerator WaitAndRunCmdStr(string _input, bool _saveCmd)
    {
        yield return new WaitUntil(() => isReady == true);
        isReady = false;

        _input = ApplyVariables(_input);
        AppendLogLine("$ " + _input);
        if (_input == "..")
            SelectParent();
        else if (_input[0] == '=')
            SelectItem(_input.Substring(1));
        else if (_input[0] == '.')
            ParseLoad(_input.Substring(1), _saveCmd);
        else if (_input[0] == '+')
            ParseCreate(_input.Substring(1));
        else if (_input[0] == '-')
            StartCoroutine(DeleteItem(_input.Substring(1)));
        else if (_input.Contains(".") && _input.Contains("="))
            SetAttribute(_input);
        else
        {
            AppendLogLine("Unknown command", "red");
            isReady = true;
        }
        if (timer)
        {
            isReady = false;
            yield return new WaitForSeconds(timerValue);
            isReady = true;
        }
    }

    #region HierarchyMethods()

    ///<summary>
    /// Set GameManager.currentItem as the parent of it in Ogree objects hierarchy.
    ///</summary>
    private void SelectParent()
    {
        if (!GameManager.gm.currentItems[0])
        {
            isReady = true;
            return;
        }
        else if (GameManager.gm.currentItems[0].GetComponent<Customer>())
            GameManager.gm.SetCurrentItem(null);
        else
        {
            GameObject parent = GameManager.gm.currentItems[0].transform.parent.gameObject;
            if (parent)
                GameManager.gm.SetCurrentItem(parent);
        }

        isReady = true;
    }

    ///<summary>
    /// Look in all HierarchyNames for _input, set it as GameManager.currentItem.
    ///</summary>
    ///<param name="_input">HierarchyName of the object to select</param>
    private void SelectItem(string _input)
    {
        if (string.IsNullOrEmpty(_input))
        {
            GameManager.gm.SetCurrentItem(null);
            isReady = true;
            return;
        }
        if (_input.StartsWith("{") && _input.EndsWith("}"))
        {
            Transform root = GameManager.gm.currentItems[0].transform;
            GameManager.gm.SetCurrentItem(null);
            _input = _input.Trim('{', '}');
            string[] items = _input.Split(',');
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = $"{root.GetComponent<HierarchyName>().GetHierarchyName()}.{items[i]}";
                bool found = false;
                HierarchyName[] children = root.GetComponentsInChildren<HierarchyName>();
                foreach (HierarchyName child in children)
                {
                    Debug.Log(child.name);
                    if (child.GetHierarchyName() == items[i])
                    {
                        if (GameManager.gm.currentItems.Count == 0)
                            GameManager.gm.SetCurrentItem(child.gameObject);
                        else
                            GameManager.gm.UpdateCurrentItems(child.gameObject);
                        found = true;
                    }
                }
                if (!found)
                    AppendLogLine($"Error: \"{items[i]}\" is not a child of {root.name} or does not exist", "yellow");
            }
        }
        else if (GameManager.gm.allItems.Contains(_input))
            GameManager.gm.SetCurrentItem((GameObject)GameManager.gm.allItems[_input]);
        else
            AppendLogLine($"Error: \"{_input}\" does not exist", "yellow");

        isReady = true;
    }

    ///<summary>
    /// Look in all HierarchyNames for _input, Delete it with GameManager.DeleteItem().
    ///</summary>
    ///<param name="_input">HierarchyName of the object to delete</param>
    private IEnumerator DeleteItem(string _input)
    {
        if (_input == "selection")
        {
            List<string> itemsToDel = new List<string>();
            foreach (GameObject item in GameManager.gm.currentItems)
                itemsToDel.Add(item.GetComponent<HierarchyName>().fullname);
            foreach (string item in itemsToDel)
                GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[item]);
        }
        // Try to delete an Ogree object
        else if (GameManager.gm.allItems.Contains(_input))
            GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[_input]);
        // Try to delete a tenant
        else if (GameManager.gm.tenants.ContainsKey(_input))
            GameManager.gm.tenants.Remove(_input);
        else
            AppendLogLine($"Error: \"{_input}\" does not exist", "yellow");

        yield return new WaitForEndOfFrame();
        isReady = true;
    }

    #endregion

    #region LoadMethods

    ///<summary>
    /// Look at the first word of a "load" command and call the corresponding Load method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    ///<param name="_saveCmd">If "cmds", save it in GameManager ?</param>
    private void ParseLoad(string _input, bool _saveCmd)
    {
        string[] str = _input.Split(new char[] { ':' }, 2);
        if (str[0] == "cmds")
            LoadCmdsFile(str[1], _saveCmd);
        else if (str[0] == "template" || str[0] == "t")
            LoadTemplateFile(str[1]);
        else if (str[0] == "var")
            SaveVariable(str[1]);
        else
            AppendLogLine("Unknown command", "red");

        isReady = true;
    }

    ///<summary>
    /// Open given file and call RunCommandString() for each line in it.
    ///</summary>
    ///<param name="_input">Path of the file to load</param>
    ///<param name="_saveCmd">Save _input it in GameManager ?</param>
    private void LoadCmdsFile(string _input, bool _saveCmd)
    {
        string[] lines = new string[0];
        try
        {
            using (StreamReader sr = File.OpenText(_input))
                lines = Regex.Split(sr.ReadToEnd(), System.Environment.NewLine);
            if (_saveCmd)
                GameManager.gm.SetReloadBtn(_input);
        }
        catch (System.Exception e)
        {
            AppendLogLine(e.Message, "red");
            if (_saveCmd)
                GameManager.gm.SetReloadBtn(null);
        }
        foreach (string cmd in lines)
            RunCommandString(cmd, false);
    }

    ///<summary>
    /// Look at the first word, Open given file and call corresponding ReadFromJson.CreateTemplate method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    private void LoadTemplateFile(string _input)
    {
        string[] str = _input.Split(new char[] { '@' }, 2);
        if (str.Length == 2)
        {
            string json = "";
            try
            {
                using (StreamReader sr = File.OpenText(str[1]))
                    json = sr.ReadToEnd();
            }
            catch (System.Exception e)
            {
                AppendLogLine(e.Message, "red");
            }
            if (!string.IsNullOrEmpty(json))
            {
                if (str[0] == "rack")
                    rfJson.CreateRackTemplate(json);
                else if (str[0] == "device")
                    rfJson.CreateDeviceTemplate(json);
                else if (str[0] == "room")
                    rfJson.CreateRoomTemplate(json);
                else
                    AppendLogLine("Unknown template type", "red");
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Save a given variable in Dictionnary.
    ///</summary>
    ///<param name="_input">The variable to save in "[key]=[value]" format</param>
    private void SaveVariable(string _input)
    {
        string regex = "^[a-zA-Z0-9]+=.+$";
        if (Regex.IsMatch(_input, regex))
        {
            string[] data = _input.Split(new char[] { '=' }, 2);
            if (variables.ContainsKey(data[0]))
                AppendLogLine($"{data[0]} already exists", "yellow");
            else
                variables.Add(data[0], data[1]);
        }
        else
            AppendLogLine("Syntax Error on variable creation", "red");
    }

    #endregion

    #region CreateMethods

    ///<summary>
    /// Look at the first word of a "create" command and call the corresponding Create method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
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
        else if (str[0] == "zones" || str[0] == "zo")
            SetRoomZones(str[1]);
        else if (str[0] == "rack" || str[0] == "rk")
            CreateRack(str[1]);
        else if (str[0] == "device")
            CreateDevice(str[1]);
        else if (str[0] == "tenant" || str[0] == "tn")
            CreateTenant(str[1]);
        else
            AppendLogLine("Unknown command", "red");

        isReady = true;
    }

    ///<summary>
    /// Parse a "create customer" command and call CustomerGenerator.CreateCustomer().
    ///</summary>
    ///<param name="_input">Name of the customer</param>
    private void CreateCustomer(string _input)
    {
        string pattern = "^[^@\\s.]+$";
        if (Regex.IsMatch(_input, pattern))
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

    ///<summary>
    /// Parse a "create datacenter" command and call CustomerGenerator.CreateDatacenter().
    ///</summary>
    ///<param name="_input">String with datacenter data to parse</param>
    private void CreateDataCenter(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@(EN|NW|WS|SE)$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('@');

            SDataCenterInfos infos = new SDataCenterInfos();
            infos.orient = data[1];
            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                IsolateParent(data[0], out infos.parent, out infos.name);
                if (infos.parent)
                    CustomerGenerator.instance.CreateDatacenter(infos, false);
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItems[0].transform;
                CustomerGenerator.instance.CreateDatacenter(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create building" command and call BuildingGenerator.CreateBuilding().
    ///</summary>
    ///<param name="_input">String with building data to parse</param>
    private void CreateBuilding(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@\\[[0-9.-]+,[0-9.-]+,[0-9.-]+\\]@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]$";
        if (Regex.IsMatch(_input, patern))
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
                infos.parent = GameManager.gm.currentItems[0].transform;
                BuildingGenerator.instance.CreateBuilding(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create room" command and call BuildingGenerator.CreateRoom().
    ///</summary>
    ///<param name="_input">String with room data to parse</param>
    private void CreateRoom(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(EN|NW|WS|SE)|[^\\[][^@]+)$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('@');

            SRoomInfos infos = new SRoomInfos();
            infos.pos = ParseVector3(data[1]);
            if (data[2].StartsWith("["))
            {
                infos.size = ParseVector3(data[2]);
                infos.orient = data[3];
            }
            else
                infos.template = data[2];
            if (data[0].StartsWith("/"))
            {
                data[0] = data[0].Substring(1);
                IsolateParent(data[0], out infos.parent, out infos.name);
                if (infos.parent)
                    BuildingGenerator.instance.CreateRoom(infos, false);
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItems[0].transform;
                BuildingGenerator.instance.CreateRoom(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create rack" command and call ObjectGenerator.CreateRack().
    ///</summary>
    ///<param name="_input">String with rack data to parse</param>
    private void CreateRack(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@\\[[0-9.-]+(\\/[0-9.]+)*,[0-9.-]+(\\/[0-9.]+)*\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]|[^\\[][^@]+)@(front|rear|left|right)$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('@');

            SRackInfos infos = new SRackInfos();
            infos.pos = ParseVector2(data[1]);
            if (data[2].StartsWith("[")) // if vector to parse...
            {
                Vector3 tmp = ParseVector3(data[2], false);
                infos.size = new Vector2(tmp.x, tmp.y);
                infos.height = (int)tmp.z;
            }
            else // ...else: is template name
                infos.template = data[2];
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
                infos.parent = GameManager.gm.currentItems[0].transform;
                ObjectGenerator.instance.CreateRack(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create device" command and call ObjectGenerator.CreateDevice().
    ///</summary>
    ///<param name="_input">String with device data to parse</param>
    private void CreateDevice(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@[^@\\s]+@[^@\\s]+$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('@');
            SDeviceInfos infos = new SDeviceInfos();

            if (int.TryParse(data[1], out infos.posU) == false)
                infos.slot = data[1];
            if (float.TryParse(data[2], out infos.sizeU) == false)
                infos.template = data[2];
            if (data[0].StartsWith("/"))
            {
                IsolateParent(data[0].Substring(1), out infos.parent, out infos.name);
                if (infos.parent)
                    ObjectGenerator.instance.CreateDevice(infos, false);
            }
            else
            {
                infos.name = data[0];
                infos.parent = GameManager.gm.currentItems[0].transform;
                ObjectGenerator.instance.CreateDevice(infos, true);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create tenant" command and call CustomerGenerator.CreateTenant().
    ///</summary>
    ///<param name="String with tenant data to parse"></param>
    private void CreateTenant(string _input)
    {
        string patern = "^[^@\\s]+@[0-9a-fA-F]{6}$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('@');
            CustomerGenerator.instance.CreateTenant(data[0], data[1]);
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    #endregion

    #region SetMethods

    ///<summary>
    /// Parse a "set zone" command and call corresponding Room.SetZones().
    ///</summary>
    ///<param name="_input">String with zones data to parse</param>
    private void SetRoomZones(string _input)
    {
        string patern = "^(\\/[^@\\s]+@)*\\[([0-9.]+,){3}[0-9.]+\\]@\\[([0-9.]+,){3}[0-9.]+\\]$";
        if (Regex.IsMatch(_input, patern))
        {
            _input = _input.Replace("[", "");
            _input = _input.Replace("]", "");
            string[] data = _input.Split('@', ',');
            if (data.Length == 8) // No path -> On current object
            {
                Room currentRoom = GameManager.gm.currentItems[0].GetComponent<Room>();
                if (currentRoom)
                {
                    SMargin resDim = new SMargin(float.Parse(data[0]), float.Parse(data[1]),
                                                float.Parse(data[2]), float.Parse(data[3]));
                    SMargin techDim = new SMargin(float.Parse(data[4]), float.Parse(data[5]),
                                                float.Parse(data[6]), float.Parse(data[7]));
                    currentRoom.SetZones(resDim, techDim);
                }
                else
                    AppendLogLine("Selected object must be a room", "yellow");
            }
            else // There is an object path
            {
                GameObject room = GameManager.gm.FindByAbsPath(data[0].Substring(1));
                if (room)
                {
                    SMargin resDim = new SMargin(float.Parse(data[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture), float.Parse(data[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                                                float.Parse(data[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture), float.Parse(data[4], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));
                    SMargin techDim = new SMargin(float.Parse(data[5], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture), float.Parse(data[6], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                                                float.Parse(data[7], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture), float.Parse(data[8], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));
                    room.GetComponent<Room>().SetZones(resDim, techDim);
                }
                else
                    AppendLogLine("Error: path doesn't exist", "red");
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "set attribute" command and call corresponding SetAttribute() method according to target class
    ///</summary>
    ///<param name="input">String with attribute to modify data</param>
    private void SetAttribute(string _input)
    {
        string patern = "^[a-zA-Z0-9._]+\\.[a-zA-Z0-9.]+=.+$";
        if (Regex.IsMatch(_input, patern))
        {
            string[] data = _input.Split('=');

            // Can be a tenant or a Customer...
            if (data[0].Count(f => (f == '.')) == 1)
            {
                string[] attr = data[0].Split('.');
                if (attr[0] == "selection" || attr[0] == "_")
                {
                    SetMultiAttribute(attr[1], data[1]);
                    GameManager.gm.UpdateGuiInfos();
                    isReady = true;
                    return;
                }
                if (GameManager.gm.tenants.ContainsKey(attr[0])) // ...is a tenant
                {
                    GameManager.gm.tenants[attr[0]].SetAttribute(attr[1], data[1]);
                    isReady = true;
                    return;
                }
            }
            // ...is an OgreeObject
            Transform obj;
            string attrName;
            IsolateParent(data[0], out obj, out attrName);
            if (obj)
            {
                if (obj.GetComponent<IAttributeModif>() != null)
                {
                    obj.GetComponent<IAttributeModif>().SetAttribute(attrName, data[1]);
                    GameManager.gm.UpdateGuiInfos();
                }
                else
                    AppendLogLine($"Can't modify {obj.name} attributes.", "yellow");
            }
            else
                AppendLogLine($"Object doesn't exist.", "yellow");
        }
        else
            AppendLogLine("Syntax error", "red");

        isReady = true;
    }

    ///<summary>
    /// Go through GameManager.currentItems and try to SetAttribute each object.
    ///</summary>
    ///<param name="_attr">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    private void SetMultiAttribute(string _attr, string _value)
    {
        foreach (GameObject obj in GameManager.gm.currentItems)
        {
            if (obj.GetComponent<IAttributeModif>() != null)
                obj.GetComponent<IAttributeModif>().SetAttribute(_attr, _value);
            else
                AppendLogLine($"Can't modify {obj.name} attributes.", "yellow");
        }
    }

    #endregion

    #region Utils

    ///<summary>
    /// Parse a string with format "[x,y]" into a Vector2.
    ///</summary>
    ///<param name="_input">String with format "[x,y]"</param>
    private Vector2 ParseVector2(string _input)
    {
        Vector2 res = new Vector2();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = ParseDecFrac(parts[0]);
        res.y = ParseDecFrac(parts[1]);
        return res;
    }

    ///<summary>
    /// Parse a string with format "[x,y,z]" into a Vector3. The vector can be given in Y axis or Z axis up.
    ///</summary>
    ///<param name="_input">String with format "[x,y,z]"</param>
    ///<param name="_ZUp">Is the coordinates given are in Z axis up or Y axis up ? </param>
    private Vector3 ParseVector3(string _input, bool _ZUp = true)
    {
        Vector3 res = new Vector3();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = ParseDecFrac(parts[0]);
        if (_ZUp)
        {
            res.y = ParseDecFrac(parts[2]);
            res.z = ParseDecFrac(parts[1]);
        }
        else
        {
            res.y = ParseDecFrac(parts[1]);
            res.z = ParseDecFrac(parts[2]);
        }
        return res;
    }

    ///<summary>
    /// Parse a string into a float. Can be decimal, a fraction and/or negative.
    ///</summary>
    ///<param name="_input">The string which contains the float</param>
    private float ParseDecFrac(string _input)
    {
        if (_input.Contains("/"))
        {
            string[] div = _input.Split('/');
            float a = float.Parse(div[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            float b = float.Parse(div[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            return a / b;
        }
        else
            return float.Parse(_input, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
    }

    ///<summary>
    /// Take a hierarchy name and give the parent's transform and the child's name.
    ///</summary>
    ///<param name="_input">The hierarchy name to parse in format "aaa.bbb.ccc"</param>
    ///<param name="parent">The Transform to assign with the found parent's transform</param>
    ///<param name="name">The name to assign with the found child's name</param>
    private void IsolateParent(string _input, out Transform parent, out string name)
    {
        string[] path = _input.Split('.');
        string parentPath = "";
        for (int i = 0; i < path.Length - 1; i++)
            parentPath += $"{path[i]}.";
        parentPath = parentPath.Remove(parentPath.Length - 1);
        // Debug.Log(parentPath);
        GameObject tmp = GameManager.gm.FindByAbsPath(parentPath);
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

    ///<summary>
    /// Replace variables in a string by their corresponding value
    ///</summary>
    ///<param name="_input">The string with the variables to replace</param>
    private string ApplyVariables(string _input)
    {
        string patern = "\\$\\{[a-zA-Z0-9]+\\}";
        MatchCollection matches = Regex.Matches(_input, patern);
        foreach (Match match in matches)
        {
            string key = Regex.Replace(match.Value, "[\\$\\{\\}]", "");
            _input = _input.Replace(match.Value, variables[key]);
            // Debug.Log($"[{variables[key]}] {_input}");
        }
        return _input;
    }

    #endregion

}
