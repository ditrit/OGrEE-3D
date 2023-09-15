using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float temperature;
    public string temperatureUnit = "";
    public Color color;
    public bool fromTemplate;
    public GameObject sensorTempDiagram = null;
    private bool importDone = false;

    private void Start()
    {
        EventManager.instance.ImportFinished.Add(OnImportFinished);
    }

    private void OnDestroy()
    {
        EventManager.instance.ImportFinished.Remove(OnImportFinished);
    }

    ///<summary>
    /// Check for an attribute "temperatureUnit" of the site of this sensor and assign it to temperatureUnit.
    /// Set this sensor's temperature to _value
    ///</summary>
    ///<param name="_value">The temperature value</param>
    public void SetTemperature(float _value)
    {
        temperature = _value;
        temperatureUnit = transform.parent.GetComponent<Item>().temperatureUnit;
        if (!string.IsNullOrEmpty(temperatureUnit))
            UpdateSensorColor();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Change the sensor color regarding the "temperature" attribute.
    ///</summary>
    public void UpdateSensorColor()
    {
        (int tempMin, int tempMax) = GameManager.instance.configHandler.GetTemperatureLimit(temperatureUnit);
        Texture2D text = TempDiagram.instance.heatMapGradient;
        float pixelX = Utils.MapAndClamp(temperature, tempMin, tempMax, 0, text.width);
        color = text.GetPixel(Mathf.FloorToInt(pixelX), text.height / 2);

        GetComponent<ObjectDisplayController>().ChangeColor(color);
    }

    /// <summary>
    /// When called, update the sensor temperature
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void OnImportFinished(ImportFinishedEvent _e)
    {
        if (importDone)
            return;

        importDone = true;

        Item parent = transform.parent.GetComponent<Item>();
        if (parent)
        {
            if (!fromTemplate)
                SetTemperature(parent.GetTemperatureInfos().mean);
            else if (parent.attributes.ContainsKey($"temperature_{name}"))
                SetTemperature(Utils.ParseDecFrac(parent.attributes[$"temperature_{name}"]));
            else
                SetTemperature(float.NaN);
        }
    }
}