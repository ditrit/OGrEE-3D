using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rack : Object
{
    public Rack()
    {
        family = EObjFamily.rack;
    }

    private void OnDestroy()
    {
        Filters.instance.rackRowsList.Remove(name[0].ToString());
        Filters.instance.racks.Remove(gameObject);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRackRows, Filters.instance.rackRowsList);
    }

    ///<summary>
    /// Update rack's color according to its Tenant.
    ///</summary>
    public void UpdateColor()
    {
        if (tenant == null)
            return;

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString(tenant.color, out myColor);
        mat.color = myColor;
        // Debug.Log($"{tenant.color} => {myColor}");
    }
}
