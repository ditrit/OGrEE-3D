using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class SearchMenuManager : MonoBehaviour
{
    public static SearchMenuManager instance;
    public GameObject parentList;
    public GameObject SearchMenu;
    public GameObject buttonPrefab;
    public GameObject buttonRightPrefab;
    public GameObject buttonLeftPrefab;
    public GameObject buttonReturnPrefab;
    public GameObject buildingIcon;
    public GameObject siteIcon;
    public GameObject roomIcon;
    public GameObject rackIcon;
    private GridObjectCollection gridCollection;
    private GameObject buttonLeft;
    private GameObject buttonRight;
    private GameObject buttonReturn;
    private GameObject resulstInfos;
    public int numberOfResultsPerPage;
    private string[] array;
    [SerializeField] private List<string> previousCalls = new List<string>();
    [SerializeField] private List<string> parentNames = new List<string>();
    public string tenant;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    private async Task Start()
    {
        previousCalls.Add($"tenants/{tenant}/sites");
        await Task.Delay(1000);
        parentNames.Add(tenant);
        InitializeIcons();
        InitializeButtons();
        SearchMenu.SetActive(false);
    }

    ///<summary>
    /// Activate the site icon, deactivate all other icons
    ///</summary>
    public void InitializeIcons()
    {
        siteIcon.SetActive(true);
        buildingIcon.SetActive(false);
        roomIcon.SetActive(false);
        rackIcon.SetActive(false);
    }

    ///<summary>
    /// Initialize the left, right, return buttons as well as the result infos tmp
    ///</summary>
    public void InitializeButtons()
    {
        gridCollection = parentList.GetComponent<GridObjectCollection>();
        numberOfResultsPerPage = gridCollection.Rows;

        Vector3 offsetLeft = new Vector3(-0.15f, 0, 0);
        buttonLeft = Instantiate(buttonLeftPrefab, SearchMenu.transform.position + offsetLeft, Quaternion.identity, SearchMenu.transform);
        buttonLeft.SetActive(false);

        Vector3 offsetRight = new Vector3(0.15f, 0, 0);
        buttonRight = Instantiate(buttonRightPrefab, SearchMenu.transform.position + offsetRight, Quaternion.identity, SearchMenu.transform);
        buttonRight.SetActive(false);

        buttonReturn = Instantiate(buttonReturnPrefab, SearchMenu.transform.position + new Vector3(-0.15f, (numberOfResultsPerPage - 1) * 0.02f, 0), Quaternion.identity, SearchMenu.transform);
        buttonReturn.SetActive(false);

        resulstInfos = new GameObject();
        resulstInfos.name = "Results Infos";
        resulstInfos.transform.SetParent(SearchMenu.transform);
        resulstInfos.transform.position = SearchMenu.transform.position + new Vector3(0, numberOfResultsPerPage * -0.025f, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(0.25f, 0.05f);
        tmp.fontSize = 0.2f;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    ///<summary>
    /// Initialize the local variable tenant with the config file
    ///</summary>
    ///<param name="_tenant">Tenant name from the config file</param>
    public void InitializeTenant(string _tenant)
    {
        tenant = _tenant;
    }

    ///<summary>
    /// Deactivate the search menu if active, active it and set it on the site selection if not active.
    ///</summary>
    public async void ToggleSearchMenu()
    {
        if (SearchMenu.activeSelf)
        {
            SearchMenu.SetActive(false);
        }
        else
        {
            SearchMenu.SetActive(true);
            SearchMenu.transform.Find("Results Infos").GetComponent<TextMeshPro>().fontSize = 0.2f;
            List<SApiObject> physicalObjects = await ApiManager.instance.GetObjectVincent($"tenants/{tenant}/sites");
            ClearParentList();
            InstantiateByIndex(physicalObjects, 0);
        }
    }

    ///<summary>
    /// Instantiate Buttons for the provided objects on the given pageNumber. Load 3D models when a rack is selected.
    ///</summary>
    ///<param name="_physicalObjects">List of the data for each object in the API response to use</param>
    ///<param name="_parentName">Name of the parent starting from the tenant</param>
    ///<param name="_pageNumber">Number of the pahe to display</param>
    public void InstantiateByIndex(List<SApiObject> _physicalObjects, int _pageNumber)
    {
        bool isSite = false;
        bool isRack = false;

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
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => instance.InstantiateByIndex(_physicalObjects, _pageNumber - 1));
        }
        else
            buttonLeft.SetActive(false);

        if (_pageNumber + 1 < maxNumberOfPage)
        {
            buttonRight.SetActive(true);
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => InstantiateByIndex(_physicalObjects, _pageNumber + 1));
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
            string category = obj.category;
            string subCat = null;
            string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;
            switch (category)
            {
                case "site":
                    subCat = "buildings";
                    siteIcon.SetActive(true);
                    buildingIcon.SetActive(false);
                    roomIcon.SetActive(false);
                    rackIcon.SetActive(false);
                    isSite = true;
                    break;
                case "building":
                    subCat = "rooms";
                    siteIcon.SetActive(true);
                    buildingIcon.SetActive(true);
                    roomIcon.SetActive(false);
                    rackIcon.SetActive(false);                    
                    break;
                case "room":
                    subCat = "racks";
                    siteIcon.SetActive(true);
                    buildingIcon.SetActive(true);
                    roomIcon.SetActive(true);
                    rackIcon.SetActive(false);                    
                    break;
                case "rack":
                    isRack = true;
                    siteIcon.SetActive(true);
                    buildingIcon.SetActive(true);
                    roomIcon.SetActive(true);
                    rackIcon.SetActive(true);                    
                    break;
            }
            string nextCall = $"{category}s/{obj.id}/{subCat}";
            GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
            g.name = fullname;
            g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;
            if (!isRack)
                g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () => 
                {
                    previousCalls.Add(nextCall);
                    parentNames.Add(fullname);
                    List<SApiObject> physicalObjects = await ApiManager.instance.GetObjectVincent(nextCall);
                    ClearParentList();
                    InstantiateByIndex(physicalObjects, 0);
                });
            else
            {
                g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
                {
                    array = Utils.SplitRackHierarchyName(fullname);
                    await ApiListener.instance.LoadSingleRack(array[0], array[1], array[2], array[3], array[4]);
                });
                g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => SearchMenu.SetActive(false));

            }
            gridCollection.UpdateCollection();
        }
        if (isSite)
            buttonReturn.SetActive(false);
        else
            buttonReturn.SetActive(true);
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () => 
        {
            string prevCall = previousCalls[previousCalls.Count - 2];
            string prevName = parentNames[parentNames.Count - 2];

            previousCalls.RemoveAt(previousCalls.Count - 1);
            parentNames.RemoveAt(parentNames.Count - 1);
          
            List<SApiObject> physicalObjects = await ApiManager.instance.GetObjectVincent(prevCall);
            ClearParentList();
            InstantiateByIndex(physicalObjects, 0);
        });
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
