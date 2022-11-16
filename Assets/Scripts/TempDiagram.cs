using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TempDiagram : MonoBehaviour
{
    /// <summary>
    /// true if thetemperature diagram is displayed
    /// </summary>
    public bool isDiagramShown = false;
    public bool isScatterPlotShown = false;
    public Room lastRoom;
    private OgreeObject lastScatterPlot;
    [SerializeField]
    private GameObject heatMapModel;
    [SerializeField]
    private float radiusRatio;
    [SerializeField]
    private float intensityMin;
    [SerializeField]
    private float intensityMax;
    public int heatMapSensorsMaxNumber;
    public Texture2D heatMapGradient;
    public static TempDiagram instance;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    ///<summary>
    /// When called checks if the temperature diagram is currently shown if true hide it.
    ///</summary>
    ///<param name="e">The event's instance</param>
    void OnSelectItem(OnSelectItemEvent _e)
    {
        if (isDiagramShown && GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category != "tempBar")
            HandleTempBarChart(lastRoom);
        if (isScatterPlotShown)
            HandleScatterPlot(lastScatterPlot);
    }

    /// <summary>
    /// Recursive fonction, get a list of the sensors in the object or in the children of it.
    /// </summary>
    /// <param name="_ogreeObject">The object where we get the sensors</param>
    /// <returns>a list of sensors of the object.</returns>
    private List<Sensor> GetObjectSensors(OgreeObject _ogreeObject)
    {
        List<Sensor> sensors = new List<Sensor>();
        foreach (Transform childTransform in _ogreeObject.transform)
        {
            OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
            if (childOgreeObject)
            {
                sensors.AddRange(GetObjectSensors(childOgreeObject));
            }
            Sensor childSensor = childTransform.GetComponent<Sensor>();
            if (childSensor && childSensor.fromTemplate)
                sensors.Add(childSensor);
        }
        return sensors;

    }

    /// <summary>
    /// Show the temperature diagram if it is not already shown, hide it if it is.
    /// </summary>
    /// <param name="_room">the object where we show/hide the temperature diagram</param>
    public void HandleTempBarChart(Room _room)
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
                    AdaptOObject(childOgreeObject, tempUnit, roomHeight);
            }
            lastRoom = _room;
        }
        else
            foreach (Transform childTransform in _room.transform)
            {
                OObject childOgreeObject = childTransform.GetComponent<OObject>();
                if (childOgreeObject)
                    Destroy(childOgreeObject.tempBar);

            }
        isDiagramShown = !isDiagramShown;
    }

    /// <summary>
    /// Change a sensor scale and position according to their temperature and if the temperature diagram is to be shown or hidden.
    /// </summary>
    /// <param name="_oobject">the object where the sensor is</param>
    /// <param name="_tempUnit">the temperature unit of the object</param>
    /// <param name="_roomheight">the height of the room containing the object</param>
    private void AdaptOObject(OObject _oobject, string _tempUnit, float _roomheight)
    {
        float pixelX;
        GameObject sensorBar;
        STemp tempInfos = _oobject.GetTemperatureInfos();
        if (!(tempInfos.mean is float.NaN))
        {
            (int tempMin, int tempMax) = GameManager.gm.configLoader.GetTemperatureLimit(_tempUnit);
            float height = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, _roomheight);
            float heigthStd = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, _roomheight);
            float yBase = _oobject.transform.parent.position.y + 0.01f;


            sensorBar = Instantiate(GameManager.gm.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);

            pixelX = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, heatMapGradient.width);
            Color col = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(col.r, col.g, col.b, 0.85f);

            if (tempInfos.std != 0)
            {

                GameObject sensorBarStd = Instantiate(GameManager.gm.sensorBarStdModel, _oobject.transform);
                sensorBarStd.transform.position = new Vector3(_oobject.transform.position.x, yBase + height, _oobject.transform.position.z);
                sensorBarStd.transform.GetChild(0).localScale = new Vector3(1, heigthStd, 1);
                sensorBarStd.transform.parent = sensorBar.transform;

                pixelX = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, heatMapGradient.width);

                sensorBarStd.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
                sensorBarStd.transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
                sensorBarStd.transform.GetChild(0).GetChild(2).GetComponent<Renderer>().material.color = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
            }
        }
        else
        {

            float height = _roomheight / 2;

            float yBase = _oobject.transform.parent.position.y + 0.01f;
            sensorBar = Instantiate(GameManager.gm.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 0.85f);

        }
        OgreeObject sensorBarOO = sensorBar.GetComponent<OgreeObject>();
        sensorBarOO.attributes["mean"] = $"{tempInfos.mean} {tempInfos.unit}";
        sensorBarOO.attributes["standard deviation"] = $"{tempInfos.std} {tempInfos.unit}";
        sensorBarOO.attributes["min"] = $"{tempInfos.min} {tempInfos.unit}";
        sensorBarOO.attributes["max"] = $"{tempInfos.max} {tempInfos.unit}";
        sensorBarOO.attributes["hottest child"] = tempInfos.hottestChild;
        _oobject.tempBar = sensorBar;
    }

    public void HandleScatterPlot(OgreeObject _ogreeObject)
    {
        lastScatterPlot = _ogreeObject;
        EventManager.Instance.Raise(new TemperatureScatterPlotEvent() { obj = _ogreeObject.gameObject });

        GetObjectSensors(_ogreeObject).ForEach(s => s.transform.GetChild(0).GetComponent<Renderer>().enabled = !isScatterPlotShown);

        isScatterPlotShown = !isScatterPlotShown;
    }

    public void HandleHeatMap(OObject oObject)
    {
        Transform objTransform = oObject.transform.GetChild(0);
        if (objTransform.childCount > 0)
        {
            Destroy(objTransform.GetChild(0).gameObject);
            return;
        }
        float minDimension = Mathf.Min(objTransform.localScale.x, objTransform.localScale.y, objTransform.localScale.z);

        GameObject heatmap = Instantiate(heatMapModel, objTransform);

        if (minDimension == objTransform.localScale.y)
            heatmap.transform.SetPositionAndRotation(objTransform.position + (objTransform.localScale.y / 2 + 0.01f) * (objTransform.rotation * Vector3.up), objTransform.rotation * Quaternion.Euler(90, 0, 0));
        else if (minDimension == objTransform.localScale.z)
            heatmap.transform.SetPositionAndRotation(objTransform.position + (objTransform.localScale.z / 2 + 0.01f) * (objTransform.rotation * Vector3.back), objTransform.rotation * Quaternion.Euler(0, 0, 0));
        else
            heatmap.transform.SetPositionAndRotation(objTransform.position + (objTransform.localScale.x / 2 + 0.01f) * (objTransform.rotation * Vector3.left), objTransform.rotation * Quaternion.Euler(0, 90, 0));

        List<Sensor> sensors = GetObjectSensors(oObject);

        Vector4[] sensorPositions = new Vector4[sensors.Count];
        Vector4[] sensorProperties = new Vector4[sensors.Count];

        for (int i = 0; i < sensors.Count; i++)
        {
            Sensor sensor = sensors.ElementAt(i);
            Vector3 sensorPos = sensor.transform.position;
            sensorPos -= heatmap.transform.position;
            sensorPos = Quaternion.Inverse(heatmap.transform.rotation) * sensorPos;
            sensorPos.Scale(new Vector3(1 / heatmap.transform.lossyScale.x, 1 / heatmap.transform.lossyScale.y, 1 / heatmap.transform.lossyScale.z));
            sensorPositions[i] = new Vector4(sensorPos.x, sensorPos.y, 0, 0);
            (int tempMin, int tempMax) = GameManager.gm.configLoader.GetTemperatureLimit(sensor.temperatureUnit);
            float intensity = Utils.MapAndClamp(sensor.temperature, tempMin, tempMax, intensityMin, intensityMax);
            sensorProperties[i] = new Vector4(objTransform.localScale.sqrMagnitude * radiusRatio, intensity, 0, 0);
        }
        heatmap.GetComponent<Heatmap>().SetPositionsAndProperties(sensorPositions, sensorProperties);
        objTransform.hasChanged = true;
    }

}
