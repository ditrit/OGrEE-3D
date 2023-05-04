using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float temperature;
    public string temperatureUnit = "";
    public Color color;
    public bool fromTemplate;
    public GameObject sensorTempDiagram = null;

    private void Start()
    {
        EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// Check for an attribute "temperatureUnit" of the site of this sensor and assign it to temperatureUnit.
    /// Set this sensor's temperature to _value (converted to float)
    ///</summary>
    ///<param name="_value">The temperature value</param>
    public void SetTemperature(string _value)
    {
        temperature = Utils.ParseDecFrac(_value);
        temperatureUnit = transform.parent.GetComponent<OObject>().temperatureUnit;
        if (!string.IsNullOrEmpty(temperatureUnit))
            UpdateSensorColor();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Check for an attribute "temperatureUnit" of the site of this sensor and assign it to temperatureUnit.
    /// Set this sensor's temperature to _value (converted to float)
    ///</summary>
    ///<param name="_value">The temperature value</param>
    public void SetTemperature(float _value)
    {
        temperature = _value;
        temperatureUnit = transform.parent.GetComponent<OObject>().temperatureUnit;
        if (!string.IsNullOrEmpty(temperatureUnit))
            UpdateSensorColor();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Change the sensor color regarding the "temperature" attribute.
    ///</summary>
    public void UpdateSensorColor()
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        (int tempMin, int tempMax) = GameManager.instance.configLoader.GetTemperatureLimit(temperatureUnit);
        Texture2D text = TempDiagram.instance.heatMapGradient;
        float pixelX = Utils.MapAndClamp(temperature, tempMin, tempMax, 0, text.width);
        mat.color = text.GetPixel(Mathf.FloorToInt(pixelX), text.height / 2);

        color = mat.color;
    }

    /// <summary>
    /// When called, update the sensor temperature
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void OnImportFinished(ImportFinishedEvent _e)
    {
        OObject parent = transform.parent.GetComponent<OObject>();
        if (parent)
            if (!fromTemplate)
                SetTemperature(parent.GetTemperatureInfos().mean);
            else if (parent.attributes.ContainsKey($"temperature_{name}"))
                SetTemperature(parent.attributes[$"temperature_{name}"]);
            else
                SetTemperature(float.NaN);
        EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }
}