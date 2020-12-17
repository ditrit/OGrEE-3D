using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Site : AServerItem, IAttributeModif
{
    public string description;
    public string address;
    public string zipcode;
    public string city;
    public string country;
    public ECardinalOrient orientation;
    public Vector3 gps;
    public string domain;

    public string usableColor = "DBEDF2";
    public string reservedColor = "F2F2F2";
    public string technicalColor = "EBF2DE";

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
            case "description":
                description = _value;
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
                gps = Utils.ParseVector3(_value);
                break;
            case "domain":
                if (GameManager.gm.allItems.ContainsKey(_value))
                    domain = _value;
                else
                    GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exist. Please create it before assign it.", "yellow");
                break;
            case "usableColor":
                usableColor = _value;
                break;
            case "reservedColor":
                reservedColor = _value;
                break;
            case "technicalColor":
                technicalColor = _value;
                break;
            default:
                GameManager.gm.AppendLogLine($"[Datacenter] {name}: unknowed attribute to update.", "yellow");
                break;
        }
        PutData();
    }
}
