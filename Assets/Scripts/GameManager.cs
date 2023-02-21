using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;
    public Server server;
    public ConfigLoader configLoader = new ConfigLoader();

    [Header("Materials")]
    public Material defaultMat;
    public Material alphaMat;
    public Material perfMat;
    public Material selectMat;
    public Material focusMat;
    public Material editMat;
    public Material highlightMat;
    public Material mouseHoverMat;
    public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.04445f;
    public float ouSize = 0.048f;

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
    public GameObject separatorModel;
    public GameObject pillarModel;
    public GameObject sensorExtModel;
    public GameObject sensorIntModel;
    public GameObject sensorBarModel;
    public GameObject sensorBarStdModel;

    [Header("Runtime data")]
    public string lastCmdFilePath;
    public Transform templatePlaceholder;
    public List<GameObject> currentItems = new List<GameObject>();
    public List<GameObject> previousItems = new List<GameObject>();
    public Hashtable allItems = new Hashtable();
    public Dictionary<string, SBuildingFromJson> buildingTemplates = new Dictionary<string, SBuildingFromJson>();
    public Dictionary<string, SRoomFromJson> roomTemplates = new Dictionary<string, SRoomFromJson>();
    public Dictionary<string, GameObject> objectTemplates = new Dictionary<string, GameObject>();
    public List<GameObject> focus = new List<GameObject>();
    public bool writeLogs = true;
    public bool editMode = false;
    public bool tempMode = false;
    private string startDateTime;

    public GameObject objectRoot;

    #region UnityMethods

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        EventManager.instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
    }

    private void Start()
    {
        configLoader.LoadConfig();
        server.StartServer();
        StartCoroutine(configLoader.LoadTextures());
        TempDiagram.instance.SetGradient(configLoader.GetCustomGradientColors(), configLoader.IsUsingCustomGradient());
    }

    private void OnDestroy()
    {
        AppendLogLine("--- Client closed ---\n\n", true, ELogtype.info);
    }

    #endregion

    ///<summary>
    /// Find a GameObject by its HierarchyName.
    ///</summary>
    ///<param name="_path">Which hierarchy name to look for</param>
    ///<returns>The GameObject looked for</returns>
    public GameObject FindByAbsPath(string _path)
    {
        if (allItems.Contains(_path))
            return (GameObject)allItems[_path];
        else
            return null;
    }

    ///<summary>
    /// Save current object and change the CLI idle text.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    public async Task SetCurrentItem(GameObject _obj)
    {
        try
        {
            previousItems = currentItems.GetRange(0, currentItems.Count);

            //////////////////////////////////////////////////////////
            //Should the previous selection's children be unloaded ?//
            //////////////////////////////////////////////////////////

            //if we are selecting, we don't want to unload children in the same rack as the selected object
            if (_obj != null)
            {
                AppendLogLine($"Select {_obj.name}.", true, ELogtype.success);
                OObject currentSelected = _obj.GetComponent<OObject>();
                //Checking all of the previously selected objects
                foreach (GameObject previousObj in currentItems)
                {
                    bool unloadChildren = true;
                    OObject previousSelected = previousObj.GetComponent<OObject>();

                    //Are the previous and current selection both a rack or smaller and part of the same rack ?
                    if (previousSelected != null && currentSelected != null && previousSelected.referent != null && previousSelected.referent == currentSelected.referent)
                        unloadChildren = false;

                    //if no to the previous question, previousSelected is a rack or smaller and level of details is <=1, unload its children
                    if (unloadChildren && previousSelected != null)
                    {
                        if (previousSelected.referent && previousSelected.referent.currentLod <= 1)
                            await previousSelected.referent.LoadChildren("0");
                        if (previousSelected.category != "rack" && previousSelected.referent.currentLod <= 1)
                        {
                            previousItems.Remove(previousObj);
                            if (previousSelected.referent && !previousItems.Contains(previousSelected.referent.gameObject))
                                previousItems.Add(previousSelected.referent.gameObject);
                        }
                    }
                }
                if ((_obj.GetComponent<OgreeObject>().category != "group" || _obj.GetComponent<OgreeObject>().category != "corridor")
                    && _obj.GetComponent<OgreeObject>().currentLod == 0)
                    await _obj.GetComponent<OgreeObject>().LoadChildren("1");
            }
            else // deselection => unload children if level of details is <=1
            {
                AppendLogLine("Empty selection.", true, ELogtype.success);
                foreach (GameObject previousObj in currentItems)
                {
                    OObject oObject = previousObj.GetComponent<OObject>();
                    if (oObject != null && oObject.currentLod <= 1)
                        await oObject.LoadChildren("0");
                }
            }
            //Clear current selection and add new item
            currentItems.Clear();
            if (_obj)
                currentItems.Add(_obj);

            EventManager.instance.Raise(new OnSelectItemEvent());
        }
        catch (Exception _e)
        {
            Debug.LogError(_e);
        }
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    ///<param name="_obj">The object to be added or removed from the selection</param>
    public async Task UpdateCurrentItems(GameObject _obj)
    {
        previousItems = currentItems.GetRange(0, currentItems.Count);
        if (currentItems[0].GetComponent<OgreeObject>().category != _obj.GetComponent<OgreeObject>().category
            || currentItems[0].transform.parent != _obj.transform.parent)
        {
            AppendLogLine("Multiple selection should be same type of objects and belong to the same parent.", true, ELogtype.warning);
            return;
        }
        if (currentItems.Contains(_obj))
        {
            AppendLogLine($"Remove {_obj.name} from selection.", true, ELogtype.success);
            currentItems.Remove(_obj);
            // _obj was the last item in selection
            if (currentItems.Count == 0)
            {
                OObject oObject = _obj.GetComponent<OObject>();
                if (oObject != null)
                    await oObject.LoadChildren("0");
            }
            else
            {
                bool unloadChildren = true;
                OObject currentDeselected = _obj.GetComponent<OObject>();

                //Checking all of the previously selected objects
                foreach (GameObject previousObj in currentItems)
                {
                    OObject previousSelected = previousObj.GetComponent<OObject>();

                    //Are the previous and current selection both a rack or smaller and part of the same rack ?
                    if (previousSelected != null && currentDeselected != null && previousSelected.referent != null && previousSelected.referent == currentDeselected.referent)
                        unloadChildren = false;

                    //if no to the previous question, previousSelected is a rack or smaller and level of details is <=1, unload its children
                    if (unloadChildren && previousSelected != null)
                    {
                        if (previousSelected.referent && previousSelected.referent.currentLod <= 1)
                            await previousSelected.referent.LoadChildren("0");
                        if (previousSelected.category != "rack" && previousSelected.referent.currentLod <= 1)
                        {
                            previousItems.Remove(previousObj);
                            if (previousSelected.referent && !previousItems.Contains(previousSelected.referent.gameObject))
                                previousItems.Add(previousSelected.referent.gameObject);
                        }
                    }
                }
                //if no to the previous question and previousSelected is a rack or smaller, unload its children
                if (unloadChildren)
                    await currentDeselected.LoadChildren("0");
            }
        }
        else
        {
            if ((_obj.GetComponent<OgreeObject>().category != "group" || _obj.GetComponent<OgreeObject>().category != "corridor")
                && _obj.GetComponent<OgreeObject>().currentLod == 0)
                await _obj.GetComponent<OgreeObject>().LoadChildren("1");
            AppendLogLine($"Select {_obj.name}.", true, ELogtype.success);
            currentItems.Add(_obj);
        }
        EventManager.instance.Raise(new OnSelectItemEvent());
    }

    ///<summary>
    /// Add a GameObject to focus list and disable its child's collider.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    public async Task FocusItem(GameObject _obj)
    {
        if (_obj && (!_obj.GetComponent<OObject>() || _obj.GetComponent<OObject>().category == "corridor"))
        {
            AppendLogLine($"Unable to focus {_obj.GetComponent<OgreeObject>().hierarchyName} should be a rack or a device.", true, ELogtype.warning);
            return;
        }

        OObject[] children = _obj.GetComponentsInChildren<OObject>();
        if (children.Length == 1)
        {
            AppendLogLine($"Unable to focus {_obj.GetComponent<OgreeObject>().hierarchyName}: no children found.", true, ELogtype.warning);
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
            EventManager.instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
        }
        else
            await UnfocusItem();
    }

    ///<summary>
    /// Remove last item from focus list, enable its child's collider.
    ///</summary>
    public async Task UnfocusItem()
    {
        GameObject obj = focus[focus.Count - 1];
        focus.Remove(obj);

        EventManager.instance.Raise(new OnUnFocusEvent() { obj = obj });
        if (focus.Count > 0)
        {
            EventManager.instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
            await SetCurrentItem(focus[focus.Count - 1]);
        }
        else
            await SetCurrentItem(obj);
    }

    ///<summary>
    /// Check if the given GameObject is a child (or a content) of focused object.
    ///</summary>
    ///<param name="_obj">The object to check</param>
    ///<returns>True if _obj is a child of focused object</returns>
    public bool IsInFocus(GameObject _obj)
    {
        Transform root = focus[focus.Count - 1].transform;
        if (root.GetComponent<Group>())
        {
            foreach (GameObject go in root.GetComponent<Group>().GetContent())
            {
                if (go == _obj)
                    return true;
            }
        }
        else
        {
            foreach (Transform child in root)
            {
                if (child.gameObject == _obj || IsInFocus(child.gameObject))
                    return true;
            }
        }
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
        if (_deselect)
            await SetCurrentItem(null);

        // Should count type of deleted objects
        if (_serverDelete)
        {
            ApiManager.instance.CreateDeleteRequest(_toDel.GetComponent<OgreeObject>());
            foreach (Transform child in _toDel.transform)
            {
                if (child.GetComponent<OgreeObject>())
                    ApiManager.instance.CreateDeleteRequest(child.GetComponent<OgreeObject>());
            }
        }
        Destroy(_toDel);
        StartCoroutine(Utils.ImportFinished());
    }

    ///<summary>
    /// Delete all tenants unless an _exception is given.
    ///</summary>
    ///<param name="_exception">The name of the tenant to keep</param>
    public async Task PurgeTenants(string _exception = null)
    {
        await SetCurrentItem(null);
        List<GameObject> tnToDel = new List<GameObject>();
        foreach (DictionaryEntry de in allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>().category == "tenant" && go.name != _exception)
                tnToDel.Add(go);
        }
        for (int i = 0; i < tnToDel.Count; i++)
            Destroy(tnToDel[i]);
    }

    ///<summary>
    /// Delete all room and object templates.
    ///</summary>
    public void PurgeTemplates()
    {
        List<GameObject> templatesToDel = new List<GameObject>();
        foreach (KeyValuePair<string, GameObject> kvp in objectTemplates)
            templatesToDel.Add(kvp.Value);
        for (int i = 0; i < templatesToDel.Count; i++)
            Destroy(templatesToDel[i]);
        objectTemplates.Clear();
        roomTemplates.Clear();
    }

    ///<summary>
    /// Display a message in the CLI.
    ///</summary>
    ///<param name="_line">The text to display</param>
    ///<param name="_writeInCli">Should the message be send to the CLI ?</param>
    ///<param name="_type">The type of message. Default is info</param>
    public void AppendLogLine(string _line, bool _writeInCli, ELogtype _type = ELogtype.info)
    {
        if (!writeLogs)
            return;

        // Legacy build-in CLI
        string color = "";
        if (_type == ELogtype.info || _type == ELogtype.infoCli || _type == ELogtype.infoApi)
            color = "white";
        else if (_type == ELogtype.success || _type == ELogtype.successCli || _type == ELogtype.successApi)
            color = "green";
        else if (_type == ELogtype.warning || _type == ELogtype.warningCli || _type == ELogtype.warningApi)
            color = "yellow";
        else if (_type == ELogtype.error || _type == ELogtype.errorCli || _type == ELogtype.errorApi)
            color = "red";

        // Remote CLI
        if (_writeInCli)
        {
            try
            {
                UiManager.instance.AppendLogLine(_line, color);
                server.Send(_line);
            }
            catch (System.Exception e)
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

        string fileName = $"{configLoader.GetCacheDir()}/{startDateTime}_log.txt";
        FileStream fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Append);
            using StreamWriter writer = new StreamWriter(fs);
            writer.Write($"{dateTime} | {type} : {_str}");
        }
        catch (Exception _e)
        {
            Debug.LogError(_e.Message);
        }
        finally
        {
            if (fs != null)
                fs.Dispose();
        }
    }

    ///<summary>
    /// Store a path to a command file. Turn on or off the reload button if there is a path or not.
    ///</summary>
    ///<param name="_value">If the button should be interatable</param>
    ///<param name="_lastPath">The command file path to store</param>
    public void SetReloadBtn(bool _value, string _lastPath = null)
    {
        if (_lastPath != null)
            lastCmdFilePath = _lastPath;
        if (!string.IsNullOrEmpty(lastCmdFilePath))
        {
            UiManager.instance.SetReloadBtn(_value);
            EventManager.instance.Raise(new ImportFinishedEvent());
        }
    }
}
