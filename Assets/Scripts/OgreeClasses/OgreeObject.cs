using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;


public class OgreeObject : MonoBehaviour, IAttributeModif, ISerializationCallbackReceiver
{
    [Header("Standard attributes")]
    public new string name;
    public string hierarchyName;
    public string id;
    public string parentId;
    public string category;
    public List<string> description = new List<string>();
    public string domain; // = tenant

    [Header("Specific attributes")]
    [SerializeField] private List<string> attributesKeys = new List<string>();
    [SerializeField] private List<string> attributesValues = new List<string>();
    public Dictionary<string, string> attributes = new Dictionary<string, string>();

    [Header("LOD")]
    public int currentLod = 0;

    [Header("Internal behavior")]
    private Coroutine updatingCoroutine = null;

    public void OnBeforeSerialize()
    {
        attributesKeys.Clear();
        attributesValues.Clear();
        foreach (var kvp in attributes)
        {
            attributesKeys.Add(kvp.Key);
            attributesValues.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        attributes = new Dictionary<string, string>();
        for (int i = 0; i != Mathf.Min(attributesKeys.Count, attributesValues.Count); i++)
            attributes.Add(attributesKeys[i], attributesValues[i]);
    }

    private void OnEnable()
    {
        UpdateHierarchyName();
    }

    protected virtual void OnDestroy()
    {
        if (category == "tenant")
        {
            Filters.instance.tenantsList.Remove($"<color=#{attributes["color"]}>{name}</color>");
            Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        }
        GameManager.gm.allItems.Remove(hierarchyName);
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public virtual void SetAttribute(string _param, string _value)
    {
        bool updateAttr = false;
        if (_param.StartsWith("description"))
        {
            SetDescription(_param.Substring(11), _value);
            updateAttr = true;
        }
        else
        {
            switch (_param)
            {
                case "label":
                    GetComponent<DisplayObjectData>().SetLabel(_value);
                    break;
                case "labelFont":
                    GetComponent<DisplayObjectData>().SetLabelFont(_value);
                    break;
                case "domain":
                    if (_value.EndsWith("@recursive"))
                    {
                        string[] data = _value.Split('@');
                        SetAllDomains(data[0]);
                    }
                    else
                        SetDomain(_value);
                    updateAttr = true;
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    updateAttr = true;
                    break;
            }
        }
        if (updateAttr && ApiManager.instance.isInit)
            PutData();
        GetComponent<DisplayObjectData>()?.UpdateLabels();
    }

    ///<summary>
    /// Set a description at the correct index.
    ///</summary>
    ///<param name="_index">The index to set the description</param>
    ///<param name="_value">The value of the description</param>
    protected void SetDescription(string _index, string _value)
    {
        string pattern = "^[0-9]+$";
        if (_index != "0" && Regex.IsMatch(_index, pattern))
        {
            int index = int.Parse(_index);
            if (index > description.Count)
            {
                if (index != description.Count + 1)
                    GameManager.gm.AppendLogLine($"Description set at index {description.Count + 1}.", "yellow");
                description.Add(_value);
            }
            else
                description[index - 1] = _value;
        }
        else
            GameManager.gm.AppendLogLine("Wrong description index.", "red");
    }

    ///<summary>
    /// Parse the index and give the correct description.
    ///</summary>
    ///<param name="_index">The index of the wanted description</param>
    ///<returns>The asked description</returns>
    protected string GetDescriptionAt(string _index)
    {
        int index = int.Parse(_index);
        return description[index];
    }

    ///<summary>
    /// Change the OgreeObject's domain
    ///</summary>
    ///<param name="_newDomain">The domain name to assign</param>
    protected void SetDomain(string _newDomain)
    {
        if (GameManager.gm.allItems.ContainsKey(_newDomain))
            domain = _newDomain;
        else
            GameManager.gm.AppendLogLine($"Tenant \"{_newDomain}\" doesn't exist. Please create it before assign it.", "yellow");
    }

    ///<summary>
    /// Change the domain for the OgreeObject and all its children
    ///</summary>
    ///<param name="_newDomain">The domain name to assign</param>
    protected void SetAllDomains(string _newDomain)
    {
        SetAttribute("domain", _newDomain);
        foreach (Transform child in transform)
        {
            if (child.GetComponent<OgreeObject>())
                child.GetComponent<OgreeObject>().SetAttribute("domain", _newDomain);
        }
    }

    ///<summary>
    /// Update the OgreeObject's hierarchyName with it's parent's one.
    ///</summary>
    public string UpdateHierarchyName()
    {
        Transform parent = transform.parent;
        if (parent)
            hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{name}";
        else
            hierarchyName = name;
        return hierarchyName;
    }

    ///<summary>
    /// Update the OgreeObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    ///<param name="_copyAttr">True by default: allows to update attributes dictionary</param>
    public virtual void UpdateFromSApiObject(SApiObject _src, bool _copyAttr = true)
    {
        name = _src.name;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        domain = _src.domain;
        description = _src.description;
        if (_copyAttr)
            attributes = _src.attributes;
    }

    ///<summary>
    /// If a WaitAndPut coroutine is running, stop it. Then, start WaitAndPut.
    ///</summary>
    public void PutData()
    {
        if (updatingCoroutine != null)
            StopCoroutine(updatingCoroutine);
        updatingCoroutine = StartCoroutine(WaitAndPut());
    }

    ///<summary>
    /// Wait 2 seconds and call ApiManager.CreatePutRequest().
    ///</summary>
    private IEnumerator WaitAndPut()
    {
        yield return new WaitForSeconds(2f);
        ApiManager.instance.CreatePutRequest(this);
    }

    ///<summary>
    /// Get children from API according to wanted LOD
    ///</summary>
    ///<param name="_level">Wanted LOD to get</param>
    public async Task LoadChildren(string _level)
    {
        int lvl = 0;
        int.TryParse(_level, out lvl);
        if (lvl < 0)
            lvl = 0;

        if (currentLod != lvl)
        {
            DeleteChildren(lvl);

            string apiCall = "";
            if (lvl != 0)
                apiCall = $"{category}s/{id}/all?limit={lvl}";

            if (!string.IsNullOrEmpty(apiCall))
            {
                Debug.Log(apiCall);
                await ApiManager.instance.GetObject(apiCall);
            }

            SetCurrentLod(lvl);
            if (GameManager.gm.currentItems.Contains(gameObject))
                UiManager.instance.detailsInputField.UpdateInputField(currentLod.ToString());
        }
    }

    ///<summary>
    /// Set currentLod value for this object and it's OgreeObject children.
    ///</summary>
    ///<param name="_level">The value to set at currentLod</param>
    protected void SetCurrentLod(int _level)
    {
        currentLod = _level;
        //GameManager.gm.AppendLogLine($"Set {name}'s details level to {currentLod}", "green");

        if (_level != 0)
        {
            foreach (Transform child in transform)
            {
                OgreeObject obj = child.GetComponent<OgreeObject>();
                if (obj)
                    obj.SetCurrentLod(currentLod - 1);
            }
        }
    }

    ///<summary>
    /// Delete OgreeObject children according to _askedLevel.
    ///</summary>
    ///<param name="_askedLevel">The LOD to switch on</param>
    protected void DeleteChildren(int _askedLevel)
    {
        List<OgreeObject> objsToDel = new List<OgreeObject>();
        foreach (Transform child in transform)
        {
            OgreeObject obj = child.GetComponent<OgreeObject>();
            if (obj && obj.id != "") // Exclude components
                objsToDel.Add(obj);
        }

        if (_askedLevel == 0) // Delete all children
        {
            foreach (OgreeObject obj in objsToDel)
            {
                Debug.Log($"[Delete] {obj.hierarchyName}");
                GameManager.gm.DeleteItem(obj.gameObject, false);
            }
        }
        else
        {
            foreach (OgreeObject go in objsToDel)
                go.GetComponent<OgreeObject>().DeleteChildren(_askedLevel - 1);
        }
    }
}
