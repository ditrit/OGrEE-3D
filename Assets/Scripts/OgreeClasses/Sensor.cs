using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float temperature = 0f;

    public Color color;

    ///<summary>
    /// Check for a _param attribute "temperature" and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetTemperature(string _value)
    {
        temperature = Utils.ParseDecFrac(_value);
        UpdateSensorColor();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Change the sensor color regarding the "temperature" attribute.
    ///</summary>
    public void UpdateSensorColor()
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        int tempMin = GameManager.gm.configLoader.GetTemperatureLimit("min", "c");
        int tempMax = GameManager.gm.configLoader.GetTemperatureLimit("max", "c");
        print(tempMin);
        print(tempMax);
        float blue = map(temperature, tempMin, tempMax, 1, 0);
        float red = map(temperature, tempMin, tempMax, 0, 1);

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