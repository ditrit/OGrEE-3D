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
    ///<summary>Value used for delaying cmds execution</summary>
    public float timerValue = 0f;


    ///<summary>
    /// Collecting log output by Eliot Lash.
    ///</summary>
    ///<param name="_line">The line to display</param>
    ///<param name="_color">The color of the line, white by default</param>
    public void AppendLogLine(string _line, string _color = "white")
    {
        if (!GameManager.gm.writeLogs)
            return;

        // Troncate too long strings
        int limit = 103;
        if (_line.Length > limit)
            _line = _line.Substring(0, limit) + "[...]";

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
    /// Turn isReady to false and disable reload button.
    ///</summary>
    private void LockController()
    {
        isReady = false;
        GameManager.gm.SetReloadBtn(false);

        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Loading });
    }

    ///<summary>
    /// Turn isReady to true and enable reload button.
    ///</summary>
    private void UnlockController()
    {
        isReady = true;
        StartCoroutine(WaitAndEnableBtn());

        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }

    ///<summary>
    /// Wait 1 second to check if the reload button can be enabled.
    ///</summary>
    private IEnumerator WaitAndEnableBtn()
    {
        yield return new WaitForSeconds(1);
        if (isReady)
            GameManager.gm.SetReloadBtn(true);
    }

    ///<summary>
    /// Execute a command line. Look for the first char to call the corresponding method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    ///<param name="_saveCmd">If ".cmds", save it in GameManager ? true by default</param>
    public void RunCommandString(string _input, bool _saveCmd = true)
    {
        _input = RemoveComment(_input);
        if (string.IsNullOrEmpty(_input.Trim()))
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
        Task task;
        yield return new WaitUntil(() => isReady == true);
        LockController();

        lastCmd = _input;

        _input = ApplyVariables(_input);
        GameManager.gm.AppendLogLine("$ " + _input, false);
        if (_input == "..")
        {
            task = SelectParent();
            yield return new WaitUntil(() => task.IsCompleted);
        }
        else if (_input[0] == '=')
            StartCoroutine(SelectItem(_input.Substring(1)));
        else if (_input[0] == '>')
        {
            task = FocusItem(_input.Substring(1));
            yield return new WaitUntil(() => task.IsCompleted);
        }
        else if (_input[0] == '.')
            StartCoroutine(ParseLoad(_input.Substring(1), _saveCmd));
        else if (_input[0] == '+')
            ParseCreate(_input.Substring(1));
        else if (_input[0] == '-')
            StartCoroutine(DeleteItem(_input.Substring(1)));
        else if (_input.StartsWith("ui."))
            ParseUiCommand(_input.Substring(3));
        else if (_input.StartsWith("camera."))
            MoveCamera(_input.Substring(7));
        else if (_input.StartsWith("api."))
            CallApi(_input.Substring(4));
        else if (_input.Contains(":") && _input.Contains("="))
        {
            task = SetAttribute(_input);
            yield return new WaitUntil(() => task.IsCompleted);
        }
        else
        {
            GameManager.gm.AppendLogLine("Unknown command", false, eLogtype.error);
            UnlockController();
        }
        if (timerValue > 0)
        {
            LockController();
            yield return new WaitForSeconds(timerValue);
            UnlockController();
        }
    }

    #region HierarchyMethods()

    ///<summary>
    /// Set GameManager.currentItem as the parent of it in Ogree objects hierarchy.
    ///</summary>
    private async Task SelectParent()
    {
        if (!GameManager.gm.currentItems[0])
        {
            UnlockController();
            return;
        }
        else if (GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category == "tenant")
            await GameManager.gm.SetCurrentItem(null);
        else
        {
            GameObject parent = GameManager.gm.currentItems[0].transform.parent.gameObject;
            if (parent)
                await GameManager.gm.SetCurrentItem(parent);
        }

        UnlockController();
    }

    ///<summary>
    /// Look in all HierarchyNames for _input, set it as GameManager.currentItem.
    ///</summary>
    ///<param name="_input">HierarchyName of the object to select</param>
    private IEnumerator SelectItem(string _input)
    {
        Task task;
        if (string.IsNullOrEmpty(_input))
        {
            task = GameManager.gm.SetCurrentItem(null);
            yield return new WaitUntil(() => task.IsCompleted);
            UnlockController();
            yield break;
        }
        if (_input.StartsWith("{") && _input.EndsWith("}"))
        {
            if (GameManager.gm.currentItems.Count == 0)
            {
                UnlockController();
                yield break;
            }
            Transform root = GameManager.gm.currentItems[0].transform;
            task = GameManager.gm.SetCurrentItem(null);
            yield return new WaitUntil(() => task.IsCompleted);
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
                        {
                            task = GameManager.gm.SetCurrentItem(child.gameObject);
                            yield return new WaitUntil(() => task.IsCompleted);
                        }
                        else
                        {
                            task = GameManager.gm.UpdateCurrentItems(child.gameObject);
                            yield return new WaitUntil(() => task.IsCompleted);
                        }
                        found = true;
                    }
                }
                if (!found)
                    GameManager.gm.AppendLogLine($"\"{items[i]}\" is not a child of {root.name} or does not exist", false, eLogtype.warning);
            }
        }
        else if (GameManager.gm.allItems.Contains(_input))
        {
            task = GameManager.gm.SetCurrentItem((GameObject)GameManager.gm.allItems[_input]);
            yield return new WaitUntil(() => task.IsCompleted);
        }
        else
            GameManager.gm.AppendLogLine($"\"{_input}\" does not exist", false, eLogtype.warning);

        yield return new WaitForEndOfFrame();
        UnlockController();
    }

    ///<summary>
    /// Look in all HierarchyNames for _input, Delete it with GameManager.DeleteItem().
    ///</summary>
    ///<param name="_input">HierarchyName of the object to delete</param>
    private IEnumerator DeleteItem(string _input)
    {
        Task task;
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
                    {
                        task = GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[item], true);
                        yield return new WaitUntil(() => task.IsCompleted);
                    }
                    else
                    {
                        task = GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[item], false);
                        yield return new WaitUntil(() => task.IsCompleted);
                    }
                }
            }
            // Try to delete an Ogree object
            else if (GameManager.gm.allItems.Contains(data[0]))
            {
                if (data.Length > 1)
                {
                    task = GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[data[0]], true);
                    yield return new WaitUntil(() => task.IsCompleted);
                }
                else
                {
                    task = GameManager.gm.DeleteItem((GameObject)GameManager.gm.allItems[data[0]], false);
                    yield return new WaitUntil(() => task.IsCompleted);
                }
            }
            // Try to delete a tenant
            // else if (GameManager.gm.tenants.ContainsKey(data[0]))
            //     GameManager.gm.tenants.Remove(data[0]);
            else
                GameManager.gm.AppendLogLine($"\"{data[0]}\" does not exist", false, eLogtype.warning);
        }

        yield return new WaitForEndOfFrame();
        UnlockController();
    }

    ///<summary>
    /// Set focus to given object
    ///</summary>
    ///<param name="_input">The item to focus</param>
    private async Task FocusItem(string _input)
    {
        if (string.IsNullOrEmpty(_input))
        {
            // unfocus all items
            int count = GameManager.gm.focus.Count;
            for (int i = 0; i < count; i++)
                await GameManager.gm.UnfocusItem();
        }
        else if (GameManager.gm.allItems.Contains(_input))
        {
            GameObject obj = (GameObject)GameManager.gm.allItems[_input];
            if (obj.GetComponent<OObject>())
            {
                await GameManager.gm.SetCurrentItem(obj);
                await GameManager.gm.FocusItem(obj);
            }
            else
                GameManager.gm.AppendLogLine($"Can't focus \"{_input}\"", false, eLogtype.warning);

        }
        else
            GameManager.gm.AppendLogLine($"\"{_input}\" does not exist", false, eLogtype.error);

        UnlockController();
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
            GameManager.gm.AppendLogLine("Unknown command", false, eLogtype.error);

        UnlockController();
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
                GameManager.gm.SetReloadBtn(false, _input);
        }
        catch (System.Exception e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.error);
            if (_saveCmd)
                GameManager.gm.SetReloadBtn(false, "");
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
        LockController();

        eLogtype color;
        if (errorsCount > 0)
            color = eLogtype.error;
        else if (warningsCount > 0)
            color = eLogtype.warning;
        else
            color = eLogtype.success;

        lastCmd = "LogCount";
        GameManager.gm.AppendLogLine($"Read lines: {_linesCount}; Warnings: {warningsCount}; Errors:{errorsCount}", false, color);
        warningsCount = 0;
        errorsCount = 0;

        UnlockController();
    }

    ///<summary>
    /// Look at the first word, Open given file and call corresponding ReadFromJson.CreateTemplate method.
    ///</summary>
    ///<param name="_input">Command line to parse</param>
    private async void LoadTemplateFile(string _input)
    {
        string json = "";
        try
        {
            using (StreamReader sr = File.OpenText(_input))
                json = sr.ReadToEnd();
        }
        catch (System.Exception e)
        {
            GameManager.gm.AppendLogLine(e.Message, false, eLogtype.error);
        }
        if (!string.IsNullOrEmpty(json))
        {
            if (Regex.IsMatch(json, "\"category\"[ ]*:[ ]*\"room\""))
            {
                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostTemplateObject(json, "room");
                else
                    rfJson.CreateRoomTemplateJson(json);
            }
            else // rack or device
            {
                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostTemplateObject(json, "obj");
                else
                    rfJson.CreateObjTemplateJson(json);
            }
        }
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
                GameManager.gm.AppendLogLine($"{data[0]} already exists", false, eLogtype.warning);
            else
                variables.Add(data[0], data[1]);
        }
        else
            GameManager.gm.AppendLogLine("Syntax Error on variable creation", false, eLogtype.error);
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
            {
                // bool isObjArray = Regex.IsMatch(data[1], "(?:^[a-z]+$)|(?:[a-z]+\\?[a-z0-9]+=.+$)");
                await ApiManager.instance.GetObject(data[1], ApiManager.instance.DrawObject);
            }
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
                    GameManager.gm.AppendLogLine($"{data[1]} doesn't exist", false, eLogtype.error);
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax Error on API call", false, eLogtype.error);

        UnlockController();
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
        else if (str[0] == "rack" || str[0] == "rk")
            await CreateRack(str[1]);
        else if (str[0] == "device" || str[0] == "dv")
            await CreateDevice(str[1]);
        else if (str[0] == "group" || str[0] == "gr")
            await CreateGroup(str[1]);
        else if (str[0] == "corridor" || str[0] == "co")
            await CreateCorridor(str[1]);
        else if (str[0] == "sensor" || str[0] == "se")
            CreateSensor(str[1]);
        else
            GameManager.gm.AppendLogLine("Unknown command", false, eLogtype.error);

        UnlockController();
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
            SApiObject tn = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                name = data[0],
                category = "tenant",
                domain = data[0]
            };
            tn.attributes["color"] = data[1];
            if (ApiManager.instance.isInit)
                await ApiManager.instance.PostObject(tn);
            else
            {
                tn.id = data[0];
                await OgreeGenerator.instance.CreateItemFromSApiObject(tn);
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
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
            SApiObject si = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "site"
            };
            IsolateParent(data[0], out Transform parent, out si.name);
            si.attributes["orientation"] = data[1];
            if (parent)
            {
                si.parentId = parent.GetComponent<OgreeObject>().id;
                si.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(si);
                else
                {
                    si.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(si, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    ///<summary>
    /// Parse a "create building" command and call BuildingGenerator.CreateBuilding().
    ///</summary>
    ///<param name="_input">String with building data to parse</param>
    private async Task CreateBuilding(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.-]+,[0-9.-]+\\]@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            Vector3 pos = Utils.ParseVector2(data[1]);
            Vector3 size = Utils.ParseVector3(data[2]);
            SApiObject bd = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "building"
            };

            IsolateParent(data[0], out Transform parent, out bd.name);
            bd.attributes["posXY"] = JsonUtility.ToJson(new Vector2(pos.x, pos.y));
            bd.attributes["posXYUnit"] = "m";
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
                {
                    bd.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(bd, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    ///<summary>
    /// Parse a "create room" command and call BuildingGenerator.CreateRoom().
    ///</summary>
    ///<param name="_input">String with room data to parse</param>
    private async Task CreateRoom(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\[[0-9.]+,[0-9.]+\\]@(\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@(\\+|\\-)[ENSW]{1}(\\+|\\-)[ENSW]{1}|[^\\[][^@]+)(@(t|m|f)){0,1}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            SApiObject ro = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "room"
            };
            Vector3 pos = Utils.ParseVector2(data[1]);
            ro.attributes["posXY"] = JsonUtility.ToJson(new Vector2(pos.x, pos.y));
            ro.attributes["posXYUnit"] = "m";

            Vector3 size;
            if (data[2].StartsWith("["))
            {
                ro.attributes["template"] = "";
                ro.attributes["orientation"] = data[3];
                ro.attributes["floorUnit"] = "t";
                size = Utils.ParseVector3(data[2]);
            }
            else
            {
                ro.attributes["template"] = data[2];
                ReadFromJson.SRoomFromJson template = new ReadFromJson.SRoomFromJson();
                if (GameManager.gm.roomTemplates.ContainsKey(ro.attributes["template"]))
                    template = GameManager.gm.roomTemplates[ro.attributes["template"]];
                else if (ApiManager.instance.isInit)
                {
                    await ApiManager.instance.GetObject($"room-templates/{ro.attributes["template"]}", ApiManager.instance.DrawObject);
                    template = GameManager.gm.roomTemplates[ro.attributes["template"]];
                }

                if (!string.IsNullOrEmpty(template.slug))
                {
                    ro.attributes["orientation"] = template.orientation;
                    ro.attributes["floorUnit"] = template.floorUnit;
                    size = new Vector3(template.sizeWDHm[0], template.sizeWDHm[2], template.sizeWDHm[1]);
                }
                else
                {
                    GameManager.gm.AppendLogLine($"Unknown template \"{data[2]}\"", false, eLogtype.warning);
                    return;
                }
            }
            ro.attributes["size"] = JsonUtility.ToJson(new Vector2(size.x, size.z));
            ro.attributes["sizeUnit"] = "m";
            ro.attributes["height"] = size.y.ToString();
            ro.attributes["heightUnit"] = "m";
            if (data.Length == 5)
                ro.attributes["floorUnit"] = data[4];

            IsolateParent(data[0], out Transform parent, out ro.name);
            if (parent)
            {
                ro.parentId = parent.GetComponent<OgreeObject>().id;
                ro.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(ro);
                else
                {
                    ro.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(ro, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
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

            SApiObject rk = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "rack"
            };
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
            {
                rk.attributes["template"] = data[2];
                OgreeObject template = null;
                if (GameManager.gm.objectTemplates.ContainsKey(rk.attributes["template"]))
                    template = GameManager.gm.objectTemplates[rk.attributes["template"]].GetComponent<OgreeObject>();
                else if (ApiManager.instance.isInit)
                {
                    await ApiManager.instance.GetObject($"obj-templates/{rk.attributes["template"]}", ApiManager.instance.DrawObject);
                    template = GameManager.gm.objectTemplates[rk.attributes["template"]].GetComponent<OgreeObject>();
                }

                if (template)
                {
                    rk.description = template.description;
                    foreach (KeyValuePair<string, string> kvp in template.attributes)
                    {
                        if (kvp.Key != "template")
                            rk.attributes[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    GameManager.gm.AppendLogLine($"Unknown template \"{rk.attributes["template"]}\"", false, eLogtype.warning);
                    return;
                }
            }
            Vector2 pos = Utils.ParseVector2(data[1]);
            rk.attributes["posXY"] = JsonUtility.ToJson(pos);
            rk.attributes["posXYUnit"] = "tile";
            rk.attributes["orientation"] = data[3];
            IsolateParent(data[0], out Transform parent, out rk.name);
            if (parent)
            {
                rk.parentId = parent.GetComponent<OgreeObject>().id;
                rk.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(rk);
                else
                {
                    rk.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(rk, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
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

            SApiObject dv = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "device"
            };
            if (float.TryParse(data[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float sizeU))
            {
                dv.attributes["sizeU"] = sizeU.ToString();
                dv.attributes["template"] = "";
            }
            else
                dv.attributes["template"] = data[2];

            IsolateParent(data[0], out Transform parent, out dv.name);
            if (parent)
            {
                if (dv.attributes["template"] == "")
                {
                    Vector3 scale = parent.GetChild(0).localScale * 1000;
                    dv.attributes["size"] = JsonUtility.ToJson(new Vector2(scale.x, scale.z));
                    dv.attributes["sizeUnit"] = "mm";
                    dv.attributes["height"] = (sizeU * GameManager.gm.uSize * 1000).ToString();
                    dv.attributes["heightUnit"] = "mm";
                }
                else
                {
                    OgreeObject template = null;

                    if (GameManager.gm.objectTemplates.ContainsKey(dv.attributes["template"]))
                        template = GameManager.gm.objectTemplates[dv.attributes["template"]].GetComponent<OgreeObject>();
                    else if (ApiManager.instance.isInit)
                    {
                        await ApiManager.instance.GetObject($"obj-templates/{dv.attributes["template"]}", ApiManager.instance.DrawObject);
                        template = GameManager.gm.objectTemplates[dv.attributes["template"]]?.GetComponent<OgreeObject>();
                    }

                    if (template)
                    {
                        dv.description = template.description;
                        foreach (KeyValuePair<string, string> kvp in template.attributes)
                        {
                            if (kvp.Key != "template")
                                dv.attributes[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        GameManager.gm.AppendLogLine($"Unknown template \"{dv.attributes["template"]}\"", false, eLogtype.warning);
                        return;
                    }
                }
                if (float.TryParse(data[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float posU))
                {
                    dv.attributes["posU"] = posU.ToString();
                    dv.attributes["slot"] = "";
                }
                else
                    dv.attributes["slot"] = data[1];
                if (data.Length == 4)
                    dv.attributes["orientation"] = data[3];
                else
                    dv.attributes["orientation"] = "front";

                dv.parentId = parent.GetComponent<OgreeObject>().id;
                dv.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(dv);
                else
                {
                    dv.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(dv, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    ///<summary>
    /// Parse a "create group" command and call ObjectGenerator.CreateGroup().
    ///</summary>
    ///<param name="_input">String with rackgroup data to parse</param>
    private async Task CreateGroup(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\{[^@\\s\\},]+(,[^@\\s\\},]+)*\\}$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            SApiObject gr = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "group"
            };
            IsolateParent(data[0], out Transform parent, out gr.name);
            gr.attributes["content"] = data[1].Trim('{', '}');
            if (parent)
            {
                gr.parentId = parent.GetComponent<OgreeObject>().id;
                gr.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(gr);
                else
                {
                    gr.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(gr, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    ///<summary>
    /// Parse a "create corridor" command and call ObjectGenerator.CreateCorridor().
    ///</summary>
    ///<param name="_input">String with corridor data to parse</param>
    private async Task CreateCorridor(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@\\{[^@\\s\\},]+,[^@\\s\\}]+\\}@(cold|warm)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');

            SApiObject co = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "corridor"
            };
            IsolateParent(data[0], out Transform parent, out co.name);
            co.attributes["content"] = data[1].Trim('{', '}');
            co.attributes["temperature"] = data[2];
            if (parent)
            {
                co.parentId = parent.GetComponent<OgreeObject>().id;
                co.domain = parent.GetComponent<OgreeObject>().domain;

                if (ApiManager.instance.isInit)
                    await ApiManager.instance.PostObject(co);
                else
                {
                    co.id = data[0];
                    await OgreeGenerator.instance.CreateItemFromSApiObject(co, parent);
                }
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    ///<summary>
    /// Parse a "create sensor" command and call ObjectGenerator.CreateSensor().
    ///</summary>
    ///<param name="_input">String with sensor data to parse</param>
    private async void CreateSensor(string _input)
    {
        _input = Regex.Replace(_input, " ", "");
        string pattern = "^[^@\\s]+@(ext@[0-9.]+|int@\\[[0-9.]+,[0-9.]+,[0-9.]+\\]@[0-9.]+)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('@');
            SApiObject se = new SApiObject
            {
                description = new List<string>(),
                attributes = new Dictionary<string, string>(),

                category = "sensor"
            };
            se.attributes["formFactor"] = data[1];
            Transform parent;
            if (data[1] == "ext")
            {
                se.name = "sensor"; // ?
                parent = GameManager.gm.FindByAbsPath(data[0])?.transform;
                se.attributes["linkedObject"] = data[0];
                se.attributes["temperature"] = data[2];
            }
            else
            {
                IsolateParent(data[0], out parent, out se.name);
                se.attributes["temperature"] = data[3];
                Vector3 tmp = Utils.ParseVector3(data[2], false);
                se.attributes["posXY"] = JsonUtility.ToJson(new Vector2(tmp.x, tmp.y));
                se.attributes["posU"] = tmp.z.ToString();
            }
            if (parent)
            {
                se.parentId = parent.GetComponent<OgreeObject>().id;
                se.domain = parent.GetComponent<OgreeObject>().domain;

                se.id = data[0];
                await OgreeGenerator.instance.CreateItemFromSApiObject(se, parent);
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);
    }

    #endregion

    #region SetMethods

    ///<summary>
    /// Parse a "set attribute" command and call corresponding SetAttribute() method according to target class
    ///</summary>
    ///<param name="input">String with attribute to modify data</param>
    private async Task SetAttribute(string _input)
    {
        string pattern = "^[a-zA-Z0-9._]+\\:[a-zA-Z0-9.]+=.+$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split(new char[] { ':', '=' });

            // Can be a selection...
            if (data[0] == "selection" || data[0] == "_")
            {
                await SetMultiAttribute(data[1], data[2]);
                UiManager.instance.UpdateGuiInfos();
                UnlockController();
                return;

            }
            // ...else is an OgreeObject
            GameObject obj = GameManager.gm.FindByAbsPath(data[0]);
            if (obj)
            {
                if (obj.GetComponent<OgreeObject>() != null)
                {
                    if (data[1] == "details")
                        await obj.GetComponent<OgreeObject>().LoadChildren(data[2]);
                    else
                    {
                        obj.GetComponent<OgreeObject>().SetAttribute(data[1], data[2]);
                        if (obj.GetComponent<OgreeObject>().category == "tenant" && data[1] == "color")
                            EventManager.Instance.Raise(new UpdateTenantEvent { name = obj.name });
                    }
                    UiManager.instance.UpdateGuiInfos();
                }
                else
                    GameManager.gm.AppendLogLine($"Can't modify {obj.name} attributes.", false, eLogtype.warning);
            }
            else
                GameManager.gm.AppendLogLine($"Object doesn't exist.", false, eLogtype.warning);
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);

        UnlockController();
    }

    ///<summary>
    /// Go through GameManager.currentItems and try to SetAttribute each object.
    ///</summary>
    ///<param name="_attr">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    private async Task SetMultiAttribute(string _attr, string _value)
    {
        foreach (GameObject obj in GameManager.gm.currentItems)
        {
            if (obj.GetComponent<OgreeObject>() != null)
            {
                if (_attr == "details")
                    await obj.GetComponent<OgreeObject>().LoadChildren(_value);
                else
                    obj.GetComponent<OgreeObject>().SetAttribute(_attr, _value);
            }
            else
                GameManager.gm.AppendLogLine($"Can't modify {obj.name} attributes.", false, eLogtype.warning);
        }
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
                    GameManager.gm.AppendLogLine("Unknown Camera control", false, eLogtype.warning);
                    break;
            }
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);

        UnlockController();
    }

    ///<summary>
    /// Parse an ui command and call the corresponding method.
    ///</summary>
    ///<param name="_input">The input to parse</param>
    private void ParseUiCommand(string _input)
    {
        string pattern = "^(infos|debug)=(true|false)$";
        if (Regex.IsMatch(_input, pattern))
        {
            string[] data = _input.Split('=');
            switch (data[0])
            {
                case "infos":
                    if (data[1] == "true")
                        UiManager.instance.MovePanel("infos", true);
                    else
                        UiManager.instance.MovePanel("infos", false);
                    break;
                case "debug":
                    if (data[1] == "true")
                        UiManager.instance.MovePanel("debug", true);
                    else
                        UiManager.instance.MovePanel("debug", false);
                    break;
            }
        }
        else if (Regex.IsMatch(_input, "^delay=[0-9.]+$"))
        {
            SetTimer(_input.Substring(_input.IndexOf('=') + 1));
        }
        else if (Regex.IsMatch(_input, "^(highlight|hl)=[^@\\s]+$"))
        {
            string[] data = _input.Split('=');
            StartCoroutine(HighlightItem(data[1]));
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);

        UnlockController();
    }

    ///<summary>
    /// Raise a new HighlightEvent with given gameObject.
    ///</summary>
    ///<param name="_name">The hierarchyName of the object to send</param>
    private IEnumerator HighlightItem(string _name)
    {
        yield return new WaitForEndOfFrame();
        if (GameManager.gm.allItems.Contains(_name))
        {
            GameObject obj = (GameObject)GameManager.gm.allItems[_name];
            EventManager.Instance.Raise(new HighlightEvent { obj = obj });
        }
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
                GameManager.gm.AppendLogLine("Delay is a value between 0 and 2s", false, eLogtype.warning);
            }
            GameObject.FindObjectOfType<TimerControl>().UpdateTimerValue(time);
            GameObject.FindObjectOfType<Server>().timer = (int)(time * 1000);
        }
        else
            GameManager.gm.AppendLogLine("Syntax error", false, eLogtype.error);

        UnlockController();
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
            GameManager.gm.AppendLogLine($"Error: path doesn't exist ({parentPath})", false, eLogtype.error);
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

    ///<summary>
    /// Removes every characters after a "//" indicator.
    ///</summary>
    ///<param name="_input">The string to refine</param>
    ///<returns>The given string without comment</returns>
    private string RemoveComment(string _input)
    {
        string[] data = _input.Split(new string[] { "//" }, StringSplitOptions.None);
        return data[0];
    }

    #endregion

}
