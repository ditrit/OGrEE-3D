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

    private bool isTenant;
    // Start is called before the first frame update
    void Start()
    {
        numberOfResultsPerPage = gridCollection.Rows;
        buttonReturn = Instantiate(buttonReturnPrefab, transform.position + new Vector3(-0.25f, (numberOfResultsPerPage - 1) * 0.02f, 0), Quaternion.identity, transform);
        buttonReturn.SetActive(false);

    }

    private async void OnEnable()
    {
        List<SApiObject> tmp = await ApiManager.instance.GetObjectAPIMenu("tenants", null);
        if (tmp != null)
        {
            physicalObjects = tmp;
            pageNumber = 0;
            UpdateGrid(physicalObjects.Count, AssignButtonFunction);
        } 
    }

    private void AssignButtonFunction(int index)
    {
        isTenant = false;
        bool isRack = false;

        SApiObject obj = physicalObjects[index];
        string category = obj.category;
        string subCat = null;

        string fullname = ApiManager.instance.parentNames[ApiManager.instance.parentNames.Count - 1] + "." + obj.name;
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
                List<SApiObject> tmp = await ApiManager.instance.GetObjectAPIMenu(nextCall, fullname);
                if (tmp != null)
                {
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

    protected new void UpdateGrid(int ObjectNumber, GridButtonDelegate buttonDelegate)
    {
        UpdateGridDefault(ObjectNumber, buttonDelegate);

        buttonReturn.SetActive(!isTenant);
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
        buttonReturn.GetComponent<ButtonConfigHelper>().OnClick.AddListener(async () =>
        {
            List<SApiObject> tmp = await ApiManager.instance.GetObjectAPIMenu(ApiManager.instance.previousCalls[ApiManager.instance.previousCalls.Count - 2], ApiManager.instance.parentNames[ApiManager.instance.parentNames.Count - 2]);
            if (tmp != null)
            {
                physicalObjects = tmp;
                pageNumber = 0;
                UpdateGrid(physicalObjects.Count, AssignButtonFunction);
            }
        });
    }
}
