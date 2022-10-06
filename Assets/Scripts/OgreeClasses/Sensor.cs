using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sensor : MonoBehaviour
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

    public Color color;

    ///<summary>
    /// Check for a _param attribute "temperature" and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetAttribute(string _param, string _value)
    {
        if (_param == "temperature")
        {
            attributes["temperature"] = _value;
            UpdateSensorColor();
        }
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Update the Sensor attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public void UpdateFromSApiObject(SApiObject _src)
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
    /// Change the sensor color regarding the "temperature" attribute.
    ///</summary>
    public void UpdateSensorColor()
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        float temp = float.Parse(attributes["temperature"]);

        float blue = map(temp, 0, 100, 1, 0);
        float red = map(temp, 0, 100, 0, 1);

        mat.color = new Color(red, 0, blue);
        color = mat.color;
    }

    ///<summary>
    /// Map a Value from a given range to another range.
    ///</summary>
    ///<param name="_input">The value to map</param>
    ///<param name="_inMin">The minimal value of the input range</param>
    ///<param name="_inMax">The maximal value of the input range</param>
    ///<param name="_outMin">The minimal value of the input range</param>
    ///<param name="_outMax">The maximal value of the input range</param>
    ///<returns>The maped value</returns>
    float map(float _input, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        return (_input - _inMin) * (_outMax - _outMin) / (_inMax - _inMin) + _outMin;
    }
}