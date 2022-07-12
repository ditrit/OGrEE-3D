using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Input;
using System;

public class SettingsMenuManager : GridMenuHandler
{
    public static SettingsMenuManager instance;
    public List<SApiObject> physicalObjects;
    public GameObject buttonPrefab;
    private GameObject rackButton;
    private GameObject mdiButton;
    public GameObject GameManagerTest;
    public GameObject mainMenu;
    [SerializeField] private List<string> previousCalls = new List<string>();
    [SerializeField] private List<string> parentNames = new List<string>();
    private string tenant;
    [SerializeField] private bool isDeviceTypeSelected = false;
    [SerializeField] private bool isSiteSelected = false;
    [SerializeField] private bool isFirstHelpMenuCreated = false;
    [SerializeField] private bool isFirstHelpMenuOpen = false;
    [SerializeField] private bool isHelpDialogActive = false;
    private Dialog myDialog;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeUIElements();
        InitializeDeviceButtons();
        menu.name = "Settings Menu";
        numberOfResultsPerPage = UiManagerVincent.instance.ReturnGridNumberOfRows(gridCollection);
    }

    private async void OnEnable()
    {
        while (String.IsNullOrEmpty(tenant))
        {
            await Task.Delay(50);
            try
            {
                tenant = GameManager.gm.configLoader.GetTenant();
            }
            catch { }
        }
        while (!ApiManager.instance.isInit)
        {
            await Task.Delay(50);
        }
        InitializeDeviceButtons();
        await FirstSearch();
    }

    public void InitializeDeviceButtons()
    {

        rackButton = menu.transform.Find("rack").gameObject;
        UiManagerVincent.instance.ChangeButtonColor(rackButton, Color.blue);
        var onClickEvent1 = UiManagerVincent.instance.ButtonOnClick(rackButton);

        onClickEvent1.AddListener(() =>
        {
            SelectRackButton();
        });

        mdiButton = menu.transform.Find("mdi").gameObject;
        UiManagerVincent.instance.ChangeButtonColor(mdiButton, Color.blue);
        var onClickEvent2 = UiManagerVincent.instance.ButtonOnClick(mdiButton);

        onClickEvent2.AddListener(() =>
        {
            SelectMdiButton();
        });
    }

    public void SelectRackButton()
    {
        ApiListener.instance.deviceType = rackButton.name;
        isDeviceTypeSelected = true;
        UiManagerVincent.instance.ChangeButtonColor(rackButton, Color.green);
        UiManagerVincent.instance.ChangeButtonColor(mdiButton, Color.blue);
    }

    public void SelectMdiButton()
    {
        ApiListener.instance.deviceType = mdiButton.name;
        isDeviceTypeSelected = true;
        UiManagerVincent.instance.ChangeButtonColor(rackButton, Color.blue);
        UiManagerVincent.instance.ChangeButtonColor(mdiButton, Color.green);
    }


    ///<summary>
    /// Open a dialog with voice commands when settings are done
    ///</summary>
    void Update()
    {
        if (isDeviceTypeSelected && isSiteSelected && !isFirstHelpMenuCreated)
        {
            AttachSettingsToMainMenu();
            UiManagerVincent.instance.EnableDialogHelp();
            UiManagerVincent.instance.ConfigureDialog(UiManagerVincent.instance.dialogHelp);

            isFirstHelpMenuCreated = true;
            isFirstHelpMenuOpen = true;
            isHelpDialogActive = true;
        }

        if (isFirstHelpMenuCreated && isFirstHelpMenuOpen)
        {
            if (UiManagerVincent.instance.dialogHelp.State == DialogState.Closed)
            {
                GameManagerTest.GetComponent<SpeechInputHandler>().enabled = true;
                isFirstHelpMenuOpen = false;

            }
        }

        if (isHelpDialogActive)
        {
            if (UiManagerVincent.instance.dialogHelp.State == DialogState.Closed)
            {
                isHelpDialogActive = false;
            }
        }
    }

    ///<summary>
    /// Open the dialog with voice commands when called
    ///</summary>
    public async void ToggleHelpMenu()
    {
        if (!isHelpDialogActive)
        {
            isHelpDialogActive = true;
            UiManagerVincent.instance.EnableDialogHelp();
            UiManagerVincent.instance.ConfigureDialog(myDialog);
            await Task.Delay(15000);
            if (UiManagerVincent.instance.dialogHelp.State != DialogState.Closed)
            {
                UiManagerVincent.instance.dialogHelp.DismissDialog();
                isHelpDialogActive = false;
            }
        }
    }

    ///<summary>
    /// Configure Menu and attach it to the main menu
    ///</summary>
    public void AttachSettingsToMainMenu()
    {
        UiManagerVincent.instance.AttachSettingsToMainMenu(menu, mainMenu);
    }


    ///<summary>
    /// Add correct Listener to the button
    ///</summary>
    ///<param name="_index">the element index in physicalObjects</param>
    private void AssignButtonFunction(int _index)
    {
        SApiObject obj = physicalObjects[_index];

        string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;

        GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
        g.name = fullname;
        UiManagerVincent.instance.ChangeButtonText(g, "Current site = " + fullname);
        UiManagerVincent.instance.ChangeButtonColor(g, Color.blue);
        var onClickEvent = UiManagerVincent.instance.ButtonOnClick(g);
        onClickEvent.AddListener(() =>
        {
            ApiListener.instance.site = obj.name;
            ApiListener.instance.customerAndSite = ApiListener.instance.customer + '.' + ApiListener.instance.site;
            if (isSiteSelected)
            {
                for (int i = pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
                {
                    string name = parentNames[parentNames.Count - 1] + "." + physicalObjects[i].name;
                    if (name != fullname)
                    {
                        Transform t = parentList.transform.Find($"{name}");
                        UiManagerVincent.instance.ChangeButtonColor(t.gameObject, Color.blue);
                    }
                }
            }
            UiManagerVincent.instance.ChangeButtonColor(g, Color.green);
            isSiteSelected = true;

        });
    }

    /*public void SelectMenuButton()
    {

    }*/

    ///<summary>
    /// Calls UpdateGridDefault then set the return button according to the previous call
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected new void UpdateGrid(int _elementNumber, ElementDelegate _elementDelegate)
    {
        UpdateGridDefault(_elementNumber, _elementDelegate);
        UiManagerVincent.instance.UpdateGrid(gridCollection);
    }

    ///<summary>
    /// Update the menu with the initial Search.
    ///</summary>
    public async Task FirstSearch()
    {
        List<SApiObject> tmp = await ApiManager.instance.GetObject($"tenants/{tenant}/sites", ApiManager.instance.GetAllSApiObject);
        if (tmp != null)
        {
            previousCalls.Add($"tenants/{tenant}/sites");
            parentNames.Add(tenant);
            physicalObjects = tmp;
            pageNumber = 0;
            UpdateGrid(physicalObjects.Count, AssignButtonFunction);
        }
    }

    ///<summary>
    /// Activate the menu if it is not active and move it in front of the camera. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleAndMoveSettingsMenu()
    {
        if (menu.activeSelf)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
            Utils.MoveObjectToCamera(menu, GameManager.gm.mainCamera, 0.6f, -0.25f, 0, 25);
        }
    }

    ///<summary>
    /// Activatethe menu if it is not active. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleSettingsMenu()
    {
        if (menu.activeSelf)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
        }
    }

    ///<summary>
    /// Deactivate the menu.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void DisableSettingsMenu()
    {
        menu.SetActive(false);
    }
}
