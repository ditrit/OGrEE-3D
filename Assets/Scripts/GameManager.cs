using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;
    private ConsoleController consoleController;

    [Header("Current Item")]
    [SerializeField] private TextMeshProUGUI currentItemText = null;
    public GameObject currentItem {get; private set;} = null;

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

    public void AppendLogLine(string line, string color = "white")
    {
        consoleController.AppendLogLine(line, color);
    }
}
