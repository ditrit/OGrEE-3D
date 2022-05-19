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
    private GridObjectCollection gridCollection;
    private GameObject buttonLeft;
    private GameObject buttonRight;
    private GameObject buttonReturn;
    private GameObject resulstInfos;
    public int numberOfResultsPerPage = 5;

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

        Vector3 offsetLeft = new Vector3 (-0.2f, 0, 0);
        buttonLeft = Instantiate(buttonLeftPrefab, parentListAndButtons.transform.position + offsetLeft, Quaternion.identity, parentListAndButtons.transform);
        buttonLeft.SetActive(false);

        Vector3 offsetRight = new Vector3 (0.2f, 0, 0);
        buttonRight = Instantiate(buttonRightPrefab, parentListAndButtons.transform.position + offsetRight , Quaternion.identity, parentListAndButtons.transform);
        buttonRight.SetActive(false);

        buttonReturn = Instantiate(buttonReturnPrefab, parentListAndButtons.transform.position + new Vector3 (-0.2f, +0.1f, 0) , Quaternion.identity, parentListAndButtons.transform);
        buttonReturn.SetActive(false);

        resulstInfos = new GameObject();
        resulstInfos.name = "Results Infos";
        resulstInfos.transform.SetParent(parentListAndButtons.transform);
        resulstInfos.transform.position = parentListAndButtons.transform.position + new Vector3 (0.25f, -0.1f, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();
        tmp.fontSize = 0.1f;
        tmp.rectTransform.sizeDelta  = new Vector2 (0.1f, 0.05f);
        tmp.autoSizeTextContainer = true;
        tmp.fontSizeMax = 0.1f;
        tmp.fontSizeMin = 0.05f;
        tmp.ForceMeshUpdate( true );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ///<summary>
    /// Instantiate Buttons for the objects on the provided pageNumber.
    ///</summary>
    ///<param name="_physicalObjects">List of the data for each object in the API response to use</param>
    ///<param name="_parentName">Name of the parent starting from the tenant</param>
    ///<param name="_pageNumber">Number of the pahe to display</param>

    public void InstantiateByIndex(List<SApiObject> _physicalObjects, List<string> _parentNames, int _pageNumber, List<string> _previousCalls)
    {   
        bool isSites = false;

        int maxNumberOfPage = 0;

        if (_physicalObjects.Count  % numberOfResultsPerPage == 0)
            maxNumberOfPage = _physicalObjects.Count  / numberOfResultsPerPage;
        else
            maxNumberOfPage = _physicalObjects.Count  / numberOfResultsPerPage +1;
        
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
            switch (category)
            {
                case "site":
                    subCat = "buildings";
                    isSites = true;
                    break;
                case "building":
                    subCat = "rooms";
                    break;
                case "room":
                    subCat = "racks";
                    break;
            }
            string nextCall = $"{category}s/{obj.id}/{subCat}";
            GameObject g = Instantiate(buttonPrefab, Vector3.zero , Quaternion.identity, parentList.transform);
            string fullname = _parentNames[_parentNames.Count - 1] + "." + obj.name;
            g.name = fullname;
            g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;
            g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => ApiManager.instance.GetObjectVincent(nextCall, fullname));
            gridCollection.UpdateCollection();
        }
        if (isSites)
            buttonReturn.SetActive(false);
        else
            buttonReturn.SetActive(true);
            buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => ApiManager.instance.GetObjectVincent(_previousCalls[_previousCalls.Count - 2], _parentNames[_parentNames.Count - 2]));

    }

    public void CreateList(List<SApiObject> _physicalObjects, string parentName)
    {
        gridCollection.UpdateCollection();
        /*if (_physicalObjects.Count > numberOfResultsPerPage)
            buttonRight.SetActive(true);*/

        foreach (SApiObject obj in _physicalObjects)
        {
            string category = obj.category;
            string subCat = null;
            switch (category)
            {
                case "tenant":
                    subCat = "sites";
                    break;
                case "site":
                    subCat = "buildings";
                    break;
                case "building":
                    subCat = "rooms";
                    break;
                case "room":
                    subCat = "devices";
                    break;
            }
            string call = $"{category}s/{obj.id}/{subCat}";
            GameObject g = Instantiate(buttonPrefab, Vector3.zero , Quaternion.identity, parentList.transform);
            string fullname = parentName + "." + obj.name;
            g.name = fullname;
            g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;
            g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => ApiManager.instance.GetObjectVincent(call, fullname));
            gridCollection.UpdateCollection();
        }
    }

    public async void ClearParentList()
    {
        for (int i = 0; i <= parentList.transform.childCount - 1; i++)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }
        await Task.Delay(5);
        gridCollection.UpdateCollection();
    }
}