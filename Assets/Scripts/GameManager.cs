using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;
    public Server server;
    public ConfigHandler configHandler = new();

    [Header("Materials")]
    public Material defaultMat;
    public Material alphaMat;
    public Material perfMat;
    public Material selectMat;
    public Material focusMat;
    public Material editMat;
    public Material highlightMat;
    public Material highlightCubeMat;
    public Material mouseHoverMat;
    public Material scatterPlotMat;
    public Dictionary<string, Texture> textures = new();

    [Header("Models")]
    public GameObject buildingModel;
    public GameObject nonConvexBuildingModel;
    public GameObject roomModel;
    public GameObject nonConvexRoomModel;
    public GameObject rackModel;
    public GameObject labeledBoxModel;
    public GameObject tileModel;
    public GameObject tileNameModel;
    public GameObject uLocationModel;
    public GameObject coordinateSystemModel;
    public GameObject childOriginModel;
    public GameObject separatorModel;
    public GameObject pillarModel;
    public GameObject sensorExtModel;
    public GameObject sensorIntModel;
    public GameObject sensorBarModel;
    public GameObject sensorBarStdModel;
    public GameObject labelModel;
    public GameObject floatingLabelModel;
    public GameObject clearanceModel;
    public GameObject highlightCubeModel;
    public GameObject breakerModel;
    public GameObject genericCubeModel;
    public GameObject genericSphereModel;
    public GameObject genericCylinderModel;
    public List<GameObject> fbxModels;
    public GameObject positionTransform;

    [Header("Runtime data")]
    public Transform templatePlaceholder;
    private List<GameObject> currentItems = new();
    private List<GameObject> previousItems = new();
    public Hashtable allItems = new();
    public List<Tag> tags = new();
    public Dictionary<string, SBuildingTemplate> buildingTemplates = new();
    public Dictionary<string, SRoomTemplate> roomTemplates = new();
    public Dictionary<string, GameObject> objectTemplates = new();
    private readonly List<GameObject> focus = new();
    public bool writeLogs = true;
    public CameraControl cameraControl;

    /// <summary>
    /// True if Temperature Color mode is toggled on
    /// </summary>
    public bool tempColorMode = false;
    private string startDateTime;

    public GameObject objectRoot;

    /// <summary>
    /// True if edit mode is on
    /// </summary>
    public bool editMode = false;

    /// <summary>
    /// True if  select mode is on
    /// </summary>
    public bool selectMode = false;

    /// <summary>
    /// True if focus mode is on
    /// </summary>
    public bool focusMode = false;

    /// <summary>
    /// True if getCoords mode is on
    /// </summary>
    public bool getCoordsMode = false;

    /// <summary>
    /// True if position mode is on
    /// </summary>
    public bool positionMode = false;

    #region UnityMethods

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private void Start()
    {
        EventManager.instance.Raise(new ChangeCursorEvent(CursorChanger.CursorType.Idle));
        configHandler.LoadConfig();
        server.StartServer();
        StartCoroutine(configHandler.LoadTextures());
        TempDiagram.instance.SetGradient(configHandler.config.customTemperatureGradient, configHandler.config.useCustomGradient);

        // Add mandatory attribute in the placeholder for racks & generics
        templatePlaceholder.GetComponent<Room>().attributes["axisOrientation"] = "+x+y";
    }

    private void OnDestroy()
    {
        AppendLogLine(new LocalizedString("Logs", "Client closed"), ELogTarget.cli, ELogtype.info);
    }

    #endregion

    ///<summary>
    /// Save current object and change the CLI idle text.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    public async Task SetCurrentItem(GameObject _obj)
    {
        try
        {
            selectMode = false;
            List<GameObject> previousItemsTMP = currentItems.GetRange(0, currentItems.Count);
            previousItems = currentItems.GetRange(0, currentItems.Count);
            //Clear current selection
            currentItems.Clear();

            //////////////////////////////////////////////////////////
            //Should the previous selection's children be unloaded ?//
            //////////////////////////////////////////////////////////

            //if we are selecting, we don't want to unload children in the same referent as the selected object
            if (_obj)
            {
                AppendLogLine(new ExtendedLocalizedString("Logs", "Select object", _obj.name), ELogTarget.both, ELogtype.success);
                Item currentSelected = _obj.GetComponent<Item>();
                //Checking all of the previously selected objects
                foreach (GameObject previousObj in previousItemsTMP)
                {
                    if (!previousObj)
                    {
                        previousItems.Remove(previousObj);
                        continue;
                    }

                    Item previousSelected = previousObj.GetComponent<Item>();
                    //Are the previous and current selection part of the same referent ?
                    if (!previousSelected || !previousSelected.referent || (currentSelected && currentSelected.referent == previousSelected.referent))
                        continue;

                    //If the previous selection is not a referent, it will deleted during LoadChildren("0") and we don't want
                    //missing references in lists
                    if (previousSelected.referent != previousSelected)
                        previousItems.Remove(previousObj);

                    await previousSelected.referent.LoadChildren(0);
                }

                OgreeObject selectOgree = _obj.GetComponent<OgreeObject>();
                if (!(selectOgree is Group || selectOgree is Corridor || selectOgree.category == "tempBar") && selectOgree.currentLod == 0)
                    await selectOgree.LoadChildren(1);
            }
            else // deselection => unload children if level of details is <=1
            {
                AppendLogLine(new LocalizedString("Logs", "Empty selection"), ELogTarget.both, ELogtype.success);
                foreach (GameObject previousObj in previousItemsTMP)
                {
                    //There could be missing references due to scene changes and other edge cases
                    if (!previousObj)
                    {
                        previousItems.Remove(previousObj);
                        continue;
                    }
                    previousObj.GetComponent<Item>()?.LoadChildren(0);
                }
            }
            // add new item
            if (_obj)
                currentItems.Add(_obj);

            selectMode = currentItems.Count != 0;
            EventManager.instance.Raise(new OnSelectItemEvent());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    ///<param name="_obj">The object to be added or removed from the selection</param>
    public async Task UpdateCurrentItems(GameObject _obj)
    {
        try
        {
            selectMode = false;
            previousItems = currentItems.GetRange(0, currentItems.Count);
            if (currentItems[0].GetComponent<OgreeObject>().category != _obj.GetComponent<OgreeObject>().category
                || currentItems[0].transform.parent != _obj.transform.parent)
            {
                AppendLogLine(new LocalizedString("Logs", "Multiple selection error"), ELogTarget.both, ELogtype.warning);
                return;
            }
            if (currentItems.Contains(_obj))
            {
                AppendLogLine(new ExtendedLocalizedString("Logs", "Remove object from selection", _obj.name), ELogTarget.both, ELogtype.success);
                currentItems.Remove(_obj);
                // _obj was the last item in selection
                if (currentItems.Count == 0)
                {
                    Item item = _obj.GetComponent<Item>();
                    if (item && item.currentLod <= 1)
                        await item.LoadChildren(0);
                    if (focusMode)
                        currentItems.Add(focus[^1]);
                }
            }
            else
            {
                currentItems.Add(_obj);
                OgreeObject selectOgree = _obj.GetComponent<OgreeObject>();
                if (selectOgree is not Group && selectOgree is not Corridor && selectOgree.currentLod == 0)
                    await selectOgree.LoadChildren(1);
                AppendLogLine(new ExtendedLocalizedString("Logs", "Add object to selection", _obj.name), ELogTarget.both, ELogtype.success);
            }
            selectMode = currentItems.Count != 0;
            EventManager.instance.Raise(new OnSelectItemEvent());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    ///<summary>
    /// Add a GameObject to focus list and disable its child's collider.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    public async Task FocusItem(GameObject _obj)
    {
        if (_obj && !(_obj.GetComponent<Rack>() || _obj.GetComponent<Device>()))
        {
            AppendLogLine(new ExtendedLocalizedString("Logs", "Focus warning type", _obj.GetComponent<OgreeObject>().id), ELogTarget.both, ELogtype.warning);
            return;
        }

        Item[] children = _obj.GetComponentsInChildren<Item>();
        if (children.Length == 1)
        {
            AppendLogLine(new ExtendedLocalizedString("Logs", "Focus warning children", _obj.GetComponent<OgreeObject>().id), ELogTarget.both, ELogtype.warning);
            return;
        }

        bool canFocus;
        if (focus.Count == 0)
            canFocus = true;
        else
            canFocus = IsInFocus(_obj);

        if (canFocus == true)
        {
            _obj.SetActive(true);
            focus.Add(_obj);
            AppendLogLine(new ExtendedLocalizedString("Logs", "Focus object", _obj.GetComponent<OgreeObject>().id), ELogTarget.both, ELogtype.success);

            focusMode = focus.Count != 0;
            EventManager.instance.Raise(new OnFocusEvent(focus[^1]));
        }
        else
            await UnfocusItem();
    }

    ///<summary>
    /// Remove last item from focus list, enable its child's collider.
    ///</summary>
    public async Task UnfocusItem()
    {
        GameObject obj = focus[^1];
        focus.Remove(obj);

        focusMode = focus.Count != 0;
        EventManager.instance.Raise(new OnUnFocusEvent(obj));
        if (focus.Count > 0)
        {
            GameObject lastFocus = focus[^1];
            EventManager.instance.Raise(new OnFocusEvent(lastFocus));
            AppendLogLine(new ExtendedLocalizedString("Logs", "Focus object", lastFocus.GetComponent<OgreeObject>().id), ELogTarget.both, ELogtype.success);
            if (!currentItems.Contains(lastFocus))
                await SetCurrentItem(lastFocus);
        }
        else
        {
            if (!currentItems.Contains(obj))
                await SetCurrentItem(obj);
            AppendLogLine(new LocalizedString("Logs", "No focus"), ELogTarget.both, ELogtype.success);
        }
    }

    ///<summary>
    /// Remove all items from focus list .
    ///</summary>
    public async Task UnfocusAll()
    {
        for (int i = 0; i < focus.Count; i++)
            await UnfocusItem();
    }

    ///<summary>
    /// Check if the given GameObject is a child (or a content) of focused object.
    ///</summary>
    ///<param name="_obj">The object to check</param>
    ///<returns>True if _obj is a child of focused object or if there is no focused object</returns>
    public bool IsInFocus(GameObject _obj)
    {
        if (!focusMode)
            return true;

        Transform root = focus[^1].transform;
        if (root.GetComponent<Group>())
        {
            foreach (GameObject go in root.GetComponent<Group>().GetContent())
                if (go == _obj)
                    return true;
        }
        else if (_obj.GetComponent<OgreeObject>().id.Contains(root.GetComponent<OgreeObject>().id))
            return true;

        return false;
    }

    ///<summary>
    /// Delete a GameObject, set currentItem to null.
    ///</summary>
    ///<param name="_toDel">The object to delete</param>
    ///<param name="_serverDelete">True if _toDel have to be deleted from server</param>
    ///<param name="_deselect">Should we remove current selection ?</param>
    public async Task DeleteItem(GameObject _toDel, bool _serverDelete, bool _deselect = true)
    {
        UiManager.instance.DisableGetCoordsMode();
        if (_deselect)
            await SetCurrentItem(null);

        // Should count type of deleted objects
        if (_serverDelete)
        {
            ApiManager.instance.CreateDeleteRequest(_toDel.GetComponent<OgreeObject>());
            foreach (Transform child in _toDel.transform)
                if (child.GetComponent<OgreeObject>())
                    ApiManager.instance.CreateDeleteRequest(child.GetComponent<OgreeObject>());
        }
        Destroy(_toDel);
        StartCoroutine(Utils.ImportFinished());
    }

    ///<summary>
    /// Delete all domains. If an _exception is given, this domain will not be deleted.
    ///</summary>
    ///<param name="_exception">The name of the domain to keep</param>
    public async Task PurgeDomains(string _exception = null)
    {
        await SetCurrentItem(null);
        List<GameObject> doToDel = new();
        foreach (DictionaryEntry de in allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<Domain>() && go.name != _exception)
                doToDel.Add(go);
        }
        for (int i = 0; i < doToDel.Count; i++)
            Destroy(doToDel[i]);
    }

    ///<summary>
    /// Delete all room and object templates.
    ///</summary>
    public void PurgeTemplates()
    {
        List<GameObject> templatesToDel = new();
        foreach (KeyValuePair<string, GameObject> kvp in objectTemplates)
            templatesToDel.Add(kvp.Value);
        for (int i = 0; i < templatesToDel.Count; i++)
            Destroy(templatesToDel[i]);
        objectTemplates.Clear();
        buildingTemplates.Clear();
        roomTemplates.Clear();
    }

    ///<summary>
    /// Delete a template if not used
    ///</summary>
    ///<param name="_category">The type of the template</param>
    ///<param name="_template">The name of the template</param>
    public void DeleteTemplateIfUnused(string _category, string _template)
    {
        int count = 0;
        foreach (DictionaryEntry de in allItems)
        {
            OgreeObject obj = ((GameObject)de.Value).GetComponent<OgreeObject>();
            if (obj && obj.attributes.ContainsKey("template") && (string)obj.attributes["template"] == _template)
                count++;
        }

        if (count == 0)
        {
            GameObject toDel;
            switch (_category)
            {
                case Category.Building:
                    buildingTemplates.Remove(_template);
                    break;
                case Category.Room:
                    roomTemplates.Remove(_template);
                    break;
                case Category.Rack:
                    toDel = objectTemplates[_template];
                    objectTemplates.Remove(_template);
                    Destroy(toDel);
                    break;
                case Category.Device:
                    toDel = objectTemplates[_template];
                    objectTemplates.Remove(_template);
                    Destroy(toDel);
                    break;
            }
        }
    }

    ///<summary>
    /// Display a message in the CLI.
    ///</summary>
    ///<param name="_line">The text to display</param>
    ///<param name="_writeInCli">Should the message be send to the CLI ?</param>
    ///<param name="_type">The type of message. Default is info</param>
    public void AppendLogLine(LocalizedString _line, ELogTarget _target, ELogtype _type = ELogtype.info)
    {
        string str = _line.GetLocalizedString();
        AppendLogLine(str, _target, _type);
    }

    ///<summary>
    /// Display a message in the CLI.
    ///</summary>
    ///<param name="_line">The text to display</param>
    ///<param name="_writeInCli">Should the message be send to the CLI ?</param>
    ///<param name="_type">The type of message. Default is info</param>
    public void AppendLogLine(string _line, ELogTarget _target, ELogtype _type = ELogtype.info)
    {
        if (!writeLogs)
            return;

        if (_target == ELogTarget.logger || _target == ELogTarget.both)
        {
            try
            {
                UiManager.instance.AppendLogLine(_line, _type);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        if (_target == ELogTarget.cli || _target == ELogTarget.both)
        {
            try
            {
                server.Send(_line);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        WriteLogFile(_line, _type);
        switch (_type)
        {
            case ELogtype.warning:
                Debug.LogWarning(_line);
                break;
            case ELogtype.error:
                Debug.LogError(_line);
                break;
            default:
                Debug.Log(_line);
                break;
        }
    }

    ///<summary>
    /// Write a message in the log file. Use _type to add prefix.
    ///</summary>
    ///<param name="_str">The message to write</param>
    ///<param name="_type">The type of message</param>
    private void WriteLogFile(string _str, ELogtype _type)
    {
        if (string.IsNullOrEmpty(startDateTime))
        {
            startDateTime = DateTime.Now.ToString("yyMMdd_HH-mm");
            Debug.Log($"=>{startDateTime}<=");
        }

        string dateTime = DateTime.Now.ToString();
        string type = "";
        switch (_type)
        {
            case ELogtype.info:
                type = "INFO";
                break;
            case ELogtype.infoCli:
                type = "INFO [CLI]";
                break;
            case ELogtype.infoApi:
                type = "INFO [API]";
                break;
            case ELogtype.success:
                type = "SUCCESS";
                break;
            case ELogtype.successCli:
                type = "SUCCESS [CLI]";
                break;
            case ELogtype.successApi:
                type = "SUCCESS [API]";
                break;
            case ELogtype.warning:
                type = "WARNING";
                break;
            case ELogtype.warningCli:
                type = "WARNING [CLI]";
                break;
            case ELogtype.warningApi:
                type = "WARNING [API]";
                break;
            case ELogtype.error:
                type = "ERROR";
                break;
            case ELogtype.errorCli:
                type = "ERROR [CLI]";
                break;
            case ELogtype.errorApi:
                type = "ERROR [API]";
                break;
        }
        if (_str[_str.Length - 1] != '\n')
            _str += "\n";

        string fileName = $"{configHandler.GetCacheDir()}/{startDateTime}_log.txt";
        FileStream fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Append);
            using StreamWriter writer = new(fs);
            writer.Write($"{dateTime} | {type} : {_str}");
        }
        catch (Exception _e)
        {
            Debug.LogError(_e);
        }
        finally
        {
            fs?.Dispose();
        }
    }

    /// <summary>
    /// Check if the first object of the selected objects is of the current type <typeparamref name="T"/> and category <paramref name="_category"/>
    /// </summary>
    /// <typeparam name="T">The type of OObject you want to check</typeparam>
    /// <param name="_category">If you need to precise the category because <typeparamref name="T"/> is too broad, like "device" <br/> Leave empty if there is no need </param>
    /// <returns>False if the select list is empty or if the first object is not of right type and category </returns>
    public bool SelectIs<T>(string _category = "") where T : OgreeObject
    {
        if (currentItems.Count == 0)
            return false;

        return currentItems[0].GetComponent<T>() && (_category == "" || _category == currentItems[0].GetComponent<OgreeObject>().category);
    }

    /// <summary>
    /// Check if the last focused object is of the current type <typeparamref name="T"/> and category <paramref name="_category"/>
    /// </summary>
    /// <typeparam name="T">The type of OObject you want to check</typeparam>
    /// <param name="_category">If you need to precise the category because <typeparamref name="T"/> is too broad, like "device" <br/> Leave empty if there is no need </param>
    /// <returns>False if the select list is empty or if the last focused object is not of right type and category </returns>
    public bool FocusIs<T>(string _category = "") where T : Item
    {
        if (focus.Count == 0)
            return false;

        return focus[^1].GetComponent<T>() && (_category == "" || _category == focus[^1].GetComponent<OgreeObject>().category);
    }

    /// <summary>
    /// Create a copy of the currently selected objects to be checked
    /// </summary>
    /// <returns>a copy of the list of currently selected objects</returns>
    public List<GameObject> GetSelected()
    {
        return currentItems.GetRange(0, currentItems.Count);
    }

    /// <summary>
    /// Create a copy of the currently selected objects to be checked
    /// </summary>
    /// <returns>a copy of the list of currently selected objects</returns>
    public List<Item> GetSelectedReferents()
    {
        return selectMode ? currentItems.GetRange(0, currentItems.Count).Select(go => go.GetComponent<Item>()?.referent).Where(item => item).ToList() : new List<Item>();
    }

    /// <summary>
    /// Create a copy of the previously selected objects to be checked
    /// </summary>
    /// <returns>a copy of the list of previously selected objects</returns>
    public List<GameObject> GetPrevious()
    {
        return previousItems.GetRange(0, previousItems.Count);
    }

    /// <summary>
    /// Create a copy of the currently selected objects to be checked
    /// </summary>
    /// <returns>a copy of the list of currently selected objects</returns>
    public List<Item> GetPreviousReferents()
    {
        return previousItems.GetRange(0, previousItems.Count).Select(go => go.GetComponent<Item>()?.referent).Where(item => item).ToList();
    }

    /// <summary>
    /// Create a copy of the focused objects to be checked
    /// </summary>
    /// <returns>a copy of the list of currently focused objects</returns>
    public List<GameObject> GetFocused()
    {
        return focus.GetRange(0, focus.Count);
    }

    #region Tags
    /// <summary>
    /// Get a tag in <see cref="tags"/> by it's slug.
    /// </summary>
    /// <param name="_tagName">The slug of the tag to search</param>
    /// <returns>The asked tag or null otherwise</returns>
    public Tag GetTag(string _tagName)
    {
        foreach (Tag tag in tags)
            if (tag.slug == _tagName)
                return tag;
        return null;
    }

    /// <summary>
    /// If given tag doesn't exist, create a new <see cref="Tag"/> and add it to <see cref="tags"/> and <see cref="tags"/>.
    /// </summary>
    /// <param name="_data">Data from API</param>
    public void CreateTag(SApiTag _data)
    {
        if (GetTag(_data.slug) == null)
        {
            tags.Add(new Tag(_data));
            tags.Sort();
        }
    }

    /// <summary>
    /// Add an ogreeObject (by it's <paramref name="_objId"/> ) to a given <paramref name="_tagName"/>.
    /// If the tag doesn't exists, create it.
    /// </summary>
    /// <param name="_tagName">The tag to modify</param>
    /// <param name="_objId">The <see cref="OgreeObject.id"/> of the object to add</param>
    public async void AddToTag(string _tagName, string _objId)
    {
        if (GetTag(_tagName) == null)
            await ApiManager.instance.GetObject($"tags/{_tagName}", ApiManager.instance.CreateTag);

        Tag tag = GetTag(_tagName);
        tag.linkedObjects.Add(_objId);
        if (tag.objHightlighted)
            EventManager.instance.Raise(new HighlightEvent(Utils.GetObjectById(_objId), tag.color));
    }

    /// <summary>
    /// Remove an ogreeObject (by it's <paramref name="_objId"/> ) from a given <paramref name="_tagName"/>.
    /// If the tag has no linked object, remove it.
    /// </summary>
    /// <param name="_tagName">The tag to modify</param>
    /// <param name="_objId">The <see cref="OgreeObject.id"/> of the object to remove</param>
    public void RemoveFromTag(string _tagName, string _objId)
    {
        Tag targetedTag = GetTag(_tagName);
        targetedTag.linkedObjects.Remove(_objId);
        Utils.GetObjectById(_objId)?.GetComponent<OgreeObject>().tags.Remove(_tagName);
        if (targetedTag.linkedObjects.Count == 0)
        {
            tags.Remove(targetedTag);
            UiManager.instance.tagsList.RebuildMenu(UiManager.instance.BuildTagButtons);
        }
    }
    #endregion
}
