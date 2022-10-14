using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float temperature = 0f;
    public string temperatureUnit = "°C";
    public Color color;
    public bool fromTemplate;
    public GameObject sensorTempDiagram = null;

    ///<summary>
    /// Check for a _param attribute "temperature" and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetTemperature(string _value)
    {
        temperature = Utils.ParseDecFrac(_value);
        OgreeObject site;
        if (transform.parent.GetComponent<OObject>())
        {
            if (transform.parent.GetComponent<OObject>().referent)
                site = transform.parent.GetComponent<OObject>().referent.transform.parent?.parent?.parent?.GetComponent<OgreeObject>();
            else if (transform.parent.parent && transform.parent.parent.GetComponent<OgreeObject>().category == "room")
                site = transform.parent.parent.parent?.parent?.GetComponent<OgreeObject>();
            else
                site = transform.parent.parent?.GetComponent<OObject>().referent?.transform.parent?.parent?.parent?.GetComponent<OgreeObject>();
        }
        else
            site = transform.parent?.parent?.parent?.GetComponent<OgreeObject>();
        if (site && site.attributes.ContainsKey("temperatureUnit"))
            temperatureUnit = site.attributes["temperatureUnit"];
        UpdateSensorColor();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Change the sensor color regarding the "temperature" attribute.
    ///</summary>
    public void UpdateSensorColor()
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        int tempMin = GameManager.gm.configLoader.GetTemperatureLimit("min", temperatureUnit);
        int tempMax = GameManager.gm.configLoader.GetTemperatureLimit("max", temperatureUnit);
        float blue = MapAndClamp(temperature, tempMin, tempMax, 1, 0);
        float red = MapAndClamp(temperature, tempMin, tempMax, 0, 1);

        mat.color = new Color(red, 0, blue);
        color = mat.color;
    }

    ///<summary>
    /// Map a Value from a given range to another range and clamp it.
    ///</summary>
    ///<param name="_input">The value to map</param>
    ///<param name="_inMin">The minimal value of the input range</param>
    ///<param name="_inMax">The maximal value of the input range</param>
    ///<param name="_outMin">The minimal value of the output range</param>
    ///<param name="_outMax">The maximal value of the output range</param>
    ///<returns>The maped and clamped value</returns>
    public float MapAndClamp(float _input, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        return Mathf.Clamp((_input - _inMin) * (_outMax - _outMin) / (_inMax - _inMin) + _outMin, Mathf.Min(_outMin, _outMax), Mathf.Max(_outMin, _outMax));
    }
}