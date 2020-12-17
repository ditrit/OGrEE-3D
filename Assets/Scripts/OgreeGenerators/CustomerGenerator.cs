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
    /// Create an OgreeObject of tenant category with given name and color.
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

        GameObject tenant = new GameObject(_name);
        OgreeObject tn = tenant.AddComponent<OgreeObject>();
        tn.name = tenant.name;
        tn.category = "tenant";
        tn.attributes.Add("color", _color);

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{_color}>{_name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        tenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_name, tenant);

        ApiManager.instance.CreatePostRequest(tn.name);

        return tn;
    }

    ///<summary>
    /// Create OgreeObject of tenant category from Json.
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

        GameObject tenant = new GameObject(_tn.name);
        OgreeObject tn = tenant.AddComponent<OgreeObject>();
        tn.name = _tn.name;
        tn.id = _tn.id;
        tn.category = _tn.category;
        tn.description = _tn.description;
        tn.attributes = _tn.attributes;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{tn.attributes["color"]}>{tn.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        tenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_tn.name, tenant);

        return tn;
    }

    ///<summary>
    /// Create a Site and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the site</param>
    ///<returns>The created Site</returns>
    public Site CreateSite(SSiteInfos _data)
    {
        if (_data.parent.GetComponent<Tenant>() == null)
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

        Site si = newSite.AddComponent<Site>();
        si.name = newSite.name;
        switch (_data.orient)
        {
            case "EN":
                si.orientation = ECardinalOrient.EN;
                newSite.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                si.orientation = ECardinalOrient.WS;
                newSite.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                si.orientation = ECardinalOrient.NW;
                newSite.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                si.orientation = ECardinalOrient.SE;
                newSite.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }

        si.parentId = _data.parent.GetComponent<Tenant>().id;

        // By default, tenant is the hierarchy's root
        si.tenant = si.transform.parent.GetComponent<Tenant>();

        string hn = newSite.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newSite);

        ApiManager.instance.CreatePostRequest(hn);

        return si;
    }

    ///<summary>
    /// Create a Site and assign values from json
    ///</summary>
    ///<param name="_si">The site data to apply</param>
    ///<returns>The created Site</returns>
    public Site CreateSite(SSiteFromJson _si)
    {
        Tenant[] tenants = GameObject.FindObjectsOfType<Tenant>();
        Tenant tn = null;
        foreach (Tenant tenant in tenants)
        {
            if (tenant.id == _si.parentId)
                tn = tenant;
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

        Site si = newSite.AddComponent<Site>();
        si.name = newSite.name;
        switch (_si.orient)
        {
            case "EN":
                si.orientation = ECardinalOrient.EN;
                newSite.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                si.orientation = ECardinalOrient.WS;
                newSite.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                si.orientation = ECardinalOrient.NW;
                newSite.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                si.orientation = ECardinalOrient.SE;
                newSite.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }
        si.description = _si.comment;
        si.address = _si.address;
        si.zipcode = _si.zipcode;
        si.city = _si.zipcode;
        si.country = _si.country;
        si.gps = _si.gps;
        si.id = _si.id;
        si.SetAttribute("usableColor", _si.usableColor);
        si.SetAttribute("reservedColor", _si.reservedColor);
        si.SetAttribute("technicalColor", _si.technicalColor);

        si.parentId = _si.parentId;

        // By default, tenant is the hierarchy's root
        si.tenant = si.transform.parent.GetComponent<Tenant>();

        string hn = newSite.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newSite);

        return si;
    }
}
