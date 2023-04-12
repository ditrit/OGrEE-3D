using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerGenerator
{
    ///<summary>
    /// Create OgreeObject of "domain" category from given data.
    ///</summary>
    ///<param name="_do">The domain data to apply</param>
    ///<returns>The created domain</returns>
    public OgreeObject CreateDomain(SApiObject _do)
    {
        if (GameManager.instance.allItems.Contains(_do.name))
        {
            GameManager.instance.AppendLogLine($"{_do.name} already exists.", ELogTarget.both, ELogtype.error);
            return null;
        }

        GameObject newDomain = new GameObject(_do.name);
        OgreeObject domain = newDomain.AddComponent<OgreeObject>();
        domain.UpdateFromSApiObject(_do);
        domain.hierarchyName = _do.name;

        GameManager.instance.allItems.Add(_do.name, newDomain);
        return domain;
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
            GameManager.instance.AppendLogLine($"{_si.hierarchyName} already exists.", ELogTarget.both, ELogtype.warning);
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
