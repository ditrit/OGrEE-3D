using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OgreeObject : MonoBehaviour, ISerializationCallbackReceiver, IComparable<OgreeObject>
{
    [Header("Standard attributes")]
    public new string name;
    public string id;
    public string parentId;
    public string category;
    public List<string> description = new();
    public string domain;

    [Header("Specific attributes")]
    [SerializeField] private List<string> attributesKeys = new();
    [SerializeField] private List<string> attributesValues = new();
    public Dictionary<string, string> attributes = new();

    [Header("LOD")]
    public int currentLod = 0;

    [Header("Internal behavior")]
    private Coroutine updatingCoroutine = null;
    public Vector3 originalLocalPosition = Vector3.negativeInfinity;
    public Quaternion originalLocalRotation = Quaternion.identity;
    public Vector3 originalLocalScale = Vector3.one;
    public GameObject heatMap;
    public bool scatterPlot = false;
    public GameObject localCS = null;
    public bool isDoomed = false;
    public bool isLodLocked = false;

    public void OnBeforeSerialize()
    {
        attributesKeys.Clear();
        attributesValues.Clear();
        foreach (KeyValuePair<string, string> kvp in attributes)
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

    public int CompareTo(OgreeObject _other)
    {
        // A null value means that this object is greater.
        if (_other == null)
            return 1;
        else
            return this.id.CompareTo(_other.id);
    }

    protected virtual void OnDestroy()
    {
        GameManager.instance.allItems.Remove(id);

        if (attributes.ContainsKey("template") && !string.IsNullOrEmpty(attributes["template"]))
            GameManager.instance.DeleteTemplateIfUnused(category, attributes["template"]);
    }

    private void OnDisable()
    {
        if (gameObject.activeInHierarchy)
            Doom();
    }

    /// <summary>
    /// Doom this object and all of its children
    /// </summary>
    private void Doom()
    {
        isDoomed = true;
        foreach (Transform child in transform)
            child.GetComponent<OgreeObject>()?.Doom();
    }

    ///<summary>
    /// Update the OgreeObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public virtual void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
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
    public async Task LoadChildren(int _level)
    {
        if (!ApiManager.instance.isInit || isLodLocked || (this is Device dv && dv.isComponent))
            return;

        if (_level < 0)
            _level = 0;

        if (currentLod > _level)
            await DeleteChildren(_level);
        else if (_level != currentLod)
            await ApiManager.instance.GetObject($"{category}s/{id}/all?limit={_level}", ApiManager.instance.DrawObject);

        SetCurrentLod(_level);
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
                if (child.GetComponent<OgreeObject>() is OgreeObject obj)
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
        List<OgreeObject> objsToDel = new();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<OgreeObject>() is OgreeObject obj && obj is Device dv && !dv.isComponent)
                objsToDel.Add(obj);
        }

        if (_askedLevel == 0) // Delete all children
        {
            foreach (OgreeObject obj in objsToDel)
            {
                Debug.Log($"[Delete] {obj.id}");
                await GameManager.instance.DeleteItem(obj.gameObject, false, false);
            }
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
        transform.SetLocalPositionAndRotation(originalLocalPosition, originalLocalRotation);
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

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    public void ToggleCS()
    {
        if (localCS)
            localCS.CleanDestroy($"Hide local Coordinate System for {name}");
        else
            BuildLocalCS();
    }

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    ///<param name="_value">true of false value</param>
    public void ToggleCS(bool _value)
    {
        if (localCS && !_value)
            localCS.CleanDestroy($"Hide local Coordinate System for {name}");
        else if (!localCS && _value)
            BuildLocalCS();
    }

    ///<summary>
    /// Create a local Coordinate System for this object.
    ///</summary>
    ///<param name="_name">The name of the local CS</param>
    protected void BuildLocalCS()
    {
        float scale = this is Item ? 1 : 7;
        localCS = Instantiate(GameManager.instance.coordinateSystemModel);
        localCS.name = "localCS";
        localCS.transform.parent = transform;
        localCS.transform.localScale = scale * Vector3.one;
        localCS.transform.localEulerAngles = Vector3.zero;
        localCS.transform.localPosition = this is Group ? transform.GetChild(0).localScale / -2f : Vector3.zero;
        GameManager.instance.AppendLogLine($"Display local Coordinate System for {name}", ELogTarget.logger, ELogtype.success);
    }
}
