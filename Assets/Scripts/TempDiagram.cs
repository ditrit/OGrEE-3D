using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class TempDiagram
{
    /// <summary>
    /// true if thetemperature diagram is displayed
    /// </summary>
    public static bool isDiagramShown = false;
    public static Room lastRoom;
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
        if (isDiagramShown && GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category != "tempBar")
            HandleTempBarChart(lastRoom);
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
    /// <param name="_room">the object where we show/hide the temperature diagram</param>
    public static void HandleTempBarChart(Room _room)
    {
        float roomHeight = Utils.ParseDecFrac(_room.attributes["height"]);

        switch (_room.attributes["heightUnit"])
        {
            case "m":
                break;
            case "cm":
                roomHeight /= 100; break;
            case "mm":
                roomHeight /= 1000; break;
            case "U":
                roomHeight *= GameManager.gm.uSize; break;
            default:
                GameManager.gm.AppendLogLine($"Room height unit not supported :{_room.attributes["heightUnit"]}", true, eLogtype.warning); break;   
        }

        if (_room.attributes["heightUnit"] == "mm")
            roomHeight /= 1000;
        else if (_room.attributes["heightUnit"] == "cm")
            roomHeight /= 100;
        else if (_room.attributes["heightUnit"] == "U")
            roomHeight *= GameManager.gm.uSize;

        string tempUnit = "";
        OgreeObject site = _room.transform.parent?.parent?.GetComponent<OgreeObject>();
        if (site && site.attributes.ContainsKey("temperatureUnit"))
            tempUnit = site.attributes["temperatureUnit"];


        EventManager.Instance.Raise(new TemperatureDiagramEvent() { obj = _room.gameObject });

        if (!isDiagramShown)
        {

            foreach (Transform childTransform in _room.transform)
            {
                OObject childOgreeObject = childTransform.GetComponent<OObject>();
                if (childOgreeObject && !childTransform.GetComponent<Group>())
                    AdaptSensor(childOgreeObject,tempUnit, roomHeight);
            }
            lastRoom = _room;
        }
        else
            foreach (Transform childTransform in _room.transform)
            {
                OObject childOgreeObject = childTransform.GetComponent<OObject>();
                if (childOgreeObject)
                {
                    Debug.Log(childOgreeObject.tempBar);
                    Object.Destroy(childOgreeObject.tempBar);
                }
            }
        isDiagramShown = !isDiagramShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the temperature diagram is to be shown or hidden.
    /// </summary>
    /// <param name="_sensor">the sensor to adapt</param>
    /// <param name="_oobject">the object where the sensor is</param>
    private static void AdaptSensor(OObject _oobject,string _tempUnit,float _roomheight)
    {
        GameObject sensorBar;
        STemp tempInfos = _oobject.GetTemperatureInfos();
        if (!(tempInfos.mean is float.NaN))
        {
            (int tempMin, int tempMax) = GameManager.gm.configLoader.GetTemperatureLimit(_tempUnit);
            float height = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, _roomheight);
            float heigthStd = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, _roomheight);
            float yBase = _oobject.transform.parent.position.y + 0.01f;


            sensorBar = Object.Instantiate(GameManager.gm.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);

            float blue = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 1, 0);
            float red = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, 1);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(red, 0, blue, 0.85f);

            if (tempInfos.std != 0)
            {

                GameObject sensorBarStd = Object.Instantiate(GameManager.gm.sensorBarStdModel, _oobject.transform);
                sensorBarStd.transform.position = new Vector3(_oobject.transform.position.x, yBase + height, _oobject.transform.position.z);
                sensorBarStd.transform.GetChild(0).localScale = new Vector3(1, heigthStd, 1);
                sensorBarStd.transform.parent = sensorBar.transform;

                blue = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 1, 0);
                red = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, 1);
                sensorBarStd.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
                sensorBarStd.transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
                sensorBarStd.transform.GetChild(0).GetChild(2).GetComponent<Renderer>().material.color = new Color(red, 0, blue);
            }
        }
        else
        {

            float height = _roomheight/ 2;

            float yBase = _oobject.transform.parent.position.y + 0.01f;
            sensorBar = Object.Instantiate(GameManager.gm.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 0.85f);

        }
        OgreeObject sensorBarOO = sensorBar.GetComponent<OgreeObject>();
        sensorBarOO.attributes["mean"] = tempInfos.mean.ToString();
        sensorBarOO.attributes["standard deviation"] = tempInfos.std.ToString();
        sensorBarOO.attributes["min"] = tempInfos.min.ToString();
        sensorBarOO.attributes["max"] = tempInfos.max.ToString();
        sensorBarOO.attributes["hottest child"] = tempInfos.hottestChild;
        sensorBarOO.attributes["temperture unit"] = tempInfos.unit;
        _oobject.tempBar = sensorBar;
    }
}
