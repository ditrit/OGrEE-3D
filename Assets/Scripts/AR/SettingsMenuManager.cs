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
    [SerializeField][Tooltip("Assign DialogLarge_192x192.prefab")] private GameObject dialogPrefabLarge;
    public List<SApiObject> physicalObjects;
    public GameObject buttonPrefab;
    private GameObject rackButton;
    private GameObject mdiButton;
    public GameObject GameManagerTest;
    public GameObject mainMenu;
    [SerializeField]private List<string> previousCalls = new List<string>();
    [SerializeField]private List<string> parentNames = new List<string>();
    private string tenant;
    [SerializeField] private bool isDeviceTypeSelected = false;
    [SerializeField] private bool isSiteSelected = false;
    [SerializeField] private bool isFirstHelpMenuCreated = false;
    [SerializeField] private bool isFirstHelpMenuOpen = false;
    [SerializeField] private bool isHelpDialogActive = false;
    private Dialog myDialog;


    /// <summary>
    /// Large Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeUIElements();
        InitializeDeviceButtons();
        menu.name = "Settings Menu";
        numberOfResultsPerPage = gridCollection.Rows;
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
        rackButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        rackButton.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            ApiListener.instance.deviceType = rackButton.name;
            isDeviceTypeSelected = true;
            mdiButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
            rackButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
        });

        mdiButton = menu.transform.Find("mdi").gameObject;
        mdiButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        mdiButton.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            ApiListener.instance.deviceType = mdiButton.name;
            isDeviceTypeSelected = true;
            mdiButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
            rackButton.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        });
    }



    ///<summary>
    /// Open a dialog with voice commands when settings are done
    ///</summary>
    void Update()
    {
        if (isDeviceTypeSelected && isSiteSelected && !isFirstHelpMenuCreated)
        {
            menu.SetActive(false);
            menu.GetComponent<Follow>().enabled = false;
            menu.transform.SetParent(mainMenu.transform);
            menu.transform.localPosition = new Vector3(0.02f, -0.08f, 0);
            menu.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Destroy(menu.transform.Find("Backplate").gameObject);
            Destroy(menu.transform.Find("ButtonClose").gameObject);
            myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
            ApiListener.instance.ConfigureDialog(myDialog);
            isFirstHelpMenuCreated = true;
            isFirstHelpMenuOpen = true;
            isHelpDialogActive = true;
        }

        if (isFirstHelpMenuCreated && isFirstHelpMenuOpen)
        {
            if (myDialog.State == DialogState.Closed)
            {
                GameManagerTest.GetComponent<SpeechInputHandler>().enabled = true;
                isFirstHelpMenuOpen = false;

            }
        }

        if (isHelpDialogActive)
        {
            if (myDialog.State == DialogState.Closed)
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
            myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
            ApiListener.instance.ConfigureDialog(myDialog);
            await Task.Delay(15000);
            if (myDialog.State != DialogState.Closed)
            {
                myDialog.DismissDialog();
                isHelpDialogActive = false;
            }
        }
    }

    ///<summary>
    /// Instantiates three buttons: one to load the object, one to delete it and one to display the list of its children
    ///</summary>
    ///<param name="_index">the element index in physicalObjects</param>
    private void AssignButtonFunction(int _index)
    {
        SApiObject obj = physicalObjects[_index];

        string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;

        GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
        g.name = fullname;
        g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = "Current site = " + fullname;
        g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            ApiListener.instance.site = obj.name;
            if (isSiteSelected)
            {
                for (int i = pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
                {
                    string name = parentNames[parentNames.Count - 1] + "." + physicalObjects[i].name;
                    if (name != fullname)
                        parentList.transform.Find($"{name}/BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
                }
            }
            g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
            isSiteSelected = true;
        });
    }

    ///<summary>
    /// Calls UpdateGridDefault then set the return button according to the previous call
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected new void UpdateGrid(int _elementNumber, ElementDelegate _elementDelegate)
    {
        UpdateGridDefault(_elementNumber, _elementDelegate);
        gridCollection.UpdateCollection();
    }

    ///<summary>
    /// Destroy the buttons that were instantiated and update the grid collection
    ///</summary>
    public async void ClearParentList()
    {
        for (int i = 0; i <= parentList.transform.childCount - 1; i++)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }
        await Task.Delay(1);
        gridCollection.UpdateCollection();
    }

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
    /// Activate a Gameobject if it is not active and move it in front of the camera. Deactivate it if it is active.
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
    /// Activate a Gameobject if it is not active and move it in front of the camera. Deactivate it if it is active.
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
    /// Activate a Gameobject if it is not active and move it in front of the camera. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void DisableSettingsMenu()
    {
            menu.SetActive(false);
    }
}
