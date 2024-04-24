using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Group : Item
{
    private List<GameObject> content = new();
    public bool isDisplayed = true;

    protected override void Start()
    {
        base.Start();
        DisplayContent(false);
    }

    protected override void OnDestroy()
    {
        ToggleContent(true);
        foreach (GameObject gameObject in content)
            gameObject.GetComponent<Item>().group = null;
        UiManager.instance.openedGroups.Remove(this);
        UiManager.instance.groupsList.RebuildMenu(UiManager.instance.BuildGroupButtons);
        base.OnDestroy();
    }

    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if (domain != _src.domain)
            UpdateColorByDomain(_src.domain);

        if (HasAttributeChanged(_src, "color"))
            SetColor((string)_src.attributes["color"]);

        if (HasAttributeChanged(_src, "content"))
        {
            RegisterContent(_src);
            ShapeGroup();
        }

        base.UpdateFromSApiObject(_src);
    }

    ///<summary>
    /// Display or hide the rackGroup and its content.
    ///</summary>
    ///<param name="_value">true or false value</param>
    public void ToggleContent(bool _value)
    {
        isDisplayed = !_value;
        GetComponent<ObjectDisplayController>().Display(!_value, !_value, !_value);
        DisplayContent(_value);
        if (_value)
        {
            GetComponent<ObjectDisplayController>().UnsubscribeEvents();
            if (!UiManager.instance.openedGroups.Contains(this))
            {
                UiManager.instance.openedGroups.Add(this);
                UiManager.instance.openedGroups.Sort();
            }
        }
        else
        {
            ObjectDisplayController objectDisplayController = GetComponent<ObjectDisplayController>();
            objectDisplayController.SubscribeEvents();
            objectDisplayController.HandleMaterial();
            if (UiManager.instance.openedGroups.Contains(this))
                UiManager.instance.openedGroups.Remove(this);
        }
        UiManager.instance.groupsList.RebuildMenu(UiManager.instance.BuildGroupButtons);
    }

    ///<summary>
    /// Enable or disable GameObjects in <see cref="content"/>.
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    private void DisplayContent(bool _value)
    {
        foreach (GameObject go in content)
        {
            if (go && !go.GetComponent<OgreeObject>().isDoomed)
            {
                ObjectDisplayController itemsOdc = go.GetComponent<ObjectDisplayController>();
                itemsOdc.Display(_value, _value, _value);
                itemsOdc.isHiddenInGroup = !_value;
                itemsOdc.ForceHighlightCube();
                if (_value)
                    itemsOdc.SubscribeEvents();
                else
                    itemsOdc.UnsubscribeEvents();
            }
        }
    }

    ///<summary>
    /// Get all GameObjects listed in <see cref="content"/>.
    ///</summary>
    ///<returns>The list of GameObject corresponding to <see cref="content"/></returns>
    public List<GameObject> GetContent()
    {
        return content;
    }

    /// <summary>
    /// Fill <see cref="content"/> based on "content" attribute given by <paramref name="_src"/>
    /// </summary>
    /// <param name="_src">The SAPiObject used to update this Group</param>
    public void RegisterContent(SApiObject _src)
    {
        content.Clear();

        List<string> names = ((JArray)_src.attributes["content"]).ToObject<List<string>>();;
        foreach (string rn in names)
            if (Utils.GetObjectById($"{_src.parentId}.{rn}") is GameObject go)
            {
                content.Add(go);
                go.GetComponent<Item>().group = this;
            }
    }

    /// <summary>
    /// Reshape a group according to its content
    /// </summary>
    /// <param name="_content">The content of the group</param>
    public void ShapeGroup()
    {
        string _parentCategory = transform.parent.GetComponent<OgreeObject>().category;
        // According to group type, set pos, rot & scale
        Vector3 pos;
        Vector3 scale;
        if (_parentCategory == Category.Room)
        {
            RackGroupPosScale(content.Select(go => go.transform), out pos, out scale);
            transform.localEulerAngles += new Vector3(0, 180, 0);
        }
        else // if (_parentCategory == Category.Rack)
        {
            DeviceGroupPosScale(content.Select(go => go.transform), out pos, out scale);
            transform.localEulerAngles = Vector3.zero;
        }
        transform.position = pos;
        transform.GetChild(0).localScale = scale;
    }

    ///<summary>
    /// For a group of Racks, set _pos and _scale
    ///</summary>
    ///<param name="_content">The list of racks or corridors in the group</param>
    ///<param name="_pos">The position to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private void RackGroupPosScale(IEnumerable<Transform> _content, out Vector3 _pos, out Vector3 _scale)
    {
        // x axis
        float left = float.PositiveInfinity;
        float right = float.NegativeInfinity;
        // y axis
        float bottom = float.PositiveInfinity;
        float top = float.NegativeInfinity;
        // z axis
        float rear = float.PositiveInfinity;
        float front = float.NegativeInfinity;

        foreach (Transform obj in _content)
        {
            Bounds bounds = obj.GetChild(0).GetComponent<Renderer>().bounds;
            left = Mathf.Min(bounds.min.x, left);
            right = Mathf.Max(bounds.max.x, right);
            bottom = Mathf.Min(bounds.min.y, bottom);
            top = Mathf.Max(bounds.max.y, top);
            rear = Mathf.Min(bounds.min.z, rear);
            front = Mathf.Max(bounds.max.z, front);
        }

        _scale = new Vector3(right - left, top - bottom, front - rear);
        _pos = 0.5f * new Vector3(left + right, bottom + top, rear + front);
    }

    ///<summary>
    /// For a group of devices, set _pos and _scale
    ///</summary>
    ///<param name="_devices">The list of devices in the group</param>
    ///<param name="_pos">The localPosition to apply to the group</param>
    ///<param name="_scale">The localScale to apply to the group</param>
    private void DeviceGroupPosScale(IEnumerable<Transform> _devices, out Vector3 _pos, out Vector3 _scale)
    {
        Transform lowerDv = _devices.First();
        Transform upperDv = _devices.First();
        float maxWidth = 0;
        float maxLength = 0;
        foreach (Transform dv in _devices)
        {
            if (dv.localPosition.y < lowerDv.localPosition.y)
                lowerDv = dv;
            if (dv.localPosition.y > upperDv.localPosition.y)
                upperDv = dv;

            if (dv.GetChild(0).localScale.x > maxWidth)
                maxWidth = dv.GetChild(0).localScale.x;
            if (dv.GetChild(0).localScale.z > maxLength)
                maxLength = dv.GetChild(0).localScale.z;
        }
        float height = upperDv.localPosition.y - lowerDv.localPosition.y;
        height += (upperDv.GetChild(0).localScale.y + lowerDv.GetChild(0).localScale.y) / 2;

        _scale = new(maxWidth, height, maxLength);

        _pos = lowerDv.position;
        _pos += new Vector3(0, (_scale.y - lowerDv.GetChild(0).localScale.y) / 2, 0);
    }

}
