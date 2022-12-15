﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rack : OObject
{
    public Transform uRoot;
    public GameObject gridForULocation;

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        bool updateAttr = false;
        if (_param.StartsWith("description"))
        {
            SetDescription(_param.Substring(11), _value);
            updateAttr = true;
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
                case "labelBackground":
                    GetComponent<DisplayObjectData>().SetBackgroundColor(_value);
                    break;
                case "domain":
                    if (_value.EndsWith("@recursive"))
                    {
                        string[] data = _value.Split('@');
                        SetAllDomains(data[0]);
                    }
                    else
                    {
                        SetDomain(_value);
                        UpdateColorByTenant();
                    }
                    updateAttr = true;
                    break;
                case "color":
                    SetColor(_value);
                    updateAttr = true;
                    break;
                case "alpha":
                    UpdateAlpha(_value);
                    break;
                case "slots":
                    ToggleSlots(_value);
                    break;
                case "localCS":
                    ToggleCS(_value);
                    break;
                case "U":
                    if (_value == "true")
                    {
                        UHelpersManager.um.ToggleU(transform, true);
                        GameManager.instance.AppendLogLine($"U helpers ON for {name}.", false, ELogtype.info);
                    }
                    else if (_value == "false")
                    {
                        UHelpersManager.um.ToggleU(transform, false);
                        GameManager.instance.AppendLogLine($"U helpers OFF for {name}.", false, ELogtype.info);
                    }
                    break;
                default:
                    if (_param.StartsWith("temperature_"))
                    {
                        SetTemperature(_value, _param.Substring(12));
                    }
                    else
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    updateAttr = true;
                    break;
            }
        }
        if (updateAttr && ApiManager.instance.isInit)
            PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }
}
