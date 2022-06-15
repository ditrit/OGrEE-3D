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

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager instance;
    public ConfigLoader configLoader = new ConfigLoader();
    public GameObject GameManagerTest;
    public GameObject parentList;
    public GameObject SettingsMenu;
    public GameObject buttonPrefab;
    public GameObject buttonRightPrefab;
    public GameObject buttonLeftPrefab;
    public GameObject siteIcon;
    public GameObject rackIcon;
    private GridObjectCollection gridCollection;
    private GameObject buttonLeft;
    private GameObject buttonRight;
    private GameObject resulstInfos;
    public int numberOfResultsPerPage;
    public GameObject menu;
    public string tenant;
    [SerializeField][Tooltip("Assign DialogLarge_192x192.prefab")] private GameObject dialogPrefabLarge;
    [SerializeField] private List<string> parentNames = new List<string>();
    [SerializeField] private bool isDeviceTypeSelected = false;
    [SerializeField] private bool isSiteSelected = false;
    [SerializeField] private bool isFirstHelpMenuCreated = false;
    [SerializeField] private bool isFirstHelpMenuOpen = false;
    [SerializeField] private bool isHelpDialogActive = false;
    private Dialog myDialog;

    // Start is called before the first frame update
    private async Task Start()
    {
        configLoader.LoadConfig();
        tenant = configLoader.GetTenant();

        //tenant = SearchMenuManager.instance.tenant;
        parentNames.Add(tenant);
        siteIcon.SetActive(true);
        rackIcon.SetActive(true);

        gridCollection = parentList.GetComponent<GridObjectCollection>();
        numberOfResultsPerPage = gridCollection.Rows;

        Vector3 offsetLeft = new Vector3(-0.15f, 0, 0);
        buttonLeft = Instantiate(buttonLeftPrefab, SettingsMenu.transform.position + offsetLeft, Quaternion.identity, SettingsMenu.transform);
        buttonLeft.SetActive(false);

        Vector3 offsetRight = new Vector3(0.15f, 0, 0);
        buttonRight = Instantiate(buttonRightPrefab, SettingsMenu.transform.position + offsetRight, Quaternion.identity, SettingsMenu.transform);
        buttonRight.SetActive(false);


        resulstInfos = new GameObject("Results Infos");
        resulstInfos.transform.SetParent(SettingsMenu.transform);
        resulstInfos.transform.position = SettingsMenu.transform.position + new Vector3(0, 0, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();

        SettingsMenu.SetActive(true);  // bug with tmp font size when object is not active
        tmp.rectTransform.sizeDelta = new Vector2(0.25f, 0.05f);
        tmp.fontSize = 0.12f;
        SettingsMenu.SetActive(false);

        tmp.alignment = TextAlignmentOptions.Center;

        GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, SettingsMenu.transform);
        GameObject gmdi = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, SettingsMenu.transform);

        g.transform.localPosition = new Vector3(0, -0.08f, 0);
        g.name = "rack";
        g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = "Device Type = " + g.name;
        g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            ApiListener.instance.deviceType = g.name;
            isDeviceTypeSelected = true;
            gmdi.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
            g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
        });


        gmdi.transform.localPosition = new Vector3(0, -0.15f, 0);
        gmdi.name = "mdi";
        gmdi.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = "Device Type = " + gmdi.name;
        gmdi.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        gmdi.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            ApiListener.instance.deviceType = gmdi.name;
            isDeviceTypeSelected = true;
            gmdi.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
            g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        });
        gridCollection.UpdateCollection();

        await Task.Delay(3000);
        List<SApiObject> physicalObjects = await ApiManager.instance.GetObjectVincent($"tenants/{tenant}/sites");
        InstantiateMenuForSite(physicalObjects, 0);
    }

    /// <summary>
    /// Large Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }

    ///<summary>
    /// Open a dialog with voice commands when settings are done
    ///</summary>
    void Update()
    {
        if (isDeviceTypeSelected && isSiteSelected && !isFirstHelpMenuCreated)
        {
            SettingsMenu.SetActive(false);
            SettingsMenu.GetComponent<Follow>().enabled = false;
            SettingsMenu.transform.SetParent(menu.transform);
            SettingsMenu.transform.localPosition = new Vector3(0.02f, -0.08f, 0);
            SettingsMenu.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Destroy(SettingsMenu.transform.Find("Backplate").gameObject);
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
    /// Instantiate Buttons for the provided objects on the given pageNumber. Buttons set variables in ApiListener class
    ///</summary>
    ///<param name="_physicalObjects">List of the data for each object in the API response to use</param>
    ///<param name="_pageNumber">Number of the pahe to display</param>
    public void InstantiateMenuForSite(List<SApiObject> _physicalObjects, int _pageNumber)
    {
        int maxNumberOfPage = 0;
        if (_physicalObjects.Count % numberOfResultsPerPage == 0)
            maxNumberOfPage = _physicalObjects.Count / numberOfResultsPerPage;
        else
            maxNumberOfPage = _physicalObjects.Count / numberOfResultsPerPage + 1;

        ClearParentList();
        TextMeshPro tmp = resulstInfos.GetComponent<TextMeshPro>();
        tmp.text = $"{_physicalObjects.Count} Results. Page {_pageNumber + 1} / {maxNumberOfPage}";
        if (_pageNumber > 0)
        {
            buttonLeft.SetActive(true);
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => InstantiateMenuForSite(_physicalObjects, _pageNumber - 1));
        }
        else
            buttonLeft.SetActive(false);

        if (_pageNumber + 1 < maxNumberOfPage)
        {
            buttonRight.SetActive(true);
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => InstantiateMenuForSite(_physicalObjects, _pageNumber + 1));
        }
        else
            buttonRight.SetActive(false);

        int iteratorUpperBound = 0;

        if (_pageNumber + 1 == maxNumberOfPage)
            iteratorUpperBound = _physicalObjects.Count - 1;
        else
            iteratorUpperBound = (_pageNumber + 1) * numberOfResultsPerPage - 1;

        Debug.Log($"Number of Results: {_physicalObjects.Count}, Page Number: {_pageNumber}, MaxNumberOfPage: {maxNumberOfPage}, NumberOfResultsPerPage: {numberOfResultsPerPage}, iteratorUpperBound: {iteratorUpperBound}");
        for (int i = _pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
        {
            SApiObject obj = _physicalObjects[i];
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
                    for (int i = _pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
                    {
                        string name = parentNames[parentNames.Count - 1] + "." + _physicalObjects[i].name;
                        if (name != fullname)
                            transform.Find($"{name}/BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    }
                }
                g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
                isSiteSelected = true;

            });
            gridCollection.UpdateCollection();
        }
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
}
