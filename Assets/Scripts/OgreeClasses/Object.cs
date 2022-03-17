using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class OObject : OgreeObject
{
    public Color color;
    public bool isHidden = false;

    private void Awake()
    {
        EventManager.Instance.AddListener<UpdateTenantEvent>(UpdateColorByTenant);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.Instance.RemoveListener<UpdateTenantEvent>(UpdateColorByTenant);
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        bool updateAttr = false;
        if (_param.StartsWith("description"))
        {
            SetDescription(_param.Substring(11), _value);
            updateAttr = true;
        }
        else
        {
            switch (_param)
            {
                case "label":
                    GetComponent<DisplayObjectData>().SetLabel(_value);
                    break;
                case "labelFont":
                    GetComponent<DisplayObjectData>().SetLabelFont(_value);
                    break;
                case "domain":
                    if (_value.EndsWith("@recursive"))
                    {
                        string[] data = _value.Split('@');
                        SetAllDomains(data[0]);
                    }
                    else
                    {
                        SetDomain(_value);
                        UpdateColorByTenant();
                    }
                    updateAttr = true;
                    break;
                case "color":
                    SetColor(_value);
                    updateAttr = true;
                    break;
                case "alpha":
                    UpdateAlpha(_value);
                    break;
                case "slots":
                    ToggleSlots(_value);
                    break;
                case "localCS":
                    ToggleCS(_value);
                    break;
                case "temperature":
                    SetTemperature(_value);
                    updateAttr = true;
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    updateAttr = true;
                    break;
            }
        }
        if (updateAttr && ApiManager.instance.isInit)
            PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Update the OObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    ///<param name="_copyAttr">True by default: allows to update attributes dictionary</param>
    public override void UpdateFromSApiObject(SApiObject _src, bool _copyAttr = true)
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
        if (_copyAttr)
        {
            if (attributes.ContainsKey("temperature") && _src.attributes.ContainsKey("temperature")
                && attributes["temperature"] != _src.attributes["temperature"])
                SetTemperature(_src.attributes["temperature"]);
            else if (!attributes.ContainsKey("temperature") && _src.attributes.ContainsKey("temperature"))
                SetTemperature(_src.attributes["temperature"]);
            else if (attributes.ContainsKey("temperature") && !_src.attributes.ContainsKey("temperature"))
                Destroy(transform.Find("sensor").gameObject);

            attributes = _src.attributes;
        }
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
            GameManager.gm.AppendLogLine("alpha value has to be true or false", "yellow");
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
    protected void SetColor(string _hex)
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
            GameManager.gm.AppendLogLine("Unknown color", "yellow");
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

        OgreeObject tenant = ((GameObject)GameManager.gm.allItems[domain]).GetComponent<OgreeObject>();

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        color = new Color();
        ColorUtility.TryParseHtmlString($"#{tenant.attributes["color"]}", out color);

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
            GameManager.gm.AppendLogLine("slots value has to be true or false", "yellow");
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
            Destroy(localCS);
            GameManager.gm.AppendLogLine($"Hide local Coordinate System for {name}", "yellow");
        }
        else
            localCS = PopLocalCS(csName);
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
            GameManager.gm.AppendLogLine("slots value has to be true or false", "yellow");
            return;
        }

        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS && _value == "false")
        {
            Destroy(localCS);
            GameManager.gm.AppendLogLine($"Hide local Coordinate System for {name}", "yellow");
        }
        else if (!localCS && _value == "true")
            localCS = PopLocalCS(csName);
    }

    ///<summary>
    /// Create a local Coordinate System for this object.
    ///</summary>
    ///<param name="_name">The name of the local CS</param>
    private GameObject PopLocalCS(string _name)
    {
        GameObject localCS = Instantiate(GameManager.gm.coordinateSystemModel);
        localCS.name = _name;
        localCS.transform.parent = transform;
        localCS.transform.localScale = Vector3.one;
        localCS.transform.localEulerAngles = Vector3.zero;
        localCS.transform.localPosition = transform.GetChild(0).localScale / -2f;
        GameManager.gm.AppendLogLine($"Display local Coordinate System for {name}", "yellow");
        return localCS;
    }

    ///<summary>
    /// Set temperature attribute and create/update related sensor object.
    ///</summary>
    ///<param name="_value">The temperature value</param>
    protected void SetTemperature(string _value)
    {
        if (Regex.IsMatch(_value, "^[0-9.]+$"))
        {
            attributes["temperature"] = _value;
            GameObject sensor = GameManager.gm.FindByAbsPath($"{hierarchyName}.sensor");
            if (sensor)
                sensor.GetComponent<Sensor>().SetAttribute("temperature", _value);
            else
            {
                SApiObject se = new SApiObject();
                se.description = new List<string>();
                se.attributes = new Dictionary<string, string>();

                se.name = "sensor"; // ?
                se.category = "sensor";
                se.attributes["formFactor"] = "ext";
                se.attributes["temperature"] = _value;
                se.parentId = id;
                se.domain = domain;

                ObjectGenerator.instance.CreateSensor(se, transform);
            }
        }
        else
            GameManager.gm.AppendLogLine("Temperature must be a numeral value", "yellow");
    }

}
