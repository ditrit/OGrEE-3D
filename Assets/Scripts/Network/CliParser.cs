using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CliParser// : MonoBehaviour
{
    #region Structures
    ///<summary>
    /// Standard structure, helps to know which kind of data in received.
    ///</summary>
    // struct SData
    // {
    //     public string type;
    //     public string data;
    // }

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

    ReadFromJson rfJson = new ReadFromJson();

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
            GameManager.gm.AppendLogLine("Received data with unknow format.", true, eLogtype.errorCli);
        }
        GameObject obj;
        switch (command["type"])
        {
            case "login":
                Login(command["data"].ToString());
                break;
            case "load template":
                rfJson.CreateObjTemplateJson(command["data"].ToString());
                break;
            case "select":
                obj = Utils.GetObjectById(command["data"].ToString());
                if (obj)
                    await GameManager.gm.SetCurrentItem(obj);
                else
                    GameManager.gm.AppendLogLine("Error on select", true, eLogtype.errorCli);
                break;
            case "delete":
                obj = Utils.GetObjectById(command["data"].ToString());
                if (obj)
                    await GameManager.gm.DeleteItem(obj, false); // deleteServer == true ??
                else
                    GameManager.gm.AppendLogLine("Error on delete", true, eLogtype.errorCli);
                break;
            case "focus":
                obj = Utils.GetObjectById(command["data"].ToString());
                if (obj)
                {
                    await GameManager.gm.SetCurrentItem(obj);
                    await GameManager.gm.FocusItem(obj);
                }
                else
                    GameManager.gm.AppendLogLine("Error on focus", true, eLogtype.errorCli);
                break;
            case "create":
                CreateObjectFromData(command["data"].ToString());
                break;
            case "modify":
                ModifyObject(command["data"].ToString());
                break;
            case "interact":
                InteractWithObject(command["data"].ToString());
                break;
            case "ui":
                ManipulateUi(command["data"].ToString());
                break;
            case "camera":
                ManipulateCamera(command["data"].ToString());
                break;
            default:
                GameManager.gm.AppendLogLine("Unknown type", true, eLogtype.errorCli);
                break;
        }
    }

    ///<summary>
    /// Connect client to the API with.
    ///</summary>
    ///<param name="_input">Login credentials given by CLI</param>
    private async void Login(string _input)
    {
        SLogin logData = JsonConvert.DeserializeObject<SLogin>(_input);
        GameManager.gm.configLoader.RegisterApi(logData.api_url, logData.api_token);
        await GameManager.gm.configLoader.ConnectToApi();
    }

    ///<summary>
    /// Deserialize given SApiObject and call the good generator.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private async void CreateObjectFromData(string _input)
    {
        SApiObject src = JsonConvert.DeserializeObject<SApiObject>(_input);
        List<SApiObject> physicalObjects = new List<SApiObject>();
        List<SApiObject> logicalObjects = new List<SApiObject>();
        Utils.ParseNestedObjects(physicalObjects, logicalObjects, src);

        foreach (SApiObject obj in physicalObjects)
            await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        foreach (SApiObject obj in logicalObjects)
            await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

        GameManager.gm.AppendLogLine($"{physicalObjects.Count + logicalObjects.Count} object(s) created", true, eLogtype.infoCli);
    }

    ///<summary>
    /// Deserialize given SApiObject and apply modification to corresponding object.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private void ModifyObject(string _input)
    {
        SApiObject newData = JsonConvert.DeserializeObject<SApiObject>(_input);
        OgreeObject obj = Utils.GetObjectById(newData.id).GetComponent<OgreeObject>();

        // Case domain for all OgreeObjects
        bool tenantColorChanged = false;
        if (newData.category == "tenant" && obj.attributes["color"] != newData.attributes["color"])
            tenantColorChanged = true;

        // Case color/temperature for racks & devices
        if (newData.category == "rack" || newData.category == "device")
        {
            OObject item = (OObject)obj;
            if (newData.attributes.ContainsKey("color"))
            {
                if ((obj.attributes.ContainsKey("color") && obj.attributes["color"] != newData.attributes["color"])
                    || !item.attributes.ContainsKey("color"))
                {
                    item.SetColor(newData.attributes["color"]);
                }
            }
            if (newData.attributes.ContainsKey("temperature"))
            {
                if ((obj.attributes.ContainsKey("temperature") && obj.attributes["temperature"] != newData.attributes["temperature"])
                    || !item.attributes.ContainsKey("temperature"))
                {
                    item.SetTemperature(newData.attributes["temperature"]);
                }
            }
        }

        // Case of a separator/areas modification in a room
        if (newData.category == "room")
        {
            Room room = (Room)obj;
            if (newData.attributes.ContainsKey("separators"))
            {
                if ((room.attributes.ContainsKey("separators") && room.attributes["separators"] != newData.attributes["separators"])
                    || !room.attributes.ContainsKey("separators"))
                {
                    foreach (Transform wall in room.walls)
                    {
                        if (wall.name.Contains("separator"))
                            Object.Destroy(wall.gameObject);
                    }
                    List<ReadFromJson.SSeparator> separators = JsonConvert.DeserializeObject<List<ReadFromJson.SSeparator>>(newData.attributes["separators"]);
                    foreach (ReadFromJson.SSeparator sep in separators)
                        room.AddSeparator(sep);
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
        if (tenantColorChanged)
            EventManager.Instance.Raise(new UpdateTenantEvent { name = newData.name });
    }

    ///<summary>
    /// Deserialize a SInteract command and run it.
    ///</summary>
    ///<param name="_data">The serialized command to execute</param>
    private void InteractWithObject(string _data)
    {
        List<string> usableParams;
        SInteract command = JsonConvert.DeserializeObject<SInteract>(_data);
        OgreeObject obj = Utils.GetObjectById(command.id).GetComponent<OgreeObject>();
        switch (obj.category)
        {
            case "room":
                Room room = (Room)obj;
                usableParams = new List<string>() { "tilesName", "tilesColor" };
                if (usableParams.Contains(command.param))
                    room.SetAttribute(command.param, command.value);
                else
                    GameManager.gm.AppendLogLine("Incorrect room interaction", true, eLogtype.warningCli);
                break;
            case "rack":
                Rack rack = (Rack)obj;
                usableParams = new List<string>() { "label", "labelFont", "alpha", "slots", "localCS", "U" };
                if (usableParams.Contains(command.param))
                    rack.SetAttribute(command.param, command.value);
                else
                    GameManager.gm.AppendLogLine("Incorrect rack interaction", true, eLogtype.warningCli);
                break;
            case "device":
                OObject device = (OObject)obj;
                usableParams = new List<string>() { "label", "labelFont", "alpha", "slots", "localCS" };
                if (usableParams.Contains(command.param))
                    device.SetAttribute(command.param, command.value);
                else
                    GameManager.gm.AppendLogLine("Incorrect device interaction", true, eLogtype.warningCli);
                break;
            case "group":
                Group group = (Group)obj;
                usableParams = new List<string>() { "label", "labelFont", "content" };
                if (usableParams.Contains(command.param))
                    group.SetAttribute(command.param, command.value);
                else
                    GameManager.gm.AppendLogLine("Incorrect group interaction", true, eLogtype.warningCli);
                break;
            default:
                GameManager.gm.AppendLogLine("Unknown category to interact with", true, eLogtype.warningCli);
                break;
        }
    }

    ///<summary>
    /// Parse a UI command and execute it.
    ///</summary>
    ///<param name="_input">The SUiManip to deserialize</param>
    private void ManipulateUi(string _input)
    {
        SUiManip manip = JsonConvert.DeserializeObject<SUiManip>(_input);
        switch (manip.command)
        {
            case "delay": // ?? Still needed ??
                float time = Utils.ParseDecFrac(manip.data);
                GameObject.FindObjectOfType<TimerControl>().UpdateTimerValue(time);
                break;
            case "infos":
                if (manip.data == "true")
                    UiManager.instance.MovePanel("infos", true);
                else
                    UiManager.instance.MovePanel("infos", false);
                break;
            case "debug":
                if (manip.data == "true")
                    UiManager.instance.MovePanel("debug", true);
                else
                    UiManager.instance.MovePanel("debug", false);
                break;
            case "highlight":
                GameObject obj = Utils.GetObjectById(manip.data);
                if (obj)
                    EventManager.Instance.Raise(new HighlightEvent { obj = obj });
                else
                    GameManager.gm.AppendLogLine("Error on highlight", true, eLogtype.errorCli);
                break;
            default:
                GameManager.gm.AppendLogLine("Unknown command", true, eLogtype.errorCli);
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
                GameManager.gm.AppendLogLine("Unknown command", true, eLogtype.errorCli);
                break;
        }

    }
}
