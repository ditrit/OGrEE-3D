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
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    public void CreateCustomer(string _name, bool _changeHierarchy)
    {
        if (GameManager.gm.allItems.Contains(_name))
        {
            GameManager.gm.AppendLogLine($"{_name} already exists.", "yellow");
            return;
        }

        GameObject customer = new GameObject(_name);
        customer.AddComponent<Customer>();

        // Create default tenant
        CreateTenant(_name, "ffffff");

        customer.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(_name, customer);

        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(customer);
    }

    ///<summary>
    /// Create a Datacenter and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the datacenter</param>
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    public void CreateDatacenter(SDataCenterInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Customer>() == null)
        {
            GameManager.gm.AppendLogLine("Datacenter must be child of a customer", "yellow");
            return;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return;
        }

        GameObject newDC = new GameObject(_data.name);
        newDC.transform.parent = _data.parent;

        Datacenter dc = newDC.AddComponent<Datacenter>();
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

        // By default, tenant is customer's one
        dc.tenant = GameManager.gm.tenants[_data.parent.name];

        newDC.AddComponent<HierarchyName>();

        GameManager.gm.allItems.Add(hierarchyName, newDC);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newDC);
    }

    ///<summary>
    /// Create a Tenant and apply _data to it and store it in GameManager.tenants.
    ///</summary>
    ///<param name="_name">Name of the tenant</param>
    ///<param name="_color">Color of the tenant in hexadecimal format (xxxxxx)</param>
    public void CreateTenant(string _name, string _color)
    {
        Tenant newTenant = new Tenant(_name, $"#{_color}");
        GameManager.gm.DictionaryAddIfUnknown(GameManager.gm.tenants, _name, newTenant);
        Filters.instance.AddIfUnknown(Filters.instance.tenantsList, $"<color={newTenant.color}>{newTenant.name}</color>");
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownTenants, Filters.instance.tenantsList);
    }
}
