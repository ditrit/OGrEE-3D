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
    /// Create an OgreeObject of "tenant" category with given name and color.
    ///</summary>
    ///<param name="_name">The tenant's name</param>
    ///<param name="_color">The tenant's rendering color</param>
    ///<returns>The created Tenant</returns>
    public OgreeObject CreateTenant(string _name, string _color)
    {
        if (GameManager.gm.allItems.Contains(_name))
        {
            GameManager.gm.AppendLogLine($"{_name} already exists.", "yellow");
            return null;
        }

        GameObject newTenant = new GameObject(_name);
        OgreeObject tenant = newTenant.AddComponent<OgreeObject>();
        tenant.name = newTenant.name;
        tenant.category = "tenant";
        tenant.domain = tenant.name;
        tenant.attributes["color"] = _color;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{_color}>{_name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        newTenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_name, newTenant);

        ApiManager.instance.CreatePostRequest(tenant.name);

        return tenant;
    }

    ///<summary>
    /// Create OgreeObject of "tenant" category from Json.
    ///</summary>
    ///<param name="_tn">The tenant data to apply</param>
    ///<returns>The created Tenant</returns>
    public OgreeObject CreateTenant(SApiObject _tn)
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
        tenant.category = _tn.category;
        tenant.description = _tn.description;
        tenant.domain = _tn.domain;
        tenant.attributes = _tn.attributes;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{tenant.attributes["color"]}>{tenant.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        newTenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_tn.name, newTenant);

        return tenant;
    }

    ///<summary>
    /// Create an OgreeObject of "site" category and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the site</param>
    ///<returns>The created Site</returns>
    public OgreeObject CreateSite(SSiteInfos _data)
    {
        if (_data.parent.GetComponent<OgreeObject>().category != "tenant")
        {
            GameManager.gm.AppendLogLine("Site must be child of a tenant", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newSite = new GameObject(_data.name);
        newSite.transform.parent = _data.parent;

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.name = newSite.name;
        site.parentId = _data.parent.GetComponent<OgreeObject>().id;
        site.category = "site";
        site.domain = _data.parent.GetComponent<OgreeObject>().domain;

        site.attributes["orientation"] = _data.orient;
        switch (_data.orient)
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
        // Set default colors
        site.attributes["usableColor"] = "DBEDF2";
        site.attributes["reservedColor"] = "F2F2F2";
        site.attributes["technicalColor"] = "EBF2DE";

        string hn = newSite.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newSite);

        ApiManager.instance.CreatePostRequest(hn);

        return site;
    }

    ///<summary>
    /// Create an OgreeObject of "site" category and assign values from json
    ///</summary>
    ///<param name="_si">The site data to apply</param>
    ///<returns>The created Site</returns>
    public OgreeObject CreateSite(SApiObject _si)
    {
        GameObject tn = null;
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (go.GetComponent<OgreeObject>().id == _si.parentId)
                tn = go;
        }
        if (!tn)
        {
            GameManager.gm.AppendLogLine($"Parent tenant not found (id = {_si.parentId})", "red");
            return null;
        }

        string hierarchyName = $"{tn.GetComponent<HierarchyName>()?.fullname}.{_si.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newSite = new GameObject(_si.name);
        newSite.transform.parent = tn.transform;

        OgreeObject site = newSite.AddComponent<OgreeObject>();
        site.name = newSite.name;
        site.id = _si.id;
        site.parentId = _si.parentId;
        site.category = _si.category;
        site.description = _si.description;
        site.domain = site.transform.parent.GetComponent<OgreeObject>().domain;

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

        return site;
    }
}
