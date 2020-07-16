using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateDataCenter : MonoBehaviour
{
    public void GenerateDC(SDataCenterInfos _data)
    {
        GameObject newDC = new GameObject(_data.name);
        Datacenter dc = newDC.AddComponent<Datacenter>();
        dc.address = _data.address;
        dc.zipcode = _data.zipcode;
        dc.city = _data.city;
        dc.country = _data.country;
        dc.description = _data.description;
    }
}
