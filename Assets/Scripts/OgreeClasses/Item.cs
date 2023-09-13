using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class Item : OgreeObject
{
    public Color color;
    public bool isHidden = false;

    /// <summary>
    /// The direct child of a room which is a parent of this object or which is this object
    /// </summary>
    public Item referent;
    public GameObject tempBar;
    public string temperatureUnit;
    public bool hasSlotColor = false;

    public ClearanceHandler clearanceHandler = new ClearanceHandler();

    protected virtual void Start()
    {
        EventManager.instance.UpdateDomain.Add(UpdateColorByDomain);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.UpdateDomain.Remove(UpdateColorByDomain);
    }

    ///<summary>
    /// Update the OObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        domain = _src.domain;
        description = _src.description;

        foreach (string attribute in _src.attributes.Keys)
        {
            if (attribute.StartsWith("temperature_")
                && (!attributes.ContainsKey(attribute)
                    || attributes[attribute] != _src.attributes[attribute]))
                SetTemperature(_src.attributes[attribute], attribute.Substring(12));
            if (attribute == "clearance")
            {
                try
                {
                    List<float> lengths = JsonConvert.DeserializeObject<List<float>>(_src.attributes[attribute]);
                    if (lengths != null && lengths.Count == 5)
                        clearanceHandler.Initialize(lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], transform);
                    else
                        GameManager.instance.AppendLogLine("wrong vector cardinalty for clearance", ELogTarget.both, ELogtype.error);
                } catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        attributes = _src.attributes;

        if (!transform.parent || transform.parent.GetComponent<OgreeObject>().category == Category.Room)
            referent = this;
        else if (transform.parent?.GetComponent<Item>().referent != null)
            referent = transform.parent.GetComponent<Item>().referent;
        else
            referent = null;

        GetComponent<DisplayObjectData>()?.UpdateLabels();
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    public void SetColor(string _hex)
    {
        color = new Color();
        bool validColor = ColorUtility.TryParseHtmlString($"#{_hex}", out color);
        if (validColor)
            color = GetComponent<ObjectDisplayController>().ChangeColor(color);
        else
        {
            UpdateColorByDomain();
            GameManager.instance.AppendLogLine($"[{id}] Unknown color to display", ELogTarget.both, ELogtype.warning);
        }
    }

    ///<summary>
    /// On an UpdateDomainEvent, update the object's color if its the right domain
    ///</summary>
    ///<param name="_event">The event to catch</param>
    private void UpdateColorByDomain(UpdateDomainEvent _event)
    {
        if (_event.name == domain && !hasSlotColor && !attributes.ContainsKey("color"))
            UpdateColorByDomain();
    }

    ///<summary>
    /// Update object's color according to its domain.
    ///</summary>
    public void UpdateColorByDomain()
    {
        if (string.IsNullOrEmpty(base.domain))
            return;

        if (!GameManager.instance.allItems.Contains(base.domain))
        {
            GameManager.instance.AppendLogLine($"Domain \"{base.domain}\" doesn't exist.", ELogTarget.both, ELogtype.error);
            return;
        }

        OgreeObject domain = ((GameObject)GameManager.instance.allItems[base.domain]).GetComponent<OgreeObject>();

        color = Utils.ParseHtmlColor($"#{domain.attributes["color"]}");

        GetComponent<ObjectDisplayController>().ChangeColor(color.r, color.g, color.b);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleSlots(bool _value)
    {
        Slot[] slots = GetComponentsInChildren<Slot>();

        foreach (Slot s in slots)
        {
            if (s.transform.parent == transform && s.used == false)
                s.GetComponent<ObjectDisplayController>().Display(_value, _value);
        }
    }

    ///<summary>
    /// Set temperature attribute and create/update related sensor object.
    ///</summary>
    ///<param name="_value">The temperature value</param>
    ///<param name="_sensorName">The sensor to modify</param>
    public void SetTemperature(string _value, string _sensorName)
    {
        if (Regex.IsMatch(_value, "^[0-9.]+$"))
        {
            Transform sensorTransform = transform.Find(_sensorName);
            if (sensorTransform)
                sensorTransform.GetComponent<Sensor>().SetTemperature(_value);
            else
            {
                GameManager.instance.AppendLogLine($"[{id}] Sensor {_sensorName} does not exist", ELogTarget.both, ELogtype.warning);
                return;
            }

            sensorTransform = transform.Find("sensor");
            if (sensorTransform)
                sensorTransform.GetComponent<Sensor>().SetTemperature(GetTemperatureInfos().mean);
            else
            {
                SApiObject se = new SApiObject
                {
                    description = new List<string>(),
                    attributes = new Dictionary<string, string>(),

                    name = "sensor", // ?
                    category = Category.Sensor,
                    parentId = id,
                    domain = domain
                };
                se.attributes["temperature"] = _value;

                Sensor sensor = OgreeGenerator.instance.CreateSensorFromSApiObject(se, transform);
                sensor.SetTemperature(GetTemperatureInfos().mean);
            }
        }
        else
            GameManager.instance.AppendLogLine($"[{id}] Temperature must be a numerical value", ELogTarget.both, ELogtype.warning);
    }

    /// <summary>
    /// Compute recursively temperature average, standard deviation, min, max and hottes child of the object
    /// </summary>
    /// <returns>a STemp instance containg all temperature infos of the object</returns>
    public STemp GetTemperatureInfos()
    {
        List<(float temp, float volume, string childName)> temps = new List<(float, float, string)>();
        List<(float temp, string sensorName)> sensorsTemps = new List<(float, string)>();
        foreach (Transform child in transform)
        {
            Item childItem = child.GetComponent<Item>();
            if (childItem)
            {
                temps.Add((childItem.GetTemperatureInfos().mean, Utils.VolumeOfMesh(child.GetChild(0).GetComponent<MeshFilter>()), childItem.name));
            }
            else
            {
                Sensor childSensor = child.GetComponent<Sensor>();
                if (childSensor && childSensor.fromTemplate && !float.IsNaN(childSensor.temperature))
                    sensorsTemps.Add((childSensor.temperature, childSensor.name));
            }
        }

        float mean = float.NaN;
        float std = float.NaN;
        float min = float.NaN;
        float max = float.NaN;
        string hottestChild = "";

        List<(float temp, float volume, string childName)> tempsNoNaN = temps.Where(v => !float.IsNaN(v.temp)).ToList();
        float totalEffectiveVolume = tempsNoNaN.Sum(v => v.volume);
        if (sensorsTemps.Count() > 0)
        {
            totalEffectiveVolume = Utils.VolumeOfMesh(transform.GetChild(0).GetComponent<MeshFilter>()) - temps.Where(v => float.IsNaN(v.temp)).Sum(v => v.volume);
            float sensorVolume = (totalEffectiveVolume - tempsNoNaN.Sum(v => v.volume)) / sensorsTemps.Count();

            sensorsTemps.ForEach(sensor => tempsNoNaN.Add((sensor.temp, sensorVolume, sensor.sensorName)));
        }
        if (tempsNoNaN.Count() > 0)
        {
            mean = tempsNoNaN.Sum(v => v.temp * v.volume) / totalEffectiveVolume;
            std = Mathf.Sqrt(tempsNoNaN.Sum(v => Mathf.Pow(v.temp - mean, 2) * v.volume) / totalEffectiveVolume);
            min = tempsNoNaN.Min(v => v.temp);
            max = tempsNoNaN.Max(v => v.temp);
            hottestChild = tempsNoNaN.Where(v => v.temp >= max).First().childName;
        }
        return new STemp(mean, std, min, max, hottestChild, temperatureUnit);
    }

}
