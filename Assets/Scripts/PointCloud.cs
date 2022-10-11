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
            HandlePointCloud(GameManager.gm.previousItems[0].GetComponent<Room>());
    }

    /// <summary>
    /// Call GetObjectSensors with a room as param.
    /// </summary>
    /// <param name="_room">the room where we get the sensors</param>
    /// <returns>a list of sensors of the room belonging to an object which has no child or no sensor in their children.</returns>
    private static List<Sensor> GetRoomSensors(Room _room)
    {
        return GetObjectSensors(_room);
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
    /// <param name="_room">the room where we show/hide the pointcloud</param>
    public static void HandlePointCloud(Room _room)
    {
        List<Sensor> _sensors = GetRoomSensors(_room);
        foreach (Transform childTransform in _room.transform)
        {
            FocusHandler focusHandler = childTransform.GetComponent<FocusHandler>();
            if (focusHandler)
            {
                focusHandler.UpdateOwnMeshRenderers(isCloudShown);
            }
        }

        foreach (Sensor sensor in _sensors)
        {
            sensor.transform.GetChild(0).GetComponent<Renderer>().enabled = !isCloudShown;
            AdaptSensor(sensor, _room);
        }
        isCloudShown = !isCloudShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the point cloud is to be shown or hidden.
    /// </summary>
    /// <param name="_sensor">the sensor to adapt</param>
    /// <param name="_room">the room where the sensor is</param>
    private static void AdaptSensor(Sensor _sensor, Room _room)
    {
        if (!isCloudShown)
        {
            float height = _sensor.map(_sensor.temperature, GameManager.gm.configLoader.GetTemperatureLimit("min", _sensor.temperatureUnit), GameManager.gm.configLoader.GetTemperatureLimit("max", _sensor.temperatureUnit), 0, Utils.ParseDecFrac(_room.attributes["height"]));
            _sensor.transform.position = new Vector3(_sensor.transform.position.x, _room.transform.position.y + 0.01f + 0.5f * height, _sensor.transform.position.z);
            _sensor.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
        }
        else
        {
            _sensor.transform.localPosition = _sensor.basePosition;
            _sensor.transform.GetChild(0).localScale = _sensor.baseScale;
        }
    }
}
