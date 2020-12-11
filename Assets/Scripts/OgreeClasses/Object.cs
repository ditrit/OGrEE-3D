using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class Object : AServerItem, IAttributeModif, ISerializationCallbackReceiver
{
    public string description;
    public EObjFamily family;

    public Vector2 posXY;
    public EUnit posXYUnit;
    public float posZ;
    public EUnit posZUnit;
    public Vector2 size;
    public EUnit sizeUnit;
    public float height;
    public EUnit heightUnit;
    public EObjOrient orientation;

    public Tenant tenant;
    public string vendor;
    public string type;
    public string model;
    public string serial;

    public Dictionary<string, string> extras = new Dictionary<string, string>();
    [SerializeField] private List<string> extraKeys = new List<string>();
    [SerializeField] private List<string> extraValues = new List<string>();


    public void OnBeforeSerialize()
    {
        extraKeys.Clear();
        extraValues.Clear();
        foreach (var kvp in extras)
        {
            extraKeys.Add(kvp.Key);
            extraValues.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        extras = new Dictionary<string, string>();
        for (int i = 0; i != Mathf.Min(extraKeys.Count, extraValues.Count); i++)
            extras.Add(extraKeys[i], extraValues[i]);
    }

    // protected virtual void OnDestroy()
    // {
    //     if (GetComponent<HierarchyName>())
    //         GameManager.gm.allItems.Remove(GetComponent<HierarchyName>().fullname);
    // }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public virtual void SetAttribute(string _param, string _value)
    {
        switch (_param)
        {
            case "description":
                description = _value;
                break;
            case "vendor":
                vendor = _value;
                break;
            case "type":
                type = _value;
                break;
            case "model":
                model = _value;
                break;
            case "serial":
                serial = _value;
                break;
            case "tenant":
                AssignTenant(_value);
                break;
            case "color":
                SetColor(_value);
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
            default:
                GameManager.gm.AppendLogLine($"[Object] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

    ///<summary>
    /// If Tenant exists, assign it to the object. If object is a Rack, call Rack.UpdateColor().
    ///</summary>
    ///<param name="_tenantName">The name of the tenant</param>
    protected void AssignTenant(string _tenantName)
    {
        if (GameManager.gm.tenants.ContainsKey(_tenantName))
        {
            tenant = GameManager.gm.tenants[_tenantName];
            if (family == EObjFamily.rack)
                GetComponent<Rack>().UpdateColor();
        }
        else
            GameManager.gm.AppendLogLine($"Tenant \"{_tenantName}\" doesn't exists. Please create it before assign it.", "yellow");
    }

    ///<summary>
    /// Update object's alpha according to _input, from 0 to 100.
    ///</summary>
    ///<param name="_input">Alpha wanted for the rack</param>
    protected void UpdateAlpha(string _input)
    {
        string regex = "^[0-9]+$";
        if (Regex.IsMatch(_input, regex))
        {
            float a = float.Parse(_input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            a = Mathf.Clamp(a, 0, 100);
            if (a == 0)
                transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            else
            {
                transform.GetChild(0).GetComponent<Renderer>().enabled = true;
                Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, a / 100);
            }
        }
        else
            GameManager.gm.AppendLogLine("Please use a value between 0 and 100", "yellow");
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    protected void SetColor(string _hex)
    {
        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString($"#{_hex}", out myColor);
        mat.color = new Color(myColor.r, myColor.g, myColor.b, mat.color.a);
    }

    ///<summary>
    /// Display or hide all unused slots of the object.
    ///</summary>
    ///<param name="_value">True or false value</param>
    protected void ToggleSlots(string _value)
    {
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
}
