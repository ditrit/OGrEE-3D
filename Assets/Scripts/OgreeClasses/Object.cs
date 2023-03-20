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

    private void Awake()
    {
        EventManager.instance.AddListener<UpdateTenantEvent>(UpdateColorByTenant);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.instance.RemoveListener<UpdateTenantEvent>(UpdateColorByTenant);
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
        if (domain != _src.domain)
        {
            domain = _src.domain;
            UpdateColorByTenant();
        }
        description = _src.description;

        if (attributes.ContainsKey("temperature") && !_src.attributes.ContainsKey("temperature"))
            Destroy(transform.Find("sensor").gameObject);

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
    }

    ///<summary>
    /// Update object's alpha according to _input, true or false.
    ///</summary>
    ///<param name="_value">Alpha wanted for the rack</param>
    public void UpdateAlpha(string _value)
    {
        _value = _value.ToLower();
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine("alpha value has to be true or false", true, ELogtype.warning);
            return;
        }

        DisplayObjectData dod = GetComponent<DisplayObjectData>();
        if (_value == "true")
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            dod?.ToggleLabel(false);
            isHidden = true;
        }
        else
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            dod?.ToggleLabel(true);
            isHidden = false;
        }
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
            attributes["color"] = _hex;
        }
        else
        {
            UpdateColorByTenant();
            attributes.Remove("color");
            GameManager.instance.AppendLogLine("Unknown color", true, ELogtype.warning);
        }
    }

    ///
    private void UpdateColorByTenant(UpdateTenantEvent _event)
    {
        if (_event.name == domain)
            UpdateColorByTenant();
    }

    ///<summary>
    /// Update object's color according to its Tenant.
    ///</summary>
    public void UpdateColorByTenant()
    {
        if (string.IsNullOrEmpty(domain))
            return;

        if (!GameManager.instance.allItems.Contains(domain))
        {
            GameManager.instance.AppendLogLine($"Tenant \"{domain}\" doesn't exist.", false, ELogtype.error);
            return;
        }

        OgreeObject tenant = ((GameObject)GameManager.instance.allItems[domain]).GetComponent<OgreeObject>();

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        color = Utils.ParseHtmlColor($"#{tenant.attributes["color"]}");

        CustomRendererOutline cro = GetComponent<CustomRendererOutline>();
        if (cro && !cro.isSelected && !cro.isHovered && !cro.isHighlighted && !cro.isFocused)
            mat.color = new Color(color.r, color.g, color.b, mat.color.a);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleSlots(string _value)
    {
        _value = _value.ToLower();
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine("slots value has to be true or false", true, ELogtype.warning);
            return;
        }

        Slot[] slots = GetComponentsInChildren<Slot>();
        if (slots.Length == 0)
            return;

        foreach (Slot s in slots)
        {
            if (s.transform.parent == transform && s.used == false)
            {
                if (_value == "true")
                    s.Display(true);
                else
                    s.Display(false);
            }
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
        {
            localCS.SetActive(false); //for UI
            Destroy(localCS);
            GameManager.instance.AppendLogLine($"Hide local Coordinate System for {name}", false, ELogtype.success);
        }
        else
            PopLocalCS(csName);
    }

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    ///<param name="_value">true of false value</param>
    public void ToggleCS(string _value)
    {
        _value = _value.ToLower();
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine("slots value has to be true or false", true, ELogtype.warning);
            return;
        }

        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS && _value == "false")
        {
            Destroy(localCS);
            GameManager.instance.AppendLogLine($"Hide local Coordinate System for {name}", false, ELogtype.success);
        }
        else if (!localCS && _value == "true")
            PopLocalCS(csName);
    }

    ///<summary>
    /// Create a local Coordinate System for this object.
    ///</summary>
    ///<param name="_name">The name of the local CS</param>
    private void PopLocalCS(string _name)
    {
        GameObject localCS = Instantiate(GameManager.instance.coordinateSystemModel);
        localCS.name = _name;
        localCS.transform.parent = transform;
        localCS.transform.localScale = Vector3.one;
        localCS.transform.localEulerAngles = Vector3.zero;
        localCS.transform.localPosition = transform.GetChild(0).localScale / -2f;
        GameManager.instance.AppendLogLine($"Display local Coordinate System for {name}", false, ELogtype.success);
    }

    ///<summary>
    /// Set temperature attribute and create/update related sensor object.
    ///</summary>
    ///<param name="_value">The temperature value</param>
    ///<param name="_sensorName">The sensor to modify</param>
    public void SetTemperature(string _value, string _sensorName)
    {
        if (category == "corridor")
        {
            if (_sensorName != "")
                GameManager.instance.AppendLogLine("Corridors can not have sensors", true, ELogtype.warning);
            else if (Regex.IsMatch(_value, "^(cold|warm)$"))
                attributes["temperature"] = _value;
            else
                GameManager.instance.AppendLogLine("Temperature must be \"cold\" or \"warm\"", true, ELogtype.warning);
        }
        else
        {
            if (Regex.IsMatch(_value, "^[0-9.]+$"))
            {
                Transform sensorTransform = transform.Find("sensor");
                if (sensorTransform)
                    sensorTransform.GetComponent<Sensor>().SetTemperature(GetTemperatureInfos().mean.ToString());
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
                    sensor.SetTemperature(GetTemperatureInfos().mean.ToString());
                }
                sensorTransform = transform.Find(_sensorName);
                if (sensorTransform)
                    sensorTransform.GetComponent<Sensor>().SetTemperature(_value);
                else
                    GameManager.instance.AppendLogLine($"Sensor {_sensorName} does not exist", true, ELogtype.warning);
            }
            else
                GameManager.instance.AppendLogLine("Temperature must be a numerical value", true, ELogtype.warning);
        }
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
