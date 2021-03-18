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
        if (_param.StartsWith("description"))
            SetDescription(_param.Substring(11), _value);
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
                    SetDomain(_value);
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
        }
        // PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
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
            UpdateAlpha("true");
            DisplayRacks(true);
        }
        else
        {
            UpdateAlpha("false");
            DisplayRacks(false);
        }
    }

    ///<summary>
    /// Enable or disable racks from attributes["rackList"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayRacks(bool _value)
    {
        List<GameObject> racks = new List<GameObject>();
        string[] rackNames = attributes["racksList"].Split(',');
        foreach (string rn in rackNames)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{transform.parent.GetComponent<HierarchyName>().fullname}.{rn}");
            if (go)
                racks.Add(go);
        }
        foreach (GameObject r in racks)
            r.gameObject.SetActive(_value);

        GetComponent<DisplayObjectData>().ToggleLabel(!_value);
    }
}
