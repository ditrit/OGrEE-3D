using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
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
            default:
                GameManager.gm.AppendLogLine($"[Object] {name}: unknowed attribute to update.", "yellow");
                break;
        }

        DisplayRackData drd = GetComponent<DisplayRackData>();
        if (drd)
            drd.FillTexts();
    }

    ///<summary>
    /// If Tenant exists, assign it to the object. If object is a Rack, call Rack.UpdateColor().
    ///</summary>
    ///<param name="_tenantName">The name of the tenant</param>
    private void AssignTenant(string _tenantName)
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
}
