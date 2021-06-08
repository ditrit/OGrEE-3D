using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MoveObject))]
public class GameManager : MonoBehaviour
{
    static public GameManager gm;
    public ConsoleController consoleController;
    private ConfigLoader configLoader = new ConfigLoader();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI currentItemText = null;
    [SerializeField] private Button reloadBtn = null;
    [SerializeField] private Camera currentCam = null;
    [SerializeField] private GUIObjectInfos objInfos = null;
    [SerializeField] private Toggle toggleWireframe = null;
    [SerializeField] private TextMeshProUGUI focusText = null;

    [Header("Panels")]
    [SerializeField] private GameObject menu = null;
    [SerializeField] private GameObject infosPanel = null;
    [SerializeField] private GameObject debugPanel = null;

    [Header("Materials")]
    public Material defaultMat;
    public Material wireframeMat;
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
        configLoader.LoadConfig();
        StartCoroutine(configLoader.ConnectToApi());
        StartCoroutine(configLoader.LoadTextures());

        UpdateFocusText();

#if DEBUG
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/testCmds.txt");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__DEMO__/demoApi.ocli");
        // consoleController.RunCommandString(".cmds:K:/_Orness/Nextcloud/Ogree/4_customers/__EDF__/EDF_EXAION.ocli");
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            menu.SetActive(!menu.activeSelf);

#if DEBUG
        if (Input.GetKeyDown(KeyCode.Insert) && currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(currentItems[0].GetComponent<OgreeObject>())));
#endif

        if (!EventSystem.current.IsPointerOverGameObject() && !GetComponent<MoveObject>().hasDrag
            && Input.GetMouseButtonUp(0))
        {
            clickCount++;
        }

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
        RaycastHit hit;
        Physics.Raycast(currentCam.transform.position, currentCam.ScreenPointToRay(Input.mousePosition).direction, out hit);
        if (hit.collider && hit.collider.tag == "Selectable")
        {
            bool canSelect = false;
            if (focus.Count > 0)
            {
                foreach (Transform child in focus[focus.Count - 1].transform)
                {
                    if (child == hit.collider.transform.parent)
                        canSelect = true;
                }
            }
            else
                canSelect = true;

            if (canSelect)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                    UpdateCurrentItems(hit.collider.transform.parent.gameObject);
                else
                    SetCurrentItem(hit.collider.transform.parent.gameObject);
            }
        }
        else if (hit.collider == null || (hit.collider && hit.collider.tag != "Selectable"))
        {
            if (currentItems.Count > 0)
                AppendLogLine("Empty selection.", "green");
            SetCurrentItem(null);
        }
    }

    ///<summary>
    /// Method called when single click on a gameObject.
    ///</summary>
    private void DoubleClick()
    {
        // Debug.Log("Double Click");
        RaycastHit hit;
        Physics.Raycast(currentCam.transform.position, currentCam.ScreenPointToRay(Input.mousePosition).direction, out hit);
        if (hit.collider && hit.collider.tag == "Selectable" && hit.collider.transform.parent.GetComponent<OObject>())
            FocusItem(hit.collider.transform.parent.gameObject);
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
            currentItemText.text = currentItems[0].GetComponent<OgreeObject>().hierarchyName;
        }
        else
            currentItemText.text = "Ogree3D";
        UpdateGuiInfos();
    }

    ///<summary>
    /// Add selected object to currentItems if not in it, else remove it.
    ///</summary>
    public void UpdateCurrentItems(GameObject _obj)
    {
        if ((currentItems[0].GetComponent<Building>() && !_obj.GetComponent<Building>())
            || (currentItems[0].GetComponent<OObject>() && !_obj.GetComponent<OObject>()))
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
            currentItemText.text = "Selection";
        else if (currentItems.Count == 1)
            currentItemText.text = currentItems[0].GetComponent<OgreeObject>().hierarchyName;
        else
            currentItemText.text = "Ogree3D";

        UpdateGuiInfos();
    }

    ///<summary>
    /// Add _obj to currentItems, enable outline if possible.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    private void SelectItem(GameObject _obj)
    {
        currentItems.Add(_obj);
        if (_obj.GetComponent<OObject>())
        {
            cakeslice.Outline ol = _obj.transform.GetChild(0).GetComponent<cakeslice.Outline>();
            if (ol)
                ol.enabled = true;
                // ol.eraseRenderer = false;
        }
    }

    ///<summary>
    /// Remove _obj from currentItems, disable outline if possible.
    ///</summary>
    ///<param name="_obj">The GameObject to remove</param>
    private void DeselectItem(GameObject _obj)
    {
        currentItems.Remove(_obj);
        if (_obj.GetComponent<OObject>())
        {
            cakeslice.Outline ol = _obj.transform.GetChild(0).GetComponent<cakeslice.Outline>();
            if (ol)
                ol.enabled = false;
                // ol.eraseRenderer = true;
        }
    }

    ///<summary>
    /// Add a GameObject to focus list and disable its child's collider.
    ///</summary>
    ///<param name="_obj">The GameObject to add</param>
    public void FocusItem(GameObject _obj)
    {
        bool canFocus = false;
        if (focus.Count == 0)
            canFocus = true;
        else
        {
            Transform root = focus[focus.Count - 1].transform;
            foreach (Transform child in root)
            {
                if (child.gameObject == _obj)
                    canFocus = true;
            }
        }
        if (canFocus == true)
        {
            focus.Add(_obj);
            if (_obj.GetComponent<Group>())
                _obj.GetComponent<Group>().SetAttribute("racks", "true");
            else
            {
                _obj.transform.GetChild(0).GetComponent<Collider>().enabled = false;
                _obj.GetComponent<OObject>().SetAttribute("alpha", "true");
                _obj.GetComponent<OObject>().SetAttribute("slots", "false");
            }
            UpdateFocusText();
            SetCurrentItem(_obj);
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
        if (obj.GetComponent<Group>())
            obj.GetComponent<Group>().SetAttribute("racks", "false");
        else
        {
            obj.transform.GetChild(0).GetComponent<Collider>().enabled = true;
            obj.GetComponent<OObject>().SetAttribute("alpha", "false");
            obj.GetComponent<OObject>().SetAttribute("slots", "true");
        }
        UpdateFocusText();
    }

    ///<summary>
    /// Update focusText according to focus' last item.
    ///</summary>
    private void UpdateFocusText()
    {
        if (focus.Count > 0)
        {
            string objName = focus[focus.Count - 1].GetComponent<OgreeObject>().hierarchyName;
            focusText.text = $"Focus on {objName}";
        }
        else
            focusText.text = "No focus";

        AppendLogLine(focusText.text, "green");
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
    /// Call GUIObjectInfos 'UpdateFields' method according to currentItems.Count.
    ///</summary>
    public void UpdateGuiInfos()
    {
        if (currentItems.Count == 0)
            objInfos.UpdateSingleFields(null);
        else if (currentItems.Count == 1)
            objInfos.UpdateSingleFields(currentItems[0]);
        else
            objInfos.UpdateMultiFields(currentItems);
    }

    ///<summary>
    /// Display a message in the CLI.
    ///</summary>
    ///<param name="_line">The text to display</param>
    ///<param name="_color">The color of the text. Default is white</param>
    public void AppendLogLine(string _line, string _color = "white")
    {
        consoleController.AppendLogLine(_line, _color);
    }

    ///<summary>
    /// Store a path to a command file. Turn on or off the reload button if there is a path or not.
    ///</summary>
    ///<param name="_lastPath">The command file path to store</param>
    public void SetReloadBtn(string _lastPath)
    {
        lastCmdFilePath = _lastPath;
        reloadBtn.interactable = (!string.IsNullOrEmpty(lastCmdFilePath));

    }

    ///<summary>
    /// Called by GUI button: Delete all Tenants and reload last loaded file.
    ///</summary>
    public void ReloadFile()
    {
        SetCurrentItem(null);
        focus.Clear();
        UpdateFocusText();

        List<GameObject> tenants = new List<GameObject>();
        foreach (DictionaryEntry de in allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>()?.category == "tenant")
                tenants.Add(go);
        }
        for (int i = 0; i < tenants.Count; i++)
            Destroy(tenants[i]);

        foreach (var kpv in objectTemplates)
            Destroy(kpv.Value);
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
    /// Called by GUI button: If currentItem is a room, toggle tiles name.
    ///</summary>
    public void ToggleTilesName()
    {
        if (currentItems.Count == 0)
        {
            AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Room currentRoom = currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            currentRoom.ToggleTilesName();
            AppendLogLine($"Tiles name toggled for {currentItems[0].name}.", "yellow");
        }
        else
            AppendLogLine("Selected item must be a room", "red");
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles color.
    ///</summary>
    public void ToggleTilesColor()
    {
        if (currentItems.Count == 0)
        {
            AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Room currentRoom = currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            if (!roomTemplates.ContainsKey(currentRoom.attributes["template"]))
            {
                GameManager.gm.AppendLogLine($"There is no template for {currentRoom.name}", "yellow");
                return;
            }
            currentRoom.ToggleTilesColor();
            AppendLogLine($"Tiles color toggled for {currentItems[0].name}.", "yellow");
        }
        else
            AppendLogLine("Selected item must be a room", "red");
    }

    ///<summary>
    /// Called by GUI button: if currentItem is a rack, toggle U helpers.
    ///</summary>
    public void ToggleUHelpers()
    {
        if (currentItems.Count == 0)
        {
            AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Rack rack = currentItems[0].GetComponent<Rack>();
        if (rack)
        {
            rack.ToggleU();
            AppendLogLine($"U helpers toggled for {currentItems[0].name}.", "yellow");
        }
        else
            AppendLogLine("Selected item must be a rack.", "red");
    }

    ///<summary>
    /// Called by GUI: foreach Object in currentItems, toggle local Coordinate System.
    ///</summary>
    public void GuiToggleCS()
    {
        if (currentItems.Count == 0)
        {
            AppendLogLine("Empty selection.", "yellow");
            return;
        }

        foreach (GameObject obj in currentItems)
        {
            if (obj.GetComponent<OObject>())
                obj.GetComponent<OObject>().ToggleCS();
        }
    }

    ///<summary>
    /// Called by GUI checkbox.
    /// Change material of all Racks.
    ///</summary>
    ///<param name="_value">The checkbox value</param>
    public void ToggleRacksMaterials(bool _value)
    {
        toggleWireframe.isOn = _value;
        isWireframe = _value;
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            string cat = obj.GetComponent<OgreeObject>()?.category;
            if (cat == "rack" || cat == "rackGroup" || cat == "corridor")
                SetRackMaterial(obj.transform);
        }
    }

    ///<summary>
    /// Set material of a rack according to isWireframe value.
    ///</summary>
    ///<param name="_rack">The rack to set the material</param>
    public void SetRackMaterial(Transform _rack)
    {
        Renderer r = _rack.GetChild(0).GetComponent<Renderer>();
        Color color = r.material.color;
        if (isWireframe)
            r.material = GameManager.gm.wireframeMat;
        else
            r.material = GameManager.gm.defaultMat;
        r.material.color = color;
    }

    ///<summary>
    /// Set animator triger of _panel according to its current state and _value
    ///</summary>
    ///<param name="_panel">The panel to modify</param>
    ///<param name="_value">Should the panel be "on"?</param>
    public void MovePanel(string _panel, bool _value)
    {
        Animator anim = null;
        if (_panel == "infos")
            anim = infosPanel.GetComponent<Animator>();
        else if (_panel == "debug")
            anim = debugPanel.GetComponent<Animator>();

        if ((_value == true && anim.GetCurrentAnimatorStateInfo(0).IsName("PanelOff"))
            || (_value == false && anim.GetCurrentAnimatorStateInfo(0).IsName("PanelOn")))
            anim.SetTrigger("Transition");
    }

    ///<summary>
    /// Quit the application.
    ///</summary>
    public void QuitApp()
    {
        Application.Quit();
    }

}
