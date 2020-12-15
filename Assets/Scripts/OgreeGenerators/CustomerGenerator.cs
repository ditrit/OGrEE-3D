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
    /// Create a Tenant with given name.
    ///</summary>
    ///<param name="_name">The tenant's name</param>
    ///<param name="_color">The tenant's rendering color</param>
    ///<returns>The created Tenant</returns>
    public Tenant CreateTenant(string _name, string _color)
    {
        if (GameManager.gm.allItems.Contains(_name))
        {
            GameManager.gm.AppendLogLine($"{_name} already exists.", "yellow");
            return null;
        }

        GameObject tenant = new GameObject(_name);
        Tenant tn = tenant.AddComponent<Tenant>();
        tn.name = tenant.name;
        tn.color = _color;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{_color}>{_name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        
        tenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_name, tenant);
        
        ApiManager.instance.CreatePostRequest(tn.name);

        return tn;
    }

    ///<summary>
    /// Create Tenant and associated Tenant from Json.
    ///</summary>
    ///<param name="_tn">The tenant data to apply</param>
    ///<returns>The created Tenant</returns>
    public Tenant CreateTenant(STnFromJson _tn)
    {
        if (GameManager.gm.allItems.Contains(_tn.name))
        {
            GameManager.gm.AppendLogLine($"{_tn.name} already exists.", "yellow");
            return null;
        }

        GameObject tenant = new GameObject(_tn.name);
        Tenant tn = tenant.AddComponent<Tenant>();
        tn.name = _tn.name;
        tn.id = _tn.id;
        tn.color = _tn.color;
        tn.mainContact = _tn.mainContact;
        tn.mainPhone = _tn.mainPhone;
        tn.mainEmail = _tn.mainEmail;

        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color=#{tn.color}>{tn.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);

        tenant.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_tn.name, tenant);
        
        return tn;
    }

    ///<summary>
    /// Create a Datacenter and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the datacenter</param>
    ///<returns>The created Datacenter</returns>
    public Datacenter CreateDatacenter(SDataCenterInfos _data)
    {
        if (_data.parent.GetComponent<Tenant>() == null)
        {
            GameManager.gm.AppendLogLine("Datacenter must be child of a tenant", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newDC = new GameObject(_data.name);
        newDC.transform.parent = _data.parent;

        Datacenter dc = newDC.AddComponent<Datacenter>();
        dc.name = newDC.name;
        switch (_data.orient)
        {
            case "EN":
                dc.orientation = ECardinalOrient.EN;
                newDC.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                dc.orientation = ECardinalOrient.WS;
                newDC.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                dc.orientation = ECardinalOrient.NW;
                newDC.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                dc.orientation = ECardinalOrient.SE;
                newDC.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }

        dc.parentId = _data.parent.GetComponent<Tenant>().id;

        // By default, tenant is the hierarchy's root
        dc.tenant = dc.transform.parent.GetComponent<Tenant>();

        string hn = newDC.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newDC);

        ApiManager.instance.CreatePostRequest(hn);

        return dc;
    }

    ///<summary>
    /// Create a Datacenter and assign values from json
    ///</summary>
    ///<param name="_dc">The datacebter data to apply</param>
    ///<returns>The created Datacenter</returns>
    public Datacenter CreateDatacenter(SDcFromJson _dc)
    {
        Tenant[] tenants = GameObject.FindObjectsOfType<Tenant>();
        Tenant tn = null;
        foreach (Tenant tenant in tenants)
        {
            if (tenant.id == _dc.parentId)
                tn = tenant;
        }
        if (!tn)
        {
            GameManager.gm.AppendLogLine($"Parent tenant not found (id = {_dc.parentId})", "red");
            return null;
        }

        string hierarchyName = $"{tn.GetComponent<HierarchyName>()?.fullname}.{_dc.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newDC = new GameObject(_dc.name);
        newDC.transform.parent = tn.transform;

        Datacenter dc = newDC.AddComponent<Datacenter>();
        dc.name = newDC.name;
        switch (_dc.orient)
        {
            case "EN":
                dc.orientation = ECardinalOrient.EN;
                newDC.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                dc.orientation = ECardinalOrient.WS;
                newDC.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                dc.orientation = ECardinalOrient.NW;
                newDC.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                dc.orientation = ECardinalOrient.SE;
                newDC.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }
        dc.comment = _dc.comment;
        dc.address = _dc.address;
        dc.zipcode = _dc.zipcode;
        dc.city = _dc.zipcode;
        dc.country = _dc.country;
        dc.gps = _dc.gps;
        dc.id = _dc.id;
        dc.SetAttribute("usableColor", _dc.usableColor);
        dc.SetAttribute("reservedColor", _dc.reservedColor);
        dc.SetAttribute("technicalColor", _dc.technicalColor);

        dc.parentId = _dc.parentId;

        // By default, tenant is the hierarchy's root
        dc.tenant = dc.transform.parent.GetComponent<Tenant>();

        string hn = newDC.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newDC);

        return dc;
    }
}
