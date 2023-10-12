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

        GameObject newDomain = new(_do.name);
        Domain domain = newDomain.AddComponent<Domain>();
        _do.tags = new(); // Temporary fix
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
    public Site CreateSite(SApiObject _si)
    {
        if (GameManager.instance.allItems.Contains(_si.id))
        {
            GameManager.instance.AppendLogLine($"{_si.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        GameObject newSite = new(_si.name);

        Site site = newSite.AddComponent<Site>();
        site.UpdateFromSApiObject(_si);

        GameManager.instance.allItems.Add(site.id, newSite);
        return site;
    }
}
