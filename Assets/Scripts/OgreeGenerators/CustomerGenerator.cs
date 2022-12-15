using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerGenerator
{
    ///<summary>
    /// Create OgreeObject of "tenant" category from given data.
    ///</summary>
    ///<param name="_tn">The tenant data to apply</param>
    ///<returns>The created Tenant</returns>
    public OgreeObject CreateTenant(SApiObject _tn)
    {
        if (GameManager.instance.allItems.Contains(_tn.name))
        {
            GameManager.instance.AppendLogLine($"{_tn.name} already exists.", true, ELogtype.error);
            return null;
        }

        GameObject newTenant = new GameObject(_tn.name);
        OgreeObject tenant = newTenant.AddComponent<OgreeObject>();
        tenant.hierarchyName = _tn.name;
        tenant.UpdateFromSApiObject(_tn);

        GameManager.instance.allItems.Add(_tn.name, newTenant);
        return tenant;
    }

    ///<summary>
    /// Create an OgreeObject of "site" category and assign given values to it
    ///</summary>
    ///<param name="_si">The site data to apply</param>
    ///<param name="_parent">The parent of the created site</param>
    ///<returns>The created Site</returns>
    public OgreeObject CreateSite(SApiObject _si, Transform _parent)
    {
        string hierarchyName;
        if (_parent)
            hierarchyName = $"{_parent.GetComponent<OgreeObject>().hierarchyName}.{_si.name}";
        else
            hierarchyName = _si.name;
        if (GameManager.instance.allItems.Contains(hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{hierarchyName} already exists.", true, ELogtype.warning);
            return null;
        }

        GameObject newSite = new GameObject(_si.name);
        newSite.transform.parent = _parent;

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.hierarchyName = hierarchyName;
        site.UpdateFromSApiObject(_si);

        switch (site.attributes["orientation"])
        {
            case "EN":
                newSite.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                newSite.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                newSite.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                newSite.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }

        GameManager.instance.allItems.Add(hierarchyName, newSite);
        return site;
    }
}
