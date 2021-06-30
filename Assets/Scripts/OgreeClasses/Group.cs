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
        // bool updateAttr = false;
        if (_param.StartsWith("description"))
        {
            SetDescription(_param.Substring(11), _value);
            // updateAttr = true;
        }
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
                    // updateAttr = true;
                    break;
                case "color":
                    SetColor(_value);
                    break;
                case "alpha":
                    UpdateAlpha(_value);
                    break;
                case "content":
                    ToggleContent(_value);
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    // updateAttr = true;
                    break;
            }
        }
        // if (updateAttr && ApiManager.instance.isInit)
        //     PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Display or hide the rackGroup and its content.
    ///</summary>
    ///<param name="_value">"true" or "false" value</param>
    public void ToggleContent(string _value)
    {
        if (_value != "true" && _value != "false")
            return;

        if (_value == "true")
        {
            UpdateAlpha("true");
            DisplayContent(true);
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        else
        {
            UpdateAlpha("false");
            DisplayContent(false);
            transform.GetChild(0).GetComponent<Collider>().enabled = true;
        }
    }

    ///<summary>
    /// Enable or disable racks from attributes["rackList"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayContent(bool _value)
    {
        foreach (GameObject r in GetContent())
            r.gameObject.SetActive(_value);

        GetComponent<DisplayObjectData>().ToggleLabel(!_value);
    }

    ///<summary>
    /// Get all GameObjects listed in attributes["content"].
    ///</summary>
    ///<returns>The list of GameObject corresponding to attributes["content"]</returns>
    public List<GameObject> GetContent()
    {
        List<GameObject> content = new List<GameObject>();
        string[] names = attributes["content"].Split(',');

        foreach (string rn in names)
        {
            GameObject go = GameManager.gm.FindByAbsPath($"{transform.parent.GetComponent<OgreeObject>().hierarchyName}.{rn}");
            if (go)
                content.Add(go);
        }
        return content;
    }
}
