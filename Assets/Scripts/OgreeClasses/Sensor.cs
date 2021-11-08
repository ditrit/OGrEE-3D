using TMPro;
using UnityEngine;

public class Sensor : OObject
{
    public override void SetAttribute(string _param, string _value)
    {
        if (_param == "temperature")
        {
            attributes["temperature"] = _value;
            UpdateSensorColor();
        }
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    ///
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