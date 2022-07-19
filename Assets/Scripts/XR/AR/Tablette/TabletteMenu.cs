using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

public class TabletteMenu : GridMenuHandler
{
    public GameObject buttonReturnPrefab;
    private GameObject buttonReturn;
    public List<SApiObject> physicalObjects;
    public GameObject buttonPrefab;

    [SerializeField] private bool isSite;
    [SerializeField] private List<string> previousCalls = new List<string>();
    [SerializeField] private List<string> parentNames = new List<string>();
    public string tenant;

    // Start is called before the first frame update
    async void Start()
    {
        InitializeUIElements();
        menu.name = "Search Menu";
        menu.transform.localPosition = new Vector3(-150, 250, 0);
        menu.transform.localScale = new Vector3(2,2,2);

        numberOfResultsPerPage = UiManagerVincent.instance.ReturnGridNumberOfRows(gridCollection);
        buttonReturn = Instantiate(buttonReturnPrefab, Vector3.zero, Quaternion.identity, menu.transform);
        buttonReturn.transform.localPosition = new Vector3(-35, 20, 0);
        buttonReturn.SetActive(false);
    }

    ///<summary>
    /// Wait for key parameters and launch inital search.
    ///</summary>
    ///<param name="_index">the element index in physicalObjects</param>
    private async Task OnEnable()
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
        await FirstSearch();
    }


    ///<summary>
    /// Instantiates three buttons: one to load the object, one to delete it and one to display the list of its children
    ///</summary>
    ///<param name="_index">the element index in physicalObjects</param>
    private void AssignButtonFunction(int _index)
    {
        isSite = false;
        bool isRack = false;

        SApiObject obj = physicalObjects[_index];
        string category = obj.category;
        string subCat = null;
        string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;

        switch (category)
        {
            case "tenant":
                subCat = "sites";
                break;
            case "site":
                subCat = "buildings";
                isSite = true;
                break;
            case "building":
                subCat = "rooms";
                break;
            case "room":
                subCat = "racks";
                break;
            case "rack":
                isRack = true;
                break;
        }
        string nextCall = $"{category}s/{obj.id}/{subCat}";

        GameObject button1 = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
        button1.name = fullname;
        UiManagerVincent.instance.ChangeButtonText(button1, fullname);
        UnityEvent onClickEvent = UiManagerVincent.instance.ButtonOnClick(button1);
        if (!isRack)
        {
            onClickEvent.AddListener(async () =>
            {
                List<SApiObject> tmp = await ApiManager.instance.GetObject(nextCall, ApiManager.instance.GetAllSApiObject);
                if (tmp != null)
                {
                    previousCalls.Add(nextCall);
                    parentNames.Add(fullname);
                    physicalObjects = tmp;
                    pageNumber = 0;
                    UpdateGrid(physicalObjects.Count, AssignButtonFunction);
                }
            });
        }
        else
            {
                onClickEvent.AddListener(async () =>
                {
                    string[] array = Utils.SplitRackHierarchyName(fullname);
                    await ApiListener.instance.LoadSingleRack(array[0], array[1], array[2], array[3], array[4]);
                });
                onClickEvent.AddListener(() => menu.SetActive(false));

            }
        }

    ///<summary>
    /// Calls UpdateGridDefault then set the return button according to the previous call
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected new void UpdateGrid(int _elementNumber, ElementDelegate _elementDelegate)
    {
        UpdateGridDefault(_elementNumber, _elementDelegate);
        //gridCollection.UpdateCollection();
        buttonReturn.SetActive(!isSite);
        var onClickEvent1 = UiManagerVincent.instance.ButtonOnClick(buttonReturn);
        onClickEvent1.RemoveAllListeners();
        onClickEvent1.AddListener(async () =>
        {
            List<SApiObject> tmp = await ApiManager.instance.GetObject(previousCalls[previousCalls.Count - 2], ApiManager.instance.GetAllSApiObject);
            if (tmp != null)
            {
                previousCalls.RemoveAt(previousCalls.Count - 1);
                parentNames.RemoveAt(parentNames.Count - 1);
                physicalObjects = tmp;
                pageNumber = 0;
                UpdateGrid(physicalObjects.Count, AssignButtonFunction);
            }
        });
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
    /// Deactivate the search menu if active, active it and set it on the site selection if not active.
    ///</summary>
    public async void ToggleSearchMenu()
    {
        if (menu.activeSelf)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
            Utils.MoveObjectToCamera(menu, Camera.main, 0.6f, -0.25f, 0, 25);
        }
    }

    ///<summary>
    /// Deactivate the search menu if active, active it and set it on the site selection if not active.
    ///</summary>
    public async void ToggleSearchMenuNoMove()
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
}