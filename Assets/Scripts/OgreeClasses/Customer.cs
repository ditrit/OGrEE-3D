﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : AServerItem, IAttributeModif
{
    public string color;
    public string mainContact;
    public string mainPhone;
    public string mainEmail;

    private void OnDestroy()
    {
        // if (GameManager.gm.tenants.ContainsKey(name))
        // {
        //     GameManager.gm.tenants.Remove(name);
            Filters.instance.tenantsList.Remove($"<color=#{color}>{name}</color>");
            Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        // }
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
            case "mainContact":
                mainContact = _value;
                break;
            case "mainPhone":
                mainPhone = _value;
                break;
            case "mainEmail":
                mainEmail = _value;
                break;
            default:
                GameManager.gm.AppendLogLine($"[Customer] {name}: unknowed attribute to update.", "yellow");
                break;        
        }
    }
}
