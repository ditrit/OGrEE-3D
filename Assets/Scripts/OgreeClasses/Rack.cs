using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Rack : Item
{
    public Transform uRoot;
    public GameObject gridForULocation;
    public bool areUHelpersToggled = false;
    public List<GameObject> breakersBoxes = new();

    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if ((HasAttributeChanged(_src, "posXYZ")
            || HasAttributeChanged(_src, "posXYUnit")
            || HasAttributeChanged(_src, "rotation"))
            && transform.parent)
        {
            PlaceInRoom(_src);
            group?.ShapeGroup();
        }

        if (domain != _src.domain)
            UpdateColorByDomain(_src.domain);

        if (HasAttributeChanged(_src, "color"))
            SetColor((string)_src.attributes["color"]);

        if (HasAttributeChanged(_src, "breakers"))
            GenerateBreakersBoxes(_src);

        base.UpdateFromSApiObject(_src);
    }

    /// <summary>
    /// Cast breakers dictionary and fill <see cref="breakersBoxes"/>
    /// </summary>
    /// <param name="_src">New data of the rack</param>
    private void GenerateBreakersBoxes(SApiObject _src)
    {
        for (int i = 0; i < breakersBoxes.Count; i++)
            Destroy(breakersBoxes[i]);
        breakersBoxes.Clear();

        Dictionary<string, Dictionary<string, dynamic>> breakers = ((JObject)_src.attributes["breakers"]).ToObject<Dictionary<string, Dictionary<string, dynamic>>>();
        int index = 0;
        foreach (var breaker in breakers)
            breakersBoxes.Add(CreateBreakerBox(_src, index++, breaker.Key, breaker.Value));
    }

    /// <summary>
    /// Create a breakerBox and place it at the top left corner of this rack
    /// </summary>
    /// <param name="_src">New data of the rack</param>
    /// <param name="_index">Index of the breaker to create, used for placement</param>
    /// <param name="_name">Name of the breaker to create</param>
    /// <param name="_attrs">Attributes of the breaker to create</param>
    /// <returns>The breakerBox GameObject</returns>
    private GameObject CreateBreakerBox(SApiObject _src, int _index, string _name, Dictionary<string, dynamic> _attrs)
    {
        Debug.Log($"{_index}: {_name}");
        Vector3 parentSize = transform.GetChild(0).localScale;

        GameObject newBreaker = Instantiate(GameManager.instance.breakerModel, transform);
        newBreaker.name = $"breaker_{_name}";

        Vector3 shapeSize = newBreaker.transform.GetChild(0).localScale;
        newBreaker.transform.localPosition = new(parentSize.x - shapeSize.x, parentSize.y - shapeSize.y / 2, parentSize.z);
        newBreaker.transform.localPosition += _index * (shapeSize.y + 0.005f) * Vector3.down;

        Device breakerOO = newBreaker.GetComponent<Device>();
        breakerOO.name = _name;
        breakerOO.id = $"{_src.id}.{_name}";
        breakerOO.parentId = _src.id;
        breakerOO.attributes = _attrs;

        DisplayObjectData breakerDoD = newBreaker.GetComponent<DisplayObjectData>();
        breakerDoD.PlaceTexts(LabelPos.Front);
        breakerDoD.SetLabel(breakerOO.name);
        breakerDoD.SwitchLabel(ELabelMode.Default);

        return newBreaker;
    }
}
