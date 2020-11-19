using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Datacenter : MonoBehaviour, IAttributeModif
{
    public string comment;
    public string address;
    public string zipcode;
    public string city;
    public string country;
    public EOrientation orientation;
    public string gpsX;
    public string gpsY;
    public string gpsZ;
    public Tenant tenant;
    public int id;

    public Color usableColor = new Color(0.86f, 0.93f, 0.95f);
    public Color reservedColor = new Color(0.95f, 0.95f, 0.95f);
    public Color technicalColor = new Color(0.92f, 0.95f, 0.87f);

    private void OnDestroy()
    {
        GameManager.gm.allItems.Remove(GetComponent<HierarchyName>().fullname);
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetAttribute(string _param, string _value)
    {
        switch (_param)
        {
            case "comment":
                comment = _value;
                break;
            case "address":
                address = _value;
                break;
            case "zipcode":
                zipcode = _value;
                break;
            case "city":
                city = _value;
                break;
            case "country":
                country = _value;
                break;
            case "gps":
                _value = _value.Trim('[', ']');
                string[] coords = _value.Split(',');
                gpsX = coords[0];
                gpsY = coords[1];
                gpsZ = coords[2];
                break;
            case "tenant":
                if (GameManager.gm.tenants.ContainsKey(_value))
                    tenant = GameManager.gm.tenants[_value];
                else
                    GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exist. Please create it before assign it.", "yellow");
                break;
            case "usableColor":
                SetColor(_value, out usableColor);
                break;
            case "reservedColor":
                SetColor(_value, out reservedColor);
                break;
            case "technicalColor":
                SetColor(_value, out technicalColor);
                break;
            default:
                GameManager.gm.AppendLogLine($"[Datacenter] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_color">The color to set</param>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    private void SetColor(string _hex, out Color _color)
    {
        ColorUtility.TryParseHtmlString($"#{_hex}", out _color);
        Room[] rooms = transform.GetComponentsInChildren<Room>();
        foreach (Room r in rooms)
            r.UpdateZonesColor();
    }

}
