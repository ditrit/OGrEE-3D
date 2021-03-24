using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerGenerator : MonoBehaviour
{
    public static CustomerGenerator instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Create OgreeObject of "tenant" category from given data.
    ///</summary>
    ///<param name="_tn">The tenant data to apply</param>
    ///<returns>The created Tenant</returns>
    public OgreeObject CreateTenant(SApiObject _tn, bool _serverPost = true)
    {
        if (GameManager.gm.allItems.Contains(_tn.name))
        {
            GameManager.gm.AppendLogLine($"{_tn.name} already exists.", "yellow");
            return null;
        }

        GameObject newTenant = new GameObject(_tn.name);
        OgreeObject tenant = newTenant.AddComponent<OgreeObject>();
        tenant.name = _tn.name;
        tenant.id = _tn.id;
        tenant.category = "tenant";
        tenant.description = _tn.description;
        tenant.domain = _tn.domain;
        tenant.attributes = _tn.attributes;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{tenant.attributes["color"]}>{tenant.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        newTenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_tn.name, newTenant);

        if (_serverPost)
            ApiManager.instance.CreatePostRequest(tenant);

        return tenant;
    }

    ///<summary>
    /// Create an OgreeObject of "site" category and assign given values to it
    ///</summary>
    ///<param name="_si">The site data to apply</param>
    ///<param name="_parent">The parent of the created site. Leave null if _bd contains the parendId</param>
    ///<returns>The created Site</returns>
    public OgreeObject CreateSite(SApiObject _si, Transform _parent = null, bool _serverPost = true)
    {
        Transform tn = Utils.FindParent(_parent, _si.parentId);
        if (!tn || tn.GetComponent<OgreeObject>().category != "tenant")
        {
            GameManager.gm.AppendLogLine($"Parent tenant not found", "red");
            return null;
        }

        string hierarchyName = $"{tn.GetComponent<HierarchyName>()?.fullname}.{_si.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newSite = new GameObject(_si.name);
        newSite.transform.parent = tn;

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.name = newSite.name;
        site.id = _si.id;
        site.parentId = _si.parentId;
        if (string.IsNullOrEmpty(site.parentId))
            site.parentId = tn.GetComponent<OgreeObject>().id;
        site.category = "site";
        site.description = _si.description;
        site.domain = _si.domain;
        if (string.IsNullOrEmpty(site.domain))
            site.domain = tn.GetComponent<OgreeObject>().domain;
        site.attributes = _si.attributes;

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

        string hn = newSite.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newSite);

        if (_serverPost)
            ApiManager.instance.CreatePostRequest(site);

        return site;
    }
}
