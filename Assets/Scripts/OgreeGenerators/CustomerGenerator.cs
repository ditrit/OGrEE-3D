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
        tenant.UpdateFromSApiObject(_tn);
        tenant.hierarchyName = _tn.name;

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
        if (GameManager.instance.allItems.Contains(_si.hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{_si.hierarchyName} already exists.", true, ELogtype.warning);
            return null;
        }

        GameObject newSite = new GameObject(_si.name);
        newSite.transform.parent = _parent;

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.UpdateFromSApiObject(_si);

        GameManager.instance.allItems.Add(site.hierarchyName, newSite);
        return site;
    }
}
