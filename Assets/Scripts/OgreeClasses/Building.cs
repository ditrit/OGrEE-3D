using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : AServerItem, IAttributeModif
{
    public string description;
    public string nbFloors;

    public Vector2 posXY;
    public EUnit posXYUnit;
    public float posZ;
    public EUnit posZUnit;

    public Vector2 size; // width;depth
    public EUnit sizeUnit;
    public float height;
    public EUnit heightUnit;

    [Header("BD References")]
    public Transform walls;

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
            case "nbFloors":
                nbFloors = _value;
                break;
            default:
                GameManager.gm.AppendLogLine($"[Building] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

}
