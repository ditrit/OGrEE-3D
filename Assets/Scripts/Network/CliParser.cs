using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CliParser
{
    #region Structures
    struct SLogin
    {
        public string api_url;
        public string api_token;
    }

    struct SInteract
    {
        public string id;
        public string param;
        public string value;
    }

    struct SUiManip
    {
        public string command;
        public string data;
    }

    struct SCameraManip
    {
        public string command;
        public Vector3 position;
        public Vector2 rotation;
    }

    #endregion

    readonly ReadFromJson rfJson = new ReadFromJson();

    ///<summary>
    /// Deserialize CLI input and parse it. 
    ///</summary>
    ///<param name="_input">The json to deserialize</param>
    public async Task DeserializeInput(string _input)
    {
        Hashtable command = new Hashtable();
        try
        {
            command = JsonConvert.DeserializeObject<Hashtable>(_input);
        }
        catch (System.Exception)
        {
            GameManager.instance.AppendLogLine("Received data with unknow format.", ELogTarget.both, ELogtype.errorCli);
        }
        switch (command["type"])
        {
            case "login":
                await Login(command["data"].ToString());
                break;
            case "load template":
                await rfJson.CreateObjTemplateJson(command["data"].ToString());
                break;
            case "select":
                List<GameObject> objsToSelect = Utils.GetObjectsById(command["data"].ToString());
                if (objsToSelect.Count == 0)
                    await GameManager.instance.SetCurrentItem(null);
                else if (objsToSelect.Count == 1)
                    await GameManager.instance.SetCurrentItem(objsToSelect[0]);
                else
                {
                    await GameManager.instance.SetCurrentItem(objsToSelect[0]);
                    for (int i = 1; i < objsToSelect.Count; i++)
                    {
                        GameObject obj = objsToSelect[i];
                        await GameManager.instance.UpdateCurrentItems(obj);
                    }
                }
                break;
            case "delete":
                if (string.IsNullOrEmpty(command["data"].ToString()))
                {
                    await GameManager.instance.DeleteItem(GameManager.instance.objectRoot, false);
                    await GameManager.instance.PurgeDomains();
                }
                else
                {
                    GameObject objToDel = Utils.GetObjectById(command["data"].ToString());
                    if (objToDel)
                        await GameManager.instance.DeleteItem(objToDel, false);
                    else
                        GameManager.instance.AppendLogLine("Error on delete", ELogTarget.both, ELogtype.errorCli);
                }
                break;
            case "focus":
                GameObject objToFocus = Utils.GetObjectById(command["data"].ToString());
                if (objToFocus)
                {
                    await GameManager.instance.SetCurrentItem(objToFocus);
                    await GameManager.instance.FocusItem(objToFocus);
                }
                else
                {
                    int count = GameManager.instance.GetFocused().Count;
                    for (int i = 0; i < count; i++)
                        await GameManager.instance.UnfocusItem();
                }
                break;
            case "create":
                await CreateObjectFromData(command["data"].ToString());
                break;
            case "modify":
                ModifyObject(command["data"].ToString());
                break;
            case "interact":
                InteractWithObject(command["data"].ToString());
                break;
            case "ui":
                await ManipulateUi(command["data"].ToString());
                break;
            case "camera":
                ManipulateCamera(command["data"].ToString());
                break;
            default:
                GameManager.instance.AppendLogLine("Command received with unknown type", ELogTarget.both, ELogtype.errorCli);
                break;
        }
    }

    ///<summary>
    /// Connect client to the API with.
    ///</summary>
    ///<param name="_input">Login credentials given by CLI</param>
    private async Task Login(string _input)
    {
        SLogin logData = JsonConvert.DeserializeObject<SLogin>(_input);
        ApiManager.instance.RegisterApi(logData.api_url, logData.api_token);
        await ApiManager.instance.Initialize();
    }

    ///<summary>
    /// Deserialize given SApiObject and call the good generator.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private async Task CreateObjectFromData(string _input)
    {
        SApiObject src = new SApiObject();
        try
        {
            src = JsonConvert.DeserializeObject<SApiObject>(_input);
        }
        catch (System.Exception e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.both, ELogtype.errorCli);
        }

        List<string> leafIds = new List<string>();
        if (!string.IsNullOrEmpty(src.category))
        {
            List<SApiObject> physicalObjects = new List<SApiObject>();
            List<SApiObject> logicalObjects = new List<SApiObject>();
            Utils.ParseNestedObjects(physicalObjects, logicalObjects, src, leafIds);

            foreach (SApiObject obj in physicalObjects)
                await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

            foreach (SApiObject obj in logicalObjects)
                await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

            foreach (string id in leafIds)
            {
                Transform leaf = Utils.GetObjectById(id)?.transform;
                if (leaf)
                    Utils.RebuildLods(leaf);
            }

            GameManager.instance.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", ELogTarget.both, ELogtype.infoCli);
        }
    }

    ///<summary>
    /// Deserialize given SApiObject and apply modification to corresponding object.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private async void ModifyObject(string _input)
    {
        SApiObject newData = JsonConvert.DeserializeObject<SApiObject>(_input);
        OgreeObject obj = Utils.GetObjectById(newData.id)?.GetComponent<OgreeObject>();
        if (!obj)
            return;

        // Get domain from API if new domain isn't loaded
        if (!string.IsNullOrEmpty(newData.domain) && !GameManager.instance.allItems.Contains(newData.domain))
            await ApiManager.instance.GetObject($"domains/{newData.domain}", ApiManager.instance.DrawObject);

        // Case domain for all OgreeObjects
        bool domainColorChanged = (newData.category == "domain" && obj.attributes["color"] != newData.attributes["color"]);

        // Case color for racks & devices
        if (obj is OObject item)
        {
            if (newData.category != "corridor")
            {
                if (newData.attributes.ContainsKey("color")
                    && (!item.attributes.ContainsKey("color")
                        || item.attributes.ContainsKey("color") && item.attributes["color"] != newData.attributes["color"]))
                    item.SetColor(newData.attributes["color"]);
            }
            // Case temperature for corridors
            else
            {
                if (newData.attributes.ContainsKey("temperature")
                    && (!item.attributes.ContainsKey("temperature")
                        || item.attributes.ContainsKey("temperature") && item.attributes["temperature"] != newData.attributes["temperature"]))
                {
                    if (newData.attributes["temperature"] == "cold")
                        item.SetColor("000099");
                    else
                        item.SetColor("990000");
                }
            }
        }

        // Case of a separators/pillars/areas modification in a room
        if (obj is Room room)
        {
            if (newData.attributes.ContainsKey("separators"))
            {
                if ((room.attributes.ContainsKey("separators") && room.attributes["separators"] != newData.attributes["separators"])
                    || !room.attributes.ContainsKey("separators"))
                {
                    foreach (Transform wall in room.walls)
                    {
                        if (wall.name.Contains("Separator"))
                            Object.Destroy(wall.gameObject);
                    }
                    List<SSeparator> separators = JsonConvert.DeserializeObject<List<SSeparator>>(newData.attributes["separators"]);
                    foreach (SSeparator sep in separators)
                        room.BuildSeparator(sep);
                }
            }
            if (newData.attributes.ContainsKey("pillars"))
            {
                if ((room.attributes.ContainsKey("pillars") && room.attributes["pillars"] != newData.attributes["pillars"])
                    || !room.attributes.ContainsKey("pillars"))
                {
                    foreach (Transform wall in room.walls)
                    {
                        if (wall.name.Contains("Pillar"))
                            Object.Destroy(wall.gameObject);
                    }
                    List<SPillar> pillars = JsonConvert.DeserializeObject<List<SPillar>>(newData.attributes["pillars"]);
                    foreach (SPillar pillar in pillars)
                        room.BuildPillar(pillar);
                }
            }
            if (newData.attributes.ContainsKey("reserved"))
            {
                if ((room.attributes.ContainsKey("reserved") && room.attributes["reserved"] != newData.attributes["reserved"])
                    || !room.attributes.ContainsKey("reserved"))
                {
                    SMargin reserved = JsonUtility.FromJson<SMargin>(newData.attributes["reserved"]);
                    SMargin technical = JsonUtility.FromJson<SMargin>(newData.attributes["technical"]);
                    room.SetAreas(reserved, technical);
                }
            }
        }
        obj.UpdateFromSApiObject(newData);

        if (domainColorChanged)
            EventManager.instance.Raise(new UpdateDomainEvent { name = newData.name });
    }

    ///<summary>
    /// Deserialize a SInteract command and run it.
    ///</summary>
    ///<param name="_data">The serialized command to execute</param>
    private void InteractWithObject(string _data)
    {
        SInteract command = JsonConvert.DeserializeObject<SInteract>(_data);
        OgreeObject obj = Utils.GetObjectById(command.id).GetComponent<OgreeObject>();
        switch (obj)
        {
            case Room room:
                switch (command.param)
                {
                    case "tilesName":
                        room.ToggleTilesName(command.value == "true");
                        break;
                    case "tilesColor":
                        room.ToggleTilesColor(command.value == "true");
                        break;
                    case "localCS":
                        room.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine("Incorrect room interaction", ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Building building:
                switch (command.param)
                {
                    case "localCS":
                        building.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine("Incorrect building interaction", ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Rack rack:
                switch (command.param)
                {
                    case "label":
                        rack.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case "labelFont":
                        rack.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case "labelBackground":
                        rack.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case "alpha":
                        rack.ToggleAlpha(command.value == "true");
                        break;
                    case "slots":
                        rack.ToggleSlots(command.value == "true");
                        break;
                    case "localCS":
                        rack.ToggleCS(command.value == "true");
                        break;
                    case "U":
                        UHelpersManager.instance.ToggleU(rack.gameObject, command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine("Incorrect rack interaction", ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case OObject device when device.category == "device":
                switch (command.param)
                {
                    case "label":
                        device.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case "labelFont":
                        device.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case "labelBackground":
                        device.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case "alpha":
                        device.ToggleAlpha(command.value == "true");
                        break;
                    case "slots":
                        device.ToggleSlots(command.value == "true");
                        break;
                    case "localCS":
                        device.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine("Incorrect device interaction", ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Group group:
                switch (command.param)
                {
                    case "label":
                        group.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case "labelFont":
                        group.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case "labelBackground":
                        group.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case "content":
                        group.ToggleContent(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine("Incorrect group interaction", ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            default:
                GameManager.instance.AppendLogLine("Unknown category to interact with", ELogTarget.both, ELogtype.warningCli);
                break;
        }
    }

    ///<summary>
    /// Parse a UI command and execute it.
    ///</summary>
    ///<param name="_input">The SUiManip to deserialize</param>
    private async Task ManipulateUi(string _input)
    {
        SUiManip manip = JsonConvert.DeserializeObject<SUiManip>(_input);
        switch (manip.command)
        {
            case "delay":
                float time = Utils.ParseDecFrac(manip.data);
                UiManager.instance.UpdateTimerValue(time);
                break;
            case "infos":
                UiManager.instance.MovePanel("infos", manip.data == "true");
                break;
            case "debug":
                UiManager.instance.MovePanel("debug", manip.data == "true");
                break;
            case "highlight":
                GameObject obj = Utils.GetObjectById(manip.data);
                if (obj)
                    EventManager.instance.Raise(new HighlightEvent { obj = obj });
                else
                    GameManager.instance.AppendLogLine("Error on highlight", ELogTarget.both, ELogtype.errorCli);
                break;
            case "clearcache":
                if (GameManager.instance.objectRoot)
                {
                    Prompt prompt = UiManager.instance.GeneratePrompt("Clearing cache will erase current scene", "Continue", "Cancel");
                    while (prompt.state == EPromptStatus.wait)
                        await Task.Delay(10);
                    if (prompt.state == EPromptStatus.accept)
                    {
                        await GameManager.instance.DeleteItem(GameManager.instance.objectRoot, false);
                        await GameManager.instance.PurgeDomains();
                        UiManager.instance.ClearCache();
                        UiManager.instance.DeletePrompt(prompt);
                    }
                    else
                        UiManager.instance.DeletePrompt(prompt);
                }
                else
                    UiManager.instance.ClearCache();
                break;
            default:
                GameManager.instance.AppendLogLine("Unknown ui command", ELogTarget.both, ELogtype.errorCli);
                break;
        }
    }

    ///<summary>
    /// Parse a camera command and execute it.
    ///</summary>
    ///<param name="_input">The SCameraManip to deserialize</param>
    private void ManipulateCamera(string _input)
    {
        SCameraManip manip = JsonConvert.DeserializeObject<SCameraManip>(_input);
        Vector3 refinedPos = new Vector3(manip.position.x, manip.position.z, manip.position.y);
        CameraControl cc = GameObject.FindObjectOfType<CameraControl>();
        switch (manip.command)
        {
            case "move":
                cc.MoveCamera(refinedPos, manip.rotation);
                break;
            case "translate":
                cc.TranslateCamera(refinedPos, manip.rotation);
                break;
            case "wait":
                cc.WaitCamera(manip.rotation.y);
                break;
            default:
                GameManager.instance.AppendLogLine("Unknown camera command", ELogTarget.both, ELogtype.errorCli);
                break;
        }

    }
}
