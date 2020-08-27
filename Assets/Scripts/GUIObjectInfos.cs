using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIObjectInfos : MonoBehaviour
{
    [Header("Single object")]
    [SerializeField] private GameObject singlePanel = null;
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
    
    [Header("Multi objects")]
    [SerializeField] private GameObject multiPanel = null;
    [SerializeField] private TextMeshProUGUI objList = null;

    ///<summary>
    /// Update Texts in singlePanel.
    ///</summary>
    ///<param name="_obj">The object whose information are displayed</param>
    public void UpdateSingleFields(GameObject _obj)
    {
        singlePanel.SetActive(true);
        multiPanel.SetActive(false);

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

    ///<summary>
    /// Update Texts in multiPanel.
    ///</summary>
    ///<param name="The objects whose name are displayed"></param>
    public void UpdateMultiFields(List<GameObject> _objects)
    {
        singlePanel.SetActive(false);
        multiPanel.SetActive(true);

        objList.text = "";
        foreach(GameObject obj in _objects)
            objList.text += $"{obj.GetComponent<HierarchyName>().fullname}\n";
    }

    ///<summary>
    /// Update singlePanel texts from a Rack.
    ///</summary>
    ///<param name="_rack">The rack whose information are displayed</param>
    private void UpdateFields(Rack _rack)
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

    ///<summary>
    /// Update singlePanel texts from a Room.
    ///</summary>
    ///<param name="_room">The room whose information are displayed</param>
    private void UpdateFields(Room _room)
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
