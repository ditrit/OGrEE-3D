using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    static public UiManager instance;

    public bool isEditing = false;

    [SerializeField] private GameObject menuPanel = null;

    [Header("Updated Canvas")]
    [SerializeField] private TMP_Text mouseName;

    [Header("Panel Top")]
    [SerializeField] private Button focusBtn = null;
    [SerializeField] private Button unfocusBtn = null;
    [SerializeField] private Button selectParentBtn = null;
    [SerializeField] private TMP_Text focusText = null;
    [SerializeField] private Button editBtn = null;
    [SerializeField] private TMP_Text toggleLabelsText = null;
    [SerializeField] private Button resetTransBtn = null;
    [SerializeField] private Button resetChildrenBtn = null;

    [Header("Panel Bottom")]
    [SerializeField] private Button reloadBtn = null;
    [SerializeField] private Button apiBtn = null;
    [SerializeField] private TMP_Text apiUrl = null;
    [SerializeField] private TMP_Text currentItemText = null;

    [Header("Panel Debug")]
    [SerializeField] private GameObject debugPanel = null;

    [Header("Panel Infos")]
    [SerializeField] private GameObject infosPanel = null;
    [SerializeField] private GUIObjectInfos objInfos = null;
    public DetailsInputField detailsInputField = null;

#if VR
    [Header("VR")]
    [SerializeField] private MeshRenderer apiButtonVRBackPlate = null;
#endif



    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        menuPanel.SetActive(false);
        focusBtn.interactable = false;
        unfocusBtn.interactable = false;
        editBtn.interactable = false;
        selectParentBtn.interactable = false;
        resetTransBtn.interactable = false;
        resetChildrenBtn.interactable = false;
        mouseName.gameObject.SetActive(false);

        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
            menuPanel.SetActive(!menuPanel.activeSelf);

        if (mouseName.gameObject.activeSelf)
        {
            mouseName.transform.position = Input.mousePosition;
            NameUnderMouse();
        }*/
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
#if !VR
        if (GameManager.gm.currentItems.Count == 0)
        {
            focusBtn.interactable = false;
            selectParentBtn.interactable = false;
            resetTransBtn.interactable = false;
        }
        else
        {
            focusBtn.interactable = true;
            selectParentBtn.interactable = true;
        }
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] == GameManager.gm.currentItems[0])
        {
            selectParentBtn.interactable = false;
            editBtn.interactable = true;
        }
        else
            editBtn.interactable = false;
        SetCurrentItemText();
        UpdateGuiInfos();
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent _e)
    {
        UpdateFocusText();
        unfocusBtn.interactable = true;
        resetChildrenBtn.interactable = true;
        if (_e.obj == GameManager.gm.currentItems[0])
        {
            selectParentBtn.interactable = false;
            editBtn.interactable = true;
        }
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        UpdateFocusText();
        resetChildrenBtn.interactable = false;
        if (GameManager.gm.focus.Count == 0)
            unfocusBtn.interactable = false;
    }

    ///<summary>
    /// When called, change buttons behavior.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnEditModeIn(EditModeInEvent _e)
    {
        focusBtn.interactable = false;
        selectParentBtn.interactable = false;
        resetTransBtn.interactable = true;
    }

    ///<summary>
    /// When called, change buttons behavior.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        focusBtn.interactable = true;
        selectParentBtn.interactable = true;
        resetTransBtn.interactable = false;
    }

    ///<summary>
    /// Get the object under the mouse and displays its hierarchyName in mouseName text.
    ///</summary>
    private void NameUnderMouse()
    {
        GameObject obj = Utils.RaycastFromCameraToMouse();
        if (obj && obj.GetComponent<OgreeObject>())
            mouseName.text = obj.GetComponent<OgreeObject>().hierarchyName;
        else
            mouseName.text = "";
    }

    ///<summary>
    /// Call GUIObjectInfos 'UpdateFields' method according to currentItems.Count.
    ///</summary>
    public void UpdateGuiInfos()
    {
        if (GameManager.gm.currentItems.Count == 0)
            objInfos.UpdateSingleFields(null);
        else if (GameManager.gm.currentItems.Count == 1)
            objInfos.UpdateSingleFields(GameManager.gm.currentItems[0]);
        else
            objInfos.UpdateMultiFields(GameManager.gm.currentItems);
    }

    ///<summary>
    /// Update focusText according to focus' last item.
    ///</summary>
    public void UpdateFocusText()
    {
        if (GameManager.gm.focus.Count > 0)
        {
            string objName = GameManager.gm.focus[GameManager.gm.focus.Count - 1].GetComponent<OgreeObject>().hierarchyName;
            focusText.text = $"Focus on {objName}";
        }
        else
            focusText.text = "No focus";

        GameManager.gm.AppendLogLine(focusText.text, true, eLogtype.success);
    }

    ///<summary>
    /// Change text and color of apiBtn.
    ///</summary>
    ///<param name="_str">The new text of the button</param>
    ///<param name="_color">The new color of the button</param>
    public void ChangeApiButton(string _str, Color _color)
    {
        apiBtn.GetComponentInChildren<TextMeshProUGUI>().text = _str;
        apiBtn.GetComponent<Image>().color = _color;
#if VR
        //apiButtonVRBackPlate.material.SetColor("_Color", _color);
#endif
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

    #region SetValues

    ///<summary>
    /// Set the current item text
    ///</summary>
    public void SetCurrentItemText()
    {
        if (GameManager.gm.currentItems.Count == 1)
            currentItemText.text = (GameManager.gm.currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        else if (GameManager.gm.currentItems.Count > 1)
            currentItemText.text = ("Selection");
        else
            currentItemText.text = ("OGrEE-3D");
    }

    ///<summary>
    /// Set the api text
    ///</summary>
    ///<param name="_str">The text to display</param>
    public void SetApiUrlText(string _str)
    {
        apiUrl.text = _str;
    }

    ///<summary>
    /// Make the reload button interatable or not.
    ///</summary>
    ///<param name="_value">Boolean if the button should be interatable</param>
    public void SetReloadBtn(bool _value)
    {
        reloadBtn.interactable = _value;
    }

    #endregion

    #region CalledByGUI

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles name.
    ///</summary>
    public void ToggleTilesName()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", false, eLogtype.warning);
            return;
        }

        Room currentRoom = GameManager.gm.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            currentRoom.ToggleTilesName();
            GameManager.gm.AppendLogLine($"Tiles name toggled for {GameManager.gm.currentItems[0].name}.", false, eLogtype.success);
        }
        else
            GameManager.gm.AppendLogLine("Selected item must be a room", false, eLogtype.error);
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles color.
    ///</summary>
    public void ToggleTilesColor()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", false, eLogtype.warning);
            return;
        }

        Room currentRoom = GameManager.gm.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            if (!GameManager.gm.roomTemplates.ContainsKey(currentRoom.attributes["template"]))
            {
                GameManager.gm.AppendLogLine($"There is no template for {currentRoom.name}", false, eLogtype.warning);
                return;
            }
            currentRoom.ToggleTilesColor();
            GameManager.gm.AppendLogLine($"Tiles color toggled for {GameManager.gm.currentItems[0].name}.", false, eLogtype.success);
        }
        else
            GameManager.gm.AppendLogLine("Selected item must be a room", false, eLogtype.error);
    }

    ///<summary>
    /// Called by GUI button: if currentItem is a rack, toggle U helpers.
    ///</summary>
    public void ToggleUHelpers()
    {
        UHelpersManager.um.ToggleU(GameManager.gm.currentItems[0].transform);
    }

    ///<summary>
    /// Called by GUI: foreach Object in currentItems, toggle local Coordinate System.
    ///</summary>
    public void GuiToggleCS()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", false, eLogtype.warning);
            return;
        }

        foreach (GameObject obj in GameManager.gm.currentItems)
        {
            if (obj.GetComponent<OObject>())
                obj.GetComponent<OObject>().ToggleCS();
        }
    }

    ///<summary>
    /// Called by GUI button: Connect or disconnect to API using configLoader.ConnectToApi().
    ///</summary>
    public async void ToggleApi()
    {
        if (ApiManager.instance.isInit)
        {
            ApiManager.instance.isInit = false;
            ChangeApiButton("Connect to Api", Color.white);
            apiUrl.text = "";
            GameManager.gm.AppendLogLine("Disconnected from API", true, eLogtype.success);
        }
        else
            await GameManager.gm.ConnectToApi();
    }

    ///<summary>
    /// Called by GUI button: Focus selected object.
    ///</summary>
    public async void FocusSelected()
    {
        if (GameManager.gm.currentItems.Count > 0 && GameManager.gm.currentItems[0].GetComponent<OObject>())
            await GameManager.gm.FocusItem(GameManager.gm.currentItems[0]);
    }

    ///<summary>
    /// Called by GUI button: Focus selected object.
    ///</summary>
    public async void UnfocusSelected()
    {
        if (GameManager.gm.focus.Count > 0)
            await GameManager.gm.UnfocusItem();
    }

    ///<summary>
    /// Called by GUI button: Toggle Edit on focused object.
    ///</summary>
    public void EditFocused()
    {
        if (isEditing)
        {
            isEditing = false;
            EventManager.Instance.Raise(new EditModeOutEvent() { obj = GameManager.gm.currentItems[0] });
            Debug.Log($"Edit out: {GameManager.gm.currentItems[0]}");
        }
        else
        {
            isEditing = true;
            EventManager.Instance.Raise(new EditModeInEvent() { obj = GameManager.gm.currentItems[0] });
            Debug.Log($"Edit in: {GameManager.gm.currentItems[0]}");
        }
    }

    ///<summary>
    /// Called by GUI button: Reset transforms of the selected item.
    ///</summary>
    public void ResetTransform()
    {
        GameObject obj = GameManager.gm.currentItems[0];
        if (obj)
            obj.GetComponent<OgreeObject>().ResetTransform();
    }

    ///<summary>
    /// Called by GUI button: Reset tranforms of the children of the selected item.
    ///</summary>
    public void ResetChildrenTransforms()
    {
        GameObject obj = GameManager.gm.currentItems[0];
        if (obj)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.GetComponent<OgreeObject>())
                    child.GetComponent<OgreeObject>().ResetTransform();
            }
        }
    }

    ///<summary>
    /// Called by GUI button: Select the parent of the selected object.
    ///</summary>
    public async void SelectParentItem()
    {
        if (GameManager.gm.currentItems.Count == 0)
            return;

        await GameManager.gm.SetCurrentItem(GameManager.gm.currentItems[0].transform.parent?.gameObject);
    }

    ///
    public void ToggleCLI(bool _value)
    {
        if (_value)
        {
            GameManager.gm.writeLogs = true;
            GameManager.gm.AppendLogLine("Enable CLI", false, eLogtype.success);
        }
        else
        {
            GameManager.gm.AppendLogLine("Disable CLI", false, eLogtype.success);
            GameManager.gm.writeLogs = false;
        }
    }

    ///<summary>
    /// Send a ToggleLabelEvent and change the toggle text.
    ///</summary>
    public void ToggleLabels(bool _value)
    {
        EventManager.Instance.Raise(new ToggleLabelEvent() { value = _value });
        mouseName.gameObject.SetActive(!_value);
        if (_value)
            toggleLabelsText.text = "Hide labels";
        else
            toggleLabelsText.text = "Display labels";
    }

    ///<summary>
    /// Delete all files stored in cache directory.
    ///</summary>
    public void ClearCache()
    {
        DirectoryInfo dir = new DirectoryInfo(GameManager.gm.configLoader.GetCacheDir());
        foreach (FileInfo file in dir.GetFiles())
        {
            if (file.Name != "log.txt")
                file.Delete();
        }
        GameManager.gm.AppendLogLine($"Cache cleared at \"{GameManager.gm.configLoader.GetCacheDir()}\"", true, eLogtype.success);
    }

    ///<summary>
    /// Called by GUI button: Delete all Tenants and reload last loaded file.
    ///</summary>
    public async void ReloadFile()
    {
        await GameManager.gm.SetCurrentItem(null);
        GameManager.gm.focus.Clear();
        UiManager.instance.UpdateFocusText();

        List<GameObject> tenants = new List<GameObject>();
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>()?.category == "tenant")
                tenants.Add(go);
        }
        for (int i = 0; i < tenants.Count; i++)
            Destroy(tenants[i]);
        GameManager.gm.allItems.Clear();

        foreach (KeyValuePair<string, GameObject> kvp in GameManager.gm.objectTemplates)
            Destroy(kvp.Value);
        GameManager.gm.objectTemplates.Clear();
        GameManager.gm.roomTemplates.Clear();
        GameManager.gm.consoleController.variables.Clear();
        GameManager.gm.consoleController.ResetCounts();
        StartCoroutine(LoadFile());
    }

    ///<summary>
    /// Coroutine for waiting until end of frame to trigger all OnDestroy() methods before loading file.
    ///</summary>
    private IEnumerator LoadFile()
    {
        yield return new WaitForEndOfFrame();
        GameManager.gm.consoleController.RunCommandString($".cmds:{GameManager.gm.lastCmdFilePath}");
    }
    #endregion
}
