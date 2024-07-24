using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    static public UiManager instance;

    [SerializeField] private Transform canvas;
    [SerializeField] private GameObject promptPrefab;

    [SerializeField] private GameObject menuPanel;

    [Header("Updated Canvas")]
    [SerializeField] private TMP_Text mouseName;

    [Header("GetCoordsMode")]
    public GameObject coordSystem;
    public bool previousClick = false;

    [Header("Right Click Menu")]
    [SerializeField] private GameObject rightClickMenu;
    [SerializeField] private Color selectColor;
    [SerializeField] private Color defaultColor;
    public GameObject menuTarget;
    [SerializeField] private TMP_Text buildDate;

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
    [SerializeField] private ButtonHandler toggleSepNamesBtn;
    [SerializeField] private ButtonHandler toggleGenNamesBtn;
    [SerializeField] private ButtonHandler toggleUHelpersBtn;
    [SerializeField] private ButtonHandler toggleBreakersBtn;
    [SerializeField] private ButtonHandler toggleLocalCSBtn;
    [SerializeField] private ButtonHandler toggleChildrenOriginBtn;
    [SerializeField] private ButtonHandler toggleClearanceBtn;
    [SerializeField] private ButtonHandler barChartBtn;
    [SerializeField] private ButtonHandler scatterPlotBtn;
    [SerializeField] private ButtonHandler heatMapBtn;
    [SerializeField] private ButtonHandler hideObjectBtn;
    [SerializeField] private ButtonHandler positionModeBtn;
    [SerializeField] private ButtonHandler SnappingBtn;

    [Header("Panel Top")]
    [SerializeField] private TMP_InputField selectionInputField;
    [SerializeField] private TMP_InputField focusInputField;
    [SerializeField] private TMP_Text apiText;
    [SerializeField] private TMP_Text apiInfos;
#pragma warning disable IDE1006 // Name assignment styles
    public string apiCustomer { get; private set; }
    public string apiBuildDate { get; private set; }
    public string apiBuildHash { get; private set; }
    public string apiBuildTree { get; private set; }
    public string apiCommitDate { get; private set; }
#pragma warning restore IDE1006 // Name assignment styles 
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
    private readonly Queue<string> loggerQueue = new(loggerSize);

    [Header("Groups")]
    public DynamicButtonList groupsList;
    public List<Group> openedGroups;

    [Header("Tags")]
    public DynamicButtonList tagsList;

    [Header("Hidden Object")]
    public DynamicButtonList hiddenObjList;
    public List<Item> hiddenObjects;

    [Header("Layers")]
    public DynamicButtonList layersList;

    [Header("Settings Panel")]
    [SerializeField] private Toggle autoUHelpersToggle;
    [SerializeField] private bool defaultAutoUHelpers;
    [SerializeField] private Toggle autoBreakersToggle;
    [SerializeField] private bool defaultAutoBreakers;
    [SerializeField] private Slider doubleClickSlider;
    [SerializeField] private float defaultDoubleClickDelay;
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private float defaultMoveSpeed;
    [SerializeField] private Slider rotationSpeedSlider;
    [SerializeField] private float defaultRotationSpeed;
    [SerializeField] private Slider humanHeightSlider;
    [SerializeField] private float defaultHumanHeight;

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
#if !TRILIB
        buildDate.text += "\n<color=\"red\">Build without TriLib plugin</color>";
#endif
        selectBtn = new(selectBtn.button, false)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.GetSelected().Contains(menuTarget)
        };
        selectBtn.Check();

        addSelectBtn = new(addSelectBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            menuTarget
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.GetSelected().Contains(menuTarget)
            &&
            menuTarget.GetComponent<OgreeObject>()
            &&
            GameManager.instance.SelectIs<OgreeObject>(menuTarget.GetComponent<OgreeObject>().category)
        };
        addSelectBtn.Check();

        removeSelectBtn = new(removeSelectBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.positionMode
            &&
            GameManager.instance.GetSelected().Contains(menuTarget)
            &&
            (
                !GameManager.instance.focusMode
                ||
                GameManager.instance.GetFocused()[^1] != menuTarget
            )
        };
        removeSelectBtn.Check();

        focusBtn = new(focusBtn.button, false)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            !GameManager.instance.editMode
            &&
            !GameManager.instance.positionMode
            &&
            menuTarget
            &&
            menuTarget.GetComponent<Item>() is Item item
            &&
            (
                item is Rack
                ||
                item is Device
            )
            &&
            !GameManager.instance.GetFocused().Contains(menuTarget)
        };
        focusBtn.Check();

        unfocusBtn = new(unfocusBtn.button, true)
        {
            interactCondition = () => GameManager.instance.focusMode
            &&
            !GameManager.instance.editMode
        };
        unfocusBtn.Check();

        selectParentBtn = new(selectParentBtn.button, true)
        {
            interactCondition = () => !GameManager.instance.getCoordsMode
            &&
            !GameManager.instance.positionMode
            &&
            GameManager.instance.selectMode
            &&
            GameManager.instance.GetSelected()[0] == menuTarget
            &&
            (
                !GameManager.instance.focusMode
                ||
                GameManager.instance.GetFocused()[^1] != GameManager.instance.GetSelected()[0]
            )
            &&
            !GameManager.instance.SelectIs<OgreeObject>("tempBar")
        };
        selectParentBtn.Check();

        OpenGroupBtn = new(OpenGroupBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Group>()
        };
        OpenGroupBtn.Check();

        editBtn = new(editBtn.button, true)
        {
            interactCondition = () => GameManager.instance.editMode
            ||
            (
                GameManager.instance.focusMode
                &&
                GameManager.instance.GetFocused()[^1] == menuTarget
            ),

            toggledCondition = () => GameManager.instance.editMode,
            toggledColor = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("edit"))
        };
        editBtn.Check();

        resetTransBtn = new(resetTransBtn.button, true)
        {
            interactCondition = () => (GameManager.instance.editMode
            &&
            GameManager.instance.GetFocused()[^1] == menuTarget)
            ||
            GameManager.instance.positionMode
        };
        resetTransBtn.Check();

        resetChildrenBtn = new(resetChildrenBtn.button, true)
        {
            interactCondition = () => GameManager.instance.focusMode
            &&
            GameManager.instance.GetFocused()[^1] == menuTarget
        };
        resetChildrenBtn.Check();

        barChartBtn = new(barChartBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.barChart
        };
        barChartBtn.Check();

        scatterPlotBtn = new(scatterPlotBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            (
                (
                    menuTarget.GetComponent<Item>() is Item item
                    &&
                    (
                        item is Rack
                        ||
                        item is Device
                    )
                )
                ||
                menuTarget.GetComponent<Room>()
            ),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>() is OgreeObject ogree
            &&
            ogree.scatterPlot
        };
        scatterPlotBtn.Check();

        heatMapBtn = new(heatMapBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Device>()
            &&
            DepthCheck(menuTarget.GetComponent<Item>()) <= 1,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Item>() is Item item
            &&
            item.heatMap

        };
        heatMapBtn.Check();

        toggleTilesNameBtn = new(toggleTilesNameBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.tileName
        };
        toggleTilesNameBtn.Check();

        toggleTilesColorBtn = new(toggleTilesColorBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.tileColor
        };
        toggleTilesColorBtn.Check();

        toggleWallsBtn = new(toggleWallsBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Building>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Building>() is Building building
            &&
            building.displayWalls
        };
        toggleWallsBtn.Check();

        toggleSepNamesBtn = new(toggleSepNamesBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.displayWalls
            &&
            room.separators.Count > 0,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.sepNamesDisplayed
        };
        toggleSepNamesBtn.Check();

        toggleGenNamesBtn = new(toggleGenNamesBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.displayWalls
            &&
            room.GetComponentsInChildren<GenericObject>().Length > 0,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room room
            &&
            room.genNamesDisplayed
        };
        toggleGenNamesBtn.Check();

        toggleUHelpersBtn = new(toggleUHelpersBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<Item>()),

            toggledCondition = () => menuTarget
            &&
            Utils.GetRackReferent(menuTarget.GetComponent<Item>()) is Rack rack
            &&
            rack.uRoot
            &&
            rack.uRoot.gameObject.activeSelf
        };
        toggleUHelpersBtn.Check();

        toggleBreakersBtn = new(toggleBreakersBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Rack>() is Rack rack
            &&
            rack.breakersBoxes.Count > 0,

            toggledCondition = () => menuTarget
            && 
            menuTarget.GetComponent<Rack>() is Rack rack
            &&
            rack.areBreakersToggled
        };
        toggleBreakersBtn.Check();

        toggleLocalCSBtn = new(toggleLocalCSBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            (
                (
                    menuTarget.GetComponent<Item>() is Item item
                    &&
                    item is not Corridor
                )
                ||
                menuTarget.GetComponent<Building>()
            ),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<OgreeObject>() is OgreeObject obj
            &&
            obj.localCS
        };
        toggleLocalCSBtn.Check();

        toggleChildrenOriginBtn = new(toggleChildrenOriginBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>(),

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Room>() is Room ro
            &&
            ro.childrenOrigin.activeSelf
        };
        toggleChildrenOriginBtn.Check();

        getCoordsBtn = new(getCoordsBtn.button, true)
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

        toggleClearanceBtn = new(toggleClearanceBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Item>() is Item item
            &&
            item.clearanceHandler.isInitialized,

            toggledCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Item>() is Item item
            &&
            item.clearanceHandler.isToggled
        };
        toggleClearanceBtn.Check();

        hideObjectBtn = new(hideObjectBtn.button, true)
        {
            interactCondition = () => menuTarget
            &&
            menuTarget.GetComponent<Item>() is Item item
            &&
            !item.GetComponent<ObjectDisplayController>().isHidden
            &&
            !GameManager.instance.positionMode
        };
        hideObjectBtn.Check();

        positionModeBtn = new(positionModeBtn.button, true)
        {
            interactCondition = () => GameManager.instance.positionMode
            ||
            (
                menuTarget
                &&
                !GameManager.instance.focusMode
                &&
                menuTarget.GetComponent<Item>() is Item item
                &&
                (
                    item is Corridor
                    ||
                    item is GenericObject
                    ||
                    item is Rack
                )
                &&
                menuTarget.transform.parent
            ),
            toggledCondition = () => GameManager.instance.positionMode
        };
        positionModeBtn.Check();

        SnappingBtn = new(SnappingBtn.button, true)
        {
            interactCondition = () => GameManager.instance.positionMode,
            toggledCondition = () => Positionner.instance.snapping
        };
        SnappingBtn.Check();

        SetupColors();
        menuPanel.SetActive(false);
        coordSystem.SetActive(false);
        rightClickMenu.SetActive(false);
        mouseName.gameObject.SetActive(false);
        UpdateTimerValue(slider.value);

        SetupSettingsPanel();

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
        SetupRightClickMenu();
        layersList.RebuildMenu(BuildLayersList);
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
        apiCustomer = _e.apiData["Customer"];
        apiBuildDate = _e.apiData["BuildDate"];
        apiBuildHash = _e.apiData["BuildHash"];
        apiBuildTree = _e.apiData["BuildTree"];
        apiCommitDate = _e.apiData["CommitDate"];
        if (ApiManager.instance.isInit)
        {
            apiText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "Connected to API";
            apiText.color = Color.green;
        }
        else if (!string.IsNullOrEmpty(ApiManager.instance.GetApiUrl()))
        {
            apiText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "Failed to connect to API";
            apiText.color = Color.red;
        }
        else
        {
            apiText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = "Not connected to API";
            apiText.color = Color.white;
        }
        apiInfos.GetComponent<LocalizeStringEvent>().RefreshString();
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

    /// <summary>
    /// Save values given by config.toml file and initialize UI elements of Settings panel
    /// </summary>
    private void SetupSettingsPanel()
    {
        defaultAutoUHelpers = GameManager.instance.configHandler.config.autoUHelpers;
        autoUHelpersToggle.isOn = defaultAutoUHelpers;
        
        defaultAutoBreakers = GameManager.instance.configHandler.config.autoBreakers;
        autoBreakersToggle.isOn = defaultAutoBreakers;

        defaultDoubleClickDelay = GameManager.instance.configHandler.config.DoubleClickDelay;
        doubleClickSlider.value = defaultDoubleClickDelay;

        defaultMoveSpeed = GameManager.instance.configHandler.config.MoveSpeed;
        moveSpeedSlider.value = defaultMoveSpeed;

        defaultRotationSpeed = GameManager.instance.configHandler.config.RotationSpeed;
        rotationSpeedSlider.value = defaultRotationSpeed;

        defaultHumanHeight = GameManager.instance.configHandler.config.HumanHeight;
        humanHeightSlider.value = defaultHumanHeight;
    }

    ///<summary>
    /// Uses colors from config to set select & focus texts color background
    ///</summary>
    private void SetupColors()
    {
        float alpha = 0.5f;
        string selectColorCode = GameManager.instance.configHandler.GetColor("selection");
        if (!string.IsNullOrEmpty(selectColorCode))
        {
            Color c = Utils.ParseHtmlColor(selectColorCode);
            selectColor = new(c.r, c.g, c.b, alpha);
            selectionInputField.GetComponent<Image>().color = selectColor;
        }

        string focusColorCode = GameManager.instance.configHandler.GetColor("focus");
        if (!string.IsNullOrEmpty(focusColorCode))
        {
            Color c = Utils.ParseHtmlColor(focusColorCode);
            focusInputField.GetComponent<Image>().color = new(c.r, c.g, c.b, alpha);
        }
    }

    ///<summary>
    /// Get the object under the mouse and displays its hierarchyName in mouseName text.
    ///</summary>
    private void NameUnderMouse()
    {
        mouseName.text = Utils.RaycastFromCameraToMouse() is GameObject go && go.TryGetComponent(out OgreeObject obj) ? obj.id.Replace(".", "/") : "";
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
    ///<param name="_strVariable">A string variable to be used in Localization Table</param>
    ///<returns>The Prompt class of the generated item</returns>
    public Prompt GeneratePrompt(string _mainText, string _buttonAText, string _buttonBText, string _strVariable = null)
    {
        Prompt prompt = Instantiate(promptPrefab, canvas).GetComponent<Prompt>();
        prompt.Setup(_mainText, _buttonAText, _buttonBText, _strVariable);
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
            if (btn.gameObject.activeSelf)
                displayedButtons++;
        if (displayedButtons > 0)
        {
            // Setup the menu
            rightClickMenu.SetActive(true);
            Vector2 menuDim = SetupRightClickMenu(displayedButtons);

            // Move the menu at mouse position and prevent it to be out of the window
            float canvasScale = canvas.GetComponent<RectTransform>().localScale.x;
            rightClickMenu.transform.position = Input.mousePosition;
            if (Screen.width - Input.mousePosition.x < menuDim.x * canvasScale)
                rightClickMenu.transform.position -= new Vector3(menuDim.x * canvasScale, 0, 0);
            if (Input.mousePosition.y < menuDim.y * canvasScale)
                rightClickMenu.transform.position += new Vector3(0, menuDim.y * canvasScale, 0);
        }
    }

    /// <summary>
    /// Count displayed buttons in rightClickMenu and adjust its background size
    /// </summary>
    /// <returns>Width and height of rightClickMenu's background</returns>
    public Vector2 SetupRightClickMenu()
    {
        int displayedButtons = 0;
        foreach (Transform btn in rightClickMenu.transform)
            if (btn.gameObject.activeSelf)
                displayedButtons++;
        return SetupRightClickMenu(displayedButtons);
    }

    /// <summary>
    /// Adjust rightClickMenu's background size according to <paramref name="_displayedButtons"/>
    /// </summary>
    /// <param name="_displayedButtons">The number of displayed buttons in rightClickMenu</param>
    /// <returns>Width and height of rightClickMenu's background</returns>
    public Vector2 SetupRightClickMenu(int _displayedButtons)
    {
        rightClickMenu.GetComponent<Image>().color = GameManager.instance.selectMode ? selectColor : defaultColor;

        float btnHeight = rightClickMenu.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        float padding = rightClickMenu.GetComponent<VerticalLayoutGroup>().padding.top;
        float spacing = rightClickMenu.GetComponent<VerticalLayoutGroup>().spacing;

        float menuWidth = rightClickMenu.GetComponent<RectTransform>().sizeDelta.x;
        float menuHeight = padding * 2 + (btnHeight + spacing) * _displayedButtons;
        rightClickMenu.GetComponent<RectTransform>().sizeDelta = new(menuWidth, menuHeight);

        return new(menuWidth, menuHeight);
    }

    ///<summary>
    /// Disable the rightClickMenu's GameObject
    ///</summary>
    public void HideRightClickMenu()
    {
        rightClickMenu.SetActive(false);
        GetComponent<Inputs>().lockMouseInteract = false;
    }

    /// <summary>
    /// Generate buttons under <see cref="groupsList"/> from <see cref="openedGroups"/>
    /// </summary>
    /// <returns>The number of created buttons</returns>
    public int BuildGroupButtons()
    {
        foreach (Group gr in openedGroups)
        {
            GameObject newButton = Instantiate(groupsList.buttonPrefab, groupsList.transform);
            newButton.name = $"ButtonOpenGr_{gr.name}";
            newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = gr.id;

            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() => gr.ToggleContent(false));
            btn.onClick.AddListener(() => Destroy(newButton));
        }
        return openedGroups.Count;
    }

    /// <summary>
    /// Generate buttons under <see cref="tagsList"/> from <see cref="GameManager.instance.tags"/>
    /// </summary>
    /// <returns>The number of created buttons</returns>
    public int BuildTagButtons()
    {
        foreach (Tag tag in GameManager.instance.tags)
        {
            GameObject newButton = Instantiate(tagsList.buttonPrefab, tagsList.transform);
            newButton.name = $"ButtonTag_{tag.slug}";
            newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = tag.slug;

            Button btn = newButton.GetComponent<Button>();
            btn.GetComponent<Image>().color = tag.color;
            btn.onClick.AddListener(() =>
            {
                // Disable all other tags' highlight
                foreach (Tag t in GameManager.instance.tags)
                    if (t != tag)
                        t.HighlightObjects(false);
                // Highlight this tag
                tag.HighlightObjects(!tag.objHightlighted);

                // Set text color of tags buttons
                foreach (Transform button in tagsList.GetButtons())
                {
                    TMP_Text buttonText = button.GetChild(0).GetComponent<TMP_Text>();
                    Tag relatedTag = GameManager.instance.GetTag(buttonText.text);
                    buttonText.color = relatedTag.objHightlighted ? Color.green : Color.white;
                }
            });
        }
        return GameManager.instance.tags.Count;
    }

    /// <summary>
    /// Generate buttons under <see cref="hiddenObjList"/> from <see cref="hiddenObjects"/>
    /// </summary>
    /// <returns>The number of created buttons</returns>
    public int BuildHiddenObjButtons()
    {
        foreach (Item item in hiddenObjects)
        {
            GameObject newButton = Instantiate(hiddenObjList.buttonPrefab, hiddenObjList.transform);
            newButton.name = $"ButtonHiddenObj_{item.name}";
            newButton.transform.GetChild(0).GetComponent<TMP_Text>().text = item.id;

            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() => DisplayObject(item.gameObject));
            btn.onClick.AddListener(() => Destroy(newButton));
        }
        return hiddenObjects.Count;
    }

    /// <summary>
    /// Generate buttons under <see cref="layersList"/> from first selected object's layers list.
    /// </summary>
    /// <returns>The number of created buttons</returns>
    public int BuildLayersList()
    {
        if (GameManager.instance.selectMode
            && GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>() is OgreeObject obj)
        {
            foreach (KeyValuePair<Layer, bool> kvp in obj.layers)
            {
                Layer layer = kvp.Key;
                GameObject newButton = Instantiate(layersList.buttonPrefab, layersList.transform);
                newButton.name = $"ButtonLayer_{layer.slug}";
                TMP_Text btnText = newButton.transform.GetChild(0).GetComponent<TMP_Text>();
                btnText.text = layer.slug;
                btnText.color = kvp.Value ? Color.green : Color.red;

                Button btn = newButton.GetComponent<Button>();
                btn.onClick.AddListener(async () =>
                {
                    obj.layers[kvp.Key] ^= true;
                    List<GameObject> relatedObjects = layer is AutoLayer autoLayer ? await autoLayer.GetRelatedObjects() : await layer.GetRelatedObjects(obj.id);
                    foreach (GameObject go in relatedObjects)
                        if (go.GetComponent<ObjectDisplayController>() is ObjectDisplayController odc)
                        {
                            if (obj.layers[kvp.Key])
                                odc.DisplayObject();
                            else
                                odc.HideObject();
                        }
                    btnText.color = obj.layers[kvp.Key] ? Color.green : Color.red;
                });
            }
            return obj.layers.Count;
        }
        else
            return 0;
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
            selectionInputField.text = "Multiple selection";
        else
            selectionInputField.text = "";
    }

    ///<summary>
    /// Set focusText according to focus' last item.
    ///</summary>
    private void SetFocusItemText()
    {
        if (GameManager.instance.focusMode)
        {
            string objName = GameManager.instance.GetFocused()[^1].GetComponent<OgreeObject>().id.Replace(".", "/");
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
        if (!GameManager.instance.roomTemplates.ContainsKey((string)currentRoom.attributes["template"]))
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "There is no template for", currentRoom.name), ELogTarget.logger, ELogtype.warning);
            return;
        }
        currentRoom.ToggleTilesColor();

        toggleTilesColorBtn.Check();
    }

    /// <summary>
    /// Called by GUI button: Toggle walls of targeted building
    /// </summary>
    public void ToggleWalls()
    {
        menuTarget.GetComponent<Building>().ToggleWalls();
        toggleWallsBtn.Check();
    }

    /// <summary>
    /// Called by GUI button: Toggle separators name of targeted room
    /// </summary>
    public void ToggleSeparatorsName()
    {
        menuTarget.GetComponent<Room>().ToggleSeparatorText();
        toggleSepNamesBtn.Check();
    }

    /// <summary>
    /// Called by GUI button: Toggle generics name of targeted room
    /// </summary>
    public void ToggleGenericsName()
    {
        menuTarget.GetComponent<Room>().ToggleGenericText();
        toggleGenNamesBtn.Check();
    }

    ///<summary>
    /// Called by GUI button: toggle U helpers of targeted Rack.
    ///</summary>
    public void ToggleUHelpers()
    {
        UHelpersManager.instance.ToggleU(menuTarget);
        toggleUHelpersBtn.Check();
    }

    /// <summary>
    /// Called by GUI button: toggle breakers of targeted Rack
    /// </summary>
    public void ToggleBreakers()
    {
        menuTarget.GetComponent<Rack>().ToggleBreakers();
        toggleBreakersBtn.Check();
    }

    ///<summary>
    /// Called by GUI: toggle local Coordinate System of the object targeted by right click menu.
    ///</summary>
    public void ToggleCS()
    {
        OgreeObject obj = menuTarget.GetComponent<OgreeObject>();
        if (obj is Building bd)
            bd.ToggleCS();
        else if (obj is Item item)
            item.ToggleCS();

        toggleLocalCSBtn.Check();
    }

    ///<summary>
    /// Called by GUI: toggle children origin of the room targeted by right click menu.
    ///</summary>
    public void ToggleChildrenOrigin()
    {
        Room ro = menuTarget.GetComponent<Room>();
        ro.childrenOrigin.SetActive(!ro.childrenOrigin.activeSelf);
        toggleChildrenOriginBtn.Check();
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
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Edit out", GameManager.instance.GetSelected()[0].name), ELogTarget.logger, ELogtype.info);
        }
        else
        {
            GameManager.instance.editMode = true;
            EventManager.instance.Raise(new EditModeInEvent(GameManager.instance.GetSelected()[0]));
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Edit in", GameManager.instance.GetSelected()[0].name), ELogTarget.logger, ELogtype.info);
        }
    }

    ///<summary>
    /// Called by GUI button: Reset transforms of the selected item.
    ///</summary>
    public void ResetTransform()
    {
        GameManager.instance.GetSelected().ForEach(go => go.GetComponent<OgreeObject>().ResetTransform());
        Positionner.instance.realDisplacement.SetLocalPositionAndRotation(Positionner.instance.initialPositions[0], Positionner.instance.initialRotations[0]);
    }

    ///<summary>
    /// Called by GUI button: Reset tranforms of the children of the selected item.
    ///</summary>
    public void ResetChildrenTransforms()
    {
        if (GameManager.instance.GetSelected()[0] is GameObject obj)
            foreach (Transform child in obj.transform)
                child.GetComponent<OgreeObject>()?.ResetTransform();
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
        if (menuTarget.GetComponent<Group>() is Group group)
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
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Enable CLI"), ELogTarget.logger, ELogtype.success);
        }
        else
        {
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Disable CLI"), ELogTarget.logger, ELogtype.success);
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
        DirectoryInfo dir = new(GameManager.instance.configHandler.GetCacheDir());
        foreach (FileInfo file in dir.GetFiles())
            if (!file.Name.EndsWith("log.txt"))
                file.Delete();
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Cache cleared at", GameManager.instance.configHandler.GetCacheDir()), ELogTarget.both, ELogtype.success);
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
        TempDiagram.instance.HandleHeatMap(menuTarget.GetComponent<Item>());
    }

    ///<summary>
    /// Called by GUI button: if one and only one building or room is selected, toggle the getCoords mode
    ///</summary>
    public async void ToggleGetCoordsMode()
    {
        await GameManager.instance.SetCurrentItem(menuTarget);

        GameManager.instance.getCoordsMode ^= true;
        EventManager.instance.Raise(new GetCoordModeToggleEvent());
        Building bd = GameManager.instance.GetSelected()[0].GetComponent<Building>();
        if (GameManager.instance.getCoordsMode)
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Enable Get Coordinates", bd.id), ELogTarget.logger, ELogtype.success);
        else
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Disable Get Coordinates", bd.id), ELogTarget.logger, ELogtype.success);
        bd.ToggleCS(GameManager.instance.getCoordsMode);
        coordSystem.SetActive(GameManager.instance.getCoordsMode);
        getCoordsBtn.Check();
        toggleLocalCSBtn.Check();
        if (bd is Room ro)
            ro.childrenOrigin.SetActive(GameManager.instance.getCoordsMode);
        toggleChildrenOriginBtn.Check();
    }

    ///<summary>
    /// Attached to GUI Slider. Change value of ConsoleController.timerValue. Also update text field.
    ///</summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateTimerValue(float _value)
    {
        slider.value = _value;
        GameManager.instance.server.timer = (int)_value;
        value.text = _value.ToString("0.##") + "s";
    }

    /// <summary>
    /// Attached to GUI Toggle. Change value of <see cref="GameManager.configHandler.config.autoUHelpers"/>
    /// </summary>
    /// <param name="_value"></param>
    public void UpdateAutoUHelpers(bool _value)
    {
        GameManager.instance.configHandler.config.autoUHelpers = _value;
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="autoUHelpersToggle"/> using what was given by config.toml
    /// </summary>
    public void ResetAutoUHelpers()
    {
        autoUHelpersToggle.isOn = defaultAutoUHelpers;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="autoUHelpersToggle"/> in used config.toml file
    /// </summary>
    public void SaveAutoUHelpers()
    {
        GameManager.instance.configHandler.WritePreference("autoUHelpers", autoUHelpersToggle.isOn ? "true" : "false");
    }

    /// <summary>
    /// Attached to GUI Toggle. Change value of <see cref="GameManager.configHandler.config.autoBreakers"/>
    /// </summary>
    /// <param name="_value"></param>
    public void UpdateAutoBreakers(bool _value)
    {
        GameManager.instance.configHandler.config.autoBreakers = _value;
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="autoBreakersToggle"/> using what was given by config.toml
    /// </summary>
    public void ResetAutoBreakers()
    {
        autoBreakersToggle.isOn = defaultAutoBreakers;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="autoBreakersToggle"/> in used config.toml file
    /// </summary>
    public void SaveAutoBreakers()
    {
        GameManager.instance.configHandler.WritePreference("autoBreakers", autoBreakersToggle.isOn ? "true" : "false");
    }

    /// <summary>
    /// Attached to GUI Slider. Change value of <see cref="GameManager.configLoader.config.doubleClickDelay"/>
    /// </summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateDoubleClickDelay(float _value)
    {
        GameManager.instance.configHandler.config.DoubleClickDelay = _value;
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="doubleClickSlider"/> using what was given by config.toml
    /// </summary>
    public void ResetDoubleClickDelay()
    {
        doubleClickSlider.value = defaultDoubleClickDelay;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="doubleClickSlider"/> in used config.toml file
    /// </summary>
    public void SaveDoubleClickDelay()
    {
        GameManager.instance.configHandler.WritePreference("doubleClickDelay", doubleClickSlider.value.ToString("0.##"));
    }

    /// <summary>
    /// Attached to GUI Slider. Change value of <see cref="GameManager.configLoader.config.moveSpeed"/>
    /// </summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateMoveSpeed(float _value)
    {
        GameManager.instance.configHandler.config.MoveSpeed = _value;
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="moveSpeedSlider"/> using what was given by config.toml
    /// </summary>
    public void ResetMoveSpeed()
    {
        moveSpeedSlider.value = defaultMoveSpeed;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="moveSpeedSlider"/> in used config.toml file
    /// </summary>
    public void SaveMoveSpeed()
    {
        GameManager.instance.configHandler.WritePreference("moveSpeed", moveSpeedSlider.value.ToString("0.##"));
    }

    /// <summary>
    /// Attached to GUI Slider. Change value of <see cref="GameManager.configLoader.config.rotationSpeed"/>
    /// </summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateRotationSpeed(float _value)
    {
        GameManager.instance.configHandler.config.RotationSpeed = _value;
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="rotationSpeedSlider"/> using what was given by config.toml
    /// </summary>
    public void ResetRotationSpeed()
    {
        rotationSpeedSlider.value = defaultRotationSpeed;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="rotationSpeedSlider"/> in used config.toml file
    /// </summary>
    public void SaveRotationSpeed()
    {
        GameManager.instance.configHandler.WritePreference("rotationSpeed", rotationSpeedSlider.value.ToString("0.##"));
    }

    /// <summary>
    /// Attached to GUI Slider. Change value of <see cref="GameManager.configLoader.config.humanHeight"/>
    /// </summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateHumanHeight(float _value)
    {
        GameManager.instance.configHandler.config.HumanHeight = _value;
        GameManager.instance.cameraControl.UpdateHumanModeHeight();
    }

    /// <summary>
    /// Called by GUI button. Reset value of <see cref="humanHeightSlider"/> using what was given by config.toml
    /// </summary>
    public void ResetHumanHeight()
    {
        humanHeightSlider.value = defaultHumanHeight;
    }

    /// <summary>
    /// Called by GUI button. Write the value of <see cref="humanHeightSlider"/> in used config.toml file
    /// </summary>
    public void SaveHumanHeight()
    {
        GameManager.instance.configHandler.WritePreference("humanHeight", humanHeightSlider.value.ToString("0.##"));
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
        else if (GameManager.instance.GetTag(_value) is Tag tag)
            await tag.SelectLinkedObjects();
        else
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Object doesn't exist", _value), ELogTarget.logger, ELogtype.warning);
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
                GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Cannot focus", _value), ELogTarget.logger, ELogtype.warning);
        }
        else if (!string.IsNullOrEmpty(_value))
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Object doesn't exist", _value), ELogTarget.logger, ELogtype.warning);
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
            if (child.GetComponent<OgreeObject>() is OgreeObject childOgree)
                depth = Mathf.Max(depth, DepthCheck(childOgree) + 1);
        return depth;
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
            if (bd is Room ro)
                ro.childrenOrigin.SetActive(false);
            toggleChildrenOriginBtn.Check();
        }
    }

    ///<summary>
    /// Called by GUI button
    ///</summary>
    public void RaiseImportFinishedEvent()
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
        menuTarget.GetComponent<Item>().clearanceHandler.ToggleClearance();
        toggleLocalCSBtn.Check();
    }

    /// <summary>
    /// Called by GUI: Hide all selected objects
    /// </summary>
    public async void HideTargetedObjects()
    {
        if (GameManager.instance.GetSelected().Contains(menuTarget))
        {
            foreach (GameObject obj in GameManager.instance.GetSelected())
                HideObject(obj);
            await GameManager.instance.SetCurrentItem(null);
        }
        else
            HideObject(menuTarget);
    }

    /// <summary>
    /// Hide given <paramref name="_obj"/>
    /// </summary>
    /// <param name="_obj">The GameObject to hide</param>
    public void HideObject(GameObject _obj)
    {
        Item item = _obj.GetComponent<Item>();
        _obj.GetComponent<ObjectDisplayController>().HideObject();
        if (!hiddenObjects.Contains(item))
        {
            hiddenObjects.Add(item);
            hiddenObjects.Sort();
            hiddenObjList.RebuildMenu(BuildHiddenObjButtons);
        }
    }

    /// <summary>
    /// Display given <paramref name="_obj"/>
    /// </summary>
    /// <param name="_obj">The GameObject to display</param>
    public void DisplayObject(GameObject _obj)
    {
        _obj.GetComponent<ObjectDisplayController>().DisplayObject();
        hiddenObjects.Remove(_obj.GetComponent<Item>());
        hiddenObjList.RebuildMenu(BuildHiddenObjButtons);
    }

    ///<summary>
    /// Called by GUI: toggle position mode for the selection.
    ///</summary>
    public async void TogglePositionMode()
    {
        if (menuTarget && menuTarget.GetComponent<OgreeObject>() && !GameManager.instance.GetSelected().Contains(menuTarget))
            await GameManager.instance.SetCurrentItem(menuTarget);
        await Positionner.instance.TogglePositionMode();
    }

    /// <summary>
    /// Called by GUI: toggle snapping for the position mode.
    /// </summary>
    public void ToggleSnapping()
    {
        Positionner.instance.snapping ^= true;
        if (Positionner.instance.snapping)
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Enable Snapping"), ELogTarget.logger, ELogtype.success);
        else
        {
            Positionner.instance.realDisplacement.SetPositionAndRotation(Positionner.instance.transform.position, Positionner.instance.transform.rotation);
            GameManager.instance.AppendLogLine(new LocalizedString("Logs", "Disable Snapping"), ELogTarget.logger, ELogtype.success);
        }
    }
    #endregion
}
