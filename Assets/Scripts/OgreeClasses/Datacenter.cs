using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Datacenter : MonoBehaviour
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
            default:
                GameManager.gm.AppendLogLine($"[Datacenter] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

}
