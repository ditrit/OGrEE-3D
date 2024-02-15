using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Item : OgreeObject
{
    public Color color;

    /// <summary>
    /// The direct child of a room which is a parent of this object or which is this object
    /// </summary>
    public Item referent;
    public GameObject tempBar;
    public string temperatureUnit;
    public bool hasSlotColor = false;

    public ClearanceHandler clearanceHandler = new();

    protected virtual void Start()
    {
        EventManager.instance.UpdateDomain.Add(UpdateColorByDomain);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.UpdateDomain.Remove(UpdateColorByDomain);
        if (GetComponent<ObjectDisplayController>().isHidden)
        {
            UiManager.instance.hiddenObjects.Remove(this);
            UiManager.instance.hiddenObjList.RebuildMenu(UiManager.instance.BuildHiddenObjButtons);
        }
    }

    ///<summary>
    /// Update the OObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        foreach (string attribute in _src.attributes.Keys)
        {
            if (attribute.StartsWith("temperature_")
                && (!attributes.ContainsKey(attribute) || attributes[attribute] != _src.attributes[attribute]))
                SetTemperature(_src.attributes[attribute], attribute.Substring(12));
            if (attribute == "clearance")
            {
                List<float> lengths = JsonConvert.DeserializeObject<List<float>>(_src.attributes[attribute]);
                if (lengths == null)
                {
                    GameManager.instance.AppendLogLine($"{_src.name} : can't deserialize clearance attribute", ELogTarget.both, ELogtype.error);
                    break;
                }
                if (lengths.Count != 6)
                {
                    GameManager.instance.AppendLogLine($"{_src.name} : wrong vector cardinality for clearance", ELogTarget.both, ELogtype.error);
                    break;
                }
                clearanceHandler.Initialize(lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5], transform);
            }
        }
        base.UpdateFromSApiObject(_src);

        if (!transform.parent || transform.parent.GetComponent<Room>())
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
        if (ColorUtility.TryParseHtmlString($"#{_hex}", out Color newColor))
        {
            color = newColor;
            GetComponent<ObjectDisplayController>().ChangeColor(color);
        }
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

        Domain domain = ((GameObject)GameManager.instance.allItems[base.domain]).GetComponent<Domain>();

        color = Utils.ParseHtmlColor($"#{domain.attributes["color"]}");
        GetComponent<ObjectDisplayController>().ChangeColor(color);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleSlots(bool _value)
    {
        foreach (Slot slot in GetComponentsInChildren<Slot>())
        {
            if (slot.transform.parent == transform && slot.used == false)
                slot.GetComponent<ObjectDisplayController>().Display(_value, _value);
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
                sensorTransform.GetComponent<Sensor>().SetTemperature(Utils.ParseDecFrac(_value));
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
                SApiObject se = new()
                {
                    attributes = new(),

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
        List<(float temp, float volume, string childName)> temps = new();
        List<(float temp, string sensorName)> sensorsTemps = new();
        foreach (Transform child in transform)
        {
            Item childItem = child.GetComponent<Item>();
            if (childItem)
                temps.Add((childItem.GetTemperatureInfos().mean, Utils.VolumeOfMesh(child.GetChild(0).GetComponent<MeshFilter>()), childItem.name));
            else if (child.GetComponent<Sensor>() is Sensor childSensor && childSensor.fromTemplate && !float.IsNaN(childSensor.temperature))
                sensorsTemps.Add((childSensor.temperature, childSensor.name));
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
        return new(mean, std, min, max, hottestChild, temperatureUnit);
    }

}
