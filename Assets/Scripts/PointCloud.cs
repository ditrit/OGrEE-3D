using System.Collections.Generic;
using UnityEngine;

public static class PointCloud
{
    /// <summary>
    /// true if the point cloud is displayed
    /// </summary>
    public static bool isCloudShown = false;

    static PointCloud()
    {
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    ///<summary>
    /// When called checks if the point cloud is currently shown if true hide it.
    ///</summary>
    ///<param name="e">The event's instance</param>
    static void OnSelectItem(OnSelectItemEvent _e)
    {
        if (isCloudShown)
            HandlePointCloud(GameManager.gm.previousItems[0].GetComponent<OgreeObject>());
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
    /// Show the point cloud if it is not already shown, hide it if it is.
    /// </summary>
    /// <param name="_ogreeObject">the object where we show/hide the pointcloud</param>
    public static void HandlePointCloud(OgreeObject _ogreeObject)
    {
        List<Sensor> _sensors = GetObjectSensors(_ogreeObject);
        if (_ogreeObject.category == "room")
        {
            foreach (Transform childTransform in _ogreeObject.transform)
            {
                FocusHandler focusHandler = childTransform.GetComponent<FocusHandler>();
                if (focusHandler && !childTransform.GetComponent<Slot>())
                {
                    focusHandler.UpdateChildMeshRenderers(false);
                    focusHandler.UpdateOwnMeshRenderers(isCloudShown && (!_ogreeObject.GetComponent<OObject>() || (GameManager.gm.currentItems.Count == 1 && GameManager.gm.currentItems[0] == _ogreeObject.gameObject)));
                }
            }
        }
        if (isCloudShown && (GameManager.gm.currentItems.Count == 1 && GameManager.gm.currentItems[0] == _ogreeObject.gameObject))
            _ogreeObject.GetComponent<FocusHandler>()?.UpdateChildMeshRenderers(isCloudShown, isCloudShown);
        if (!isCloudShown)
            _ogreeObject.GetComponent<FocusHandler>()?.UpdateChildMeshRenderers(isCloudShown, isCloudShown);

        foreach (Sensor sensor in _sensors)
        {
            AdaptSensor(sensor, _ogreeObject);
        }
        isCloudShown = !isCloudShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the point cloud is to be shown or hidden.
    /// </summary>
    /// <param name="_sensor">the sensor to adapt</param>
    /// <param name="_ogreeObject">the object where the sensor is</param>
    private static void AdaptSensor(Sensor _sensor, OgreeObject _ogreeObject)
    {
        if (!isCloudShown)
        {
            GameObject sensorPointCloud = Object.Instantiate(GameManager.gm.sensorPointCloudModel, _sensor.transform.parent);
            sensorPointCloud.name = _sensor.gameObject.name + "PointCLoudModel";

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
                sensorPointCloud.transform.position = new Vector3(_sensor.transform.position.x, yBase + 0.5f * height, _sensor.transform.position.z);
            else
                sensorPointCloud.transform.position = new Vector3(_sensor.transform.parent.position.x, yBase + 0.5f * height, _sensor.transform.parent.position.z);

            sensorPointCloud.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
            sensorPointCloud.transform.GetChild(0).GetComponent<Renderer>().material.color = _sensor.transform.GetChild(0).GetComponent<Renderer>().material.color;

            _sensor.sensorPointCloudModel = sensorPointCloud;
        }
        else
        {
            Object.Destroy(_sensor.sensorPointCloudModel);
            _sensor.sensorPointCloudModel = null;
        }
    }
}
