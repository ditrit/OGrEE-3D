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

    [Header("References")]
    [SerializeField] private TextMeshProUGUI currentItemText = null;
    [SerializeField] private Button reloadBtn = null;
    [SerializeField] private Camera currentCam = null;
    [SerializeField] private GUIObjectInfos objInfos = null;
    
    [Header("Panels")]
    [SerializeField] private GameObject menu = null;
    [SerializeField] private GameObject infosPanel = null;
    [SerializeField] private GameObject debugPanel = null;

    [Header("Materials")]
    public Material defaultMat;
    public Material wireframeMat;
    public Material perfMat;

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
    // Group all dictionaries?
    public Dictionary<string, GameObject> rackTemplates = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> devicesTemplates = new Dictionary<string, GameObject>();
    public Dictionary<string, Tenant> tenants = new Dictionary<string, Tenant>();
    public bool isWireframe;

    #region UnityMethods

    private void Awake()
    {
        if (!gm)
            gm = this;
        else
            Destroy(this);
        // consoleController = GameObject.FindObjectOfType<ConsoleController>();
    }

    private void Start()
    {
        //https://forum.unity.com/threads/pass-custom-parameters-to-standalone-on-launch.429144/
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length == 2)
            consoleController.RunCommandString($".cmds:{args[1]}");

#if DEBUG
        consoleController.RunCommandString(".cmds:K:\\_Orness\\Nextcloud\\Ogree\\4_customers\\__DEMO__\\testCmds.txt");
        // consoleController.RunCommandString(".cmds:K:\\_Orness\\Nextcloud\\Ogree\\4_customers\\__EDF__\\EDF_EXAION.ocli");
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            menu.SetActive(!menu.activeSelf);

        // if (Input.GetKeyDown(KeyCode.C))
        //     Debug.Log(allItems.Count);

        if (!EventSystem.current.IsPointerOverGameObject()
            && Input.GetMouseButtonUp(0))
        {
            if (!GetComponent<MoveObject>().hasDrag)
            {
                RaycastHit hit;
                Physics.Raycast(currentCam.transform.position, currentCam.ScreenPointToRay(Input.mousePosition).direction, out hit);
                if (hit.collider && hit.collider.tag == "Selectable")
                {
                    // Debug.Log(hit.collider.transform.parent.name);
                    if (Input.GetKey(KeyCode.LeftControl))
                        UpdateCurrentItems(hit.collider.transform.parent.gameObject);
                    else
                        SetCurrentItem(hit.collider.transform.parent.gameObject);
                }
                else if (hit.collider == null || (hit.collider && hit.collider.tag != "Selectable"))
                {
                    if (currentItems.Count > 0)
                        AppendLogLine("Empty selection.", "green");
                    SetCurrentItem(null);
                }
            }
        }
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
    public void SetCurrentItem(GameObject _obj)
    {
        foreach (GameObject item in currentItems)
        {
            if (item && item.GetComponent<Object>())
            {
                cakeslice.Outline ol = item.transform.GetChild(0).GetComponent<cakeslice.Outline>();
                if (ol)
                    ol.eraseRenderer = true;
            }
        }
        currentItems.Clear();
        if (_obj)
        {
            currentItems.Add(_obj);
            currentItemText.text = currentItems[0].GetComponent<HierarchyName>().fullname;
            if (_obj.GetComponent<Object>())
            {
                cakeslice.Outline ol = _obj.transform.GetChild(0).GetComponent<cakeslice.Outline>();
                if (ol)
                    ol.eraseRenderer = false;
            }
            AppendLogLine($"Select {_obj.name}.", "green");
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
            || (currentItems[0].GetComponent<Object>() && !_obj.GetComponent<Object>()))
        {
            AppendLogLine("Multiple selection should be same type of objects.", "yellow");
            return;
        }
        if (currentItems.Contains(_obj))
        {
            AppendLogLine($"Remove {_obj.name} from selection.", "green");
            currentItems.Remove(_obj);
            if (_obj.GetComponent<Object>())
            {
                cakeslice.Outline ol = _obj.transform.GetChild(0).GetComponent<cakeslice.Outline>();
                if (ol)
                    ol.eraseRenderer = true;
            }
        }
        else
        {
            AppendLogLine($"Add {_obj.name} to selection.", "green");
            currentItems.Add(_obj);
            if (_obj.GetComponent<Object>())
            {
                cakeslice.Outline ol = _obj.transform.GetChild(0).GetComponent<cakeslice.Outline>();
                if (ol)
                    ol.eraseRenderer = false;
            }
        }

        if (currentItems.Count > 1)
            currentItemText.text = "Selection";
        else if (currentItems.Count == 1)
            currentItemText.text = currentItems[0].GetComponent<HierarchyName>().fullname;
        else
            currentItemText.text = "Ogree3D";

        UpdateGuiInfos();
    }

    ///<summary>
    /// Delete a GameObject, set currentItem to null.
    ///</summary>
    ///<param name="_toDel">The object to delete</param>
    public void DeleteItem(GameObject _toDel)
    {
        SetCurrentItem(null);

        // Should count type of deleted objects
        allItems.Remove(_toDel.GetComponent<HierarchyName>().fullname);
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
    /// Called by GUI button: Delete all Customers and reload last loaded file.
    ///</summary>
    public void ReloadFile()
    {
        SetCurrentItem(null);
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (Customer cu in customers)
            Destroy(cu.gameObject);
        tenants.Clear();
        foreach (var kpv in rackTemplates)
            Destroy(kpv.Value);
        rackTemplates.Clear();
        foreach (var kpv in devicesTemplates)
            Destroy(kpv.Value);
        devicesTemplates.Clear();
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
            if (!roomTemplates.ContainsKey(currentRoom.template))
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
            if (obj.GetComponent<Object>())
                obj.GetComponent<Object>().ToggleCS();
        }
    }

    ///<summary>
    /// Called by GUI checkbox.
    /// Change material of all Racks.
    ///</summary>
    ///<param name="_value">The checkbox value</param>
    public void ToggleRacksMaterials(bool _value)
    {
        isWireframe = _value;
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Rack>())
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

    ///<summary>
    /// Add a key/value pair in a dictionary only of the key doesn't exists.
    ///</summary>
    ///<param name="_dictionary">The dictionary to modify</param>
    ///<param name="_key">The key to check/add</param>
    ///<param name="_value">The value to add</param>
    public void DictionaryAddIfUnknown<T>(Dictionary<string, T> _dictionary, string _key, T _value)
    {
        if (!_dictionary.ContainsKey(_key))
            _dictionary.Add(_key, _value);
    }
}
