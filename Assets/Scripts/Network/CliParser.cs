using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public void DeserializeInput(string _input)
    {
        GameObject obj = null;
        Hashtable command = new Hashtable();
        try
        {
            command = JsonConvert.DeserializeObject<Hashtable>(_input);
        }
        catch (System.Exception)
        {
            GameManager.gm.AppendLogLine("Received data with unknow format.", "red");
        }
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
                    GameManager.gm.SetCurrentItem(obj);
                else
                    GameManager.gm.AppendLogLine("Error on select", "red");
                break;
            case "delete":
                obj = Utils.GetObjectById(command["data"].ToString());
                if (obj)
                    GameManager.gm.DeleteItem(obj, false); // deleteServer == true ??
                else
                    GameManager.gm.AppendLogLine("Error on delete", "red");
                break;
            case "focus":
                obj = Utils.GetObjectById(command["data"].ToString());
                if (obj)
                    GameManager.gm.FocusItem(obj);
                else
                    GameManager.gm.AppendLogLine("Error on focus", "red");
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
                GameManager.gm.AppendLogLine("Unknown type", "red");
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
        GameManager.gm.RegisterApi(logData.api_url, logData.api_token);
        await GameManager.gm.ConnectToApi();
    }

    ///<summary>
    /// Deserialize given SApiObject and call the good generator.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private void CreateObjectFromData(string _input)
    {
        SApiObject obj = JsonConvert.DeserializeObject<SApiObject>(_input);
        switch (obj.category)
        {
            case "tenant":
                CustomerGenerator.instance.CreateTenant(obj);
                break;
            case "site":
                CustomerGenerator.instance.CreateSite(obj);
                break;
            case "building":
                BuildingGenerator.instance.CreateBuilding(obj);
                break;
            case "room":
                BuildingGenerator.instance.CreateRoom(obj);
                break;
            case "rack":
                if (obj.attributes["template"] == "")
                    ObjectGenerator.instance.CreateRack(obj);
                else
                    ObjectGenerator.instance.CreateRack(obj, null, false);
                break;
            case "device":
                if (obj.attributes["template"] == "")
                    ObjectGenerator.instance.CreateDevice(obj);
                else
                    ObjectGenerator.instance.CreateDevice(obj, null, false);
                break;
            case "corridor":
                ObjectGenerator.instance.CreateCorridor(obj);
                break;
            case "group":
                ObjectGenerator.instance.CreateGroup(obj);
                break;
            default:
                GameManager.gm.AppendLogLine($"Unknown object type ({obj.category})", "yellow");
                break;
        }
    }

    ///
    private void ModifyObject(string _input)
    {
        SApiObject newData = JsonConvert.DeserializeObject<SApiObject>(_input);
        OgreeObject obj = Utils.GetObjectById(newData.id).GetComponent<OgreeObject>();

        // Case domain for all OgreeObjects
        bool tenantColorChanged = false;
        if (newData.category == "tenant" && obj.attributes["color"] != newData.attributes["color"])
            tenantColorChanged = true;

        // Case color/temperature for racks & devices


        // Case of a separator modification in a room
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

    ///
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
                    Debug.LogWarning("Incorrect room interaction");
                break;
            case "rack":
                Rack rack = (Rack)obj;
                usableParams = new List<string>() { "label", "labelFont", "alpha", "slots", "localCS", "U" };
                if (usableParams.Contains(command.param))
                    rack.SetAttribute(command.param, command.value);
                else
                    Debug.LogWarning("Incorrect rack interaction");
                break;
            case "device":
                OObject device = (OObject)obj;
                usableParams = new List<string>() { "label", "labelFont", "alpha", "slots", "localCS" };
                if (usableParams.Contains(command.param))
                    device.SetAttribute(command.param, command.value);
                else
                    Debug.LogWarning("Incorrect device interaction");
                break;
            default:
                usableParams = new List<string>() { "" };
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
            case "delay":
                // ?? Still needed ??
                break;
            case "infos":
                if (manip.data == "true")
                    GameManager.gm.MovePanel("infos", true);
                else
                    GameManager.gm.MovePanel("infos", false);
                break;
            case "debug":
                if (manip.data == "true")
                    GameManager.gm.MovePanel("debug", true);
                else
                    GameManager.gm.MovePanel("debug", false);
                break;
            case "highlight":
                GameObject obj = Utils.GetObjectById(manip.data);
                if (obj)
                    EventManager.Instance.Raise(new HighlightEvent { obj = obj });
                else
                    GameManager.gm.AppendLogLine("Error on highlight", "red");
                break;
            default:
                GameManager.gm.AppendLogLine("Unknown command", "red");
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
        CameraControl cc = GameObject.FindObjectOfType<CameraControl>();
        switch (manip.command)
        {
            case "move":
                cc.MoveCamera(manip.position, manip.rotation);
                break;
            case "translate":
                cc.TranslateCamera(manip.position, manip.rotation);
                break;
            case "wait":
                cc.WaitCamera(manip.rotation.y);
                break;
            default:
                GameManager.gm.AppendLogLine("Unknown command", "red");
                break;
        }

    }
}
