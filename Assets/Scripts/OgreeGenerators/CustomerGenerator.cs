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
    public Domain CreateDomain(SApiObject _do)
    {
        if (GameManager.instance.allItems.Contains(_do.id))
        {
            GameManager.instance.AppendLogLine($"{_do.id} already exists.", ELogTarget.both, ELogtype.error);
            return null;
        }

        GameObject newDomain = new GameObject(_do.name);
        Domain domain = newDomain.AddComponent<Domain>();
        domain.UpdateFromSApiObject(_do);

        GameManager.instance.allItems.Add(_do.id, newDomain);
        return domain;
    }

    ///<summary>
    /// Create an OgreeObject of "site" category and assign given values to it
    ///</summary>
    ///<param name="_si">The site data to apply</param>
    ///<param name="_parent">The parent of the created site</param>
    ///<returns>The created Site</returns>
    public OgreeObject CreateSite(SApiObject _si)
    {
        if (GameManager.instance.allItems.Contains(_si.id))
        {
            GameManager.instance.AppendLogLine($"{_si.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newSite = new GameObject(_si.name);

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.UpdateFromSApiObject(_si);

        GameManager.instance.allItems.Add(site.id, newSite);
        return site;
    }
}
