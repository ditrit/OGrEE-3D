﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;
    public ConsoleController consoleController;
    public Server server;
    public ConfigLoader configLoader = new ConfigLoader();
    [Header("AR")]
    public Camera mainCamera;

    [Header("Materials")]
    public Material defaultMat;
    public Material alphaMat;
    public Material perfMat;
    public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.04445f;
    public float ouSize = 0.048f;

    [Header("Models")]
    public GameObject buildingModel;
    public GameObject roomModel;
    public GameObject rackModel;
    public GameObject labeledBoxModel;
    public GameObject tileNameModel;
    public GameObject uLocationModel;
    public GameObject coordinateSystemModel;
    public GameObject separatorModel;
    public GameObject sensorExtModel;
    public GameObject sensorIntModel;

    [Header("Runtime data")]
    public string lastCmdFilePath;
    public Transform templatePlaceholder;
    public List<GameObject> currentItems = new List<GameObject>();
    public List<GameObject> previousItems = new List<GameObject>();
    public Hashtable allItems = new Hashtable();
    public Dictionary<string, ReadFromJson.SRoomFromJson> roomTemplates = new Dictionary<string, ReadFromJson.SRoomFromJson>();
    public Dictionary<string, GameObject> objectTemplates = new Dictionary<string, GameObject>();
    public List<GameObject> focus = new List<GameObject>();
    public bool writeLogs = true;

    #region UnityMethods

    private void Awake()
    {
        if (!gm)
            gm = this;
        else
            Destroy(this);
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
        server.StartServer();
        configLoader.LoadConfig();
        StartCoroutine(configLoader.LoadTextures());

    }

    private async Task Start()
    {
#if API_DEBUG
        await ConnectToApi();
#endif

#if RACK
        await Task.Delay(2500);
        var api_listener = new ApiListener();
        await api_listener.LoadSingleRack("EDF","NOE","BI2","C8","C05");
#endif

#if !PROD
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/testCmds.txt");
#endif
    }

    private void OnDestroy()
    {
        AppendLogLine("--- Client closed ---\n\n", true, eLogtype.info);
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
                OObject currentSelected = _obj.GetComponent<OObject>();
                //Checking all of the previously selected objects
                foreach (GameObject previousObj in currentItems)
                {
                    bool unloadChildren = true;
                    OObject previousSelected = previousObj.GetComponent<OObject>();

                    //Are the previous and current selection both a rack or smaller and part of the same rack ?
                    if (previousSelected != null && currentSelected != null && previousSelected.referent != null && previousSelected.referent == currentSelected.referent)
                        unloadChildren = false;

                    //if no to the previous question and previousSelected is a rack or smaller, unload its children
                    if (unloadChildren && previousSelected != null)
                    {
                        if (previousSelected.referent)
                        {
                            await previousSelected.referent.LoadChildren("0");
                        }
                        if (previousSelected.category != "rack")
                        {
                            previousItems.Remove(previousObj);
                            if (previousSelected.referent && !previousItems.Contains(previousSelected.referent.gameObject))
                                previousItems.Add(previousSelected.referent.gameObject);
                        }
                    }
                }

            }
            else
            {
                foreach (GameObject previousObj in currentItems)
                {
                    OObject oObject = previousObj.GetComponent<OObject>();
                    if (oObject != null)
                        await oObject.LoadChildren("0");
                }
            }
            //Clear current selection
            currentItems.Clear();

            if (_obj)
            {
                await _obj.GetComponent<OgreeObject>().LoadChildren("1");
                AppendLogLine($"Select {_obj.name}.", true, eLogtype.success);
                currentItems.Add(_obj);
            }
            else
                AppendLogLine("Empty selection.", true, eLogtype.success);
            EventManager.Instance.Raise(new OnSelectItemEvent());
        }
        catch (System.Exception _e)
        {
            Debug.LogError(_e);
        }
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    public async Task UpdateCurrentItems(GameObject _obj)
    {
        previousItems = currentItems.GetRange(0, currentItems.Count);
        if (currentItems[0].GetComponent<OgreeObject>().category != _obj.GetComponent<OgreeObject>().category
            || currentItems[0].transform.parent != _obj.transform.parent)
        {
            AppendLogLine("Multiple selection should be same type of objects and belong to the same parent.", true, eLogtype.warning);
            return;
        }
        if (currentItems.Contains(_obj))
        {
            AppendLogLine($"Remove {_obj.name} from selection.", true, eLogtype.success);
            currentItems.Remove(_obj);
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

                }
                //if no to the previous question and previousSelected is a rack or smaller, unload its children
                if (unloadChildren)
                {
                    await currentDeselected.LoadChildren("0");
                }
            }
        }
        else
        {
            await _obj.GetComponent<OgreeObject>().LoadChildren("1");
            AppendLogLine($"Select {_obj.name}.", true, eLogtype.success);
            currentItems.Add(_obj);
        }
        EventManager.Instance.Raise(new OnSelectItemEvent());
    }

    ///<summary>
    /// Add a GameObject to focus list and disable its child's collider.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    public async Task FocusItem(GameObject _obj)
    {
        if (_obj.GetComponent<OgreeObject>().category == "corridor")
            return;

        OObject[] children = _obj.GetComponentsInChildren<OObject>();
        if (children.Length == 1)
        {
            AppendLogLine($"Unable to focus {_obj.GetComponent<OgreeObject>().hierarchyName}: no children found.", true, eLogtype.warning);
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
            EventManager.Instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
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

        EventManager.Instance.Raise(new OnUnFocusEvent() { obj = obj });
        if (focus.Count > 0)
        {
            EventManager.Instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
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
    }

    ///<summary>
    /// Display a message in the CLI.
    ///</summary>
    ///<param name="_line">The text to display</param>
    ///<param name="_color">The color of the text. Default is white</param>
    public void AppendLogLine(string _line, bool _writeInCli, eLogtype _type = eLogtype.info)
    {
        if (!writeLogs)
            return;

        // Legacy build-in CLI
        string color = "";
        if (_type == eLogtype.info)
            color = "white";
        else if (_type == eLogtype.success)
            color = "green";
        else if (_type == eLogtype.warning)
            color = "yellow";
        else if (_type == eLogtype.error)
            color = "red";
        consoleController.AppendLogLine(_line, color);

        // Remote CLI
        if (_writeInCli)
        {
            try
            {
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
            case eLogtype.warning:
                Debug.LogWarning(_line);
                break;
            case eLogtype.error:
                Debug.LogError(_line);
                break;
            default:
                Debug.Log(_line);
                break;
        }
    }

    ///
    private void WriteLogFile(string _str, eLogtype _type)
    {
        string dateTime = System.DateTime.Now.ToString();
        string type = "";
        switch (_type)
        {
            case eLogtype.info:
                type = "INFO";
                break;
            case eLogtype.success:
                type = "SUCCESS";
                break;
            case eLogtype.warning:
                type = "WARNING";
                break;
            case eLogtype.error:
                type = "ERROR";
                break;
        }
        if (_str[_str.Length - 1] != '\n')
            _str += "\n";

        string fileName = $"{configLoader.GetCacheDir()}/log.txt";
        FileStream fs = null;
        try
        {
            fs = new FileStream(fileName, FileMode.Append);
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write($"{dateTime} | {type}: {_str}");
            }
        }
        catch (System.Exception _e)
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
    /// Connect the client to registered API in configLoader.
    ///</summary>
    public async Task ConnectToApi()
    {
        await configLoader.ConnectToApi();
        if (ApiManager.instance.isInit)
            UiManager.instance.ChangeApiButton("Connected to Api", Color.green);
        else
            UiManager.instance.ChangeApiButton("Fail to connected to Api", Color.red);
        UiManager.instance.SetApiUrlText(configLoader.GetApiUrl());
        //StartCoroutine(TestAPI());
    }

    ///<summary>
    /// Store a path to a command file. Turn on or off the reload button if there is a path or not.
    ///</summary>
    ///<param name="_lastPath">The command file path to store</param>
    public void SetReloadBtn(bool _value, string _lastPath = null)
    {
        if (_lastPath != null)
            lastCmdFilePath = _lastPath;
        if (!string.IsNullOrEmpty(lastCmdFilePath))
        {
            UiManager.instance.SetReloadBtn(_value);
            EventManager.Instance.Raise(new ImportFinishedEvent());
        }
    }

    ///<summary>
    /// Clean AR scene from all existing tenants.
    ///</summary>
    public IEnumerator CleanScene()
    {
        List<GameObject> tenants = new List<GameObject>();
        foreach (DictionaryEntry de in allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>()?.category == "tenant")
                tenants.Add(go);
        }
        for (int i = 0; i < tenants.Count; i++)
            Destroy(tenants[i]);
        yield return new WaitForEndOfFrame();
    }

    ///<summary>
    /// Move a rack in front of the camera.
    ///</summary>
    public void MoveRackToCamera()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            var goArray = FindObjectsOfType<GameObject>();
            for (var i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == LayerMask.NameToLayer("Rack"))
                {
#if VR
                    Utils.MoveObjectToCamera(goArray[i], mainCamera, 1.5f, -0.7f, 90 + 180, 0);
#endif
#if !VR
                    Utils.MoveObjectInFrontOfCamera(goArray[i], mainCamera, 1.5f, 90);
#endif
                }
            }
            GameManager.gm.AppendLogLine($"No Rack to move in the scene", false, eLogtype.warning);
        }

        else
        {
            Transform t = GameManager.gm.currentItems[0].transform;
            while (t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
#if VR
                    Utils.MoveObjectToCamera(t.gameObject, mainCamera, 1.5f, -0.7f, 90, 0);
#endif
#if !VR
                    Utils.MoveObjectInFrontOfCamera(t.gameObject, mainCamera, 1.5f, 90);
#endif
                    OgreeObject ogree = t.gameObject.GetComponent<OgreeObject>();
                    ogree.originalLocalRotation = t.gameObject.transform.localRotation;  //update the originalLocalRotation to not mess up when using reset button from TIM
                    ogree.originalLocalPosition = t.gameObject.transform.localPosition;
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", false, eLogtype.error);
        }
    }

    ///<summary>
    /// Move a gameobject in front of the camera.
    ///</summary>
    public void MoveMenuToCamera(GameObject _g)
    {
        Utils.MoveObjectToCamera(_g, mainCamera, 0.6f, -0.35f, 0, 25);
    }


    ///<summary>
    /// Quit the application.
    ///</summary>
    public void QuitApp()
    {
        Application.Quit();
    }

}
