using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class OgreeObject : MonoBehaviour, ISerializationCallbackReceiver
{
    [Header("Standard attributes")]
    public new string name;
    public string hierarchyName;
    public string id;
    public string parentId;
    public string category;
    public List<string> description = new List<string>();
    public string domain;

    [Header("Specific attributes")]
    [SerializeField] private List<string> attributesKeys = new List<string>();
    [SerializeField] private List<string> attributesValues = new List<string>();
    public Dictionary<string, string> attributes = new Dictionary<string, string>();

    [Header("LOD")]
    public int currentLod = 0;

    [Header("Internal behavior")]
    private Coroutine updatingCoroutine = null;
    public Vector3 originalLocalPosition = Vector3.negativeInfinity;
    public Quaternion originalLocalRotation = Quaternion.identity;
    public Vector3 originalLocalScale = Vector3.one;
    public bool heatMap = false;
    public bool scatterPlot = false;
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
        for (int i = 0; i < attributesKeys.Count; i++)
            attributes.Add(attributesKeys[i], attributesValues[i]);
    }

    private void OnEnable()
    {
        UpdateHierarchyName();
    }

    protected async virtual void OnDestroy()
    {
        if (GameManager.instance.GetSelected().Contains(gameObject))
            await GameManager.instance.SetCurrentItem(null);
        GameManager.instance.allItems.Remove(hierarchyName);

        if (attributes.ContainsKey("template") && !string.IsNullOrEmpty(attributes["template"]))
            GameManager.instance.DeleteTemplate(category, attributes["template"]);
    }

    ///<summary>
    /// Update the OgreeObject's hierarchyName with it's parent's one.
    ///</summary>
    ///<returns>The updated hierarchyName of the object</returns>
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
    public virtual void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
        hierarchyName = _src.hierarchyName;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        domain = _src.domain;
        description = _src.description;
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
        if (!ApiManager.instance.isInit)
        {
            Debug.Log("API offline");
            return;
        }

        if (id == "")
        {
            GameManager.instance.AppendLogLine($"Id of {hierarchyName} is empty, no child loaded.", ELogTarget.logger, ELogtype.warning);
            return;
        }
        int.TryParse(_level, out int lvl);
        if (lvl < 0)
            lvl = 0;

        if (currentLod != lvl)
        {
            await DeleteChildren(lvl);

            if (lvl != 0)
            {
                string apiCall = $"{category}s/{id}/all?limit={lvl}";
                Debug.Log(apiCall);
                await ApiManager.instance.GetObject(apiCall, ApiManager.instance.DrawObject);
            }

            SetCurrentLod(lvl);
        }
    }

    ///<summary>
    /// Set currentLod value for this object and it's OgreeObject children.
    ///</summary>
    ///<param name="_level">The value to set at currentLod</param>
    protected void SetCurrentLod(int _level)
    {
        currentLod = _level;
        GameManager.instance.AppendLogLine($"Set {name}'s details level to {currentLod}", ELogTarget.logger, ELogtype.success);

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
    protected async Task DeleteChildren(int _askedLevel)
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
                obj.transform.parent = null;
                await GameManager.instance.DeleteItem(obj.gameObject, false, false);
            }
            GetComponent<FocusHandler>()?.InitHandler();
        }
        else
        {
            foreach (OgreeObject go in objsToDel)
                await go.GetComponent<OgreeObject>().DeleteChildren(_askedLevel - 1);
        }
    }

    ///<summary>
    /// Reset object's transform to its starting point
    ///</summary>
    public void ResetTransform()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
        transform.localScale = originalLocalScale;
    }

    ///<summary>
    /// Set the object's base transform
    ///</summary>
    public void SetBaseTransform()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalLocalScale = transform.localScale;
    }
}
