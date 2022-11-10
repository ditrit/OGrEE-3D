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
    /// Check for an attribute "temperatureUnit" of the site of this sensor and assign it to temperatureUnit.
    /// Set this sensor's temperature to _value (converted to float)
    ///</summary>
    ///<param name="_value">The temperature value</param>
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
        (int tempMin, int tempMax) = GameManager.gm.configLoader.GetTemperatureLimit(temperatureUnit);
        float blue = Utils.MapAndClamp(temperature, tempMin, tempMax, 1, 0);
        float red = Utils.MapAndClamp(temperature, tempMin, tempMax, 0, 1);

        mat.color = new Color(red, 0, blue);
        color = mat.color;
    }
}