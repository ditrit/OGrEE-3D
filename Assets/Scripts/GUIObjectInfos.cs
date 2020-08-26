using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIObjectInfos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpName = null;
    [SerializeField] private TextMeshProUGUI tmpTenantName = null;
    [SerializeField] private TextMeshProUGUI tmpTenantContact = null;
    [SerializeField] private TextMeshProUGUI tmpTenantPhone = null;
    [SerializeField] private TextMeshProUGUI tmpTenantEmail = null;
    [SerializeField] private TextMeshProUGUI tmpVendor = null;
    [SerializeField] private TextMeshProUGUI tmpType = null;
    [SerializeField] private TextMeshProUGUI tmpModel = null;
    [SerializeField] private TextMeshProUGUI tmpSerial = null;
    [SerializeField] private TextMeshProUGUI tmpDesc = null;

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
