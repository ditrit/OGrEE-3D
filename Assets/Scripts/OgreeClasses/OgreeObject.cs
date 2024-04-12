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
    public string description;
    public string domain;
    public List<string> tags = new();

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

    [Header("Layers")]
    public Dictionary<Layer, bool> layers = new();

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
        attributes = new();
        for (int i = 0; i < attributesKeys.Count; i++)
            attributes.Add(attributesKeys[i], attributesValues[i]);
    }

    public int CompareTo(OgreeObject _other)
    {
        // A null value means that this object is greater.
        if (_other == null)
            return 1;
        else
            return id.CompareTo(_other.id);
    }

    protected virtual void OnDestroy()
    {
        GameManager.instance.allItems.Remove(id);
        foreach (string tag in tags)
            GameManager.instance.RemoveFromTag(tag, id);

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
        description = _src.description.Replace("\\n", "\n");
        attributes = _src.attributes;

        foreach (string newTag in _src.tags)
        {
            if (!tags.Contains(newTag))
                GameManager.instance.AddToTag(newTag, id);
        }
        foreach (string oldTag in tags)
        {
            if (!_src.tags.Contains(oldTag))
                GameManager.instance.RemoveFromTag(oldTag, id);
        }
        tags = _src.tags;
        UiManager.instance.UpdateGuiInfos();
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
    ///<param name="_forceLod">Ignore isLodLocked and set it to false</param>
    public async Task LoadChildren(int _level, bool _forceLod = false)
    {
        if (!ApiManager.instance.isInit || (!_forceLod && isLodLocked) || (this is Device dv && dv.isComponent))
            return;

        if (_forceLod)
            isLodLocked = false;

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
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Set details level to", new List<string>() { name, currentLod.ToString() }), ELogTarget.logger, ELogtype.success);

        if (_level != 0)
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out OgreeObject obj))
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
            localCS.CleanDestroy("Logs", "Hide local CS", name);
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
            localCS.CleanDestroy("Logs", "Hide local CS", name);
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
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Display local CS", name), ELogTarget.logger, ELogtype.success);
    }

    /// <summary>
    /// Get a Layer using its slug
    /// </summary>
    /// <param name="_slug">The slug to look for</param>
    /// <returns>The asked Layer or null</returns>
    public Layer GetLayer(string _slug)
    {
        foreach (KeyValuePair<Layer, bool> kvp in layers)
        {
            if (kvp.Key.slug == _slug)
                return kvp.Key;
        }
        return null;
    }

    /// <summary>
    /// Check if an attribute has changed between old object and new data
    /// </summary>
    /// <param name="_newData">The new data</param>
    /// <param name="_attrKey">The name of the attribute</param>
    /// <returns>True if the attribute has been added or has had its value modified</returns>
    public bool HasAttributeChanged(SApiObject _newData, string _attrKey)
    {
        return _newData.attributes.ContainsKey(_attrKey) && (!attributes.ContainsKey(_attrKey) || attributes[_attrKey] != _newData.attributes[_attrKey]);
    }

    ///<summary>
    /// Get a posXYUnit regarding given object's attributes.
    ///</summary>
    ///<param name="_obj">The object to parse</param>
    ///<returns>The posXYUnit, 1 by default</returns>
    protected float GetUnitFromAttributes(SApiObject _obj)
    {
        if (!_obj.attributes.ContainsKey("posXYUnit"))
            return 1;
        return _obj.attributes["posXYUnit"] switch
        {
            LengthUnit.Tile => UnitValue.Tile,
            LengthUnit.Millimeter => 0.001f,
            LengthUnit.Centimeter => 0.01f,
            LengthUnit.Meter => 1.0f,
            LengthUnit.Foot => UnitValue.Foot,
            _ => 1,
        };
    }
}
