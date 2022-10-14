using System.Collections.Generic;
using UnityEngine;

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
            HandleTempDiagram(GameManager.gm.previousItems[0].GetComponent<OgreeObject>());
    }

    /// <summary>
    /// Recursive fonction, get a list of the sensors in the object or in the children of it. If an object have children, its own sensors will be ignored.
    /// </summary>
    /// <param name="_ogreeObject">The object where we get the sensors</param>
    /// <returns>a list of sensors of the object belonging to a child object which has no child or no sensor in their children.</returns>
    private static List<Sensor> GetObjectSensors(OgreeObject _ogreeObject)
    {
        List<Sensor> sensors = new List<Sensor>();
        List<Sensor> directSensors = new List<Sensor>();
        foreach (Transform childTransform in _ogreeObject.transform)
        {
            OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
            if (childOgreeObject)
            {
                sensors.AddRange(GetObjectSensors(childOgreeObject));
            }
            else if (childTransform.GetComponent<Sensor>())
                directSensors.Add(childTransform.GetComponent<Sensor>());
        }
        if (sensors.Count > 0)
            return sensors;
        else
            return directSensors;

    }

    /// <summary>
    /// Show the temperature diagram if it is not already shown, hide it if it is.
    /// </summary>
    /// <param name="_ogreeObject">the object where we show/hide the temperature diagram</param>
    public static void HandleTempDiagram(OgreeObject _ogreeObject)
    {
        List<Sensor> _sensors = GetObjectSensors(_ogreeObject);

        if (_sensors.Count == 0)
        {
            GameManager.gm.AppendLogLine($"No sensor found in {_ogreeObject.name} or any of its children", false, eLogtype.warning);
        }

        if (_ogreeObject.category == "room")
        {
            foreach (Transform childTransform in _ogreeObject.transform)
            {
                FocusHandler focusHandler = childTransform.GetComponent<FocusHandler>();
                if (focusHandler && !childTransform.GetComponent<Slot>())
                {
                    focusHandler.UpdateChildMeshRenderers(false);
                    focusHandler.UpdateOwnMeshRenderers(isDiagramShown && (!_ogreeObject.GetComponent<OObject>() || (GameManager.gm.currentItems.Count == 1 && GameManager.gm.currentItems[0] == _ogreeObject.gameObject)));
                }
            }
        }
        if (isDiagramShown && (GameManager.gm.currentItems.Count == 1 && GameManager.gm.currentItems[0] == _ogreeObject.gameObject))
            _ogreeObject.GetComponent<FocusHandler>()?.UpdateChildMeshRenderers(isDiagramShown, isDiagramShown);
        if (!isDiagramShown)
            _ogreeObject.GetComponent<FocusHandler>()?.UpdateChildMeshRenderers(isDiagramShown, isDiagramShown);

        foreach (Sensor sensor in _sensors)
        {
            AdaptSensor(sensor, _ogreeObject);
        }
        isDiagramShown = !isDiagramShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the temperature diagram is to be shown or hidden.
    /// </summary>
    /// <param name="_sensor">the sensor to adapt</param>
    /// <param name="_ogreeObject">the object where the sensor is</param>
    private static void AdaptSensor(Sensor _sensor, OgreeObject _ogreeObject)
    {
        if (!isDiagramShown)
        {
            GameObject sensorTempDiagram = Object.Instantiate(GameManager.gm.sensorTempDiagramModel, _sensor.transform.parent);
            sensorTempDiagram.name = _sensor.gameObject.name + "TempDiagramModel";

            float height = _sensor.MapAndClamp(_sensor.temperature, GameManager.gm.configLoader.GetTemperatureLimit("min", _sensor.temperatureUnit), GameManager.gm.configLoader.GetTemperatureLimit("max", _sensor.temperatureUnit), 0, Utils.ParseDecFrac(_ogreeObject.attributes["height"]));

            if (_ogreeObject.attributes["heightUnit"] == "mm")
                height /= 1000;
            else if (_ogreeObject.attributes["heightUnit"] == "cm")
                height /= 100;
            else if (_ogreeObject.attributes["heightUnit"] == "U")
                height *= GameManager.gm.uSize;

            float yBase = _ogreeObject.transform.position.y + 0.01f;

            if (_ogreeObject.category != "room")
                yBase -= _ogreeObject.transform.GetChild(0).localScale.y / 2;

            if (_sensor.fromTemplate)
                sensorTempDiagram.transform.position = new Vector3(_sensor.transform.position.x, yBase + 0.5f * height, _sensor.transform.position.z);
            else
                sensorTempDiagram.transform.position = new Vector3(_sensor.transform.parent.position.x, yBase + 0.5f * height, _sensor.transform.parent.position.z);

            sensorTempDiagram.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
            sensorTempDiagram.transform.GetChild(0).GetComponent<Renderer>().material.color = _sensor.transform.GetChild(0).GetComponent<Renderer>().material.color;

            _sensor.sensorTempDiagram = sensorTempDiagram;
        }
        else
        {
            Object.Destroy(_sensor.sensorTempDiagram);
            _sensor.sensorTempDiagram = null;
        }
    }
}
