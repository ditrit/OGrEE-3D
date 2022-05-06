﻿using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class GUIObjectInfos : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpBtnName = null;
    [Header("Single object")]
    [SerializeField] private GameObject singlePanel = null;
    [SerializeField] private TMP_Text tmpName = null;
    [SerializeField] private TMP_Text tmpTenantName = null;
    [SerializeField] private TMP_Text tmpTenantContact = null;
    [SerializeField] private TMP_Text tmpTenantPhone = null;
    [SerializeField] private TMP_Text tmpTenantEmail = null;
    [SerializeField] private TMP_Text tmpAttributes = null;

    [Header("Multi objects")]
    [SerializeField] private GameObject multiPanel = null;
    [SerializeField] private TMP_Text objList = null;

    private void Start()
    {
        UpdateSingleFields(null);
    }

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

        if (_obj && _obj.GetComponent<OgreeObject>())
            UpdateFields(_obj.GetComponent<OgreeObject>());
        else
        {
            if (_obj)
            {
                tmpName.text = _obj.name;
            }
            else
            {
                tmpName.text = "";
            }
            tmpTenantName.text = "";
            tmpTenantContact.text = "";
            tmpTenantPhone.text = "";
            tmpTenantEmail.text = "";
            tmpAttributes.text = "";
        }
    }

    ///<summary>
    /// Update Texts in multiPanel.
    ///</summary>
    ///<param name="_objects">The objects whose name are displayed</param>
    public void UpdateMultiFields(List<GameObject> _objects)
    {
        tmpBtnName.text = "Selection";
        singlePanel.SetActive(false);
        multiPanel.SetActive(true);

        objList.text = "";
        foreach (GameObject obj in _objects)
            objList.text += $"{obj.GetComponent<OgreeObject>().hierarchyName}\n";
        // Set correct height for scroll view
        RectTransform rt = objList.transform.parent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, _objects.Count * 20);
    }

    ///<summary>
    /// Update singlePanel texts from an OgreeObject.
    ///</summary>
    ///<param name="_obj">The object whose information are displayed</param>
    private void UpdateFields(OgreeObject _obj)
    {
        int i = 1;
        tmpName.text = _obj.hierarchyName;
        if (!string.IsNullOrEmpty(_obj.domain))
        {
            OgreeObject domain = ((GameObject)GameManager.gm.allItems[_obj.domain]).GetComponent<OgreeObject>();
            tmpTenantName.text = domain.name;
            tmpTenantContact.text = IfInDictionary(domain.attributes, "mainContact");
            tmpTenantPhone.text = IfInDictionary(domain.attributes, "mainPhone");
            tmpTenantEmail.text = IfInDictionary(domain.attributes, "mainEmail");
#if VR
#endif
        }
        // Display category
        tmpAttributes.text = $"<b><u>{_obj.category}</u></b>\n";

        // Display posXY if available
        if (_obj.attributes.ContainsKey("posXY") && _obj.attributes.ContainsKey("posXYUnit")
            && !string.IsNullOrEmpty(_obj.attributes["posXY"]) && !string.IsNullOrEmpty(_obj.attributes["posXYUnit"]))
        {
            Vector2 posXY = JsonUtility.FromJson<Vector2>(_obj.attributes["posXY"]);
            tmpAttributes.text += $"<b>posXY:</b> {posXY.x.ToString("0.##")}/{posXY.y.ToString("0.##")} ({_obj.attributes["posXYUnit"]})\n";
            i++;

            // If rack, display pos by tile name if available
            if (_obj.category == "rack")
            {
                Room room = _obj.transform.parent.GetComponent<Room>();
                if (room.attributes.ContainsKey("tiles"))
                {
                    List<ReadFromJson.STile> tiles = JsonConvert.DeserializeObject<List<ReadFromJson.STile>>(room.attributes["tiles"]);
                    ReadFromJson.STile tileData = new ReadFromJson.STile();
                    foreach (ReadFromJson.STile t in tiles)
                    {
                        if (t.location == $"{posXY.x.ToString("0")}/{posXY.y.ToString("0")}")
                            tileData = t;
                    }
                    if (!string.IsNullOrEmpty(tileData.location) && !string.IsNullOrEmpty(tileData.label))
                    {
                        tmpAttributes.text += $"<b>tile's label:</b> {tileData.label}\n";
                        i++;
                    }
                }
            }
        }

        // Display orientation if available
        if (_obj.attributes.ContainsKey("orientation"))
        {
            tmpAttributes.text += $"<b>orientation:</b> {_obj.attributes["orientation"]}\n";
            i++;
        }

        // Display size if available
        if (_obj.attributes.ContainsKey("size") && _obj.attributes.ContainsKey("sizeUnit"))
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(_obj.attributes["size"]);
            tmpAttributes.text += $"<b>size:</b> {size.x}{_obj.attributes["sizeUnit"]} x {size.y}{_obj.attributes["sizeUnit"]} x {_obj.attributes["height"]}{_obj.attributes["heightUnit"]}\n";
            i++;
        }

        // Display template if available
        if (_obj.attributes.ContainsKey("template") && !string.IsNullOrEmpty(_obj.attributes["template"]))
        {
            tmpAttributes.text += $"<b>template:</b> {_obj.attributes["template"]}\n";
            i++;
        }

        // Display all other attributes
        foreach (KeyValuePair<string, string> kvp in _obj.attributes)
        {
            if (!string.IsNullOrEmpty(kvp.Value) && (kvp.Key != "posXY" && kvp.Key != "posXYUnit" && kvp.Key != "orientation"
                && kvp.Key != "size" && kvp.Key != "sizeUnit" && kvp.Key != "template"))
            {
                tmpAttributes.text += $"<b>{kvp.Key}:</b> {kvp.Value}\n";
                i++;
            }
        }

        // Display all descriptions
        if (_obj.description.Count != 0)
        {
            tmpAttributes.text += "<b>description:</b>\n";
            for (int j = 0; j < _obj.description.Count; j++)
            {
                tmpAttributes.text += $"<b>{j + 1}:</b> {_obj.description[j]}\n";
                i++;
            }
        }

        // Set correct height for scroll view
#if !VR
        RectTransform rt = tmpAttributes.transform.parent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, i * 30);
#endif
        //RectTransform rtVR = tmpAttributesVR.transform.parent.GetComponent<RectTransform>();
        //rtVR.sizeDelta = new Vector2(0, i * 30);
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