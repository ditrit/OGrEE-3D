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
    public Hashtable allItems = new Hashtable();
    public Dictionary<string, ReadFromJson.SRoomFromJson> roomTemplates = new Dictionary<string, ReadFromJson.SRoomFromJson>();
    public Dictionary<string, GameObject> objectTemplates = new Dictionary<string, GameObject>();
    public bool isWireframe;

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

    private void Start()
    {
        EventManager.Instance.Raise(new ChangeCursorEvent() { type = CursorChanger.CursorType.Idle });
        configLoader.LoadConfig();
        StartCoroutine(configLoader.LoadTextures());

        UiManager.instance.UpdateFocusText();

#if API_DEBUG
        UiManager.instance.ToggleApi();
#endif

#if !PROD
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/testCmds.txt");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/perfTest.ocli");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/fbxModels.ocli");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/demoApi.ocli");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__EDF__/EDF_EXAION.ocli");
#endif
    }

    private void Update()
    {
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(currentItems[0].GetComponent<OgreeObject>())));
#endif

        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
            clickCount++;

        if (clickCount == 1 && coroutineAllowed)
            StartCoroutine(DoubleClickDetection(Time.time));
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
    /// Method called when single click on a gameObject.
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
    public void SetCurrentItem(GameObject _obj)
    {
        //Clear current selection
        for (int i = currentItems.Count - 1; i >= 0; i--)
            DeselectItem(currentItems[i]);

        if (_obj)
        {
            AppendLogLine($"Select {_obj.name}.", "green");
            SelectItem(_obj);
            UiManager.instance.SetCurrentItemText(currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        }
        else
        {
            AppendLogLine("Empty selection.", "green");
            UiManager.instance.SetCurrentItemText("Ogree3D");
        }
        UiManager.instance.UpdateGuiInfos();
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    public void UpdateCurrentItems(GameObject _obj)
    {
        if (currentItems[0].GetComponent<OgreeObject>().category != _obj.GetComponent<OgreeObject>().category)
        {
            AppendLogLine("Multiple selection should be same type of objects.", "yellow");
            return;
        }
        if (currentItems.Contains(_obj))
        {
            AppendLogLine($"Remove {_obj.name} from selection.", "green");
            DeselectItem(_obj);
        }
        else
        {
            AppendLogLine($"Add {_obj.name} to selection.", "green");
            SelectItem(_obj);
        }

        if (currentItems.Count > 1)
            UiManager.instance.SetCurrentItemText("Selection");
        else if (currentItems.Count == 1)
            UiManager.instance.SetCurrentItemText(currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        else
            UiManager.instance.SetCurrentItemText("Ogree3D");

        UiManager.instance.UpdateGuiInfos();
    }

    ///<summary>
    /// Add _obj to currentItems, enable outline if possible.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    private void SelectItem(GameObject _obj)
    {
        if (currentItems.Count == 0)
            UiManager.instance.detailsInputField.ActiveInputField(true);

        currentItems.Add(_obj);

        EventManager.Instance.Raise(new OnSelectItemEvent() { obj = _obj });
        UiManager.instance.detailsInputField.UpdateInputField(currentItems[0].GetComponent<OgreeObject>().currentLod.ToString());
    }

    ///<summary>
    /// Remove _obj from currentItems, disable outline if possible.
    ///</summary>
    ///<param name="_obj">The GameObject to remove</param>
    private void DeselectItem(GameObject _obj)
    {
        currentItems.Remove(_obj);
        if (currentItems.Count == 0)
        {
            UiManager.instance.detailsInputField.UpdateInputField("0");
            UiManager.instance.detailsInputField.ActiveInputField(false);
        }

        EventManager.Instance.Raise(new OnDeselectItemEvent() { obj = _obj });
    }

    ///<summary>
    /// Add a GameObject to focus list and disable its child's collider.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    public void FocusItem(GameObject _obj)
    {
        if (_obj.GetComponent<OgreeObject>().category == "corridor")
            return;

        OObject[] children = _obj.GetComponentsInChildren<OObject>();
        if (children.Length == 1)
        {
            AppendLogLine($"Unable to focus {_obj.GetComponent<OgreeObject>().hierarchyName}: no children found.", "yellow");
            return;
        }

        bool canFocus = false;
        if (focus.Count == 0)
            canFocus = true;
        else
            canFocus = IsInFocus(_obj);

        if (canFocus == true)
        {
            focus.Add(_obj);
            UiManager.instance.UpdateFocusText();
            EventManager.Instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
        }
        else
            UnfocusItem();
    }

    ///<summary>
    /// Remove last item from focus list, enable its child's collider.
    ///</summary>
    public void UnfocusItem()
    {
        GameObject obj = focus[focus.Count - 1];
        focus.Remove(obj);
        UiManager.instance.UpdateFocusText();

        EventManager.Instance.Raise(new OnUnFocusEvent() { obj = obj });
        if (focus.Count > 0)
        {
            EventManager.Instance.Raise(new OnFocusEvent() { obj = focus[focus.Count - 1] });
            SetCurrentItem(focus[focus.Count - 1]);
        }
        else
            SetCurrentItem(null);
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
                if (child.gameObject == _obj)
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
    public void DeleteItem(GameObject _toDel, bool _serverDelete)
    {
        SetCurrentItem(null);

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
    public void ReloadFile()
    {
        SetCurrentItem(null);
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
        Filters.instance.DefaultList(Filters.instance.tenantsList, "All");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
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
    /// Quit the application.
    ///</summary>
    public void QuitApp()
    {
        Application.Quit();
    }

}
