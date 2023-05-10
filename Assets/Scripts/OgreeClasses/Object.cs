using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class OObject : OgreeObject
{
    public Color color;
    public bool isHidden = false;

    /// <summary>
    /// The direct child of a room which is a parent of this object or which is this object
    /// </summary>
    public OObject referent;
    public GameObject tempBar;
    public string temperatureUnit;

    private void Start()
    {
        EventManager.instance.AddListener<UpdateDomainEvent>(UpdateColorByDomain);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.RemoveListener<UpdateDomainEvent>(UpdateColorByDomain);
    }

    ///<summary>
    /// Update the OObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
        hierarchyName = _src.hierarchyName;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        if (domain != _src.domain)
        {
            domain = _src.domain;
            UpdateColorByDomain();
        }
        description = _src.description;

        foreach (string attribute in _src.attributes.Keys)
            if (attribute.StartsWith("temperature_")
                && (!attributes.ContainsKey(attribute)
                    || attributes[attribute] != _src.attributes[attribute]))
                SetTemperature(_src.attributes[attribute], attribute.Substring(12));

        attributes = _src.attributes;

        if (!transform.parent || transform.parent.GetComponent<OgreeObject>().category == "room")
            referent = this;
        else if (transform.parent?.GetComponent<OObject>().referent != null)
            referent = transform.parent.GetComponent<OObject>().referent;
        else
            referent = null;

        GetComponent<DisplayObjectData>()?.UpdateLabels();
    }

    ///<summary>
    /// Update object's alpha according to _input, true or false.
    ///</summary>
    ///<param name="_value">Alpha wanted for the rack</param>
    public void ToggleAlpha(bool _value)
    {
        DisplayObjectData dod = GetComponent<DisplayObjectData>();
        transform.GetChild(0).GetComponent<Renderer>().enabled = !_value;
        dod?.ToggleLabel(!_value);
        isHidden = _value;
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    public void SetColor(string _hex)
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        color = new Color();
        bool validColor = ColorUtility.TryParseHtmlString($"#{_hex}", out color);
        if (validColor)
        {
            color.a = mat.color.a;
            CustomRendererOutline cro = GetComponent<CustomRendererOutline>();
            if (cro && !cro.isSelected && !cro.isHovered && !cro.isHighlighted && !cro.isFocused)
                mat.color = color;
        }
        else
        {
            UpdateColorByDomain();
            GameManager.instance.AppendLogLine($"[{hierarchyName}] Unknown color to display", ELogTarget.both, ELogtype.warning);
        }
    }

    ///<summary>
    /// On an UpdateDomainEvent, update the object's color if its the right domain
    ///</summary>
    ///<param name="_event">The event to catch</param>
    private void UpdateColorByDomain(UpdateDomainEvent _event)
    {
        if (_event.name == domain)
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

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        color = Utils.ParseHtmlColor($"#{domain.attributes["color"]}");

        CustomRendererOutline cro = GetComponent<CustomRendererOutline>();
        if (cro && !cro.isSelected && !cro.isHovered && !cro.isHighlighted && !cro.isFocused)
            mat.color = new Color(color.r, color.g, color.b, mat.color.a);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleSlots(bool _value)
    {
        Slot[] slots = GetComponentsInChildren<Slot>();
        if (slots.Length == 0)
            return;

        foreach (Slot s in slots)
        {
            if (s.transform.parent == transform && s.used == false)
                s.Display(_value);
        }
    }

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    public void ToggleCS()
    {
        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS)
            localCS.CleanDestroy($"Hide local Coordinate System for {name}");
        else
            BuildLocalCS(csName);
    }

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    ///<param name="_value">true of false value</param>
    public void ToggleCS(bool _value)
    {
        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS && !_value)
            localCS.CleanDestroy($"Hide local Coordinate System for {name}");
        else if (!localCS && _value)
            BuildLocalCS(csName);
    }

    ///<summary>
    /// Create a local Coordinate System for this object.
    ///</summary>
    ///<param name="_name">The name of the local CS</param>
    private void BuildLocalCS(string _name)
    {
        GameObject localCS = Instantiate(GameManager.instance.coordinateSystemModel);
        localCS.name = _name;
        localCS.transform.parent = transform;
        localCS.transform.localScale = Vector3.one;
        localCS.transform.localEulerAngles = Vector3.zero;
        localCS.transform.localPosition = transform.GetChild(0).localScale / -2f;
        GameManager.instance.AppendLogLine($"Display local Coordinate System for {name}", ELogTarget.logger, ELogtype.success);
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
                GameManager.instance.AppendLogLine($"[{hierarchyName}] Sensor {_sensorName} does not exist", ELogTarget.both, ELogtype.warning);
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
                    category = "sensor",
                    parentId = id,
                    domain = domain
                };
                se.attributes["formFactor"] = "ext";
                se.attributes["temperature"] = _value;

                Sensor sensor = OgreeGenerator.instance.CreateSensorFromSApiObject(se, transform);
                sensor.SetTemperature(GetTemperatureInfos().mean);
            }
        }
        else
            GameManager.instance.AppendLogLine($"[{hierarchyName}] Temperature must be a numerical value", ELogTarget.both, ELogtype.warning);
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
            OObject childOO = child.GetComponent<OObject>();
            if (childOO)
            {
                temps.Add((childOO.GetTemperatureInfos().mean, Utils.VolumeOfMesh(child.GetChild(0).GetComponent<MeshFilter>()), childOO.name));
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
