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
        if (!string.IsNullOrEmpty(_obj.domain))
        {
            OgreeObject tn = ((GameObject)GameManager.gm.allItems[_obj.domain]).GetComponent<OgreeObject>();
            tmpTenantName.text = tn.name;
            if (tn.attributes.ContainsKey("mainContact"))
                tmpTenantContact.text = tn.attributes["mainContact"];
            if (tn.attributes.ContainsKey("mainPhone"))
                tmpTenantPhone.text = tn.attributes["mainPhone"];
            if (tn.attributes.ContainsKey("mainEmail"))
                tmpTenantEmail.text = tn.attributes["mainEmail"];
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
        if (!string.IsNullOrEmpty(_room.domain))
        {
            OgreeObject tn = ((GameObject)GameManager.gm.allItems[_room.domain]).GetComponent<OgreeObject>();
            tmpTenantName.text = tn.name;
            if (tn.attributes.ContainsKey("mainContact"))
                tmpTenantContact.text = tn.attributes["mainContact"];
            if (tn.attributes.ContainsKey("mainPhone"))
                tmpTenantPhone.text = tn.attributes["mainPhone"];
            if (tn.attributes.ContainsKey("mainEmail"))
                tmpTenantEmail.text = tn.attributes["mainEmail"];
        }
        tmpPosXY.text = "-";
        Vector2 size = JsonUtility.FromJson<Vector2>(_room.attributes["size"]);
        tmpSize.text = $"{size.x}{_room.attributes["sizeUnit"]} x {size.y}{_room.attributes["sizeUnit"]}";
        tmpVendor.text = "-";
        tmpType.text = "-";
        tmpModel.text = "-";
        tmpSerial.text = "-";
        tmpDesc.text = _room.description;
    }
}
