using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rack : Object
{
    public Rack()
    {
        family = EObjFamily.rack;
    }

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
