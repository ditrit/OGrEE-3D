using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    static public UiManager instance;

    [SerializeField] private GameObject menuPanel = null;

    [Header("Panel Top")]
    [SerializeField] private Button focusBtn = null;
    [SerializeField] private Button selectParentBtn = null;
    [SerializeField] private TMP_Text focusText = null;
    [SerializeField] private TMP_Text toggleLabelsText = null;

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
        selectParentBtn.interactable = false;

        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            menuPanel.SetActive(!menuPanel.activeSelf);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.RemoveListener<OnDeselectItemEvent>(OnDeselectItem);
    }

    ///
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        focusBtn.interactable = true;
        selectParentBtn.interactable = true;
    }

    ///
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            focusBtn.interactable = false;
            selectParentBtn.interactable = false;
        }
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

        GameManager.gm.AppendLogLine(focusText.text, "green");
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
    ///<param name="_str">The text to display</param>
    public void SetCurrentItemText(string _str)
    {
        currentItemText.text = _str;
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
            GameManager.gm.AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Room currentRoom = GameManager.gm.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            currentRoom.ToggleTilesName();
            GameManager.gm.AppendLogLine($"Tiles name toggled for {GameManager.gm.currentItems[0].name}.", "yellow");
        }
        else
            GameManager.gm.AppendLogLine("Selected item must be a room", "red");
    }

    ///<summary>
    /// Called by GUI button: If currentItem is a room, toggle tiles color.
    ///</summary>
    public void ToggleTilesColor()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Room currentRoom = GameManager.gm.currentItems[0].GetComponent<Room>();
        if (currentRoom)
        {
            if (!GameManager.gm.roomTemplates.ContainsKey(currentRoom.attributes["template"]))
            {
                GameManager.gm.AppendLogLine($"There is no template for {currentRoom.name}", "yellow");
                return;
            }
            currentRoom.ToggleTilesColor();
            GameManager.gm.AppendLogLine($"Tiles color toggled for {GameManager.gm.currentItems[0].name}.", "yellow");
        }
        else
            GameManager.gm.AppendLogLine("Selected item must be a room", "red");
    }

    ///<summary>
    /// Called by GUI button: if currentItem is a rack, toggle U helpers.
    ///</summary>
    public void ToggleUHelpers()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", "yellow");
            return;
        }

        Rack rack = GameManager.gm.currentItems[0].GetComponent<Rack>();
        if (rack)
        {
            rack.ToggleU();
            GameManager.gm.AppendLogLine($"U helpers toggled for {GameManager.gm.currentItems[0].name}.", "yellow");
        }
        else
            GameManager.gm.AppendLogLine("Selected item must be a rack.", "red");
    }

    ///<summary>
    /// Called by GUI: foreach Object in currentItems, toggle local Coordinate System.
    ///</summary>
    public void GuiToggleCS()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", "yellow");
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
            GameManager.gm.AppendLogLine("Disconnected from API", "green");
        }
        else
            await GameManager.gm.ConnectToApi();
    }

    ///<summary>
    /// Called by GUI button: Focus selected object.
    ///</summary>
    public void FocusSelected()
    {
        if (GameManager.gm.currentItems.Count > 0 && GameManager.gm.currentItems[0].GetComponent<OObject>())
            GameManager.gm.FocusItem(GameManager.gm.currentItems[0]);
    }

    ///<summary>
    /// Called by GUI button: Select the parent of the selected object.
    ///</summary>
    public void SelectParentItem()
    {
        if (GameManager.gm.currentItems.Count == 0)
            return;

        GameManager.gm.SetCurrentItem(GameManager.gm.currentItems[0].transform.parent?.gameObject);
    }

    ///
    public void ToggleCLI(bool _value)
    {
        if (_value)
        {
            GameManager.gm.writeCLI = true;
            GameManager.gm.AppendLogLine("Enable CLI", "yellow");
        }
        else
        {
            GameManager.gm.AppendLogLine("Disable CLI", "yellow");
            GameManager.gm.writeCLI = false;
        }
    }

    ///<summary>
    /// Send a ToggleLabelEvent and change the toggle text.
    ///</summary>
    public void ToggleLabels(bool _value)
    {
        EventManager.Instance.Raise(new ToggleLabelEvent() { value = _value });
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
            file.Delete();
        GameManager.gm.AppendLogLine($"Cache cleared at \"{GameManager.gm.configLoader.GetCacheDir()}\"", "green");
    }
    #endregion
}
