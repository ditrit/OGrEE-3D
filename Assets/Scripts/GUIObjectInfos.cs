using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIObjectInfos : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpBtnName = null;
    [Header("Single object")]
    [SerializeField] private GameObject singlePanel = null;
    [SerializeField] private TMP_Text tmpName = null;
    [SerializeField] private TMP_Text tmpDomainName = null;
    [SerializeField] private TMP_Text tmpDomainAttrs = null;
    [SerializeField] private TMP_Text tmpAttributes = null;
    [SerializeField] private Scrollbar verticalScrollbar = null;

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
                tmpName.text = _obj.name;
            else
                tmpName.text = "";
            tmpDomainName.text = "";
            tmpDomainAttrs.text = "";
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
            objList.text += $"{obj.GetComponent<OgreeObject>().id}\n";

        // Set correct height for scroll view
        RectTransform rt = objList.transform.parent.GetComponent<RectTransform>();
        rt.sizeDelta = new(0, _objects.Count * 20);
    }

    ///<summary>
    /// Update singlePanel texts from an OgreeObject.
    ///</summary>
    ///<param name="_obj">The object whose information are displayed</param>
    private void UpdateFields(OgreeObject _obj)
    {
        int textHeight = 1;
        tmpName.text = _obj.id.Replace(".", "/");
        if (!string.IsNullOrEmpty(_obj.domain) && GameManager.instance.allItems.Contains(_obj.domain))
        {
            OgreeObject domain = ((GameObject)GameManager.instance.allItems[_obj.domain]).GetComponent<OgreeObject>();
            tmpDomainName.text = domain.name;
            tmpDomainAttrs.text = (domain.name == domain.id) ? "" : $"({domain.id})\n";
            foreach (KeyValuePair<string, string> kvp in domain.attributes)
                tmpDomainAttrs.text += $"<b>{kvp.Key}:</b> {kvp.Value}\n";
        }
        else
        {
            tmpDomainName.text = "-";
            tmpDomainAttrs.text = "";
        }
        // Display category
        tmpAttributes.text = $"<b><u>{_obj.category}</u></b>\n";

        if (GameManager.instance.tempColorMode && _obj is Item item)
        {
            STemp tempInfos = item.GetTemperatureInfos();
            tmpAttributes.text += $"<b>average:</b> {tempInfos.mean:0.##} {tempInfos.unit}\n";
            tmpAttributes.text += $"<b>standard deviation:</b> {tempInfos.std:0.##} {tempInfos.unit}\n";
            tmpAttributes.text += $"<b>minimum:</b> {tempInfos.min:0.##} {tempInfos.unit}\n";
            tmpAttributes.text += $"<b>maximum:</b> {tempInfos.max:0.##} {tempInfos.unit}\n";
            textHeight += 4;
            if (!string.IsNullOrEmpty(tempInfos.hottestChild))
            {
                tmpAttributes.text += $"<b>hottest child:</b> {tempInfos.hottestChild}\n";
                textHeight++;
            }
        }
        else
        {
            // Display description with multiple lines
            if (!string.IsNullOrEmpty(_obj.description))
            {
                tmpAttributes.text += "<b>description:</b>\n";
                tmpAttributes.text += $"{_obj.description}\n";
                textHeight += _obj.description.Count(c => c == '\n') + 1;
            }

            // Display all other attributes
            foreach (KeyValuePair<string, string> kvp in _obj.attributes)
            {
                tmpAttributes.text += $"<b>{kvp.Key}:</b> {kvp.Value}\n";
                textHeight++;
            }

            // Display tags using their color
            if (_obj.tags.Count > 0)
            {
                tmpAttributes.text += "<b>tags: </b>";
                textHeight++;
                foreach (string tagName in _obj.tags)
                    tmpAttributes.text += $"<b><color=#{GameManager.instance.GetTag(tagName).colorCode}>{tagName}</b></color> ";
            }
        }
        // Set correct height for scroll view
        RectTransform rt = tmpAttributes.transform.parent.GetComponent<RectTransform>();
        rt.sizeDelta = new(0, textHeight * 30);
        verticalScrollbar.value = 1;
    }

    ///<summary>
    /// Return the asked value if it exists in the dictionary
    ///</summary>
    ///<param name="_dictionary">The dictionary to search in</param>
    ///<param name="_key">The key to search</param>
    ///<returns>The asked value if the key exists in the dictionary, the default value of the Dictionary's values' type otherwise</returns>
    private T IfInDictionary<T>(Dictionary<string, T> _dictionary, string _key)
    {
        if (_dictionary.ContainsKey(_key))
            return _dictionary[_key];
        else
            return default;
    }
}
