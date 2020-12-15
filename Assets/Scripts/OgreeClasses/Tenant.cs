using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tenant : AServerItem, IAttributeModif
{
    public string color;
    public string mainContact;
    public string mainPhone;
    public string mainEmail;

    private void OnDestroy()
    {

        Filters.instance.tenantsList.Remove($"<color=#{color}>{name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
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
                GameManager.gm.AppendLogLine($"[Tenant] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }
}
