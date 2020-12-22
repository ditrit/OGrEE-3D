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
            tmpTenantContact.text = IfInDictionary(tn.attributes, "mainContact");
            tmpTenantPhone.text = IfInDictionary(tn.attributes, "mainPhone");
            tmpTenantEmail.text = IfInDictionary(tn.attributes, "mainEmail");
        }
        if (_obj.category == "rack")
        {
            Vector2 posXY = JsonUtility.FromJson<Vector2>(_obj.attributes["posXY"]);
            tmpPosXY.text = $"Tile {posXY.x.ToString("0.##")}/{posXY.y.ToString("0.##")}";
        }
        else
            tmpPosXY.text = "-";
        if (_obj.attributes.ContainsKey("size") && _obj.attributes.ContainsKey("sizeUnit"))
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(_obj.attributes["size"]);
            tmpSize.text = $"{size.x}{_obj.attributes["sizeUnit"]} x {size.y}{_obj.attributes["sizeUnit"]} x {_obj.attributes["height"]}{_obj.attributes["heightUnit"]}";
        }
        tmpVendor.text = IfInDictionary(_obj.attributes, "vendor");
        tmpType.text = IfInDictionary(_obj.attributes, "type");
        tmpModel.text = IfInDictionary(_obj.attributes, "model");
        tmpSerial.text = IfInDictionary(_obj.attributes, "serial");
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
            tmpTenantContact.text = IfInDictionary(tn.attributes, "mainContact");
            tmpTenantPhone.text = IfInDictionary(tn.attributes, "mainPhone");
            tmpTenantEmail.text = IfInDictionary(tn.attributes, "mainEmail");
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

    ///<summary>
    /// Return the asked value if it exists in the dictionary.
    ///</summary>
    ///<param name="_dictionary">The dictionary to search in</param>
    ///<param name="_key">The ke to search</param>
    ///<returns>The asked value</returns>
    private T IfInDictionary<T>(Dictionary<string, T> _dictionary, string _key)
    {
        if (_dictionary.ContainsKey(_key))
            return _dictionary[_key];
        else
            return default(T);
    }
}
