using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class OgreeObject : MonoBehaviour, IAttributeModif, ISerializationCallbackReceiver
{
    [Header("Standard attributes")]
    public new string name;
    public string id;
    public string parentId;
    public string category;
    public List<string> description = new List<string>();
    public string domain; // = tenant

    [Header("Specific attributes")]
    [SerializeField] private List<string> attributesKeys = new List<string>();
    [SerializeField] private List<string> attributesValues = new List<string>();
    public Dictionary<string, string> attributes = new Dictionary<string, string>();

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

    protected virtual void OnDestroy()
    {
        if (category == "tenant")
        {
            Filters.instance.tenantsList.Remove($"<color=#{attributes["color"]}>{name}</color>");
            Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        }
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public virtual void SetAttribute(string _param, string _value)
    {
        if (_param.StartsWith("description"))
            SetDescription(_param.Substring(11), _value);
        else
        {
            switch (_param)
            {
                case "label":
                    SetLabel(_value);
                    break;
                case "domain":
                    if (GameManager.gm.allItems.ContainsKey(_value))
                        domain = _value;
                    else
                        GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exist. Please create it before assign it.", "yellow");
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    // GameManager.gm.AppendLogLine($"[{category}] {name}: unknown attribute to update.", "yellow");
                    break;
            }
        }
        PutData();
    }

    ///<summary>
    /// Set a description at the correct index.
    ///</summary>
    ///<param name="_index">The index to set the description</param>
    ///<param name="_value">The value of the description</param>
    protected void SetDescription(string _index, string _value)
    {
        string pattern = "^[0-9]+$";
        if (Regex.IsMatch(_index, pattern))
        {
            int index = int.Parse(_index);
            if (index != description.Count + 1)
                GameManager.gm.AppendLogLine($"Description set at index {description.Count + 1}.", "yellow");
            description.Add(_value);
        }
        else
            GameManager.gm.AppendLogLine("Wrong description index.", "red");
    }

    ///<summary>
    /// Set corresponding labels with given field value. 
    ///</summary>
    ///<param name="_input">The attribute to set</param>
    protected void SetLabel(string _input)
    {
        int i = 0;
        DisplayObjectData dod = GetComponent<DisplayObjectData>();
        if (_input == "name")
            dod.UpdateLabels(name);
        else if (_input.Contains("description"))
        {
            if (_input == "description")
                dod.UpdateLabels(string.Join("\n", description));
            else if (int.TryParse(_input.Substring(11), out i) && i > 0 && description.Count >= i)
                dod.UpdateLabels(description[i - 1]);
            else
                GameManager.gm.AppendLogLine("Wrong description index", "yellow");
        }
        else if (attributes.ContainsKey(_input))
            dod.UpdateLabels(attributes[_input]);
        else
            GameManager.gm.AppendLogLine($"{name} doesn't contain {_input} attribute.", "yellow");
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
    /// Set id.
    ///</summary>
    ///<param name="_id">The id to set</param>
    public void UpdateId(string _id)
    {
        id = _id;
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
        string hierarchyName = GetComponent<HierarchyName>()?.fullname;
        ApiManager.instance.CreatePutRequest(hierarchyName);
    }
}
