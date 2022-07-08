using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

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

    // Double click
    private float doubleClickTimeLimit = 0.25f;
    private bool coroutineAllowed = true;
    private int clickCount = 0;

    public bool writeCLI = true;

    #region UnityMethods

    private void Awake()
    {
        if (!gm)
            gm = this;
        else
            Destroy(this);
    }

private async Task Start()
    {
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
        configLoader.LoadConfig();
        StartCoroutine(configLoader.LoadTextures());
        UiManager.instance.UpdateFocusText();
        mainCamera = Camera.main;
        UiManager.instance.ToggleApi();


#if API_DEBUG
        
#endif
       
#if RACK
        await Task.Delay(2500);
        var api_listener = new ApiListener();
        await api_listener.LoadSingleRack("EDF","NOE","BI2","C8","C05");
#endif

    }

    private void Update()
    {
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(currentItems[0].GetComponent<OgreeObject>())));
#endif
        //if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        //    clickCount++;

        //if (clickCount == 1 && coroutineAllowed)
        //    StartCoroutine(DoubleClickDetection(Time.time));
    }

    #endregion


    ///<summary>
    /// Check if simple or double click and call corresponding method.
    ///</summary>
    ///<param name="_firstClickTime">The time of the first click</param>
    private IEnumerator DoubleClickDetection(float _firstClickTime)
    {
        coroutineAllowed = false;
        while (Time.time < _firstClickTime + doubleClickTimeLimit)
        {
            if (clickCount == 2)
            {
                DoubleClick();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (clickCount == 1)
            SingleClick();
        clickCount = 0;
        coroutineAllowed = true;

    }

    ///<summary>
    /// Method called when single click on a gameObject.
    ///</summary>
    private void SingleClick()
    {

        GameObject objectHit = Utils.RaycastFromCameraToMouse();


        if (objectHit && objectHit.tag == "Selectable")
        {
            bool canSelect = false;
            if (focus.Count > 0)
                canSelect = IsInFocus(objectHit);
            else
                canSelect = true;

            if (canSelect)
            {
                if (Input.GetKey(KeyCode.LeftControl) && currentItems.Count > 0)
                    UpdateCurrentItems(objectHit);
                else
                    SetCurrentItem(objectHit);
            }
        }
        else if (focus.Count > 0)
            SetCurrentItem(focus[focus.Count - 1]);
        else
            SetCurrentItem(null);
    }

    ///<summary>
    /// Method called when double click on a gameObject.
    ///</summary>
    private void DoubleClick()
    {
        GameObject objectHit = Utils.RaycastFromCameraToMouse();
        if (objectHit && objectHit.tag == "Selectable" && objectHit.GetComponent<OObject>())
        {
            if (objectHit.GetComponent<Group>())
                objectHit.GetComponent<Group>().ToggleContent("true");
            else
            {
                SetCurrentItem(objectHit);
                FocusItem(objectHit);
            }
        }
        else if (focus.Count > 0)
            UnfocusItem();
    }


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
            print("call to set current item, current items :"+currentItems.Count);
            foreach (GameObject obj in currentItems)
                print(obj.name);
            previousItems = currentItems.GetRange(0,currentItems.Count);

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
                    if (previousSelected != null && currentSelected != null && previousSelected.parentRack != null && previousSelected.parentRack == currentSelected.parentRack)
                        unloadChildren = false;

                    //if no to the previous question and previousSelected is a rack or smaller, unload its children
                    if (unloadChildren && previousSelected != null)
                    {
                        previousObj.GetComponent<FocusHandler>().ogreeChildMeshRendererList.Clear();
                        previousObj.GetComponent<FocusHandler>().ogreeChildObjects.Clear();
                        await previousSelected?.LoadChildren("0");
                    }
                }

            }
            else
            {
                foreach (GameObject previousObj in currentItems)
                {
                    previousObj?.GetComponent<FocusHandler>()?.ogreeChildMeshRendererList.Clear();
                    previousObj?.GetComponent<FocusHandler>()?.ogreeChildObjects.Clear();
                    await previousObj?.GetComponent<OObject>()?.LoadChildren("0");
                }
            }

            //Clear current selection
            currentItems.Clear();
#if !VR
            UiManager.instance.detailsInputField.UpdateInputField("0");
            UiManager.instance.detailsInputField.ActiveInputField(false);
#endif

            if (_obj)
            {
                await _obj.GetComponent<OgreeObject>().LoadChildren("1");
                AppendLogLine($"Select {_obj.name}.", "green");
#if !VR
                if (currentItems.Count == 0)
                    UiManager.instance.detailsInputField.ActiveInputField(true);
#endif
                currentItems.Add(_obj);
#if !VR
                UiManager.instance.detailsInputField.UpdateInputField(currentItems[0].GetComponent<OgreeObject>().currentLod.ToString());
#endif
                UiManager.instance.SetCurrentItemText(currentItems[0].GetComponent<OgreeObject>().hierarchyName);
            }
            else
            {
                AppendLogLine("Empty selection.", "green");
                UiManager.instance.SetCurrentItemText("Ogree3D");
            }
            UiManager.instance.UpdateGuiInfos();
            EventManager.Instance.Raise(new OnSelectItemEvent());
            if (_obj)
                await UManager.um.HighlightULocation();
        } catch(System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    public async Task UpdateCurrentItems(GameObject _obj)
    {
        previousItems = currentItems;
        if (currentItems[0].GetComponent<OgreeObject>().category != _obj.GetComponent<OgreeObject>().category)
        {
            AppendLogLine("Multiple selection should be same type of objects.", "yellow");
            return;
        }
        if (currentItems.Contains(_obj))
        {
            AppendLogLine($"Remove {_obj.name} from selection.", "green");
            currentItems.Remove(_obj);
            if (currentItems.Count == 0)
            {
#if VR
                UiManager.instance.detailsInputField.UpdateInputField("0");
                UiManager.instance.detailsInputField.ActiveInputField(false);
#endif
                _obj.GetComponent<FocusHandler>()?.ogreeChildMeshRendererList.Clear();
                _obj.GetComponent<FocusHandler>()?.ogreeChildObjects.Clear();
                await _obj.GetComponent<OObject>()?.LoadChildren("0");
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
                    if (previousSelected != null && currentDeselected != null && previousSelected.parentRack != null && previousSelected.parentRack == currentDeselected.parentRack)
                        unloadChildren = false;

                }
                //if no to the previous question and previousSelected is a rack or smaller, unload its children
                if (unloadChildren)
                {
                    currentDeselected.GetComponent<FocusHandler>()?.ogreeChildMeshRendererList.Clear();
                    currentDeselected.GetComponent<FocusHandler>()?.ogreeChildObjects.Clear();
                    await currentDeselected.LoadChildren("0");
                }
            }
        }
        else
        {
            await _obj.GetComponent<OgreeObject>().LoadChildren("1");
            AppendLogLine($"Select {_obj.name}.", "green");
            if (currentItems.Count == 0)
                UiManager.instance.detailsInputField.ActiveInputField(true);
            currentItems.Add(_obj);
#if VR
            UiManager.instance.detailsInputField.UpdateInputField(currentItems[0].GetComponent<OgreeObject>().currentLod.ToString());
#endif        
            UiManager.instance.SetCurrentItemText(currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        }

        if (currentItems.Count > 1)
            UiManager.instance.SetCurrentItemText("Selection");
        else if (currentItems.Count == 1)
            UiManager.instance.SetCurrentItemText(currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        else
            UiManager.instance.SetCurrentItemText("Ogree3D");

        UiManager.instance.UpdateGuiInfos();
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
            AppendLogLine($"Unable to focus {_obj.GetComponent<OgreeObject>().hierarchyName}: no children found.", "yellow");
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
            UiManager.instance.UpdateFocusText();
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
        UiManager.instance.UpdateFocusText();

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
    private bool IsInFocus(GameObject _obj)
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
    public async Task DeleteItem(GameObject _toDel, bool _serverDelete, bool deselect = true)
    {
        if (deselect)
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
    public void AppendLogLine(string _line, string _color = "white")
    {
        if (!writeCLI)
            return;

        consoleController.AppendLogLine(_line, _color);
        try
        {
            server.Send(_line);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
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
    /// Called by GUI button: Delete all Tenants and reload last loaded file.
    ///</summary>
    public async Task ReloadFile()
    {
        await SetCurrentItem(null);
        focus.Clear();
        UiManager.instance.UpdateFocusText();

        List<GameObject> tenants = new List<GameObject>();
        foreach (DictionaryEntry de in allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>()?.category == "tenant")
                tenants.Add(go);
        }
        for (int i = 0; i < tenants.Count; i++)
            Destroy(tenants[i]);
        allItems.Clear();

        foreach (KeyValuePair<string, GameObject> kvp in objectTemplates)
            Destroy(kvp.Value);
        objectTemplates.Clear();
        roomTemplates.Clear();
        consoleController.variables.Clear();
        consoleController.ResetCounts();
        //Filters.instance.DefaultList(Filters.instance.tenantsList, "All");
        //Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        StartCoroutine(LoadFile());
    }

    ///<summary>
    /// Coroutine for waiting until end of frame to trigger all OnDestroy() methods before loading file.
    ///</summary>
    private IEnumerator LoadFile()
    {
        yield return new WaitForEndOfFrame();
        consoleController.RunCommandString($".cmds:{lastCmdFilePath}");
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
                    Utils.MoveObjectToCamera(goArray[i], mainCamera, 1.5f, -0.7f, 90 + 180, 0);
                }
            }
            GameManager.gm.AppendLogLine($"No Rack to move in the scene", "yellow");
        }

        else
        {
            Transform t = GameManager.gm.currentItems[0].transform;
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    Utils.MoveObjectToCamera(t.gameObject, mainCamera, 1.5f, -0.7f, 90, 0);
                    OgreeObject ogree = t.gameObject.GetComponent<OgreeObject>();
                    ogree.originalLocalRotation = t.gameObject.transform.localRotation;  //update the originalLocalRotation to not mess up when using reset button from TIM
                    ogree.originalLocalPosition = t.gameObject.transform.localPosition;
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
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
