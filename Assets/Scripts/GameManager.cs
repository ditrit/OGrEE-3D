using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;
    private ConsoleController consoleController;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI currentItemText = null;
    [SerializeField] private Button reloadBtn = null;
    [SerializeField] private Camera currentCam = null;
    [SerializeField] private GUIObjectInfos objInfos = null;
    public Material defaultMat;
    public Material wireframeMat;

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.045f;
    public float ouSize = 0.048f;

    [Header("Models")]
    public GameObject tileModel;
    public GameObject buildingModel;
    public GameObject roomModel;
    public GameObject rackModel;
    public GameObject serverModel;
    public GameObject deviceModel;
    public GameObject tileNameModel;

    [Header("Runtime data")]
    public string lastCmdFilePath;
    public Transform templatePlaceholder;
    public GameObject currentItem /*{ get; private set; }*/ = null;
    public Hashtable allItems = new Hashtable();
    public Dictionary<string, GameObject> rackTemplates = new Dictionary<string, GameObject>();
    public Dictionary<string, Tenant> tenants = new Dictionary<string, Tenant>();

    #region UnityMethods

    private void Awake()
    {
        if (!gm)
            gm = this;
        else
            Destroy(this);
        consoleController = GameObject.FindObjectOfType<ConsoleView>().console;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (!EventSystem.current.IsPointerOverGameObject()
            && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Physics.Raycast(currentCam.transform.position, currentCam.ScreenPointToRay(Input.mousePosition).direction, out hit);
            if (hit.collider && hit.collider.tag == "Selectable")
            {
                // Debug.Log(hit.collider.transform.parent.name);
                SetCurrentItem(hit.collider.transform.parent.gameObject);
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
        // HierarchyName[] objs = FindObjectsOfType<HierarchyName>();
        // Debug.Log($"Looking for {_path} in {objs.Length} objects");
        // for (int i = 0; i < objs.Length; i++)
        // {
            // Debug.Log($"'{objs[i].fullname}' vs '{_path}'");
        //     if (objs[i].fullname == _path)
        //         return objs[i].gameObject;
        // }
        // return null;
    }

    ///<summary>
    /// Save current object and change the CLI idle text.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    public void SetCurrentItem(GameObject _obj)
    {
        currentItem = _obj;
        if (_obj)
            currentItemText.text = currentItem.GetComponent<HierarchyName>().fullname;
        else
            currentItemText.text = "Ogree3D";
        objInfos.UpdateFields(currentItem);
    }

    ///<summary>
    /// Delete a GameObject, set currentItem to null.
    ///</summary>
    ///<param name="_toDel">The object to delete</param>
    public void DeleteItem(GameObject _toDel)
    {
        // Debug.Log($"Try to delete {_toDel.name}");
        // if (_toDel == currentItem || _toDel?.transform.Find(currentItem.name))
        SetCurrentItem(null);

        // Should count type of deleted objects
        allItems.Remove(_toDel.GetComponent<HierarchyName>().fullname);
        Destroy(_toDel);
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
        Debug.LogWarning("GM.ReloadFile()...");
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (Customer cu in customers)
            Destroy(cu.gameObject);
        tenants.Clear();
        // allItems.Clear();
        // consoleController.RunCommandString($".cmds:{lastCmdFilePath}");
        StartCoroutine(LoadFile());
    }

    ///<summary>
    /// Coroutine for waiting until end of frame to trigger all OnDestroy() methods before loading file
    ///</summary>
    private IEnumerator LoadFile()
    {
        yield return new WaitForEndOfFrame();
        consoleController.RunCommandString($".cmds:{lastCmdFilePath}");
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles name.
    ///</summary>
    public void ToggleTileNames()
    {
        Room currentRoom = currentItem.GetComponent<Room>();
        if (currentRoom)
        {
            currentRoom.ToggleTilesName();
            AppendLogLine($"Tiles names toggled for {currentItem.name}.", "yellow");
        }
        else
            AppendLogLine("Current item must be a room", "red");
    }

    ///<summary>
    /// Called by GUI checkbox.
    /// Change material of all Racks.
    ///</summary>
    ///<param name="_value">The checkbox value</param>
    public void ToggleRacksMaterials(bool _value)
    {
        Rack[] racks = GameObject.FindObjectsOfType<Rack>();
        foreach (Rack rack in racks)
        {
            Renderer r = rack.transform.GetChild(0).GetComponent<Renderer>();
            Color color = r.material.color;
            if (_value)
                r.material = GameManager.gm.wireframeMat;
            else
                r.material = GameManager.gm.defaultMat;
            r.material.color = color;
        }
    }


    ///<summary>
    /// Add a key/value pair in a dictionary only of the key doesn't exists.
    ///</summary>
    ///<param name="_dictionary">The dictionary to modify</param>
    ///<param name="_key">The key to check/add</param>
    ///<param name="_value">The value to add</param>
    public void DictionaryAddIfUnknowned<T>(Dictionary<string, T> _dictionary, string _key, T _value)
    {
        if (!_dictionary.ContainsKey(_key))
            _dictionary.Add(_key, _value);
    }
}
