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

    public void UpdateField(string _param, string _value)
    {
        switch (_param)
        {
            case "comment":
                description = _value;
                break;
            case "vendor":
                vendor = _value;
                break;
            case "model":
                model = _value;
                break;
            case "serial":
                serial = _value;
                break;
            default:
                Debug.LogWarning($"{name}: unknowed field to update.");
                break;
        }

        DisplayRackData drd = GetComponent<DisplayRackData>();
        if (drd)
            drd.FillTexts();
    }
}
