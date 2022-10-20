using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class TempDiagram
{
    /// <summary>
    /// true if thetemperature diagram is displayed
    /// </summary>
    public static bool isDiagramShown = false;

    static TempDiagram()
    {
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    ///<summary>
    /// When called checks if the temperature diagram is currently shown if true hide it.
    ///</summary>
    ///<param name="e">The event's instance</param>
    static void OnSelectItem(OnSelectItemEvent _e)
    {
        if (isDiagramShown)
            HandleTempBarChart(GameManager.gm.previousItems[0].GetComponent<OgreeObject>());
    }

    /// <summary>
    /// Recursive fonction, get a list of the sensors in the object or in the children of it. If an object have children, its own sensors will be ignored.
    /// </summary>
    /// <param name="_ogreeObject">The object where we get the sensors</param>
    /// <returns>a list of sensors of the object belonging to a child object which has no child or no sensor in their children.</returns>
    private static List<Sensor> GetObjectSensors(OgreeObject _ogreeObject)
    {
        List<Sensor> sensors = new List<Sensor>();
        List<Sensor> basicSensors = new List<Sensor>();
        foreach (Transform childTransform in _ogreeObject.transform)
        {
            OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
            if (childOgreeObject)
            {
                sensors.AddRange(GetObjectSensors(childOgreeObject));
            }
            Sensor childSensor = childTransform.GetComponent<Sensor>();
            if (childSensor)
                if (childSensor.fromTemplate)
                    sensors.Add(childSensor);
                else
                    basicSensors.Add(childSensor);

        }
        return sensors.Count > 0 ? sensors : basicSensors;

    }

    /// <summary>
    /// Show the temperature diagram if it is not already shown, hide it if it is.
    /// </summary>
    /// <param name="_ogreeObject">the object where we show/hide the temperature diagram</param>
    public static void HandleTempBarChart(OgreeObject _ogreeObject)
    {

        EventManager.Instance.Raise(new TemperatureDiagramEvent() { obj = _ogreeObject.gameObject });

        if (!isDiagramShown)
        {

            Dictionary<OgreeObject, List<Sensor>> sensors = new Dictionary<OgreeObject, List<Sensor>>();

            foreach (Transform childTransform in _ogreeObject.transform)
            {
                OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
                if (childOgreeObject)
                    sensors.Add(childOgreeObject, GetObjectSensors(childOgreeObject));
            }

            if (sensors.Count == 0)
                GameManager.gm.AppendLogLine($"No sensor found in {_ogreeObject.name}", true, eLogtype.warning);

            foreach (KeyValuePair<OgreeObject, List<Sensor>> entry in sensors)
                AdaptSensor(entry.Value, entry.Key);
        }
        else
            foreach (Transform childTransform in _ogreeObject.transform)
            {
                OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
                if (childOgreeObject)
                    Object.Destroy(childOgreeObject.tempBar);
            }
        isDiagramShown = !isDiagramShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the temperature diagram is to be shown or hidden.
    /// </summary>
    /// <param name="_sensor">the sensor to adapt</param>
    /// <param name="_ogreeObject">the object where the sensor is</param>
    private static void AdaptSensor(List<Sensor> _sensors, OgreeObject _ogreeObject)
    {
        if (_sensors.Count == 0 || isDiagramShown)
            return;

        IEnumerable<float> tempValues = _sensors.Select(sensor => sensor.temperature);

        float mean = tempValues.Average();
        float std = Mathf.Sqrt(tempValues.Average(v => Mathf.Pow(v - mean, 2)));
        int tempMin = GameManager.gm.configLoader.GetTemperatureLimit("min", _sensors[0].temperatureUnit);
        int tempMax = GameManager.gm.configLoader.GetTemperatureLimit("max", _sensors[0].temperatureUnit);
        float height = _sensors[0].MapAndClamp(mean, tempMin, tempMax, 0, Utils.ParseDecFrac(_ogreeObject.attributes["height"]));
        float heigthStd = _sensors[0].MapAndClamp(std, tempMin, tempMax, 0, Utils.ParseDecFrac(_ogreeObject.attributes["height"]));
        float yBase = _ogreeObject.transform.parent.position.y + 0.01f;

        if (_ogreeObject.attributes["heightUnit"] == "mm")
        {
            height /= 1000;
            heigthStd /= 1000;
        }
        else if (_ogreeObject.attributes["heightUnit"] == "cm")
        {
            height /= 100;
            heigthStd /= 100;
        }
        else if (_ogreeObject.attributes["heightUnit"] == "U")
        {
            height *= GameManager.gm.uSize;
            heigthStd *= GameManager.gm.uSize;
        }

        GameObject sensorBar = Object.Instantiate(GameManager.gm.sensorBarModel, _ogreeObject.transform);
        sensorBar.name = _ogreeObject.name + "TempBar";
        sensorBar.transform.position = new Vector3(_ogreeObject.transform.position.x, yBase + 0.5f * height, _ogreeObject.transform.position.z);
        sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);

        float blue = _sensors[0].MapAndClamp(mean, tempMin, tempMax, 1, 0);
        float red = _sensors[0].MapAndClamp(mean, tempMin, tempMax, 0, 1);
        Material barMat = sensorBar.transform.GetChild(0).GetComponent<Renderer>().material;
        barMat.color = new Color(red, 0, blue,0.85f);
        _ogreeObject.tempBar = sensorBar;

        if (std == 0)
            return;

        GameObject sensorBarStd = Object.Instantiate(GameManager.gm.sensorBarStdModel, _ogreeObject.transform);
        sensorBarStd.transform.position = new Vector3(_ogreeObject.transform.position.x, yBase + height, _ogreeObject.transform.position.z);
        sensorBarStd.transform.GetChild(0).localScale = new Vector3(1, heigthStd, 1);
        sensorBarStd.transform.parent = sensorBar.transform;

        blue = _sensors[0].MapAndClamp(std, tempMin, tempMax, 1, 0);
        red = _sensors[0].MapAndClamp(std, tempMin, tempMax, 0, 1);
        sensorBarStd.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
        sensorBarStd.transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
        sensorBarStd.transform.GetChild(0).GetChild(2).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
    }
}
