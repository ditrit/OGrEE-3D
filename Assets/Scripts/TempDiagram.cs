using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TempDiagram : MonoBehaviour
{
    public static TempDiagram instance;

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
    private GameObject LastHeatMap = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    /// <summary>
    /// Recursive fonction, get a list of the sensors in the object or in the children of it.
    /// </summary>
    /// <param name="_ogreeObject">The object where we get the sensors</param>
    /// <returns>a list of sensors of the object</returns>
    public List<Sensor> GetObjectSensors(OgreeObject _ogreeObject)
    {
        List<Sensor> sensors = new();
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
    public async void HandleTempBarChart(Room _room)
    {
        if (_room.scatterPlot)
            HandleScatterPlot(_room);
        float roomHeight = (float)(double)_room.attributes["height"];

        switch (_room.attributes["heightUnit"])
        {
            case LengthUnit.Meter:
                break;
            case LengthUnit.Centimeter:
                roomHeight /= 100;
                break;
            case LengthUnit.Millimeter:
                roomHeight /= 1000;
                break;
            default:
                GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Room height unit not supported", (string)_room.attributes["heightUnit"]), ELogTarget.both, ELogtype.warning);
                break;
        }

        EventManager.instance.Raise(new TemperatureDiagramEvent(_room));

        if (!_room.barChart)
        {
            foreach (Transform childTransform in _room.transform)
            {
                Item childOgreeObject = childTransform.GetComponent<Item>();
                if (childOgreeObject && childOgreeObject is not GenericObject)
                {
                    if (childOgreeObject is Group childGroup && childGroup.isDisplayed)
                    {
                        childGroup.ToggleContent(true);
                        _room.openedGroups.Add(childGroup);
                    }
                    ComputeTempBar(childOgreeObject, _room.temperatureUnit, roomHeight);
                }
            }
        }
        else
        {
            foreach (Transform childTransform in _room.transform)
            {
                Item childOgreeObject = childTransform.GetComponent<Item>();
                if (childOgreeObject)
                {
                    if (GameManager.instance.GetSelected().Contains(childOgreeObject.tempBar))
                        await GameManager.instance.SetCurrentItem(null);
                    Destroy(childOgreeObject.tempBar);
                }
            }
            foreach (Group group in _room.openedGroups)
                group.ToggleContent(false);
            _room.openedGroups.Clear();
        }
        _room.barChart = !_room.barChart;
    }

    /// <summary>
    /// Create a vertical bar representing the mean and standard deviation of the temperature of an object
    /// </summary>
    /// <param name="_item">the object whose temperature is represented by the bar</param>
    /// <param name="_tempUnit">the temperature unit of the object</param>
    /// <param name="_roomheight">the height of the room containing the object</param>
    private void ComputeTempBar(Item _item, string _tempUnit, float _roomheight)
    {
        float pixelX;
        GameObject sensorBar;
        STemp tempInfos = _item.GetTemperatureInfos();
        (int tempMin, int tempMax) = GameManager.instance.configHandler.GetTemperatureLimit(_tempUnit);
        if (!float.IsNaN(tempInfos.mean) && (tempMin, tempMax) != (0, 0))
        {
            float height = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, _roomheight);
            float heigthStd = Utils.MapAndClamp(tempInfos.std, tempMin, tempMax, 0, _roomheight);
            float yBase = _item.transform.parent.position.y + 0.01f;


            sensorBar = Instantiate(GameManager.instance.sensorBarModel, _item.transform);
            sensorBar.name = _item.name + "TempBar";
            sensorBar.transform.position = new(_item.transform.GetChild(0).position.x, yBase + 0.5f * height, _item.transform.GetChild(0).position.z);
            sensorBar.transform.GetChild(0).localScale = new(0.1f, height, 0.1f);

            pixelX = Utils.MapAndClamp(tempInfos.mean, tempMin, tempMax, 0, heatMapGradient.width);
            Color col = heatMapGradient.GetPixel(Mathf.FloorToInt(pixelX), heatMapGradient.height / 2);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new(col.r, col.g, col.b, 0.85f);

            if (tempInfos.std != 0)
            {
                GameObject sensorBarStd = Instantiate(GameManager.instance.sensorBarStdModel, _item.transform);
                sensorBarStd.transform.position = new(_item.transform.GetChild(0).position.x, yBase + height, _item.transform.GetChild(0).position.z);
                sensorBarStd.transform.GetChild(0).localScale = new(1, heigthStd, 1);
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

            float yBase = _item.transform.parent.position.y + 0.01f;
            sensorBar = Instantiate(GameManager.instance.sensorBarModel, _item.transform);
            sensorBar.name = _item.name + "TempBar";
            sensorBar.transform.position = new(_item.transform.GetChild(0).position.x, yBase + 0.5f * height, _item.transform.GetChild(0).position.z);
            sensorBar.transform.GetChild(0).localScale = new(0.1f, height, 0.1f);
            sensorBar.transform.GetChild(0).GetComponent<Renderer>().material.color = new(0.5f, 0.5f, 0.5f, 0.85f);
        }
        OgreeObject sensorBarOO = sensorBar.GetComponent<OgreeObject>();
        sensorBarOO.attributes["average"] = $"{tempInfos.mean} {tempInfos.unit}";
        sensorBarOO.attributes["standard deviation"] = $"{tempInfos.std} {tempInfos.unit}";
        sensorBarOO.attributes["minimum"] = $"{tempInfos.min} {tempInfos.unit}";
        sensorBarOO.attributes["maximum"] = $"{tempInfos.max} {tempInfos.unit}";
        sensorBarOO.attributes["hottest child"] = tempInfos.hottestChild;
        _item.tempBar = sensorBar;
    }

    /// <summary>
    /// Show or hide all sensor in an object according to isScatterPlotShown and raise a TemperatureScatterPlot event
    /// </summary>
    /// <param name="_ogreeObject">the object where the scatter plot is shown or hidden</param>
    public void HandleScatterPlot(OgreeObject _ogreeObject)
    {
        if (_ogreeObject is Room room && room.barChart)
            HandleTempBarChart(room);
        _ogreeObject.scatterPlot = !_ogreeObject.scatterPlot;
        EventManager.instance.Raise(new TemperatureScatterPlotEvent(_ogreeObject));

        GetObjectSensors(_ogreeObject).ForEach(s => s.transform.GetChild(0).GetComponent<Renderer>().enabled = _ogreeObject.scatterPlot);

    }

    /// <summary>
    /// Create a heatmap for an object which show its temperature values. The normal of the heatmap is the smallest dimension (x,y or z) of the object
    /// </summary>
    /// <param name="_item">the object which will have the heatmap</param>
    public void HandleHeatMap(Item _item)
    {
        Destroy(LastHeatMap);
        Transform objTransform = _item.transform.GetChild(0);
        if (_item.heatMap)
        {
            Destroy(_item.heatMap);
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

        List<Sensor> sensors = GetObjectSensors(_item);

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
            (int tempMin, int tempMax) = GameManager.instance.configHandler.GetTemperatureLimit(sensor.temperatureUnit);
            float intensity = Utils.MapAndClamp(sensor.temperature, tempMin, tempMax, intensityMin, intensityMax);
            sensorProperties[i] = new Vector4(objTransform.localScale.sqrMagnitude * radiusRatio, intensity, 0, 0);
        }
        heatmap.GetComponent<Heatmap>().SetPositionsAndProperties(sensorPositions, sensorProperties);
        objTransform.hasChanged = true;
        _item.heatMap = heatmap;
        LastHeatMap = heatmap;
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
            colorKeys[_colors.IndexOf(color)] = new() { color = new Color(color[0], color[1], color[2]), time = color[3] / 100.0f };

        gradient = new Gradient();
        gradient.SetKeys(colorKeys, new GradientAlphaKey[0]);
        // create texture
        Texture2D outputTex = new(1024, 128)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        // draw texture
        for (int x = 0; x < 1024; x++)
            for (int y = 0; y < 128; y++)
            {
                Color color = gradient.Evaluate(x / (float)1024);
                color = new(color.r / 255, color.g / 255, color.b / 255, 1);
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
