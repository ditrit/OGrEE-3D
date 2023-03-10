using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TempDiagram : MonoBehaviour
{
    public static TempDiagram instance;

    /// <summary>
    /// true if the temperature diagram is displayed
    /// </summary>
    public bool isDiagramShown = false;
    public bool isScatterPlotShown = false;
    public Room lastRoom;
    private OgreeObject lastScatterPlot;
    [SerializeField] private GameObject heatMapModel;
    [SerializeField] private float radiusRatio;
    [SerializeField] private float intensityMin;
    [SerializeField] private float intensityMax;
    public int heatMapSensorsMaxNumber;
    public Texture2D heatMapGradient;
    [SerializeField] private Texture2D heatMapGradientDefault;
    private Texture2D heatMapGradientCustom;
    private Gradient gradient;
    [SerializeField] private Material heatMapMat;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
    }

    ///<summary>
    /// When called checks if the temperature diagram is currently shown if true hide it.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (isDiagramShown && GameManager.instance.currentItems[0].GetComponent<OgreeObject>().category != "tempBar")
            HandleTempBarChart(lastRoom);
        if (isScatterPlotShown)
            HandleScatterPlot(lastScatterPlot);
    }

    /// <summary>
    /// Recursive fonction, get a list of the sensors in the object or in the children of it.
    /// </summary>
    /// <param name="_ogreeObject">The object where we get the sensors</param>
    /// <returns>a list of sensors of the object</returns>
    private List<Sensor> GetObjectSensors(OgreeObject _ogreeObject)
    {
        List<Sensor> sensors = new List<Sensor>();
        foreach (Transform childTransform in _ogreeObject.transform)
        {
            OgreeObject childOgreeObject = childTransform.GetComponent<OgreeObject>();
            if (childOgreeObject)
                sensors.AddRange(GetObjectSensors(childOgreeObject));
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
            default:
                GameManager.instance.AppendLogLine($"Room height unit not supported :{_room.attributes["heightUnit"]}", true, ELogtype.warning); break;
        }

        EventManager.instance.Raise(new TemperatureDiagramEvent() { obj = _room.gameObject });

        if (!isDiagramShown)
        {
            foreach (Transform childTransform in _room.transform)
            {
                OObject childOgreeObject = childTransform.GetComponent<OObject>();
                if (childOgreeObject && !childTransform.GetComponent<Group>())
                    ComputeTempBar(childOgreeObject, _room.temperatureUnit, roomHeight);
            }
            lastRoom = _room;
        }
        else
        {
            foreach (Transform childTransform in _room.transform)
            {
                OObject childOgreeObject = childTransform.GetComponent<OObject>();
                if (childOgreeObject)
                    Destroy(childOgreeObject.tempBar);
            }
        }
        isDiagramShown = !isDiagramShown;
    }

    /// <summary>
    /// Create a vertical bar representing the mean and standard deviation of the temperature of an object
    /// </summary>
    /// <param name="_oobject">the object whose temperature is represented by the bar</param>
    /// <param name="_tempUnit">the temperature unit of the object</param>
    /// <param name="_roomheight">the height of the room containing the object</param>
    private void ComputeTempBar(OObject _oobject, string _tempUnit, float _roomheight)
    {
        float pixelX;
        GameObject sensorBar;
        STemp tempInfos = _oobject.GetTemperatureInfos();
        if (!float.IsNaN(tempInfos.mean))
        {
            (int tempMin, int tempMax) = GameManager.instance.configLoader.GetTemperatureLimit(_tempUnit);
            float height = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, _roomheight);
            float heigthStd = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, _roomheight);
            float yBase = _oobject.transform.parent.position.y + 0.01f;


            sensorBar = Instantiate(GameManager.instance.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);

            pixelX = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, heatMapGradient.width);
            Color col = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(col.r, col.g, col.b, 0.85f);

            if (tempInfos.std != 0)
            {
                GameObject sensorBarStd = Instantiate(GameManager.instance.sensorBarStdModel, _oobject.transform);
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
            sensorBar = Instantiate(GameManager.instance.sensorBarModel, _oobject.transform);
            sensorBar.name = _oobject.name + "TempBar";
            sensorBar.transform.position = new Vector3(_oobject.transform.position.x, yBase + 0.5f * height, _oobject.transform.position.z);
            sensorBar.transform.GetChild(0).localScale = new Vector3(0.1f, height, 0.1f);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 0.85f);
        }
        OgreeObject sensorBarOO = sensorBar.GetComponent<OgreeObject>();
        sensorBarOO.attributes["average"] = $"{tempInfos.mean} {tempInfos.unit}";
        sensorBarOO.attributes["standard deviation"] = $"{tempInfos.std} {tempInfos.unit}";
        sensorBarOO.attributes["minimum"] = $"{tempInfos.min} {tempInfos.unit}";
        sensorBarOO.attributes["maximum"] = $"{tempInfos.max} {tempInfos.unit}";
        sensorBarOO.attributes["hottest child"] = tempInfos.hottestChild;
        _oobject.tempBar = sensorBar;
    }

    /// <summary>
    /// Show or hide all sensor in an object according to isScatterPlotShown and raise a TemperatureScatterPlot event
    /// </summary>
    /// <param name="_ogreeObject">the object where the scatter plot is shown or hidden</param>
    public void HandleScatterPlot(OgreeObject _ogreeObject)
    {
        lastScatterPlot = _ogreeObject;
        EventManager.instance.Raise(new TemperatureScatterPlotEvent() { obj = _ogreeObject.gameObject });

        GetObjectSensors(_ogreeObject).ForEach(s => s.transform.GetChild(0).GetComponent<Renderer>().enabled = !isScatterPlotShown);

        isScatterPlotShown = !isScatterPlotShown;
    }

    /// <summary>
    /// Create a heatmap for an object which show its temperature values. The normal of the heatmap is the smallest dimension (x,y or z) of the object
    /// </summary>
    /// <param name="_oObject">the object which will have the heatmap</param>
    public void HandleHeatMap(OObject _oObject)
    {
        Transform objTransform = _oObject.transform.GetChild(0);
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

        List<Sensor> sensors = GetObjectSensors(_oObject);

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
            (int tempMin, int tempMax) = GameManager.instance.configLoader.GetTemperatureLimit(sensor.temperatureUnit);
            float intensity = Utils.MapAndClamp(sensor.temperature, tempMin, tempMax, intensityMin, intensityMax);
            sensorProperties[i] = new Vector4(objTransform.localScale.sqrMagnitude * radiusRatio, intensity, 0, 0);
        }
        heatmap.GetComponent<Heatmap>().SetPositionsAndProperties(sensorPositions, sensorProperties);
        objTransform.hasChanged = true;
    }

    /// <summary>
    /// Create a custom gradient texture (Texture2D) using Unity's Gradient class.
    /// <br/><b>8 colors max, colors after the 8th one will not be used</b>
    /// </summary>
    /// <param name="_colors">The colors in the gradient, a list of int array of 4 elements : first 3 are rgb values, last is the position of the gradient (0-100)
    /// <br/><b>8 colors max, colors after the 8th one will not be used</b></param>
    public void MakeCustomGradient(List<List<int>> _colors)
    {
        GradientColorKey[] colorKeys = new GradientColorKey[Mathf.Min(_colors.Count, 8)]; //Unity gradients can only take 8 colors max
        foreach (List<int> color in _colors)
            colorKeys[_colors.IndexOf(color)] = new GradientColorKey { color = new Color(color[0], color[1], color[2]), time = color[3] / 100.0f };

        gradient = new Gradient();
        gradient.SetKeys(colorKeys, new GradientAlphaKey[0]);
        // create texture
        Texture2D outputTex = new Texture2D(1024, 128)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        // draw texture
        for (int x = 0; x < 1024; x++)
            for (int y = 0; y < 128; y++)
            {
                Color color = gradient.Evaluate(x / (float)1024);
                color = new Color(color.r / 255, color.g / 255, color.b / 255, 1);
                outputTex.SetPixel(x, y, color);
            }
        outputTex.Apply();
        heatMapGradientCustom = outputTex;
    }

    /// <summary>
    /// Create a custom gradient and set the gradient to use
    /// </summary>
    /// <param name="_colors">The colors in the gradient, a list of int array of 4 elements : first 3 are rgb values, last is the position on the gradient (0-100)</param>
    /// <param name="_useCustomGradient">if true, temperature colors will use the gradient created with <paramref name="_colors"/>, otherwise they will use the default gradient </param>
    public void SetGradient(List<List<int>> _colors, bool _useCustomGradient)
    {
        MakeCustomGradient(_colors);
        heatMapGradient = _useCustomGradient ? heatMapGradientCustom : heatMapGradientDefault;
        heatMapMat.SetTexture("_HeatTex", heatMapGradient);
    }
}
