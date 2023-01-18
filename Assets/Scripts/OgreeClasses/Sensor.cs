using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public float temperature = 0f;
    public string temperatureUnit = "";
    public Color color;
    public bool fromTemplate;
    public GameObject sensorTempDiagram = null;

    private void Start()
    {
        if (!fromTemplate)
            EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    private void OnDestroy()
    {
        if (!fromTemplate)
            EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }
    ///<summary>
    /// Check for an attribute "temperatureUnit" of the site of this sensor and assign it to temperatureUnit.
    /// Set this sensor's temperature to _value (converted to float)
    ///</summary>
    ///<param name="_value">The temperature value</param>
    public async void SetTemperature(string _value)
    {
        temperature = Utils.ParseDecFrac(_value);
        temperatureUnit = transform.parent.GetComponent<OObject>().temperatureUnit;
        if (string.IsNullOrEmpty(temperatureUnit))
        {
            temperatureUnit = await ApiManager.instance.GetObject($"tempUnit/{transform.parent.GetComponent<OObject>().id}", ApiManager.instance.TempUnitFromAPI);
        }
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
            SetTemperature(parent.GetTemperatureInfos().mean.ToString());
    }
}