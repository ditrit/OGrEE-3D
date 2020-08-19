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
        GameObject customer = new GameObject(_name);
        customer.AddComponent<Customer>();

        // Create default tenant
        CreateTenant(_name, "ffffff");

        customer.AddComponent<HierarchyName>();
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

        GameObject newDC = new GameObject(_data.name);
        newDC.transform.parent = _data.parent;

        Datacenter dc = newDC.AddComponent<Datacenter>();
        // dc.address = _data.address;
        // dc.zipcode = _data.zipcode;
        // dc.city = _data.city;
        // dc.country = _data.country;
        // dc.description = _data.description;

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

        newDC.AddComponent<HierarchyName>();
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
        GameManager.gm.DictionaryAddIfUnknowned(GameManager.gm.tenants, _name, newTenant);
    }
}
