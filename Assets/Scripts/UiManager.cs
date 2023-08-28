using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    [SerializeField] private GameObject coordSystem;

    [Header("Right Click Menu")]
    [SerializeField] private GameObject rightClickMenu;
    [SerializeField] private Color selectColor;
    [SerializeField] private Color defaultColor;
    public GameObject menuTarget;

    [Header("Button Handlers")]
    [SerializeField] private ButtonHandler selectBtn;
    [SerializeField] private ButtonHandler addSelectBtn;
    [SerializeField] private ButtonHandler removeSelectBtn;
    [SerializeField] private ButtonHandler selectParentBtn;
    [SerializeField] private ButtonHandler OpenGroupBtn;
    [SerializeField] private ButtonHandler focusBtn;
    [SerializeField] private ButtonHandler unfocusBtn;
    [SerializeField] private ButtonHandler editBtn;
    [SerializeField] private ButtonHandler resetTransBtn;
    [SerializeField] private ButtonHandler resetChildrenBtn;
    [SerializeField] private ButtonHandler getCoordsBtn;
    [SerializeField] private ButtonHandler toggleTilesNameBtn;
    [SerializeField] private ButtonHandler toggleTilesColorBtn;
    [SerializeField] private ButtonHandler toggleWallsBtn;
    [SerializeField] private ButtonHandler toggleUHelpersBtn;
    [SerializeField] private ButtonHandler toggleLocalCSBtn;
    [SerializeField] private ButtonHandler toggleClearanceBtn;
    [SerializeField] private ButtonHandler barChartBtn;
    [SerializeField] private ButtonHandler scatterPlotBtn;
    [SerializeField] private ButtonHandler heatMapBtn;

    [Header("Panel Top")]
    [SerializeField] private TMP_InputField selectionInputField;
    [SerializeField] private TMP_InputField focusInputField;
    [SerializeField] private TMP_Text apiText;
    [SerializeField] private TMP_Text apiInfos;
    public TMP_Dropdown labelsDropdown;

    [Header("Panel Debug")]
    [SerializeField] private GameObject debugPanel;

    [Header("Delay Slider")]
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI value;

    [Header("Panel Infos")]
    [SerializeField] private GameObject infosPanel;
    [SerializeField] private GUIObjectInfos objInfos;
    [SerializeField] private DetailsInputField detailsInputField;

    [Header("Logger")]
    [SerializeField] private TMP_InputField loggerText;
    [SerializeField] private Scrollbar loggerSB;
    private const int loggerSize = 100;
    private Queue<string> loggerQueue = new Queue<string>(loggerSize);

    [Header("Groups")]
    [SerializeField] private GameObject groupsMenu;
    [SerializeField] private TMP_Text groupsMenuBtnText;
    private bool expendGroupsMenu = false;
    [SerializeField] private GameObject groupBtnPrefab;
    public List<Group> openedGroups;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        loggerText.lineLimit = loggerSize;
    }

    /// <summary>
    /// Set up all UI buttons via ButtonHandler and add events' listeners
    /// </summary>
    private void Start()
    {
        selectBtn = new ButtonHandler(selectBtn.button, false)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.GetSelected().Contains(menuTarget)
        };
        selectBtn.Check();

        addSelectBtn = new ButtonHandler(addSelectBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            menuTarget
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.GetSelected().Contains(menuTarget)
            &&
            GameManager.instance.GetSelected().Count > 0
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            GameManager.instance.SelectIs<OgreeObject>(menuTarget.GetComponent<OgreeObject>().category)
        };
        addSelectBtn.Check();

        removeSelectBtn = new ButtonHandler(removeSelectBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            menuTarget
            &&
            !GameManager.instance.editMode
            &&
            GameManager.instance.GetSelected().Contains(menuTarget)
            &&
            (
                !GameManager.instance.focusMode
                ||
                GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] != menuTarget
            )
        };
        removeSelectBtn.Check();

        focusBtn = new ButtonHandler(focusBtn.button, false)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            !GameManager.instance.editMode
            &&
            menuTarget
            &&
            menuTarget.GetComponent<OObject>()
            &&
            menuTarget.GetComponent<OgreeObject>().category != Category.Corridor
            &&
            !GameManager.instance.GetFocused().Contains(menuTarget)
            &&
            !menuTarget.GetComponent<Group>()
        };
        focusBtn.Check();

        unfocusBtn = new ButtonHandler(unfocusBtn.button, true)
        {
            interactCondition = () => GameManager.instance.focusMode
            &&
            !GameManager.instance.editMode
        };
        unfocusBtn.Check();

        selectParentBtn = new ButtonHandler(selectParentBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            GameManager.instance.selectMode
            &&
            GameManager.instance.GetSelected()[0] == menuTarget
            &&
            (
                !GameManager.instance.focusMode
                ||
                GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] != GameManager.instance.GetSelected()[0]
            )
            &&
            GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().category != "tempBar"
        };
        selectParentBtn.Check();

        OpenGroupBtn = new ButtonHandler(OpenGroupBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Group>()
        };
        OpenGroupBtn.Check();

        editBtn = new ButtonHandler(editBtn.button, true)
        {
            interactCondition = () => GameManager.instance.editMode
            ||
            (
                GameManager.instance.focusMode
                &&
                GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] == menuTarget
            ),

            toggledCondition = () => GameManager.instance.editMode,
            toggledColor = Utils.ParseHtmlColor(GameManager.instance.configLoader.GetColor("edit"))
        };
        editBtn.Check();

        resetTransBtn = new ButtonHandler(resetTransBtn.button, true)
        {
            interactCondition = () => GameManager.instance.editMode
            &&
            GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] == menuTarget
        };
        resetTransBtn.Check();

        resetChildrenBtn = new ButtonHandler(resetChildrenBtn.button, true)
        {
            interactCondition = () => GameManager.instance.focusMode
            &&
            GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] == menuTarget
        };
        resetChildrenBtn.Check();

        barChartBtn = new ButtonHandler(barChartBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room
            &&
            room.barChart
        };
        barChartBtn.Check();

        scatterPlotBtn = new ButtonHandler(scatterPlotBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            !menuTarget.GetComponent<Group>()
            &&
            (
                (menuTarget.GetComponent<OObject>() && menuTarget.GetComponent<OObject>().category != Category.Corridor)
                ||
                menuTarget.GetComponent<Room>()
            ),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>() is OgreeObject ogree
            &&
            ogree
            &&
            ogree.scatterPlot
        };
        scatterPlotBtn.Check();

        heatMapBtn = new ButtonHandler(heatMapBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OObject>()
            &&
            menuTarget.GetComponent<OObject>().category == Category.Device
            &&
            DepthCheck(menuTarget.GetComponent<OObject>()) <= 1,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OObject>()
            &&
            menuTarget.GetComponent<OObject>().heatMap

        };
        heatMapBtn.Check();

        toggleTilesNameBtn = new ButtonHandler(toggleTilesNameBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>()
            &&
            menuTarget.GetComponent<Room>().tileName
        };
        toggleTilesNameBtn.Check();

        toggleTilesColorBtn = new ButtonHandler(toggleTilesColorBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>()
            &&
            menuTarget.GetComponent<Room>().tileColor
        };
        toggleTilesColorBtn.Check();
        
        toggleWallsBtn = new ButtonHandler(toggleWallsBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Building>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Building>()
            &&
            menuTarget.GetComponent<Building>().displayWalls
        };
        toggleWallsBtn.Check();

        toggleUHelpersBtn = new ButtonHandler(toggleUHelpersBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<OObject>()),

            toggledCondition = () => menuTarget
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<OObject>())
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<OObject>()).uRoot
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<OObject>()).uRoot.gameObject.activeSelf
        };
        toggleUHelpersBtn.Check();

        toggleLocalCSBtn = new ButtonHandler(toggleLocalCSBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            (
                (
                    menuTarget.GetComponent<OObject>()
                    &&
                    menuTarget.GetComponent<OObject>().category != Category.Corridor
                )
                ||
                menuTarget.GetComponent<Building>()
            ),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            menuTarget.GetComponent<OgreeObject>().localCS
        };
        toggleLocalCSBtn.Check();

        getCoordsBtn = new ButtonHandler(getCoordsBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Building>()
            &&
            !GameManager.instance.focusMode
            &&
            !GameManager.instance.editMode,

            toggledCondition = () => GameManager.instance.getCoordsMode
        };
        getCoordsBtn.Check();

        toggleClearanceBtn = new ButtonHandler(toggleClearanceBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OObject>()
            && menuTarget.GetComponent<OObject>().clearanceHandler.isInitialized,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OObject>()
            &&
            menuTarget.GetComponent<OObject>().clearanceHandler.isToggled
        };
        toggleClearanceBtn.Check();

        SetupColors();
        menuPanel.SetActive(false);
        coordSystem.SetActive(false);
        rightClickMenu.SetActive(false);
        groupsMenu.SetActive(false);
        UpdateTimerValue(slider.value);

        EventManager.instance.OnSelectItem.Add(OnSelectItem);

        EventManager.instance.OnFocus.Add(OnFocusItem);
        EventManager.instance.OnUnFocus.Add(OnUnFocusItem);

        EventManager.instance.ConnectApi.Add(OnApiConnected);
        EventManager.instance.ImportFinished.Add(OnImportFinished);
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
        EventManager.instance.OnSelectItem.Remove(OnSelectItem);

        EventManager.instance.OnFocus.Remove(OnFocusItem);
        EventManager.instance.OnUnFocus.Remove(OnUnFocusItem);

        EventManager.instance.ConnectApi.Remove(OnApiConnected);
        EventManager.instance.ImportFinished.Remove(OnImportFinished);
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        SetCurrentItemText();
        UpdateGuiInfos();
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent _e)
    {
        SetFocusItemText();
    }

    ///<summary>
    /// When called, change buttons behavior and update GUI.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        SetFocusItemText();
    }

    ///<summary>
    /// When called, update apiBtn and apiUrl. 
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnApiConnected(ConnectApiEvent _e)
    {
        if (ApiManager.instance.isInit)
        {
            apiText.text = $"Connected to {_e.apiData["Customer"]}";
            apiText.color = Color.green;
        }
        else
        {
            apiText.text = $"Fail to connected to {ApiManager.instance.GetApiUrl()}";
            apiText.color = Color.red;
        }
        apiInfos.text = $"API URL:\t\t\t{ApiManager.instance.GetApiUrl()}";
        apiInfos.text += $"\nAPI build date:\t\t{_e.apiData["BuildDate"]}";
        apiInfos.text += $"\nAPI build hash:\t\t{_e.apiData["BuildHash"]}";
        apiInfos.text += $"\nAPI build tree:\t\t{_e.apiData["BuildTree"]}";
        apiInfos.text += $"\nAPI commit date:\t{_e.apiData["CommitDate"]}";
    }

    ///<summary>
    /// When called, update the detailsInputField according to the first selected item
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent _e)
    {
        if (GameManager.instance.selectMode)
        {
            string value = GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().currentLod.ToString();
            detailsInputField.UpdateInputField(value);
        }
        else
            detailsInputField.UpdateInputField("0");
    }

    ///<summary>
    /// Uses colors from config to set select & focus texts color background
    ///</summary>
    private void SetupColors()
    {
        float alpha = 0.5f;
        string selectColorCode = GameManager.instance.configLoader.GetColor("selection");
        if (!string.IsNullOrEmpty(selectColorCode))
        {
            Color c = Utils.ParseHtmlColor(selectColorCode);
            selectColor = new Color(c.r, c.g, c.b, alpha);
            selectionInputField.GetComponent<Image>().color = selectColor;
        }

        string focusColorCode = GameManager.instance.configLoader.GetColor("focus");
        if (!string.IsNullOrEmpty(focusColorCode))
        {
            Color c = Utils.ParseHtmlColor(focusColorCode);
            focusInputField.GetComponent<Image>().color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    ///<summary>
    /// Get the object under the mouse and displays its hierarchyName in mouseName text.
    ///</summary>
    private void NameUnderMouse()
    {
        GameObject obj = Utils.RaycastFromCameraToMouse();
        if (obj && obj.GetComponent<OgreeObject>())
            mouseName.text = obj.GetComponent<OgreeObject>().id.Replace(".", "/");
        else
            mouseName.text = "";
    }

    ///<summary>
    /// Call GUIObjectInfos 'UpdateFields' method according to currentItems.Count.
    ///</summary>
    public void UpdateGuiInfos()
    {
        if (!GameManager.instance.selectMode)
            objInfos.UpdateSingleFields(null);
        else if (GameManager.instance.GetSelected().Count == 1)
            objInfos.UpdateSingleFields(GameManager.instance.GetSelected()[0]);
        else
            objInfos.UpdateMultiFields(GameManager.instance.GetSelected());
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
    /// If a button is active, display rightClickMenu
    ///</summary>
    public void DisplayRightClickMenu()
    {
        int displayedButtons = 0;
        foreach (Transform btn in rightClickMenu.transform)
        {
            if (btn.gameObject.activeSelf)
                displayedButtons++;
        }
        if (displayedButtons > 0)
        {
            // Setup the menu
            rightClickMenu.SetActive(true);
            if (GameManager.instance.GetSelected().Count > 0)
                rightClickMenu.GetComponent<Image>().color = selectColor;
            else
                rightClickMenu.GetComponent<Image>().color = defaultColor;

            float canvasScale = canvas.GetComponent<RectTransform>().localScale.x;
            float btnHeight = rightClickMenu.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            float padding = rightClickMenu.GetComponent<VerticalLayoutGroup>().padding.top;
            float spacing = rightClickMenu.GetComponent<VerticalLayoutGroup>().spacing;

            float menuWidth = rightClickMenu.GetComponent<RectTransform>().sizeDelta.x;
            float menuHeight = padding * 2 + (btnHeight + spacing) * displayedButtons;
            rightClickMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(menuWidth, menuHeight);

            // Move the menu at mouse position and prevent it to be out of the window
            rightClickMenu.transform.position = Input.mousePosition;
            if (Screen.width - Input.mousePosition.x < menuWidth * canvasScale)
                rightClickMenu.transform.position -= new Vector3(menuWidth * canvasScale, 0, 0);
            if (Input.mousePosition.y < menuHeight * canvasScale)
                rightClickMenu.transform.position += new Vector3(0, menuHeight * canvasScale, 0);
        }
    }

    ///<summary>
    /// Disable the rightClickMenu's GameObject
    ///</summary>
    public void HideRightClickMenu()
    {
        rightClickMenu.SetActive(false);
        GetComponent<Inputs>().lockMouseInteract = false;
    }

    ///<summary>
    /// Called by GUI Button: Toggle opened groups buttons and change text of <see cref="groupsMenuBtnText"/>.
    ///</summary>
    public void ToggleGroupsMenu()
    {
        expendGroupsMenu ^= true;
        groupsMenuBtnText.text = expendGroupsMenu ? "Hide opened group list" : "Display opened group list";

        GroupsMenuBackgroundSize();
        foreach (Transform btn in groupsMenu.transform)
        {
            if (btn.GetSiblingIndex() != 0)
                btn.gameObject.SetActive(expendGroupsMenu);
        }
    }

    ///<summary>
    /// Active <see cref="groupsMenu"/> depending on <see cref="openedGroups"/> count and re-generate a button for each <see cref="openedGroups"/> item.
    ///</summary>
    public void RebuildGroupsMenu()
    {
        groupsMenu.SetActive(openedGroups.Count > 0);

        // Wipe previous buttons
        foreach (Transform btn in groupsMenu.transform)
        {
            if (btn.GetSiblingIndex() != 0)
                Destroy(btn.gameObject);
        }

        // Create a button for each opened group
        foreach (Group gr in openedGroups)
        {
            GameObject newButton = Instantiate(groupBtnPrefab, groupsMenu.transform);
            newButton.name = $"ButtonOpenGr_{gr.name}";
            newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = gr.id;

            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() => gr.ToggleContent(false));
            btn.onClick.AddListener(() => Destroy(newButton));

            newButton.SetActive(expendGroupsMenu);
        }
        GroupsMenuBackgroundSize();
    }

    ///<summary>
    /// Set the <see cref="groupsMenu"/>'s background according to <see cref="expendGroupsMenu"/>
    ///</summary>
    private void GroupsMenuBackgroundSize()
    {
        int count = expendGroupsMenu ? openedGroups.Count + 1 : 1;

        float btnHeight = groupsMenu.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        float padding = groupsMenu.GetComponent<VerticalLayoutGroup>().padding.top;
        float spacing = groupsMenu.GetComponent<VerticalLayoutGroup>().spacing;

        float menuWidth = groupsMenu.GetComponent<RectTransform>().sizeDelta.x;
        float menuHeight = padding * 2 + (btnHeight + spacing) * count;
        groupsMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(menuWidth, menuHeight);
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
    private void SetCurrentItemText()
    {
        if (GameManager.instance.GetSelected().Count == 1)
            selectionInputField.text = GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().id.Replace(".", "/");
        else if (GameManager.instance.GetSelected().Count > 1)
            selectionInputField.text = ("Multiple selection");
        else
            selectionInputField.text = ("");
    }

    ///<summary>
    /// Set focusText according to focus' last item.
    ///</summary>
    private void SetFocusItemText()
    {
        if (GameManager.instance.focusMode)
        {
            string objName = GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1].GetComponent<OgreeObject>().id.Replace(".", "/");
            focusInputField.text = $"{objName}";
        }
        else
            focusInputField.text = "";
    }

    #endregion

    ///<summary>
    /// Write the given line in the logger with asked color.
    ///</summary>
    ///<param name="_line">The line to write</param>
    ///<param name="_type">The type of message, it will change the message's color</param>
    public void AppendLogLine(string _line, ELogtype _type)
    {
        string color = "";
        if (_type == ELogtype.info || _type == ELogtype.infoCli || _type == ELogtype.infoApi)
            color = "white";
        else if (_type == ELogtype.success || _type == ELogtype.successCli || _type == ELogtype.successApi)
            color = "green";
        else if (_type == ELogtype.warning || _type == ELogtype.warningCli || _type == ELogtype.warningApi)
            color = "yellow";
        else if (_type == ELogtype.error || _type == ELogtype.errorCli || _type == ELogtype.errorApi)
            color = "red";

        _line = $"<color={color}>{_line}</color>";

        if (loggerQueue.Count >= loggerSize)
            loggerQueue.Dequeue();
        loggerQueue.Enqueue(_line);

        loggerText.SetTextWithoutNotify(string.Join("\n", loggerQueue.ToArray()));
        loggerSB.value = 1;
    }

    #region CalledByGUI

    ///<summary>
    /// Called by GUI button: select object targeted by right click menu.
    ///</summary>
    public async void SelectItem()
    {
        await GameManager.instance.SetCurrentItem(menuTarget);
    }

    ///<summary>
    /// Called by GUI buttons: update the current selection wuth the object targeted by right click menu.
    ///</summary>
    public async void UpdateSelectItem()
    {
        await GameManager.instance.UpdateCurrentItems(menuTarget);
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles name.
    ///</summary>
    public void ToggleTilesName()
    {
        menuTarget.GetComponent<Room>().ToggleTilesName();
        toggleTilesNameBtn.Check();
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles color.
    ///</summary>
    public void ToggleTilesColor()
    {
        Room currentRoom = menuTarget.GetComponent<Room>();
        if (!GameManager.instance.roomTemplates.ContainsKey(currentRoom.attributes["template"]))
        {
            GameManager.instance.AppendLogLine($"There is no template for {currentRoom.name}", ELogTarget.logger, ELogtype.warning);
            return;
        }
        currentRoom.ToggleTilesColor();

        toggleTilesColorBtn.Check();
    }

    public void ToggleWalls()
    {
        menuTarget.GetComponent<Building>().ToggleWalls();
        toggleWallsBtn.Check();
    }

    ///<summary>
    /// Called by GUI button: if currentItem is a rack, toggle U helpers.
    ///</summary>
    public void ToggleUHelpers()
    {
        if (GameManager.instance.GetSelected().Contains(menuTarget))
        {
            UHelpersManager.instance.ToggleU(GameManager.instance.GetSelected());
            return;
        }
        Rack rack = Utils.GetRackReferent(menuTarget.GetComponent<OObject>());
        if (!rack)
            return;

        UHelpersManager.instance.ToggleU(menuTarget);

        toggleUHelpersBtn.Check();
    }

    ///<summary>
    /// Called by GUI: toggle local Coordinate System of the object targeted by right click menu.
    ///</summary>
    public void ToggleCS()
    {
        OgreeObject obj = menuTarget.GetComponent<OgreeObject>();
        if (obj is Building bd)
            bd.ToggleCS();
        else if (obj is OObject oobj)
            oobj.ToggleCS();

        toggleLocalCSBtn.Check();
    }

    ///<summary>
    /// Called by GUI button: Focus object targeted by right click menu.
    ///</summary>
    public async void FocusItem()
    {
        await GameManager.instance.SetCurrentItem(menuTarget);
        await GameManager.instance.FocusItem(menuTarget);
    }

    ///<summary>
    /// Called by GUI button: Unfocus an item.
    ///</summary>
    public async void UnfocusItem()
    {
        if (GameManager.instance.editMode)
            EditFocused();
        if (GameManager.instance.focusMode)
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
            EventManager.instance.Raise(new EditModeOutEvent(GameManager.instance.GetSelected()[0]));
            GameManager.instance.AppendLogLine($"Edit out: {GameManager.instance.GetSelected()[0]}", ELogTarget.logger, ELogtype.info);
        }
        else
        {
            GameManager.instance.editMode = true;
            EventManager.instance.Raise(new EditModeInEvent(GameManager.instance.GetSelected()[0]));
            GameManager.instance.AppendLogLine($"Edit in: {GameManager.instance.GetSelected()[0]}", ELogTarget.logger, ELogtype.info);
        }
    }

    ///<summary>
    /// Called by GUI button: Reset transforms of the selected item.
    ///</summary>
    public void ResetTransform()
    {
        GameObject obj = GameManager.instance.GetSelected()[0];
        if (obj)
            obj.GetComponent<OgreeObject>().ResetTransform();
    }

    ///<summary>
    /// Called by GUI button: Reset tranforms of the children of the selected item.
    ///</summary>
    public void ResetChildrenTransforms()
    {
        GameObject obj = GameManager.instance.GetSelected()[0];
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
        if (!GameManager.instance.selectMode)
            return;

        await GameManager.instance.SetCurrentItem(GameManager.instance.GetSelected()[0].transform.parent?.gameObject);
    }

    ///<summary>
    /// Called by GUI button: Toggle content of group under the mouse
    ///</summary>
    public void ToggleGroupContent()
    {
        Group group = menuTarget.GetComponent<Group>();
        group.ToggleContent(group.isDisplayed);
    }

    ///<summary>
    /// Toggle build-in CLI writing.
    ///</summary>
    ///<param name="_value">The toggle value</param>
    public void ToggleCliWriting(bool _value)
    {
        if (_value)
        {
            GameManager.instance.writeLogs = true;
            GameManager.instance.AppendLogLine("Enable CLI", ELogTarget.logger, ELogtype.success);
        }
        else
        {
            GameManager.instance.AppendLogLine("Disable CLI", ELogTarget.logger, ELogtype.success);
            GameManager.instance.writeLogs = false;
        }
    }

    ///<summary>
    /// Send a ToggleLabelEvent and change the toggle text.
    ///</summary>
    ///<param name="_value">The toggle value</param>
    public void ToggleLabels(int _value)
    {
        EventManager.instance.Raise(new SwitchLabelEvent((ELabelMode)_value));
        mouseName.gameObject.SetActive((ELabelMode)_value == ELabelMode.Hidden);
    }

    ///<summary>
    /// Called by GUI button: Delete all files stored in cache directory.
    ///</summary>
    public void ClearCache()
    {
        DirectoryInfo dir = new DirectoryInfo(GameManager.instance.configLoader.GetCacheDir());
        foreach (FileInfo file in dir.GetFiles())
        {
            if (!file.Name.EndsWith("log.txt"))
                file.Delete();
        }
        GameManager.instance.AppendLogLine($"Cache cleared at \"{GameManager.instance.configLoader.GetCacheDir()}\"", ELogTarget.both, ELogtype.success);
        GameManager.instance.PurgeTemplates();
    }


    ///<summary>
    /// Called by GUI button: if one and only one room if selected, toggle its bar chart.
    ///</summary>
    public void ToggleTempBarChart()
    {
        TempDiagram.instance.HandleTempBarChart(menuTarget.GetComponent<Room>());
        barChartBtn.Check();
    }

    ///<summary>
    /// Called by GUI button: toggle temperature color mode.
    ///</summary>
    public void TempColorMode(bool _value)
    {
        GameManager.instance.tempColorMode = _value;
        EventManager.instance.Raise(new TemperatureColorEvent());
        UpdateGuiInfos();
    }


    ///<summary>
    /// Called by GUI button: if one and only one room or OObject is seleted, toggle its sensor scatter plot
    ///</summary>
    public void ToggleTempScatterPlot()
    {
        TempDiagram.instance.HandleScatterPlot(menuTarget.GetComponent<OgreeObject>());
        scatterPlotBtn.Check();
    }


    ///<summary>
    /// Called by GUI button: if one and only one device is selected and it has no child or its children have no child, toggle its heatmap
    ///</summary>
    public void ToggleHeatMap()
    {
        TempDiagram.instance.HandleHeatMap(menuTarget.GetComponent<OObject>());
    }

    ///<summary>
    /// Called by GUI button: if one and only one building or room is selected, toggle the getCoords mode
    ///</summary>
    public async void ToggleGetCoordsMode()
    {
        await GameManager.instance.SetCurrentItem(menuTarget);

        GameManager.instance.getCoordsMode ^= true;
        Building bd = GameManager.instance.GetSelected()[0].GetComponent<Building>();
        if (GameManager.instance.getCoordsMode)
            GameManager.instance.AppendLogLine($"Enable Get Coordinates mode for {bd.id}", ELogTarget.logger, ELogtype.success);
        else
            GameManager.instance.AppendLogLine($"Disable Get Coordinates mode for {bd.id}", ELogTarget.logger, ELogtype.success);
        bd.ToggleCS(GameManager.instance.getCoordsMode);
        coordSystem.SetActive(GameManager.instance.getCoordsMode);

        getCoordsBtn.Check();
        toggleLocalCSBtn.Check();
    }

    ///<summary>
    /// Attached to GUI Slider. Change value of ConsoleController.timerValue. Also update text field.
    ///</summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateTimerValue(float _value)
    {
        slider.value = _value;
        GameManager.instance.server.timer = (int)(_value);
        value.text = _value.ToString("0.##") + "s";
    }

    ///<summary>
    /// Call at InputField End Edit. Select given object (retrieve it by its hierarchyName)
    ///</summary>
    ///<param name="_value">Value given by the InputField</param>
    public async void SelectEndEdit(string _value)
    {
        if (string.IsNullOrEmpty(_value))
            await GameManager.instance.SetCurrentItem(null);
        else if (Utils.GetObjectById(_value.Replace("/", ".")) is GameObject obj)
            await GameManager.instance.SetCurrentItem(obj);
        else if (!string.IsNullOrEmpty(_value))
            GameManager.instance.AppendLogLine($"Cannot find {_value}", ELogTarget.logger, ELogtype.warning);
        SetCurrentItemText();
    }

    ///<summary>
    /// Call at InputField End Edit. Focus given object (retrieve it by its hierarchyName)
    ///</summary>
    ///<param name="_value">Value given by the InputField</param>
    public async void FocusEndEdit(string _value)
    {
        if (Utils.GetObjectById(_value) is GameObject obj)
        {
            if (GameManager.instance.IsInFocus(obj))
            {
                await GameManager.instance.SetCurrentItem(obj);
                await GameManager.instance.FocusItem(obj);
            }
            else
                GameManager.instance.AppendLogLine($"Cannot focus {_value}", ELogTarget.logger, ELogtype.warning);
        }
        else if (!string.IsNullOrEmpty(_value))
            GameManager.instance.AppendLogLine($"Cannot find {_value}", ELogTarget.logger, ELogtype.warning);
        SetFocusItemText();
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
    /// Move the coordSystem plane to the hit point, aligned with the hitted object
    ///</summary>
    ///<param name="_hit">The hit data</param>
    public void MoveCSToHit(RaycastHit _hit)
    {
        coordSystem.transform.position = _hit.point + new Vector3(0, 0.001f, 0);
        coordSystem.transform.eulerAngles = _hit.collider.transform.parent.eulerAngles;
    }

    ///<summary>
    /// Disable getCoordsMode and hide localCS & coordSystem 
    ///</summary>
    public void DisableGetCoordsMode()
    {
        GameManager.instance.getCoordsMode = false;
        if (GameManager.instance.SelectIs<Building>())
        {
            Building bd = GameManager.instance.GetSelected()[0].GetComponent<Building>();
            bd.ToggleCS(false);
            coordSystem.SetActive(false);
            getCoordsBtn.Check();
            toggleLocalCSBtn.Check();
        }
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

    ///<summary>
    /// Called by GUI: toggle clearance of the object targeted by right click menu..
    ///</summary>
    public void ToggleClearance()
    {
        menuTarget.GetComponent<OObject>().clearanceHandler.ToggleClearance();
        toggleLocalCSBtn.Check();
    }
    #endregion
}
