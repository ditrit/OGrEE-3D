using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public class CommandParser
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

    readonly ReadFromJson rfJson = new();
    private bool canDraw = true;

    public CommandParser()
    {
        EventManager.instance.CancelGenerate.Add(OnCancelGenenerate);
    }

    ~CommandParser()
    {
        EventManager.instance.CancelGenerate.Remove(OnCancelGenenerate);
    }

    ///<summary>
    /// When called, set canDraw to false
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnCancelGenenerate(CancelGenerateEvent _e)
    {
        canDraw = false;
    }

    ///<summary>
    /// Deserialize input and parse it. 
    ///</summary>
    ///<param name="_input">The json to deserialize</param>
    public async Task DeserializeInput(string _input)
    {
        Hashtable command = new();
        try
        {
            command = JsonConvert.DeserializeObject<Hashtable>(_input);
        }
        catch (System.Exception)
        {
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Received data with unknow format"), ELogTarget.both, ELogtype.errorCli);
        }
        switch (command["type"])
        {
            case CommandType.Login:
                await Login(command["data"].ToString());
                break;
            case CommandType.Logout:
                GameManager.instance.server.ResetConnection();
                ApiManager.instance.ResetApi();
                break;
            case CommandType.LoadTemplate:
                STemplate data = JsonConvert.DeserializeObject<STemplate>(command["data"].ToString());
                await rfJson.CreateObjectTemplate(data);
                break;
            case CommandType.Select:
                //Disable coord mode
                if (GameManager.instance.getCoordsMode)
                    UiManager.instance.ToggleGetCoordsMode();
                //Disable position mode
                if (GameManager.instance.positionMode)
                    await Positionner.instance.TogglePositionMode();
                //Disable edit mode
                if (GameManager.instance.editMode)
                    UiManager.instance.EditFocused();
                //Disable focus mode
                if (GameManager.instance.focusMode)
                    await GameManager.instance.UnfocusAll();

                List<string> ids = JsonConvert.DeserializeObject<List<string>>(command["data"].ToString());
                List<GameObject> objsToSelect = Utils.GetObjectsById(ids);
                if (objsToSelect.Count == 0)
                    await GameManager.instance.SetCurrentItem(null);
                else
                {
                    //Disable scatter plot
                    if (objsToSelect[0].TryGetComponent(out ObjectDisplayController odc) && odc.scatterPlotOfOneParent)
                    {
                        Transform parent = objsToSelect[0].transform.parent;
                        while (!parent.GetComponent<OgreeObject>().scatterPlot)
                            parent = parent.parent;
                        TempDiagram.instance.HandleScatterPlot(parent.GetComponent<OgreeObject>());
                    }
                    //Disable bar chart
                    if (objsToSelect[0].TryGetComponent(out Item item) && item.referent.transform.parent?.GetComponent<Room>() is Room room && room.barChart)
                        TempDiagram.instance.HandleTempBarChart(room);
                    //Select the item(s)
                    await GameManager.instance.SetCurrentItem(objsToSelect[0]);
                    for (int i = 1; i < objsToSelect.Count; i++)
                        await GameManager.instance.UpdateCurrentItems(objsToSelect[i]);
                }
                break;
            case CommandType.Delete:
                if (string.IsNullOrEmpty(command["data"].ToString()))
                {
                    if (GameManager.instance.editMode)
                        UiManager.instance.EditFocused();
                    //Disable position mode
                    if (GameManager.instance.positionMode)
                        await Positionner.instance.TogglePositionMode();
                    await GameManager.instance.UnfocusAll();
                    await GameManager.instance.DeleteItem(GameManager.instance.objectRoot, false);
                    await GameManager.instance.PurgeDomains();
                }
                else
                {
                    if (Utils.GetObjectById(command["data"].ToString()) is GameObject objToDel)
                        await GameManager.instance.DeleteItem(objToDel, false);
                    else
                        GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Error on delete"), ELogTarget.both, ELogtype.errorCli);
                }
                break;
            case CommandType.Focus:
                //Disable coord mode
                if (GameManager.instance.getCoordsMode)
                    UiManager.instance.ToggleGetCoordsMode();
                if (GameManager.instance.positionMode)
                    await Positionner.instance.TogglePositionMode();
                if (GameManager.instance.editMode)
                    UiManager.instance.EditFocused();
                if (Utils.GetObjectById(command["data"].ToString()) is GameObject objToFocus)
                {
                    await GameManager.instance.SetCurrentItem(objToFocus);
                    await GameManager.instance.FocusItem(objToFocus);
                }
                else
                    await GameManager.instance.UnfocusAll();
                break;
            case CommandType.Create:
                await CreateObjectFromData(command["data"].ToString());
                break;
            case CommandType.Modify:
                ModifyObject(command["data"].ToString());
                break;
            case CommandType.Interact:
                InteractWithObject(command["data"].ToString());
                break;
            case CommandType.UI:
                await ManipulateUi(command["data"].ToString());
                break;
            case CommandType.Camera:
                ManipulateCamera(command["data"].ToString());
                break;
            case CommandType.ModifyTag:
                ModifyTag(command["data"].ToString());
                break;
            case CommandType.DeleteTag:
                DeleteTag(command["data"].ToString());
                break;
            case CommandType.CreateLayer:
                CreateLayer(command["data"].ToString());
                break;
            case CommandType.ModifyLayer:
                ModifyLayer(command["data"].ToString());
                break;
            case CommandType.DeleteLayer:
                DeleteLayer(command["data"].ToString());
                break;
            default:
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Command received with unknown type"), ELogTarget.both, ELogtype.errorCli);
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

    /// <summary>
    /// Remove all objets from given tag. The tag will delete himself when "empty".
    /// </summary>
    /// <param name="_input">The tag slug to delete</param>
    private void DeleteTag(string _input)
    {
        Tag tagToDel = GameManager.instance.GetTag(_input);
        if (tagToDel == null)
            return;
        foreach (string id in tagToDel.linkedObjects)
            GameManager.instance.RemoveFromTag(tagToDel.slug, id);
    }

    ///<summary>
    /// Deserialize given SApiObject and call the good generator.
    ///</summary>
    ///<param name="_input">The SApiObject to deserialize</param>
    private async Task CreateObjectFromData(string _input)
    {
        SApiObject src = new();
        try
        {
            src = JsonConvert.DeserializeObject<SApiObject>(_input);
        }
        catch (System.Exception e)
        {
            GameManager.instance.AppendLogLine(e.Message, ELogTarget.both, ELogtype.errorCli);
        }

        List<string> leafIds = new();
        if (!string.IsNullOrEmpty(src.category))
        {
            List<SApiObject> physicalObjects = new();
            List<SApiObject> logicalObjects = new();
            Utils.ParseNestedObjects(physicalObjects, logicalObjects, src, leafIds);

            foreach (SApiObject obj in physicalObjects)
                if (canDraw)
                    await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

            foreach (SApiObject obj in logicalObjects)
                if (canDraw)
                    await OgreeGenerator.instance.CreateItemFromSApiObject(obj);

            foreach (string id in leafIds)
                if (Utils.GetObjectById(id) is GameObject leaf)
                    Utils.RebuildLods(leaf.transform);

            if (canDraw)
                GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "X objects created", physicalObjects.Count + logicalObjects.Count), ELogTarget.logger, ELogtype.successApi);
        }
        canDraw = true;
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
        bool domainColorChanged = newData.category == Category.Domain && obj.attributes["color"] != newData.attributes["color"];

        obj.UpdateFromSApiObject(newData);

        if (domainColorChanged)
            EventManager.instance.Raise(new UpdateDomainEvent(newData.name));
    }

    /// <summary>
    /// Deserialize given old-slug / SApiTag pair and apply modification to corresponding tag.
    /// </summary>
    /// <param name="_input">The old-slug / SApiTag to deserialize</param>
    private void ModifyTag(string _input)
    {
        Hashtable data = JsonConvert.DeserializeObject<Hashtable>(_input);
        Tag tagToUpdate = GameManager.instance.GetTag(data["old-slug"].ToString());
        if (tagToUpdate == null)
            return;
        SApiTag newData = JsonConvert.DeserializeObject<SApiTag>(data["tag"].ToString());
        tagToUpdate.UpdateFromSApiTag(newData);
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
            case Building building and not Room:
                switch (command.param)
                {
                    case CommandParameter.LocalCS:
                        building.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Incorrect object interaction", "building"), ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Room room:
                switch (command.param)
                {
                    case CommandParameter.TilesName:
                        room.ToggleTilesName(command.value == "true");
                        break;
                    case CommandParameter.TilesColor:
                        room.ToggleTilesColor(command.value == "true");
                        break;
                    case CommandParameter.LocalCS:
                        room.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Incorrect object interaction", "room"), ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Rack rack:
                switch (command.param)
                {
                    case CommandParameter.Label:
                        rack.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case CommandParameter.LabelFont:
                        rack.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case CommandParameter.LabelBackground:
                        rack.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case CommandParameter.Alpha:
                        rack.GetComponent<ObjectDisplayController>().ToggleAlpha(command.value == "true");
                        break;
                    case CommandParameter.Slots:
                        rack.ToggleSlots(command.value == "true");
                        break;
                    case CommandParameter.LocalCS:
                        rack.ToggleCS(command.value == "true");
                        break;
                    case CommandParameter.U:
                        UHelpersManager.instance.ToggleU(rack.gameObject, command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Incorrect object interaction", "rack"), ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Device device:
                switch (command.param)
                {
                    case CommandParameter.Label:
                        device.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case CommandParameter.LabelFont:
                        device.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case CommandParameter.LabelBackground:
                        device.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case CommandParameter.Alpha:
                        device.GetComponent<ObjectDisplayController>().ToggleAlpha(command.value == "true");
                        break;
                    case CommandParameter.Slots:
                        device.ToggleSlots(command.value == "true");
                        break;
                    case CommandParameter.LocalCS:
                        device.ToggleCS(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Incorrect object interaction", "device"), ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            case Group group:
                switch (command.param)
                {
                    case CommandParameter.Label:
                        group.GetComponent<DisplayObjectData>().SetLabel(command.value);
                        break;
                    case CommandParameter.LabelFont:
                        group.GetComponent<DisplayObjectData>().SetLabelFont(command.value);
                        break;
                    case CommandParameter.LabelBackground:
                        group.GetComponent<DisplayObjectData>().SetLabelBackgroundColor(command.value);
                        break;
                    case CommandParameter.Content:
                        group.ToggleContent(command.value == "true");
                        break;
                    default:
                        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Incorrect object interaction", "group"), ELogTarget.both, ELogtype.warningCli);
                        break;
                }
                break;
            default:
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Unknown category to interact with"), ELogTarget.both, ELogtype.warningCli);
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
            case Command.Delay:
                float time = Utils.ParseDecFrac(manip.data);
                UiManager.instance.UpdateTimerValue(time);
                break;
            case Command.Infos:
            case Command.Debug:
                UiManager.instance.MovePanel(manip.command, manip.data == "true");
                break;
            case Command.Highlight:
                GameObject obj = Utils.GetObjectById(manip.data);
                if (obj)
                    EventManager.instance.Raise(new HighlightEvent(obj));
                else
                    GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Error on highlight"), ELogTarget.both, ELogtype.errorCli);
                break;
            case Command.ClearCache:
                if (GameManager.instance.objectRoot)
                {
                    Prompt prompt = UiManager.instance.GeneratePrompt("Clear cache warning", "Continue", "Cancel");
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
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Unknown ui command"), ELogTarget.both, ELogtype.errorCli);
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
        Vector3 refinedPos = new(manip.position.x, manip.position.z, manip.position.y);
        switch (manip.command)
        {
            case Command.Move:
                GameManager.instance.cameraControl.MoveCamera(refinedPos, manip.rotation);
                break;
            case Command.Translate:
                GameManager.instance.cameraControl.TranslateCamera(refinedPos, manip.rotation);
                break;
            case Command.Wait:
                GameManager.instance.cameraControl.WaitCamera(manip.rotation.y);
                break;
            default:
                GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Unknown camera command"), ELogTarget.both, ELogtype.errorCli);
                break;
        }
    }

    /// <summary>
    /// Deserialize given <see cref="SApiLayer"/> and create a Layer from it
    /// </summary>
    /// <param name="_input">The SApiLayer tp deserialize</param>
    private void CreateLayer(string _input)
    {
        SApiLayer apiLayer = JsonConvert.DeserializeObject<SApiLayer>(_input);
        LayerManager.instance.CreateLayerFromSApiLayer(apiLayer);
        UiManager.instance.layersList.RebuildMenu(UiManager.instance.BuildLayersList);
    }

    /// <summary>
    /// Deserialize given old-slug / SApiLayer pair and apply modification to corresponding layer.
    /// </summary>
    /// <param name="_input">The old-slug / SApiLayer to deserialize</param>
    private void ModifyLayer(string _input)
    {
        Hashtable data = JsonConvert.DeserializeObject<Hashtable>(_input);
        Layer layerToUpdate = LayerManager.instance.GetLayer(data["old-slug"].ToString());
        if (layerToUpdate == null)
            return;
        SApiLayer newData = JsonConvert.DeserializeObject<SApiLayer>(data["layer"].ToString());
        layerToUpdate.UpdateFromSApiLayer(newData);
        UiManager.instance.layersList.RebuildMenu(UiManager.instance.BuildLayersList);
    }

    /// <summary>
    /// Call <see cref="Layer.ClearObjects"/> and remove the slug from <see cref="LayerManager.layers"/>
    /// </summary>
    /// <param name="_input">The layer slug to delete</param>
    private void DeleteLayer(string _input)
    {
        Layer layerToDel = LayerManager.instance.GetLayer(_input);
        if (layerToDel == null)
            return;
        layerToDel.ClearObjects();
        LayerManager.instance.layers.Remove(layerToDel);
        UiManager.instance.layersList.RebuildMenu(UiManager.instance.BuildLayersList);
    }
}
