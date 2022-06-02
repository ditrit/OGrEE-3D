using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class MenuChoiceSite : MonoBehaviour
{
    public static ListGenerator instance;
    public GameObject parentList;
    public GameObject parentListAndButtons;
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
    private string tenant = "EDF";
    [SerializeField] private List<string> parentNames = new List<string>();

    // Start is called before the first frame update
    private async Task Start()
    {
        parentNames.Add(tenant);
        siteIcon.SetActive(true);
        rackIcon.SetActive(false);

        gridCollection = parentList.GetComponent<GridObjectCollection>();
        numberOfResultsPerPage = gridCollection.Rows;

        Vector3 offsetLeft = new Vector3(-0.15f, 0, 0);
        buttonLeft = Instantiate(buttonLeftPrefab, parentListAndButtons.transform.position + offsetLeft, Quaternion.identity, parentListAndButtons.transform);
        buttonLeft.SetActive(false);

        Vector3 offsetRight = new Vector3(0.15f, 0, 0);
        buttonRight = Instantiate(buttonRightPrefab, parentListAndButtons.transform.position + offsetRight, Quaternion.identity, parentListAndButtons.transform);
        buttonRight.SetActive(false);

        resulstInfos = new GameObject();
        resulstInfos.name = "Results Infos";
        resulstInfos.transform.SetParent(parentListAndButtons.transform);
        resulstInfos.transform.position = parentListAndButtons.transform.position + new Vector3(0, numberOfResultsPerPage * -0.025f, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(0.25f, 0.05f);
        tmp.fontSize = 0.2f;
        tmp.alignment = TextAlignmentOptions.Center;

        await Task.Delay(100);
        parentListAndButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void InstantiateMenuForSite(List<SApiObject> _physicalObjects, int _pageNumber)
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
            string category = obj.category;
            string subCat = null;
            string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;
            switch (category)
            {
                case "site":
                    subCat = "buildings";
                    isSite = true;
                    break;
            }

            GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
            g.name = fullname;
            g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;
            g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => Photo_Capture.instance.site = obj.name);
            g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => parentListAndButtons.SetActive(false));
            gridCollection.UpdateCollection();
        }
    }

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
