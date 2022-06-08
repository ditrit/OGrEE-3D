using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Input;

public class MenuChoiceSite : MonoBehaviour
{
    public static ListGenerator instance;
    public GameObject GameManagerTest;
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
    public GameObject menu;
    private string tenant;
    [SerializeField][Tooltip("Assign DialogLarge_192x192.prefab")] private GameObject dialogPrefabLarge;
    [SerializeField] private List<string> parentNames = new List<string>();
    private bool bool1 = false;
    private bool bool2 = false;
    private bool bool3 = true;
    private bool bool4 = true;
    private Dialog myDialog;

    // Start is called before the first frame update
    private async Task Start()
    {
        await Task.Delay(10);
        tenant = ListGenerator.instance.tenant;
        parentNames.Add(tenant);
        siteIcon.SetActive(true);
        rackIcon.SetActive(true);

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
        resulstInfos.transform.position = parentListAndButtons.transform.position + new Vector3(0, 0, 0);
        TextMeshPro tmp = resulstInfos.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(0.25f, 0.05f);
        tmp.fontSize = 0.12f;
        tmp.alignment = TextAlignmentOptions.Center;

        GameObject g = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentListAndButtons.transform);
        GameObject gmdi = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentListAndButtons.transform);

        g.transform.localPosition = new Vector3 (0, -0.08f, 0);
        g.name = "rack";
        g.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = "Device Type = " + g.name;
        g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => 
        {
            ApiListener.instance.deviceType = g.name;
            bool1 = true;
            gmdi.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
            g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
        });


        gmdi.transform.localPosition = new Vector3 (0, -0.13f, 0);
        gmdi.name = "mdi";
        gmdi.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = "Device Type = " + gmdi.name;
        gmdi.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => 
        {
            ApiListener.instance.deviceType = gmdi.name;
            bool1 = true;
            gmdi.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
            g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
        });
        gridCollection.UpdateCollection();

        await Task.Delay(50);
        List<SApiObject> physicalObjects = await ApiManager.instance.GetObjectVincent($"tenants/{tenant}/sites");
        InstantiateMenuForSite(physicalObjects, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (bool1 && bool2 && bool3)
        {
            parentListAndButtons.SetActive(false);
            parentListAndButtons.transform.SetParent(menu.transform);
            parentListAndButtons.transform.localPosition = new Vector3 (0.02f, -0.08f, 0);    
            Destroy(transform.parent.Find("Backplate").gameObject);
            myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
            myDialog.GetComponent<Follow>().MinDistance = 0.5f;
            myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
            myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
            myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
            myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
            bool3 = false;
        }

        if (!bool3 && bool4)
        {
            if (myDialog.State != DialogState.Closed)
            {
                GameManagerTest.GetComponent<SpeechInputHandler>().enabled = true;
                bool4 = false;
            }
        }
    }

    public void ToggleHelpMenu()
    {
        myDialog = Dialog.Open(DialogPrefabLarge, DialogButtonType.Confirm, "Voice Commands List", $"To take a picture of a label --> Say 'Photo'\n\nTo open a search window to choose a rack (if the search window is open, the voice command deactivate the search window) --> Say 'Search'\n\nIf a rack was loaded and you want to place it in front of you --> Say 'Move Rack'\n\nTo make the menu pop up (if the menu is open, the voice command deactivate the menu) --> Say 'Menu'\n\nTo display information about the selected object --> Say 'Info'", true);
        myDialog.GetComponent<Follow>().MinDistance = 0.5f;
        myDialog.GetComponent<Follow>().MaxDistance = 0.7f;
        myDialog.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.5f;
        myDialog.GetComponent<ConstantViewSize>().MinDistance = 0.7f;
        myDialog.GetComponent<ConstantViewSize>().MinScale = 0.05f;
    }

    /// <summary>
    /// Large Dialog example prefab to display
    /// </summary>
    public GameObject DialogPrefabLarge
    {
        get => dialogPrefabLarge;
        set => dialogPrefabLarge = value;
    }
    
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
            g.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
            {
                ApiListener.instance.site = obj.name;
                if (bool2)
                {
                    for (int i = _pageNumber * numberOfResultsPerPage; i <= iteratorUpperBound; i++)
                    {
                        string name = parentNames[parentNames.Count - 1] + "." +  _physicalObjects[i].name;
                        if (name != fullname)
                            transform.Find($"{name}/BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    }
                }
                g.transform.Find("BackPlate/Quad").gameObject.GetComponent<Renderer>().material.color = Color.green;
                bool2 = true;

            });
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
