using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : AServerItem
{
    public string contact;

    private void OnDestroy()
    {
        if (GameManager.gm.tenants.ContainsKey(name))
        {
            GameManager.gm.tenants.Remove(name);
            Filters.instance.tenantsList.Remove($"<color=#ffffff>{name}</color>");
            Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        }
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetAttribute(string _param, string _value)
    {
        if (_param == "contact")
            contact = _value;
        else
            GameManager.gm.AppendLogLine($"[Customer] {name}: unknowed attribute to update.", "yellow");
    }
}
