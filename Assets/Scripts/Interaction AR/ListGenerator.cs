using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class ListGenerator : MonoBehaviour
{
    public static ListGenerator instance;
    public GameObject parentList;
    public GameObject parentListAndButtons;
    public GameObject buttonPrefab;
    public GameObject buttonRightPrefab;
    public GameObject buttonLeftPrefab;
    public GameObject buttonReturnPrefab;
    public GameObject buttonLoadPrefab;
    public GameObject buttonUnLoadPrefab;
    public GameObject GridMenuTemplate;

    private GridObjectCollection gridCollection;
    private GameObject buttonLeft;
    private GameObject buttonRight;
    private GameObject buttonReturn;
    private GameObject resulstInfos;
    public int numberOfResultsPerPage;
    public string[] array;
    public string selectionnedObject = "";
    public string selectionnedObjectNextCall = "";

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

        gridCollection = parentList.GetComponent<GridObjectCollection>();
        numberOfResultsPerPage = gridCollection.Rows;

        Vector3 offsetLeft = new Vector3(-0.25f, 0, 0);
        buttonLeft = Instantiate(buttonLeftPrefab, parentListAndButtons.transform.position + offsetLeft, Quaternion.identity, parentListAndButtons.transform);
        buttonLeft.SetActive(false);

        Vector3 offsetRight = new Vector3(0.25f, 0, 0);
        buttonRight = Instantiate(buttonRightPrefab, parentListAndButtons.transform.position + offsetRight, Quaternion.identity, parentListAndButtons.transform);
        buttonRight.SetActive(false);

        buttonReturn = Instantiate(buttonReturnPrefab, parentListAndButtons.transform.position + new Vector3(-0.25f, (numberOfResultsPerPage - 1) * 0.02f, 0), Quaternion.identity, parentListAndButtons.transform);
        buttonReturn.SetActive(false);

        resulstInfos = new GameObject();
        resulstInfos.name = "Results Infos";
        resulstInfos.transform.SetParent(parentListAndButtons.transform);
        resulstInfos.transform.position = parentListAndButtons.transform.position + new Vector3(0, numberOfResultsPerPage * -0.025f, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(0.25f, 0.05f);
        tmp.fontSize = 0.2f;
        tmp.alignment = TextAlignmentOptions.Center;
        print(tmp.fontSize);
    }


    ///<summary>
    /// Instantiate Buttons for the objects on the provided pageNumber.
    ///</summary>
    ///<param name="_physicalObjects">List of the data for each object in the API response to use</param>
    ///<param name="_parentName">Name of the parent starting from the tenant</param>
    ///<param name="_pageNumber">Number of the page to display</param>

    public void InstantiateByIndex(List<SApiObject> _physicalObjects, List<string> _parentNames, int _pageNumber, List<string> _previousCalls)
    {
        bool isTenant = false;
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
            buttonLeft.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => instance.InstantiateByIndex(_physicalObjects, _parentNames, _pageNumber - 1, _previousCalls));
        }
        else
            buttonLeft.SetActive(false);

        if (_pageNumber + 1 < maxNumberOfPage)
        {
            buttonRight.SetActive(true);
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonRight.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => InstantiateByIndex(_physicalObjects, _parentNames, _pageNumber + 1, _previousCalls));
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
            string fullname = _parentNames[_parentNames.Count - 1] + "." + obj.name;
            switch (category)
            {
                case "tenant":
                    subCat = "sites";
                    isTenant = true;
                    break;
                case "site":
                    subCat = "buildings";
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
            GameObject empty = new GameObject(fullname + " buttons");
            empty.transform.parent = parentList.transform;
            empty.transform.localScale = Vector3.one;

            GameObject button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, empty.transform);
            button.name = fullname;
            button.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;
            if (!isRack)
            {
                button.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
                {
                    ApiManager.instance.GetObjectVincent(nextCall, fullname);
                });
            }

            GameObject buttonLoadItem = Instantiate(buttonLoadPrefab, Vector3.zero, Quaternion.identity, empty.transform);
            buttonLoadItem.transform.Translate(button.GetComponent<Collider>().bounds.size.x * 0.5f + buttonLoadItem.GetComponent<Collider>().bounds.size.x * 0.5f, 0, 0);
            buttonLoadItem.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
            {
                await LoadOObject(fullname);
            });

            GameObject buttonUnLoadItem = Instantiate(buttonUnLoadPrefab, Vector3.zero, Quaternion.identity, empty.transform);
            buttonUnLoadItem.transform.Translate(- button.GetComponent<Collider>().bounds.size.x * 0.5f - buttonUnLoadItem.GetComponent<Collider>().bounds.size.x * 0.5f, 0, 0);
            buttonUnLoadItem.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
            {
                UnLoadOObject(fullname);
            });

            gridCollection.UpdateCollection();
        }
        if (isTenant)
            buttonReturn.SetActive(false);
        else
            buttonReturn.SetActive(true);
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => ApiManager.instance.GetObjectVincent(_previousCalls[_previousCalls.Count - 2], _parentNames[_parentNames.Count - 2]));

    }


    public async void ClearParentList()
    {
        for (int i = 0; i < parentList.transform.childCount; i++)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }
        await Task.Delay(1);
        gridCollection.UpdateCollection();
    }


    ///<summary>
    /// Load the 3D model of an OObject
    ///</summary>
    ///<param name="_fullname">full hierarchy name of the object</param>
    public async Task LoadOObject(string _fullname)
    {

        string[] splittedName = Utils.SplitHierarchyName(_fullname);
        GameObject oObject;
        switch (splittedName.Length)
        {
            case 0:
                throw new System.Exception("fullname is empty or not formatted : " + _fullname);
            case 1:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0]);
                break;
            case 2:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1]);
                break;
            case 3:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2]);
                break;

            case 4:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2] + "." + splittedName[3]);
                break;
            case 5:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3]);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3] + "/racks/" + splittedName[4]);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2] + "." + splittedName[3] + "." + splittedName[4]);
                break;
            default:
                throw new System.Exception("fullname is empty or not formatted : " + _fullname);

        }

        if (oObject != null)
            GameManager.gm.AppendLogLine("OObject Found in the scene after loading from API", "green");
        else
            GameManager.gm.AppendLogLine("OObject NOT Found in the scene after loading from API", "red");

        OgreeObject ogree = oObject.GetComponent<OgreeObject>();
        ogree.originalLocalRotation = oObject.transform.localRotation;
        ogree.originalLocalPosition = oObject.transform.localPosition;

        await ogree.LoadChildren((5 - splittedName.Length).ToString());
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }

    public void UnLoadOObject(string _fullname)
    {
        _fullname = _fullname.Remove(0, 1);
        Destroy(GameManager.gm.FindByAbsPath(_fullname));
    }
}
