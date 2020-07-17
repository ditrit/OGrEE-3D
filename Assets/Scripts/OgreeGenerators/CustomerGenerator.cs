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

    public void CreateCustomer(string _name)
    {
        GameObject customer = new GameObject(_name);
        customer.AddComponent<Customer>();
    }

    public void CreateDatacenter(SDataCenterInfos _data)
    {
        GameObject newDC = new GameObject(_data.name);

        GameObject parent = GameObject.Find(_data.customer);
        if (parent)
            newDC.transform.parent = parent.transform;
        // else
        //     Debug.LogError("");

        Datacenter dc = newDC.AddComponent<Datacenter>();
        dc.address = _data.address;
        dc.zipcode = _data.zipcode;
        dc.city = _data.city;
        dc.country = _data.country;
        dc.description = _data.description;
    }
}
