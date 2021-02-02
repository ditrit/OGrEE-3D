using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RackGroup : Object
{
    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        switch (_param)
        {
            case "description":
                description = _value;
                break;
            case "domain":
                if (GameManager.gm.allItems.ContainsKey(_value))
                    domain = _value;
                else
                    GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exist. Please create it before assign it.", "yellow");
                break;
            case "color":
                SetColor(_value);
                break;
            case "alpha":
                UpdateAlpha(_value);
                break;
            case "racks":
                ToggleRacks(_value);
                break;
            default:
                if (attributes.ContainsKey(_param))
                    attributes[_param] = _value;
                else
                    attributes.Add(_param, _value);
                break;
        }
        // PutData();
    }

    ///<summary>
    /// Display or hide the rackGroup and its racks.
    ///</summary>
    ///<param name="_value">"true" or "false" value</param>
    private void ToggleRacks(string _value)
    {
        if (_value != "true" && _value != "false")
            return;

        if (_value == "true")
        {
            UpdateAlpha("0");
            DisplayRacks(true);
        }
        else
        {
            UpdateAlpha("100");
            DisplayRacks(false);
        }
    }

    ///<summary>
    /// Enable or disable racks from attributes["rackList"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayRacks(bool _value)
    {
        List<Rack> racks = new List<Rack>();
        string[] rackNames = attributes["racksList"].Split(',');
        foreach (string rn in rackNames)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{transform.parent.GetComponent<HierarchyName>().fullname}.{rn}");
            if (go)
                racks.Add(go.GetComponent<Rack>());
        }
        foreach (Rack r in racks)
            r.gameObject.SetActive(_value);
    }
}
