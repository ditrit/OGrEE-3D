using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    const int scrollbackSize = 500;
    Queue<string> scrollback = new Queue<string>(scrollbackSize);
    public string[] log { get; private set; } //Copy of scrollback as an array for easier use by ConsoleView

    public ReadFromJson rfJson = new ReadFromJson();
    public Dictionary<string, string> variables = new Dictionary<string, string>();

    private Dictionary<string, string> cmdsHistory = new Dictionary<string, string>();
    private string lastCmd = "";
    private int warningsCount = 0;
    private int errorsCount = 0;

    [SerializeField] private bool isReady = true;
    public float timerValue = 0f;


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

        if ((_color == "yellow" || _color == "red") && cmdsHistory.ContainsKey(lastCmd))
        {
            _line = $"<color={_color}>{cmdsHistory[lastCmd]}\n{_line}</color>";
            if (_color == "yellow")
                warningsCount++;
            else if (_color == "red")
                errorsCount++;
        }
        else
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
    /// Set counts variables to 0
    ///</summary>
    public void ResetCounts()
    {
        warningsCount = 0;
        errorsCount = 0;
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

        lastCmd = _input;
        // Debug.Log("=> " + lastCmd);

        _input = ApplyVariables(_input);
        AppendLogLine("$ " + _input);
        if (_input == "..")
            SelectParent();
        else if (_input[0] == '=')
            StartCoroutine(SelectItem(_input.Substring(1)));
        else if (_input[0] == '>')
            FocusItem(_input.Substring(1));
        else if (_input[0] == '.')
            StartCoroutine(ParseLoad(_input.Substring(1), _saveCmd));
        else if (_input[0] == '+')
            ParseCreate(_input.Substring(1));
        else if (_input[0] == '-')
            StartCoroutine(DeleteItem(_input.Substring(1)));
        else if (_input[0] == '~')
            MoveRack(_input.Substring(1));
        else if (_input.StartsWith("ui."))
            ParseUiCommand(_input.Substring(3));
        else if (_input.StartsWith("camera."))
            MoveCamera(_input.Substring(7));
        else if (_input.StartsWith("api."))
            CallApi(_input.Substring(4));
        else if (_input.StartsWith("zoom"))
            SetZoom(_input.Substring(4));
        else if (_input.Contains(".") && _input.Contains("="))
            SetAttribute(_input);
        else
        {
            AppendLogLine("Unknown command", "red");
            isReady = true;
        }
        if (timerValue > 0)
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
        else if (GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category == "tenant")
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
    private IEnumerator SelectItem(string _input)
    {
        if (string.IsNullOrEmpty(_input))
        {
            GameManager.gm.SetCurrentItem(null);
            isReady = true;
            yield break;
        }
        if (_input.StartsWith("{") && _input.EndsWith("}"))
        {
            if (GameManager.gm.currentItems.Count == 0)
            {
                isReady = true;
                yield break;
            }
            Transform root = GameManager.gm.currentItems[0].transform;
            GameManager.gm.SetCurrentItem(null);
            _input = _input.Trim('{', '}');
            string[] items = _input.Split(',');
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = $"{root.GetComponent<OgreeObject>().hierarchyName}.{items[i]}";
                bool found = false;
                OgreeObject[] children = root.GetComponentsInChildren<OgreeObject>();
                foreach (OgreeObject child in children)
                {
                    if (child.hierarchyName == items[i])
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

        yield return new WaitForEndOfFrame();
        isReady = true;
    }

    ///<summary>
    /// Look in all HierarchyNames for _input, Delete it with GameManager.DeleteItem().
    ///</summary>
    ///<param name="_input">HierarchyName of the object to delete</param>
    private IEnumerator DeleteItem(string _input)
    {
        string pattern = "^[^@\\s]+(@server){0,1}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            if (_input.StartsWith("selection"))
            {
                List<string> itemsToDel = new List<string>();
                foreach (GameObject item in GameManager.gm.currentItems)
                    itemsToDel.Add(item.GetComponent<OgreeObject>().hierarchyName);
                foreach (string item in itemsToDel)
                {
                    if (data.Length > 1)
                        GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[item], true);
                    else
                        GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[item], false);
                }
            }
            // Try to delete an Ogree object
            else if (GameManager.gm.allItems.Contains(data[0]))
            {
                if (data.Length > 1)
                    GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[data[0]], true);
                else
                    GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[data[0]], false);
            }
            // Try to delete a tenant
            // else if (GameManager.gm.tenants.ContainsKey(data[0]))
            //     GameManager.gm.tenants.Remove(data[0]);
            else
                AppendLogLine($"Error: \"{data[0]}\" does not exist", "yellow");
        }

        yield return new WaitForEndOfFrame();
        isReady = true;
    }

    ///<summary>
    /// Set focus to given object
    ///</summary>
    ///<param name="_input">The item to focus</param>
    private void FocusItem(string _input)
    {
        if (string.IsNullOrEmpty(_input))
        {
            // unfocus all items
            int count = GameManager.gm.focus.Count;
            for (int i = 0; i < count; i++)
                GameManager.gm.UnfocusItem();
        }
        else if (GameManager.gm.allItems.Contains(_input))
        {
            GameObject obj = (GameObject)GameManager.gm.allItems[_input];
            if (obj.GetComponent<OObject>())
                GameManager.gm.FocusItem(obj);
            else
                AppendLogLine($"Can't focus \"{_input}\"", "yellow");

        }
        else
            AppendLogLine($"Error: \"{_input}\" does not exist", "red");

        isReady = true;
    }

    #endregion

    #region LoadMethods

    ///<summary>
    /// Look at the first word of a "load" command and call the corresponding Load method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    ///<param name="_saveCmd">If "cmds", save it in GameManager ?</param>
    private IEnumerator ParseLoad(string _input, bool _saveCmd)
    {
        string[] str = _input.Split(new char[] { ':' }, 2);
        if (str[0] == "cmds")
            LoadCmdsFile(str[1], _saveCmd);
        else if (str[0] == "template" || str[0] == "t")
        {
            LoadTemplateFile(str[1]);
            yield return new WaitForSeconds(1);
        }
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
        for (int i = 0; i < lines.Length; i++)
        {
            if (!cmdsHistory.ContainsKey(lines[i].Trim()))
                cmdsHistory.Add(lines[i].Trim(), $"{_input}, l.{(i + 1).ToString()}");
            RunCommandString(lines[i], false);
        }
        StartCoroutine(DisplayLogCount(lines.Length));
    }

    ///<summary>
    /// Display read lines, warningCount and errorCount in CLI.
    ///</summary>
    ///<param name="_linesCount">The number of read lines</param>
    private IEnumerator DisplayLogCount(int _linesCount)
    {
        yield return new WaitUntil(() => isReady == true);
        isReady = false;

        string color;
        if (errorsCount > 0)
            color = "red";
        else if (warningsCount > 0)
            color = "yellow";
        else
            color = "green";

        lastCmd = "LogCount";
        AppendLogLine($"Read lines: {_linesCount}; Warnings: {warningsCount}; Errors:{errorsCount}", color);
        warningsCount = 0;
        errorsCount = 0;

        isReady = true;
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
        string pattern = "^[a-zA-Z0-9]+=.+$";
        if (Regex.IsMatch(_input, pattern))
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

    ///<summary>
    /// Call the right ApiManager.CreateXXXRequest() if syntax is good.
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private async void CallApi(string _input)
    {
        string pattern = "(get|post|put|delete)=+";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split(new char[] { '=' }, 2);
            if (data[0] == "get")
                await ApiManager.instance.GetObject(data[1]);
            else
            {
                OgreeObject obj = GameManager.gm.FindByAbsPath(data[1])?.GetComponent<OgreeObject>();
                if (obj)
                {
                    switch (data[0])
                    {
                        case "put":
                            ApiManager.instance.CreatePutRequest(obj);
                            break;
                        case "post":
                            await ApiManager.instance.PostObject(new SApiObject(obj));
                            break;
                        case "delete":
                            ApiManager.instance.CreateDeleteRequest(obj);
                            break;
                    }
                }
                else
                    GameManager.gm.AppendLogLine($"{data[1]} doesn't exist", "red");
            }
        }
        else
            AppendLogLine("Syntax Error on API call", "red");

        isReady = true;
    }

    #endregion

    #region CreateMethods

    ///<summary>
    /// Look at the first word of a "create" command and call the corresponding Create method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    private async void ParseCreate(string _input)
    {
        string[] str = _input.Split(new char[] { ':' }, 2);

        if (str[0] == "tenant" || str[0] == "tn")
            await CreateTenant(str[1]);
        else if (str[0] == "site" || str[0] == "si")
            await CreateSite(str[1]);
        else if (str[0] == "building" || str[0] == "bd")
            await CreateBuilding(str[1]);
        else if (str[0] == "room" || str[0] == "ro")
            await CreateRoom(str[1]);
        else if (str[0] == "separator" || str[0] == "sp")
            CreateSeparator(str[1]);
        else if (str[0] == "rack" || str[0] == "rk")
            await CreateRack(str[1]);
        else if (str[0] == "model" || str[0] == "mo")
            CreateModel(str[1]);
        else if (str[0] == "device" || str[0] == "dv")
            // StoreDevice($"+{_input}");
            await CreateDevice(str[1]);
        else if (str[0] == "group" || str[0] == "gr")
            CreateGroup(str[1]);
        else if (str[0] == "corridor" || str[0] == "co")
            CreateCorridor(str[1]);
        else
            AppendLogLine("Unknown command", "red");

        isReady = true;
    }

    ///<summary>
    /// Parse a "create tenant" command and call CustomerGenerator.CreateCustomer().
    ///</summary>
    ///<param name="_input">Name of the tenant</param>
    private async Task CreateTenant(string _input)
    {
        string pattern = "^[^@\\s.]+@[0-9a-fA-F]{6}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            SApiObject tn = new SApiObject();
            tn.description = new List<string>();
            tn.attributes = new Dictionary<string, string>();

            tn.name = data[0];
            tn.category = "tenant";
            tn.domain = data[0];
            tn.attributes["color"] = data[1];
            if (ApiManager.instance.isInit)
                await ApiManager.instance.PostObject(tn);
            else
                CustomerGenerator.instance.CreateTenant(tn);
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create site" command and call CustomerGenerator.CreateSite().
    ///</summary>
    ///<param name="_input">String with site data to parse</param>
    private async Task CreateSite(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@(EN|NW|WS|SE)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            Transform parent = null;
            SApiObject si = new SApiObject();
            si.description = new List<string>();
            si.attributes = new Dictionary<string, string>();

            si.category = "site";
            IsolateParent(data[0], out parent, out si.name);
            si.attributes["orientation"] = data[1];
            si.attributes["usableColor"] = "DBEDF2";
            si.attributes["reservedColor"] = "F2F2F2";
            si.attributes["technicalColor"] = "EBF2DE";
            if (parent)
            {
                si.parentId = parent.GetComponent<OgreeObject>().id;
                si.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(si);
                else
                    CustomerGenerator.instance.CreateSite(si, parent);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create building" command and call BuildingGenerator.CreateBuilding().
    ///</summary>
    ///<param name="_input">String with building data to parse</param>
    private async Task CreateBuilding(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.-]+,[0-9.-]+,[0-9.-]+\\]@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Vector3 pos = Utils.ParseVector3(data[1]);
            Vector3 size = Utils.ParseVector3(data[2]);

            Transform parent = null;
            SApiObject bd = new SApiObject();
            bd.description = new List<string>();
            bd.attributes = new Dictionary<string, string>();

            bd.category = "building";
            IsolateParent(data[0], out parent, out bd.name);
            bd.attributes["posXY"] = JsonUtility.ToJson(new Vector2(pos.x, pos.y));
            bd.attributes["posXYUnit"] = "m";
            bd.attributes["posZ"] = pos.z.ToString();
            bd.attributes["posZUnit"] = "m";
            bd.attributes["size"] = JsonUtility.ToJson(new Vector2(size.x, size.z));
            bd.attributes["sizeUnit"] = "m";
            bd.attributes["height"] = size.y.ToString();
            bd.attributes["heightUnit"] = "m";

            if (parent)
            {
                bd.parentId = parent.GetComponent<OgreeObject>().id;
                bd.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(bd);
                else
                    BuildingGenerator.instance.CreateBuilding(bd, parent);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create room" command and call BuildingGenerator.CreateRoom().
    ///</summary>
    ///<param name="_input">String with room data to parse</param>
    private async Task CreateRoom(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(\\+|\\-)[ENSW]{1}(\\+|\\-)[ENSW]{1}|[^\\[][^@]+)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent = null;
            SApiObject ro = new SApiObject();
            ro.description = new List<string>();
            ro.attributes = new Dictionary<string, string>();

            ro.category = "room";
            Vector3 pos = Utils.ParseVector3(data[1]);
            ro.attributes["posXY"] = JsonUtility.ToJson(new Vector2(pos.x, pos.y));
            ro.attributes["posXYUnit"] = "m";
            ro.attributes["posZ"] = pos.z.ToString();
            ro.attributes["posZUnit"] = "m";

            Vector3 size;
            if (data[2].StartsWith("["))
            {
                ro.attributes["template"] = "";
                ro.attributes["orientation"] = data[3];
                size = Utils.ParseVector3(data[2]);
            }
            else if (GameManager.gm.roomTemplates.ContainsKey(data[2]))
            {
                ro.attributes["template"] = data[2];
                ro.attributes["orientation"] = GameManager.gm.roomTemplates[data[2]].orientation;
                size = new Vector3(GameManager.gm.roomTemplates[data[2]].sizeWDHm[0],
                                GameManager.gm.roomTemplates[data[2]].sizeWDHm[2],
                                GameManager.gm.roomTemplates[data[2]].sizeWDHm[1]);
            }
            else
            {
                GameManager.gm.AppendLogLine($"Unknown template \"{data[2]}\"", "yellow");
                return;
            }
            ro.attributes["size"] = JsonUtility.ToJson(new Vector2(size.x, size.z));
            ro.attributes["sizeUnit"] = "m";
            ro.attributes["height"] = size.y.ToString();
            ro.attributes["heightUnit"] = "m";

            IsolateParent(data[0], out parent, out ro.name);
            if (parent)
            {
                ro.parentId = parent.GetComponent<OgreeObject>().id;
                ro.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(ro);
                else
                    BuildingGenerator.instance.CreateRoom(ro, parent);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create separator" command and call BuildingGenerator.CreateSeparator().
    ///</summary>
    ///<param name="_input">String with separator data to parse</param>
    private void CreateSeparator(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.]+,[0-9.]+\\]@\\[[0-9.]+,[0-9.]+\\]$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent;
            SApiObject sp = new SApiObject();
            sp.description = new List<string>();
            sp.attributes = new Dictionary<string, string>();

            sp.category = "separator";
            Vector2 pos1 = Utils.ParseVector2(data[1]);
            sp.attributes["startPos"] = JsonUtility.ToJson(pos1);
            Vector2 pos2 = Utils.ParseVector2(data[2]);
            sp.attributes["endPos"] = JsonUtility.ToJson(pos2);

            IsolateParent(data[0], out parent, out sp.name);
            if (parent)
                BuildingGenerator.instance.CreateSeparator(sp, parent);
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create rack" command and call ObjectGenerator.CreateRack().
    ///</summary>
    ///<param name="_input">String with rack data to parse</param>
    private async Task CreateRack(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.-]+(\\/[0-9.]+)*,[0-9.-]+(\\/[0-9.]+)*\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]|[^\\[][^@]+)@(front|rear|left|right)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent;
            SApiObject rk = new SApiObject();
            rk.description = new List<string>();
            rk.attributes = new Dictionary<string, string>();

            rk.category = "rack";
            Vector2 pos = Utils.ParseVector2(data[1]);
            rk.attributes["posXY"] = JsonUtility.ToJson(pos);
            rk.attributes["posXYUnit"] = "tile";

            if (data[2].StartsWith("[")) // if vector to parse...
            {
                Vector3 tmp = Utils.ParseVector3(data[2], false);
                rk.attributes["size"] = JsonUtility.ToJson(new Vector2(tmp.x, tmp.y));
                rk.attributes["sizeUnit"] = "cm";
                rk.attributes["height"] = ((int)tmp.z).ToString();
                rk.attributes["heightUnit"] = "U";
                rk.attributes["template"] = "";
            }
            else // ...else: is template name
                rk.attributes["template"] = data[2];
            rk.attributes["orientation"] = data[3];
            IsolateParent(data[0], out parent, out rk.name);
            if (parent)
            {
                rk.parentId = parent.GetComponent<OgreeObject>().id;
                rk.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(rk);
                else
                {
                    if (rk.attributes["template"] == "")
                        ObjectGenerator.instance.CreateRack(rk, parent);
                    else
                        ObjectGenerator.instance.CreateRack(rk, parent, false);
                }
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }
    ///<summary>
    /// Parse a "create rack" command and call ObjectGenerator.CreateRack().
    ///</summary>
    ///<param name="_input">String with rack data to parse</param>
    private void CreateModel(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.-]+(\\/[0-9.]+)*,[0-9.-]+(\\/[0-9.]+)*\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]|[^\\[][^@]+)@(front|rear|left|right)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent;
            SApiObject rk = new SApiObject();
            rk.description = new List<string>();
            rk.attributes = new Dictionary<string, string>();

            Vector2 pos = Utils.ParseVector2(data[1]);
            rk.attributes["posXY"] = JsonUtility.ToJson(pos);
            rk.attributes["posXYUnit"] = "Tile";
            rk.attributes["template"] = data[2];
            rk.attributes["orientation"] = data[3];
            IsolateParent(data[0], out parent, out rk.name);
            if (parent)
                ModelGenerator.instance.InstantiateModel(rk, parent);
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create device" command and call ObjectGenerator.CreateDevice().
    ///</summary>
    ///<param name="_input">String with device data to parse</param>
    public async Task CreateDevice(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@[^@\\s]+@[^@\\s]+(@(front|rear|frontflipped|rearflipped)){0,1}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent;
            SApiObject dv = new SApiObject();
            dv.description = new List<string>();
            dv.attributes = new Dictionary<string, string>();

            dv.category = "device";
            float posU;
            if (float.TryParse(data[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out posU))
            {
                dv.attributes["posU"] = posU.ToString();
                dv.attributes["slot"] = "";
            }
            else
                dv.attributes["slot"] = data[1];
            float sizeU;
            if (float.TryParse(data[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out sizeU))
            {
                dv.attributes["sizeU"] = sizeU.ToString();
                dv.attributes["template"] = "";
            }
            else
                dv.attributes["template"] = data[2];
            if (data.Length == 4)
                dv.attributes["orientation"] = data[3];
            else
                dv.attributes["orientation"] = "front";
            IsolateParent(data[0], out parent, out dv.name);
            if (parent)
            {
                dv.parentId = parent.GetComponent<OgreeObject>().id;
                dv.domain = parent.GetComponent<OgreeObject>().domain;
                if (dv.attributes["template"] == "")
                {
                    Vector3 scale = parent.GetChild(0).localScale * 1000;
                    dv.attributes["size"] = JsonUtility.ToJson(new Vector2(scale.x, scale.z));
                    dv.attributes["sizeUnit"] = "mm";
                    dv.attributes["height"] = scale.y.ToString();
                    dv.attributes["heightUnit"] = "mm";
                }

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(dv);
                else
                {
                    if (dv.attributes["template"] == "")
                        ObjectGenerator.instance.CreateDevice(dv, parent);
                    else
                        ObjectGenerator.instance.CreateDevice(dv, parent, false);

                }
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create group" command and call ObjectGenerator.CreateGroup().
    ///</summary>
    ///<param name="_input">String with rackgroup data to parse</param>
    private void CreateGroup(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\{[^@\\s\\},]+(,[^@\\s\\},]+)*\\}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent = null;
            SApiObject rg = new SApiObject();
            rg.description = new List<string>();
            rg.attributes = new Dictionary<string, string>();

            rg.category = "group";
            IsolateParent(data[0], out parent, out rg.name);
            rg.attributes["content"] = data[1].Trim('{', '}');
            if (parent)
            {
                rg.parentId = parent.GetComponent<OgreeObject>().id;
                rg.domain = parent.GetComponent<OgreeObject>().id;

                ObjectGenerator.instance.CreateGroup(rg, parent);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Parse a "create corridor" command and call ObjectGenerator.CreateCorridor().
    ///</summary>
    ///<param name="_input">String with corridor data to parse</param>
    private void CreateCorridor(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\{[^@\\s\\},]+,[^@\\s\\}]+\\}@(cold|warm)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Transform parent = null;
            SApiObject co = new SApiObject();
            co.description = new List<string>();
            co.attributes = new Dictionary<string, string>();

            co.category = "corridor";
            IsolateParent(data[0], out parent, out co.name);
            co.attributes["content"] = data[1].Trim('{', '}');
            co.attributes["temperature"] = data[2];
            if (parent)
            {
                co.parentId = parent.GetComponent<OgreeObject>().id;
                co.domain = parent.GetComponent<OgreeObject>().domain;

                ObjectGenerator.instance.CreateCorridor(co, parent);
            }
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    ///<summary>
    /// Store _input in a list in ZoomManager.
    ///</summary>
    ///<param name="_input">String with device data to parse</param>
    private void StoreDevice(string _input)
    {
        //+dv:/DEMO.ALPHA.B.R1.A99.PDU2@l17@ibm-smpdu@rearflipped
        _input = Regex.Replace(_input, " ", "");
        string patern = "^[^@\\s]+@[^@\\s]+@[^@\\s]+(@(front|rear|frontflipped|rearflipped)){0,1}$";
        // remove "+device:" or "+dv:"
        string cmd = _input.Substring(_input.IndexOf(':') + 1);
        if (Regex.IsMatch(cmd, patern))
        {
            string[] data = _input.Split(':', '@');
            string parentPath = IsolateParentPath(data[1]);
            ZoomManager.instance.devices.Add(new ZoomManager.SObjectCmd(data[1], parentPath, cmd));
        }
        else
            AppendLogLine("Syntax error", "red");
    }

    #endregion

    #region SetMethods

    ///<summary>
    /// Parse a "set attribute" command and call corresponding SetAttribute() method according to target class
    ///</summary>
    ///<param name="input">String with attribute to modify data</param>
    private void SetAttribute(string _input)
    {
        string pattern = "^[a-zA-Z0-9._]+\\.[a-zA-Z0-9.]+=.+$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('=');

            // Can be a selection...
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
            }
            // ...else is an OgreeObject
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
            // else if (ZoomManager.instance.IsListed(IsolateParentPath(data[0])))
            // {
            //     ZoomManager.SObjectCmd objCmd = new ZoomManager.SObjectCmd();
            //     objCmd.parentName = IsolateParentPath(data[0]);
            //     objCmd.command = _input;
            //     ZoomManager.instance.devicesAttributes.Add(objCmd);
            // }
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

    ///<summary>
    /// Move a rack to given coordinates.
    ///</summary>
    ///<param name="_input">The input to parse for a move command</param>
    private void MoveRack(string _input)
    {

        string pattern = "^[^@\\s]+@\\[[0-9.-]+,[0-9.-]+\\](@relative)*$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            if (GameManager.gm.allItems.Contains(data[0]))
            {
                GameObject obj = (GameObject)GameManager.gm.allItems[data[0]];
                Rack rk = obj.GetComponent<Rack>();
                if (rk)
                {
                    if (data.Length == 2)
                        rk.MoveRack(Utils.ParseVector2(data[1]), false);
                    else
                        rk.MoveRack(Utils.ParseVector2(data[1]), true);
                    GameManager.gm.UpdateGuiInfos();
                    GameManager.gm.AppendLogLine($"{data[0]} moved to {data[1]}", "green");
                }
                else
                    GameManager.gm.AppendLogLine($"{data[0]} is not a rack.", "yellow");
            }
            else
                GameManager.gm.AppendLogLine($"{data[0]} doesn't exist.", "yellow");
        }
        else
            GameManager.gm.AppendLogLine("Syntax error.", "red");

        isReady = true;
    }

    ///<summary>
    /// Parse a camera command and call the corresonding CameraControl method.
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private void MoveCamera(string _input)
    {
        string pattern = "^(move|translate|wait)=(\\[[0-9.-]+,[0-9.-]+,[0-9.-]+\\]@\\[[0-9.-]+,[0-9.-]+\\]|[0-9.]+)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('=', '@');
            CameraControl cc = GameObject.FindObjectOfType<CameraControl>();
            switch (data[0])
            {
                case "move":
                    cc.MoveCamera(Utils.ParseVector3(data[1]), Utils.ParseVector2(data[2]));
                    break;
                case "translate":
                    cc.TranslateCamera(Utils.ParseVector3(data[1]), Utils.ParseVector2(data[2]));
                    break;
                case "wait":
                    cc.WaitCamera(Utils.ParseDecFrac(data[1]));
                    break;
                default:
                    AppendLogLine("Unknown Camera control", "yellow");
                    break;
            }
        }
        else
            AppendLogLine("Syntax error", "red");

        isReady = true;
    }

    ///<summary>
    /// Parse an ui command and call the corresponding GameManager method
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private void ParseUiCommand(string _input)
    {
        string pattern = "^(wireframe|infos|debug)=(true|false)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('=');
            switch (data[0])
            {
                case "wireframe":
                    if (data[1] == "true")
                        GameManager.gm.ToggleRacksMaterials(true);
                    else
                        GameManager.gm.ToggleRacksMaterials(false);
                    break;
                case "infos":
                    if (data[1] == "true")
                        GameManager.gm.MovePanel("infos", true);
                    else
                        GameManager.gm.MovePanel("infos", false);
                    break;
                case "debug":
                    if (data[1] == "true")
                        GameManager.gm.MovePanel("debug", true);
                    else
                        GameManager.gm.MovePanel("debug", false);
                    break;
            }
        }
        else if (Regex.IsMatch(_input, "^delay=[0-9.]+$"))
            SetTimer(_input.Substring(_input.IndexOf('=') + 1));
        else
            AppendLogLine("Syntax error", "red");

        isReady = true;
    }

    ///<summary>
    /// Set timer to a value between 0 and 2s
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private void SetTimer(string _input)
    {
        string pattern = "^[0-9.]+$";
        if (Regex.IsMatch(_input, pattern))
        {
            float time = Utils.ParseDecFrac(_input);
            if (time < 0 || time > 2)
            {
                time = Mathf.Clamp(time, 0, 2);
                AppendLogLine("Delay is a value between 0 and 2s", "yellow");
            }
            GameObject.FindObjectOfType<TimerControl>().UpdateTimerValue(time);
        }
        else
            AppendLogLine("Syntax error", "red");

        isReady = true;
    }
    #endregion

    #region ZoomMethods

    ///<summary>
    /// Call ZoomManager.SetZoom regarding input.
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private void SetZoom(string _input)
    {
        string pattern = "^(\\+\\+|--|=[0-3])$";
        if (Regex.IsMatch(_input, pattern))
        {
            if (_input == "++")
                ZoomManager.instance.SetZoom(ZoomManager.instance.zoomLevel + 1);
            else if (_input == "--")
                ZoomManager.instance.SetZoom(ZoomManager.instance.zoomLevel - 1);
            else
                ZoomManager.instance.SetZoom(int.Parse(_input.Substring(1)));
            AppendLogLine($"Set zoom level to {ZoomManager.instance.zoomLevel}", "green");
        }
        else
            AppendLogLine("Syntax error", "red");
        isReady = true;
    }

    #endregion

    #region Utils

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
            AppendLogLine($"Error: path doesn't exist ({parentPath})", "red");
        }
    }

    ///<summary>
    /// Isolate parent path from hierarchyName
    ///</summary>
    ///<param name="_input">The hierarchyName to parse</param>
    ///<returns>The parent hierarchyName</returns>
    private string IsolateParentPath(string _input)
    {
        string[] path = _input.Split('.');
        string parentPath = "";
        for (int i = 0; i < path.Length - 1; i++)
            parentPath += $"{path[i]}.";
        parentPath = parentPath.Remove(parentPath.Length - 1);
        return parentPath;
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
