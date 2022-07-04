using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

public class APIMenuHandler : GridMenuHandler
{
    public GameObject buttonReturnPrefab;
    private GameObject buttonReturn;
    public List<SApiObject> physicalObjects;
    public GameObject buttonPrefab;
    public GameObject buttonLoadPrefab;
    public GameObject buttonUnLoadPrefab;
    public Camera mainCamera;

    private bool isTenant;
    private List<string> previousCalls = new List<string>();
    private List<string> parentNames = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        numberOfResultsPerPage = gridCollection.Rows;
        buttonReturn = Instantiate(buttonReturnPrefab, transform.position + new Vector3(-0.25f, (numberOfResultsPerPage - 1) * 0.02f, 0), Quaternion.identity, transform);
        buttonReturn.SetActive(false);

    }

    private async void OnEnable()
    {
        List<SApiObject> tmp = await ApiManager.instance.GetObject("tenants", ApiManager.instance.GetAllSApiObject);
        if (tmp != null)
        {
            previousCalls.Add("tenants");
            parentNames.Add("");
            physicalObjects = tmp;
            pageNumber = 0;
            UpdateGrid(physicalObjects.Count, AssignButtonFunction);
        }
        Utils.MoveObjectToCamera(gameObject, mainCamera);
        gameObject.transform.Rotate(0, -90, 0);
        gameObject.transform.Translate(0, 0.5f, 0);
    }


    ///<summary>
    /// Instantiates three buttons: one to load the object, one to delete it and one to display the list of its children
    ///</summary>
    ///<param name="_index">the element index in physicalObjects</param>
    private void AssignButtonFunction(int _index)
    {
        isTenant = false;
        bool isRack = false;
        SApiObject obj = physicalObjects[_index];
        string category = obj.category;
        string subCat = null;

        string fullname = parentNames[parentNames.Count - 1] + "." + obj.name;
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
            button.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
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

        GameObject buttonLoadItem = Instantiate(buttonLoadPrefab, Vector3.zero, Quaternion.identity, empty.transform);
        buttonLoadItem.transform.Translate(button.GetComponent<Collider>().bounds.size.x * 0.5f + buttonLoadItem.GetComponent<Collider>().bounds.size.x * 0.5f, 0, 0);
        buttonLoadItem.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
        {
            await LoadOObject(fullname);

        });

        GameObject buttonUnLoadItem = Instantiate(buttonUnLoadPrefab, Vector3.zero, Quaternion.identity, empty.transform);
        buttonUnLoadItem.transform.Translate(-button.GetComponent<Collider>().bounds.size.x * 0.5f - buttonUnLoadItem.GetComponent<Collider>().bounds.size.x * 0.5f, 0, 0);
        buttonUnLoadItem.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            UnLoadOObject(fullname);
        });

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
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0], ApiManager.instance.DrawObject);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0]);
                break;
            case 2:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1], ApiManager.instance.DrawObject);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1]);
                break;
            case 3:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2], ApiManager.instance.DrawObject);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2]);
                break;

            case 4:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3], ApiManager.instance.DrawObject);
                oObject = GameManager.gm.FindByAbsPath(splittedName[0] + "." + splittedName[1] + "." + splittedName[2] + "." + splittedName[3]);
                break;
            case 5:
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3], ApiManager.instance.DrawObject);
                await ApiManager.instance.GetObject($"tenants/" + splittedName[0] + "/sites/" + splittedName[1] + "/buildings/" + splittedName[2] + "/rooms/" + splittedName[3] + "/racks/" + splittedName[4], ApiManager.instance.DrawObject);
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
        ogree.SetBaseTransform(oObject.transform.localPosition, oObject.transform.localRotation, oObject.transform.localScale);

        await ogree.LoadChildren((5 - splittedName.Length).ToString());
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }


    ///<summary>
    /// Load the 3D model of an OObject
    ///</summary>
    ///<param name="_fullname">full hierarchy name of the object</param>
    public void UnLoadOObject(string _fullname)
    {
        if (_fullname[0] == '.')
            _fullname = _fullname.Remove(0, 1);
        Destroy(GameManager.gm.FindByAbsPath(_fullname));
    }


    ///<summary>
    /// Calls UpdateGridDefault then set the return button according to the previous call
    ///</summary>
    ///<param name="_objectNumber">the total number of elements</param>
    ///<param name="_elementDelegate">the function to apply to each displayed element index</param>
    protected new void UpdateGrid(int _elementNumber, ElementDelegate _elementDelegate)
    {
        UpdateGridDefault(_elementNumber, _elementDelegate);

        buttonReturn.SetActive(!isTenant);
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
        {
            List<SApiObject> tmp = await ApiManager.instance.GetObject(previousCalls[previousCalls.Count - 2], ApiManager.instance.GetAllSApiObject);
            if (tmp != null)
            {
                string prevCall = previousCalls[previousCalls.Count - 2];

                previousCalls.RemoveAt(previousCalls.Count - 1);
                parentNames.RemoveAt(parentNames.Count - 1);
                physicalObjects = tmp;
                pageNumber = 0;
                UpdateGrid(physicalObjects.Count, AssignButtonFunction);
            }
        });
    }
}
