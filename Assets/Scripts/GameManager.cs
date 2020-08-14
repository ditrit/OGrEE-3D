using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;
    private ConsoleController consoleController;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI currentItemText = null;
    [SerializeField] private Button reloadBtn;

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.045f;
    public float ouSize = 0.048f;

    [Header("Models")]
    public GameObject tileModel;
    public GameObject roomModel;
    public GameObject rackModel;
    public GameObject serverModel;
    public GameObject deviceModel;

    [Header("Runtime data")]
    public string lastCmdFilePath;
    public Transform templatePlaceholder;
    public GameObject currentItem /*{ get; private set; }*/ = null;
    public Dictionary<string, GameObject> rackPresets = new Dictionary<string, GameObject>();
    public Dictionary<string, Tenant> tenants = new Dictionary<string, Tenant>();

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
    }

    public GameObject FindAbsPath(string _path)
    {
        HierarchyName[] objs = FindObjectsOfType<HierarchyName>();
        // Debug.Log($"Looking for {_path} in {objs.Length} objects");
        for (int i = 0; i < objs.Length; i++)
        {
            // Debug.Log($"'{objs[i].fullname}' vs '{_path}'");
            if (objs[i].fullname == _path)
                return objs[i].gameObject;
        }

        return null;
    }

    public void SetCurrentItem(GameObject _obj)
    {
        currentItem = _obj;
        if (_obj)
            currentItemText.text = currentItem.GetComponent<HierarchyName>().fullname;
        else
            currentItemText.text = "Ogree3D";
    }

    public void DeleteItem(GameObject _toDel)
    {
        // Debug.Log($"Try to delete {_toDel.name}");
        // if (_toDel == currentItem || _toDel?.transform.Find(currentItem.name))
            SetCurrentItem(null);

        // Should count type of deleted objects
        Destroy(_toDel);
    }

    public void AppendLogLine(string line, string color = "white")
    {
        consoleController.AppendLogLine(line, color);
    }

    public void SetReloadBtn(string _lastPath)
    {
        lastCmdFilePath = _lastPath;
        reloadBtn.interactable = (!string.IsNullOrEmpty(lastCmdFilePath));

    }

    public void ReloadFile()
    {
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (Customer cu in customers)
            Destroy(cu.gameObject);
        consoleController.RunCommandString($".cmds:{lastCmdFilePath}");
    }

    public void DictionaryAddIfUnknowned<T>(Dictionary<string, T> _dictionary, string _key, T _value)
    {
        if (!_dictionary.ContainsKey(_key))
            _dictionary.Add(_key, _value);
    }
}
