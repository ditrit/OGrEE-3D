using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : OObject
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
        // else if (_param == "lod")
        // {
        //     int i = 0;
        //     int.TryParse(_value, out i);
        //     SetLod(i);
        // }
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
                    UpdateColor();
                    break;
                case "color":
                    SetColor(_value);
                    break;
                case "alpha":
                    UpdateAlpha(_value);
                    break;
                case "racks":
                    ToggleContent(_value);
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
    private void ToggleContent(string _value)
    {
        if (_value != "true" && _value != "false")
            return;

        if (_value == "true")
        {
            UpdateAlpha("true");
            DisplayContent(true);
        }
        else
        {
            UpdateAlpha("false");
            DisplayContent(false);
        }
    }

    ///<summary>
    /// Enable or disable racks from attributes["rackList"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayContent(bool _value)
    {
        List<GameObject> content = new List<GameObject>();
        string[] names = attributes["content"].Split(',');

        foreach (string rn in names)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{transform.parent.GetComponent<OgreeObject>().hierarchyName}.{rn}");
            if (go)
                content.Add(go);
        }
        foreach (GameObject r in content)
            r.gameObject.SetActive(_value);

        GetComponent<DisplayObjectData>().ToggleLabel(!_value);
    }
}
