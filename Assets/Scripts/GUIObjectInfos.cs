using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIObjectInfos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpName;
    [SerializeField] private TextMeshProUGUI tmpTenantName;
    [SerializeField] private TextMeshProUGUI tmpTenantContact;
    [SerializeField] private TextMeshProUGUI tmpTenantPhone;
    [SerializeField] private TextMeshProUGUI tmpTenantEmail;
    [SerializeField] private TextMeshProUGUI tmpVendor;
    [SerializeField] private TextMeshProUGUI tmpType;
    [SerializeField] private TextMeshProUGUI tmpModel;
    [SerializeField] private TextMeshProUGUI tmpSerial;
    [SerializeField] private TextMeshProUGUI tmpDesc;

    public void UpdateFields(GameObject _obj)
    {
        if (_obj && _obj.GetComponent<Rack>())
            UpdateFields(_obj.GetComponent<Rack>());
        else if (_obj && _obj.GetComponent<Room>())
            UpdateFields(_obj.GetComponent<Room>());
        else
        {
            if (_obj)
                tmpName.text = _obj.name;
            else
                tmpName.text = "-";
            tmpTenantName.text = "-";
            tmpTenantContact.text = "-";
            tmpTenantPhone.text = "-";
            tmpTenantEmail.text = "-";
            tmpVendor.text = "-";
            tmpType.text = "-";
            tmpModel.text = "-";
            tmpSerial.text = "-";
            tmpDesc.text = "-";
        }
    }

    public void UpdateFields(Rack _rack)
    {
        tmpName.text = _rack.name;
        tmpTenantName.text = _rack.tenant.name;
        tmpTenantContact.text = _rack.tenant.mainContact;
        tmpTenantPhone.text = _rack.tenant.mainPhone;
        tmpTenantEmail.text = _rack.tenant.mainEmail;
        tmpVendor.text = _rack.vendor;
        tmpType.text = _rack.type;
        tmpModel.text = _rack.model;
        tmpSerial.text = _rack.serial;
        tmpDesc.text = _rack.description;
    }

    public void UpdateFields(Room _room)
    {
        tmpName.text = _room.name;
        tmpTenantName.text = _room.tenant.name;
        tmpTenantContact.text = _room.tenant.mainContact;
        tmpTenantPhone.text = _room.tenant.mainPhone;
        tmpTenantEmail.text = _room.tenant.mainEmail;
        tmpVendor.text = "-";
        tmpType.text = "-";
        tmpModel.text = "-";
        tmpSerial.text = "-";
        tmpDesc.text = _room.description;
    }
}
