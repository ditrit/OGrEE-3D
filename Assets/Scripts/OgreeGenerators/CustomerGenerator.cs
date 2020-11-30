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
    /// Create a Customer with given name. Also create a default Tenant corresponding to the Customer.
    ///</summary>
    ///<param name="_name">The customer's name</param>
    ///<returns>The created Customer</returns>
    public Customer CreateCustomer(string _name)
    {
        if (GameManager.gm.allItems.Contains(_name))
        {
            GameManager.gm.AppendLogLine($"{_name} already exists.", "yellow");
            return null;
        }

        GameObject customer = new GameObject(_name);
        Customer cu = customer.AddComponent<Customer>();
        cu.name = customer.name;

        // Create default tenant
        CreateTenant(_name, "ffffff");

        customer.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_name, customer);
        
        ApiManager.instance.CreatePostRequest(cu.name);

        return cu;
    }

    ///<summary>
    /// Create Customer and associated Tenant from Json.
    ///</summary>
    ///<param name="_cu">The customer data to apply</param>
    ///<returns>The created Customer</returns>
    public Customer CreateCustomer(SCuFromJson _cu)
    {
        if (GameManager.gm.allItems.Contains(_cu.name))
        {
            GameManager.gm.AppendLogLine($"{_cu.name} already exists.", "yellow");
            return null;
        }

        GameObject customer = new GameObject(_cu.name);
        Customer cu = customer.AddComponent<Customer>();
        cu.name = _cu.name;
        cu.contact = _cu.mainContact;
        cu.id = _cu.id;

        // Create default tenant
        Tenant tn = CreateTenant(_cu.name, "ffffff");
        tn.mainContact = _cu.mainContact;
        tn.mainPhone = _cu.mainPhone;
        tn.mainEmail = _cu.mainEmail;
        tn.id = _cu.id;

        customer.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_cu.name, customer);
        
        return cu;
    }

    ///<summary>
    /// Create a Datacenter and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the datacenter</param>
    ///<returns>The created Datacenter</returns>
    public Datacenter CreateDatacenter(SDataCenterInfos _data)
    {
        if (_data.parent.GetComponent<Customer>() == null)
        {
            GameManager.gm.AppendLogLine("Datacenter must be child of a customer", "yellow");
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
                dc.orientation = EOrientation.N;
                newDC.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                dc.orientation = EOrientation.S;
                newDC.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                dc.orientation = EOrientation.W;
                newDC.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                dc.orientation = EOrientation.E;
                newDC.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
        }

        dc.parentId = _data.parent.GetComponent<Customer>().id;

        // By default, tenant is customer's one
        dc.tenant = GameManager.gm.tenants[_data.parent.name];

        newDC.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(hierarchyName, newDC);

        ApiManager.instance.CreatePostRequest(dc.name);

        return dc;
    }

    ///<summary>
    /// Create a Datacenter and assign values from json
    ///</summary>
    ///<param name="_dc">The datacebter data to apply</param>
    ///<returns>The created Datacenter</returns>
    public Datacenter CreateDatacenter(SDcFromJson _dc)
    {
        Customer[] customers = GameObject.FindObjectsOfType<Customer>();
        Customer cu = null;
        foreach (Customer customer in customers)
        {
            if (customer.id == _dc.parentId)
                cu = customer;
        }
        if (!cu)
        {
            GameManager.gm.AppendLogLine($"Parent customer not found (id = {_dc.parentId})", "red");
            return null;
        }

        string hierarchyName = $"{cu.GetComponent<HierarchyName>()?.fullname}.{_dc.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newDC = new GameObject(_dc.name);
        newDC.transform.parent = cu.transform;

        Datacenter dc = newDC.AddComponent<Datacenter>();
        dc.name = newDC.name;
        switch (_dc.orient)
        {
            case "EN":
                dc.orientation = EOrientation.N;
                newDC.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                dc.orientation = EOrientation.S;
                newDC.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                dc.orientation = EOrientation.W;
                newDC.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                dc.orientation = EOrientation.E;
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

        // By default, tenant is customer's one
        dc.tenant = GameManager.gm.tenants[cu.name];

        string hn = newDC.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newDC);

        return dc;
    }

    ///<summary>
    /// Create a Tenant and apply _data to it and store it in GameManager.tenants.
    ///</summary>
    ///<param name="_name">Name of the tenant</param>
    ///<param name="_color">Color of the tenant in hexadecimal format (xxxxxx)</param>
    public Tenant CreateTenant(string _name, string _color)
    {
        Tenant newTenant = new Tenant(_name, $"#{_color}");
        Utils.DictionaryAddIfUnknown(GameManager.gm.tenants, _name, newTenant);
        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color={newTenant.color}>{newTenant.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
        
        return newTenant;
    }
}
