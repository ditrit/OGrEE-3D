using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIObjectInfos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpBtnName = null;
    [Header("Single object")]
    [SerializeField] private GameObject singlePanel = null;
    [SerializeField] private TextMeshProUGUI tmpName = null;
    [SerializeField] private TextMeshProUGUI tmpTenantName = null;
    [SerializeField] private TextMeshProUGUI tmpTenantContact = null;
    [SerializeField] private TextMeshProUGUI tmpTenantPhone = null;
    [SerializeField] private TextMeshProUGUI tmpTenantEmail = null;
    [SerializeField] private TextMeshProUGUI tmpPosXY = null;
    [SerializeField] private TextMeshProUGUI tmpSize = null;
    [SerializeField] private TextMeshProUGUI tmpVendor = null;
    [SerializeField] private TextMeshProUGUI tmpType = null;
    [SerializeField] private TextMeshProUGUI tmpModel = null;
    [SerializeField] private TextMeshProUGUI tmpSerial = null;
    [SerializeField] private TextMeshProUGUI tmpDesc = null;

    [Header("Multi objects")]
    [SerializeField] private GameObject multiPanel = null;
    [SerializeField] private TextMeshProUGUI objList = null;

    ///<summary>
    /// Update Texts in singlePanel.
    ///</summary>
    ///<param name="_obj">The object whose information are displayed</param>
    public void UpdateSingleFields(GameObject _obj)
    {
        if (_obj)
            tmpBtnName.text = _obj.name;
        else
            tmpBtnName.text = "Infos";
        singlePanel.SetActive(true);
        multiPanel.SetActive(false);

        if (_obj && _obj.GetComponent<Object>())
            UpdateFields(_obj.GetComponent<Object>());
        else if (_obj && _obj.GetComponent<Room>())
            UpdateFields(_obj.GetComponent<Room>());
        else
        {
            if (_obj)
                tmpName.text = _obj.name;
            else
                tmpName.text = "Name";
            tmpTenantName.text = "Tenant Name";
            tmpTenantContact.text = "Tenant Contact";
            tmpTenantPhone.text = "Tenant Phone";
            tmpTenantEmail.text = "Tenant Email";
            tmpPosXY.text = "PosXY";
            tmpSize.text = "Size";
            tmpVendor.text = "Vendor";
            tmpType.text = "Type";
            tmpModel.text = "Model";
            tmpSerial.text = "Serial";
            tmpDesc.text = "Description";
        }
    }

    ///<summary>
    /// Update Texts in multiPanel.
    ///</summary>
    ///<param name="The objects whose name are displayed"></param>
    public void UpdateMultiFields(List<GameObject> _objects)
    {
        tmpBtnName.text = "Selection";
        singlePanel.SetActive(false);
        multiPanel.SetActive(true);

        objList.text = "";
        foreach (GameObject obj in _objects)
            objList.text += $"{obj.GetComponent<HierarchyName>().fullname}\n";
    }

    ///<summary>
    /// Update singlePanel texts from a Rack.
    ///</summary>
    ///<param name="_obj">The rack whose information are displayed</param>
    private void UpdateFields(Object _obj)
    {
        tmpName.text = _obj.GetComponent<HierarchyName>().fullname;
        if (_obj.tenant)
        {
            tmpTenantName.text = _obj.tenant.name;
            tmpTenantContact.text = _obj.tenant.mainContact;
            tmpTenantPhone.text = _obj.tenant.mainPhone;
            tmpTenantEmail.text = _obj.tenant.mainEmail;
        }
        if (_obj.family == EObjFamily.rack)
            tmpPosXY.text = $"Tile {_obj.posXY.x.ToString("0.##")}/{_obj.posXY.y.ToString("0.##")}";
        else
            tmpPosXY.text = "-";
        tmpSize.text = $"{_obj.size.x}{_obj.sizeUnit} x {_obj.size.y}{_obj.sizeUnit} x {_obj.height}{_obj.heightUnit}";
        tmpVendor.text = _obj.vendor;
        tmpType.text = _obj.type;
        tmpModel.text = _obj.model;
        tmpSerial.text = _obj.serial;
        tmpDesc.text = _obj.description;
    }

    ///<summary>
    /// Update singlePanel texts from a Room.
    ///</summary>
    ///<param name="_room">The room whose information are displayed</param>
    private void UpdateFields(Room _room)
    {
        tmpName.text = _room.GetComponent<HierarchyName>().fullname;
        tmpTenantName.text = _room.tenant.name;
        tmpTenantContact.text = _room.tenant.mainContact;
        tmpTenantPhone.text = _room.tenant.mainPhone;
        tmpTenantEmail.text = _room.tenant.mainEmail;
        tmpPosXY.text = "-";
        tmpSize.text = $"{_room.size.x}{_room.sizeUnit} x {_room.size.y}{_room.sizeUnit}";
        tmpVendor.text = "-";
        tmpType.text = "-";
        tmpModel.text = "-";
        tmpSerial.text = "-";
        tmpDesc.text = _room.description;
    }
}
