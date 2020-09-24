using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class Object : MonoBehaviour, IAttributeModif
{
    public string description;
    public EObjFamily family;

    public Vector2 posXY;
    public EUnit posXYUnit;
    public float posZ;
    public EUnit posZUnit;
    public Vector2 size;
    public EUnit sizeUnit;
    public int height;
    public EUnit heightUnit;
    public EObjOrient orient;

    public Tenant tenant;
    public string vendor;
    public string type;
    public string model;
    public string serial;

    public Dictionary<string, string> extras;

    protected virtual void OnDestroy()
    {
        GameManager.gm.allItems.Remove(GetComponent<HierarchyName>().fullname);
    }

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
            Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, a / 100);
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
}
