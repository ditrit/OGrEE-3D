using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    static public UiManager instance;

    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject promptPrefab;

    [SerializeField] private GameObject menuPanel;

    [Header("Updated Canvas")]
    [SerializeField] private TMP_Text mouseName;

    [Header("Panel Top")]
    [SerializeField] private Button focusBtn;
    [SerializeField] private Button unfocusBtn;
    [SerializeField] private Button selectParentBtn;
    [SerializeField] private TMP_Text focusText;
    [SerializeField] private Button editBtn;
    [SerializeField] private Button resetTransBtn;
    [SerializeField] private Button resetChildrenBtn;
    [SerializeField] private Button tempDiagramBBtn;
    [SerializeField] private Button tempScatterPlotBtn;
    [SerializeField] private Button heatMapBtn;

    [Header("Panel Bottom")]
    [SerializeField] private Button reloadBtn;
    [SerializeField] private Button apiBtn;
    [SerializeField] private TMP_Text apiUrl;
    [SerializeField] private TMP_Text currentItemText;

    [Header("Panel Debug")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button toggleTilesNameBtn;
    [SerializeField] private Button toggleTilesColorBtn;
    [SerializeField] private Button toggleUHelpersBtn;
    [SerializeField] private Button toggleLocalCSBtn;

    [Header("Delay Slider")]
    [SerializeField] private ConsoleController consoleController;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI value;

    [Header("Panel Infos")]
    [SerializeField] private GameObject infosPanel;
    [SerializeField] private GUIObjectInfos objInfos;
    public DetailsInputField detailsInputField;

    public GameObject test;

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
        tempDiagramBBtn.interactable = false;
        tempScatterPlotBtn.interactable = false;
        heatMapBtn.interactable = false;
        UpdateTimerValue(slider.value);

        EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.AddListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.instance.AddListener<ConnectApiEvent>(OnApiConnected);
        EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            menuPanel.SetActive(!menuPanel.activeSelf);

        if (mouseName.gameObject.activeSelf)
        {
            mouseName.transform.position = Input.mousePosition;
            NameUnderMouse();
        }
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.instance.RemoveListener<OnFocusEvent>(OnFocusItem);
        EventManager.instance.RemoveListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.instance.RemoveListener<ConnectApiEvent>(OnApiConnected);
        EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (GameManager.instance.currentItems.Count == 0)
        {
            focusBtn.interactable = false;
            selectParentBtn.interactable = false;
            resetTransBtn.interactable = false;

            tempDiagramBBtn.interactable = false;
            tempScatterPlotBtn.interactable = false;
            heatMapBtn.interactable = false;

        }
        else if (GameManager.instance.focus.Contains(GameManager.instance.currentItems[GameManager.instance.currentItems.Count - 1]))
        {
            focusBtn.interactable = false;
            selectParentBtn.interactable = true;

            tempDiagramBBtn.interactable = true;
            tempScatterPlotBtn.interactable = true;
            heatMapBtn.interactable = true;
        }
        else
        {
            focusBtn.interactable = true;
            selectParentBtn.interactable = true;

            tempDiagramBBtn.interactable = true;
            tempScatterPlotBtn.interactable = true;
            heatMapBtn.interactable = true;
        }
        if (GameManager.instance.focus.Count > 0 && GameManager.instance.focus[GameManager.instance.focus.Count - 1] == GameManager.instance.currentItems[0])
        {
            selectParentBtn.interactable = false;
            editBtn.interactable = true;
        }
        else
            editBtn.interactable = false;

        SetCurrentItemText();
        UpdateGuiInfos();

        //ugly part : checking toggling buttons status
        ColorBlock cb;
        if (GameManager.instance.currentItems.Count > 0)
        {
            Room currentRoom = GameManager.instance.currentItems[0].GetComponent<Room>();
            if (currentRoom)
            {
                //Toggle tiles name
                cb = toggleTilesNameBtn.colors;
                if (currentRoom.transform.Find("tilesNameRoot"))
                {
                    cb.normalColor = Color.gray;
                    cb.selectedColor = Color.gray;
                }
                else
                {
                    cb.normalColor = Color.white;
                    cb.selectedColor = Color.white;
                }
                toggleTilesNameBtn.colors = cb;

                //Toggle tiles color
                cb = toggleTilesColorBtn.colors;
                if (currentRoom.transform.Find("tilesColorRoot"))
                {
                    cb.normalColor = Color.gray;
                    cb.selectedColor = Color.gray;
                }
                else
                {
                    cb.normalColor = Color.white;
                    cb.selectedColor = Color.white;
                }
                toggleTilesColorBtn.colors = cb;
            }
            else
            {
                cb = toggleTilesColorBtn.colors;
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
                toggleTilesNameBtn.colors = cb;
                cb = toggleTilesNameBtn.colors;
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
                toggleTilesColorBtn.colors = cb;
            }

            //Toggle U helpers
            Transform _transform = GameManager.instance.currentItems[0].transform;
            while (_transform != null)
            {
                if (_transform.GetComponent<Rack>())
                {
                    cb = toggleUHelpersBtn.colors;
                    Transform uRoot = _transform.GetComponent<Rack>().uRoot;
                    if (!uRoot || !uRoot.gameObject.activeSelf)
                    {
                        cb.normalColor = Color.white;
                        cb.selectedColor = Color.white;
                    }
                    else
                    {
                        cb.normalColor = Color.gray;
                        cb.selectedColor = Color.gray;
                    }
                    toggleUHelpersBtn.colors = cb;
                    break;
                }
                _transform = _transform.parent;
            }
            if (_transform == null)
            {
                cb = toggleUHelpersBtn.colors;
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
                toggleUHelpersBtn.colors = cb;
            }

            //Toggle local CS
            if (GameManager.instance.currentItems[0].GetComponent<OObject>())
            {
                cb = toggleLocalCSBtn.colors;
                if (GameManager.instance.currentItems[0].transform.Find("localCS"))
                {
                    cb.normalColor = Color.gray;
                    cb.selectedColor = Color.gray;
                }
                else
                {
                    cb.normalColor = Color.white;
                    cb.selectedColor = Color.white;
                }
                toggleLocalCSBtn.colors = cb;
            }
            else
            {
                cb = toggleLocalCSBtn.colors;
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
                toggleLocalCSBtn.colors = cb;
            }
        }
        else
        {
            cb = toggleTilesColorBtn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
            toggleTilesNameBtn.colors = cb;

            cb = toggleTilesNameBtn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
            toggleTilesColorBtn.colors = cb;

            cb = toggleUHelpersBtn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
            toggleUHelpersBtn.colors = cb;

            cb = toggleLocalCSBtn.colors;
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
            toggleLocalCSBtn.colors = cb;

        }

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
        if (_e.obj == GameManager.instance.currentItems[0])
        {
            selectParentBtn.interactable = false;
            editBtn.interactable = true;
        }
        if (GameManager.instance.currentItems.Contains(_e.obj))
            focusBtn.interactable = false;
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        UpdateFocusText();
        resetChildrenBtn.interactable = false;
        if (GameManager.instance.focus.Count == 0)
            unfocusBtn.interactable = false;
        if (GameManager.instance.currentItems.Contains(_e.obj))
            focusBtn.interactable = false;
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
        editBtn.GetComponent<Image>().color = Utils.ParseHtmlColor(GameManager.instance.configLoader.GetColor("edit"));
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
        editBtn.GetComponent<Image>().color = Color.white;
    }

    ///<summary>
    /// When called, update apiBtn and apiUrl. 
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnApiConnected(ConnectApiEvent _e)
    {
        if (ApiManager.instance.isInit)
        {
            ChangeApiButton("Connected to Api", Color.green);
            apiUrl.text = "Connected to " + ApiManager.instance.GetApiUrl();
            apiUrl.color = Color.green;
        }
        else
        {
            ChangeApiButton("Fail to connected to Api", Color.red);
            apiUrl.text = "Fail to connected to " + ApiManager.instance.GetApiUrl();
            apiUrl.color = Color.red;
        }
    }

    ///<summary>
    /// When called, update the detailsInputField according to the first selected item
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent _e)
    {
        if (GameManager.instance.currentItems.Count > 0)
        {
            string value = GameManager.instance.currentItems[0].GetComponent<OgreeObject>().currentLod.ToString();
            detailsInputField.UpdateInputField(value);
        }
        else
            detailsInputField.UpdateInputField("0");
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
        if (GameManager.instance.currentItems.Count == 0)
            objInfos.UpdateSingleFields(null);
        else if (GameManager.instance.currentItems.Count == 1)
            objInfos.UpdateSingleFields(GameManager.instance.currentItems[0]);
        else
            objInfos.UpdateMultiFields(GameManager.instance.currentItems);
    }

    ///<summary>
    /// Update focusText according to focus' last item.
    ///</summary>
    public void UpdateFocusText()
    {
        if (GameManager.instance.focus.Count > 0)
        {
            string objName = GameManager.instance.focus[GameManager.instance.focus.Count - 1].GetComponent<OgreeObject>().hierarchyName;
            focusText.text = $"Focus on {objName}";
        }
        else
            focusText.text = "No focus";

        GameManager.instance.AppendLogLine(focusText.text, true, ELogtype.success);
    }

    ///<summary>
    /// Generate a prompt message with 1 or 2 buttons
    ///</summary>
    ///<param name="_mainText">Message to display</param>
    ///<param name="_buttonAText">Custom text for "accept" button</param>
    ///<param name="_buttonBText">Custom text for "refuse" button. The button will be hidden if empty</param>
    ///<returns>The Prompt class of the generated item</returns>
    public Prompt GeneratePrompt(string _mainText, string _buttonAText, string _buttonBText)
    {
        Prompt prompt = Instantiate(promptPrefab, canvas).GetComponent<Prompt>();
        prompt.Setup(_mainText, _buttonAText, _buttonBText);
        return prompt;
    }

    ///<summary>
    /// Delete the given Prompt
    ///</summary>
    ///<param name="_prompt">The Prompt to delete</param>
    public void DeletePrompt(Prompt _prompt)
    {
        _prompt.gameObject.SetActive(false);
        Destroy(_prompt.gameObject);
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
        if (GameManager.instance.currentItems.Count == 1)
            currentItemText.text = (GameManager.instance.currentItems[0].GetComponent<OgreeObject>().hierarchyName);
        else if (GameManager.instance.currentItems.Count > 1)
            currentItemText.text = ("Selection");
        else
            currentItemText.text = ("OGrEE-3D");
    }

    ///<summary>
    /// Make the reload button interatable or not.
    ///</summary>
    ///<param name="_value">If the button should be interatable</param>
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
        if (GameManager.instance.currentItems.Count == 0)
        {
            GameManager.instance.AppendLogLine("Empty selection.", false, ELogtype.warning);
            return;
        }

        Room currentRoom = GameManager.instance.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            currentRoom.ToggleTilesName();
            ColorBlock cb = toggleTilesNameBtn.colors;
            if (GameManager.instance.currentItems[0].transform.Find("tilesNameRoot") && GameManager.instance.currentItems[0].transform.Find("tilesNameRoot").gameObject.activeSelf)
            {
                cb.normalColor = Color.gray;
                cb.selectedColor = Color.gray;
            }
            else
            {
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
            }
            toggleTilesNameBtn.colors = cb;
            GameManager.instance.AppendLogLine($"Tiles name toggled for {GameManager.instance.currentItems[0].name}.", false, ELogtype.success);
        }
        else
            GameManager.instance.AppendLogLine("Selected item must be a room", false, ELogtype.error);
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles color.
    ///</summary>
    public void ToggleTilesColor()
    {
        if (GameManager.instance.currentItems.Count == 0)
        {
            GameManager.instance.AppendLogLine("Empty selection.", false, ELogtype.warning);
            return;
        }

        Room currentRoom = GameManager.instance.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            if (!GameManager.instance.roomTemplates.ContainsKey(currentRoom.attributes["template"]))
            {
                GameManager.instance.AppendLogLine($"There is no template for {currentRoom.name}", false, ELogtype.warning);
                return;
            }
            currentRoom.ToggleTilesColor();
            ColorBlock cb = toggleTilesColorBtn.colors;
            if (currentRoom.transform.Find("tilesColorRoot") && GameManager.instance.currentItems[0].transform.Find("tilesColorRoot").gameObject.activeSelf)
            {
                cb.normalColor = Color.gray;
                cb.selectedColor = Color.gray;
            }
            else
            {
                cb.normalColor = Color.white;
                cb.selectedColor = Color.white;
            }
            toggleTilesColorBtn.colors = cb;
            GameManager.instance.AppendLogLine($"Tiles color toggled for {GameManager.instance.currentItems[0].name}.", false, ELogtype.success);
        }
        else
            GameManager.instance.AppendLogLine("Selected item must be a room", false, ELogtype.error);
    }

    ///<summary>
    /// Called by GUI button: if currentItem is a rack, toggle U helpers.
    ///</summary>
    public void ToggleUHelpers()
    {
        if (GameManager.instance.currentItems.Count > 0)
        {
            UHelpersManager.instance.ToggleU(GameManager.instance.currentItems[0].transform);
            Transform _transform = GameManager.instance.currentItems[0].transform;
            while (_transform != null)
            {
                if (_transform.GetComponent<Rack>())
                {
                    ColorBlock cb = toggleUHelpersBtn.colors;
                    Transform uRoot = _transform.GetComponent<Rack>().uRoot;
                    if (!uRoot || !uRoot.gameObject.activeSelf)
                    {
                        cb.normalColor = Color.white;
                        cb.selectedColor = Color.white;
                    }
                    else
                    {
                        cb.normalColor = Color.gray;
                        cb.selectedColor = Color.gray;
                    }
                    toggleUHelpersBtn.colors = cb;
                    return;
                }
                _transform = _transform.parent;
            }
        }
    }

    ///<summary>
    /// Called by GUI: foreach Object in currentItems, toggle local Coordinate System.
    ///</summary>
    public void GuiToggleCS()
    {
        if (GameManager.instance.currentItems.Count == 0)
        {
            GameManager.instance.AppendLogLine("Empty selection.", false, ELogtype.warning);
            return;
        }

        foreach (GameObject obj in GameManager.instance.currentItems)
            obj.GetComponent<OObject>()?.ToggleCS();

        ColorBlock cb = toggleLocalCSBtn.colors;
        if (GameManager.instance.currentItems[0].transform.Find("localCS") && GameManager.instance.currentItems[0].transform.Find("localCS").gameObject.activeSelf)
        {
            cb.normalColor = Color.gray;
            cb.selectedColor = Color.gray;
        }
        else
        {
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
        }
        toggleLocalCSBtn.colors = cb;
    }

    ///<summary>
    /// Called by GUI button: Connect or disconnect to API using ApiManager.Initialize().
    ///</summary>
    public async void ToggleApi()
    {
        if (ApiManager.instance.isInit)
        {
            ApiManager.instance.isInit = false;
            ChangeApiButton("Connect to Api", Color.white);
            apiUrl.text = "";
            GameManager.instance.AppendLogLine("Disconnected from API", true, ELogtype.success);
        }
        else
            await ApiManager.instance.Initialize();
    }

    ///<summary>
    /// Called by GUI button: Focus selected object.
    ///</summary>
    public async void FocusSelected()
    {
        if (GameManager.instance.currentItems.Count > 0 && GameManager.instance.currentItems[0].GetComponent<OObject>())
            await GameManager.instance.FocusItem(GameManager.instance.currentItems[0]);
    }

    ///<summary>
    /// Called by GUI button: Focus selected object.
    ///</summary>
    public async void UnfocusSelected()
    {
        if (GameManager.instance.focus.Count > 0)
            await GameManager.instance.UnfocusItem();
    }

    ///<summary>
    /// Called by GUI button: Toggle Edit on focused object.
    ///</summary>
    public void EditFocused()
    {
        if (GameManager.instance.editMode)
        {
            GameManager.instance.editMode = false;
            EventManager.instance.Raise(new EditModeOutEvent() { obj = GameManager.instance.currentItems[0] });
            Debug.Log($"Edit out: {GameManager.instance.currentItems[0]}");
        }
        else
        {
            GameManager.instance.editMode = true;
            EventManager.instance.Raise(new EditModeInEvent() { obj = GameManager.instance.currentItems[0] });
            Debug.Log($"Edit in: {GameManager.instance.currentItems[0]}");
        }
    }

    ///<summary>
    /// Called by GUI button: Reset transforms of the selected item.
    ///</summary>
    public void ResetTransform()
    {
        GameObject obj = GameManager.instance.currentItems[0];
        if (obj)
            obj.GetComponent<OgreeObject>().ResetTransform();
    }

    ///<summary>
    /// Called by GUI button: Reset tranforms of the children of the selected item.
    ///</summary>
    public void ResetChildrenTransforms()
    {
        GameObject obj = GameManager.instance.currentItems[0];
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
        if (GameManager.instance.currentItems.Count == 0)
            return;

        await GameManager.instance.SetCurrentItem(GameManager.instance.currentItems[0].transform.parent?.gameObject);
    }

    ///<summary>
    /// Toggle build-in CLI writing.
    ///</summary>
    ///<param name="_value">The toggle value</param>
    public void ToggleCLI(bool _value)
    {
        if (_value)
        {
            GameManager.instance.writeLogs = true;
            GameManager.instance.AppendLogLine("Enable CLI", false, ELogtype.success);
        }
        else
        {
            GameManager.instance.AppendLogLine("Disable CLI", false, ELogtype.success);
            GameManager.instance.writeLogs = false;
        }
    }

    ///<summary>
    /// Send a ToggleLabelEvent and change the toggle text.
    ///</summary>
    ///<param name="_value">The toggle value</param>
    public void ToggleLabels(int _value)
    {
        EventManager.instance.Raise(new ToggleLabelEvent() { value = (ELabelMode)_value });
        mouseName.gameObject.SetActive(_value == 2);
    }

    ///<summary>
    /// Called by GUI button: Delete all files stored in cache directory.
    ///</summary>
    public void ClearCache()
    {
        DirectoryInfo dir = new DirectoryInfo(GameManager.instance.configLoader.GetCacheDir());
        foreach (FileInfo file in dir.GetFiles())
        {
            if (file.Name != "log.txt")
                file.Delete();
        }
        GameManager.instance.AppendLogLine($"Cache cleared at \"{GameManager.instance.configLoader.GetCacheDir()}\"", true, ELogtype.success);
        GameManager.instance.PurgeTemplates();
    }

    ///<summary>
    /// Called by GUI button: Delete all Tenants and reload last loaded file.
    ///</summary>
    public async void ReloadFile()
    {
        await GameManager.instance.SetCurrentItem(null);
        GameManager.instance.focus.Clear();
        UpdateFocusText();

        await GameManager.instance.PurgeTenants();
        GameManager.instance.allItems.Clear();
        GameManager.instance.PurgeTemplates();
        GameManager.instance.consoleController.variables.Clear();
        GameManager.instance.consoleController.ResetCounts();
        StartCoroutine(LoadFile());
    }

    ///<summary>
    /// Coroutine for waiting until end of frame to trigger all OnDestroy() methods before loading file.
    ///</summary>
    private IEnumerator LoadFile()
    {
        yield return new WaitForEndOfFrame();
        GameManager.instance.consoleController.RunCommandString($".cmds:{GameManager.instance.lastCmdFilePath}");
    }

    ///<summary>
    /// Called by GUI button: if one and only one room if selected, toggle its bar chart.
    ///</summary>
    public async void ToggleTempBarChart()
    {
        if (GameManager.instance.currentItems.Count == 1 && GameManager.instance.currentItems[0].GetComponent<Room>())
            TempDiagram.instance.HandleTempBarChart(GameManager.instance.currentItems[0].GetComponent<Room>());
        else if (GameManager.instance.currentItems.Count > 0 && GameManager.instance.currentItems[0].GetComponent<OgreeObject>().category == "tempBar")
        {
            TempDiagram.instance.HandleTempBarChart(TempDiagram.instance.lastRoom);
            await GameManager.instance.SetCurrentItem(null);
        }
        else
            GameManager.instance.AppendLogLine("You have to select one and only one room", true, ELogtype.warning);
    }

    ///<summary>
    /// Called by GUI button: toggle temperature color mode.
    ///</summary>
    public void TempColorMode(bool _value)
    {
        GameManager.instance.tempMode = _value;
        EventManager.instance.Raise(new TemperatureColorEvent());
        UpdateGuiInfos();
    }


    ///<summary>
    /// Called by GUI button: if one and only one room or OObject is seleted, toggle its sensor scatter plot
    ///</summary>
    public void ToggleTempScatterPlot()
    {
        if (GameManager.instance.currentItems.Count == 1 && (GameManager.instance.currentItems[0].GetComponent<OObject>() || GameManager.instance.currentItems[0].GetComponent<OgreeObject>().category == "room"))
            TempDiagram.instance.HandleScatterPlot(GameManager.instance.currentItems[0].GetComponent<OgreeObject>());
        else
            GameManager.instance.AppendLogLine("You have to select one and only one room, rack or device", true, ELogtype.warning);
    }


    ///<summary>
    /// Called by GUI button: if one and only one device is selected and it has no child or its children have no child, toggle its heatmap
    ///</summary>
    public void ToggleHeatMap()
    {
        if (GameManager.instance.currentItems.Count == 1)
        {
            OObject oObject = GameManager.instance.currentItems[0].GetComponent<OObject>();
            if (oObject && oObject.category == "device")
            {
                if (DepthCheck(GameManager.instance.currentItems[0].GetComponent<OgreeObject>()) <= 1)
                    TempDiagram.instance.HandleHeatMap(GameManager.instance.currentItems[0].GetComponent<OObject>());
                else
                    GameManager.instance.AppendLogLine("This device has too many nested children levels", true, ELogtype.warning);
            }
            else
                GameManager.instance.AppendLogLine("You have to select a device", true, ELogtype.warning);
        }
        else
            GameManager.instance.AppendLogLine("You have to select one device", true, ELogtype.warning);
    }

    ///<summary>
    /// Attached to GUI Slider. Change value of ConsoleController.timerValue. Also update text field.
    ///</summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateTimerValue(float _value)
    {
        slider.value = _value;
        consoleController.timerValue = _value;
        GameManager.instance.server.timer = (int)(_value);
        value.text = _value.ToString("0.##") + "s";
    }

    /// <summary>
    /// Recursively compute the depth of an object
    /// </summary>
    /// <param name="_ogreeObject">the object we're starting at</param>
    /// <returns>the highest number of nested children it has : 0 if it has no child, 1 if it has at least one child without child, 2 if its child has at least one child...</returns>
    private int DepthCheck(OgreeObject _ogreeObject)
    {
        int depth = 0;
        foreach (Transform child in _ogreeObject.gameObject.transform)
        {
            OgreeObject childOgree = child.GetComponent<OgreeObject>();
            if (childOgree)
                depth = Mathf.Max(depth, DepthCheck(childOgree) + 1);
        }
        return depth;
    }

    ///<summary>
    /// Called by GUI button
    ///</summary>
    public void FocusHandlerUpdateArrayButtonPressed()
    {
        EventManager.instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Quit the application.
    ///</summary>
    public void QuitApp()
    {
        Application.Quit();
    }

    #endregion
}
