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
        GameObject newDomain = new(_do.name);
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
    public Site CreateSite(SApiObject _si)
    {
        GameObject newSite = new(_si.name);

        Site site = newSite.AddComponent<Site>();
        site.UpdateFromSApiObject(_si);

        GameManager.instance.allItems.Add(site.id, newSite);
        return site;
    }
}
