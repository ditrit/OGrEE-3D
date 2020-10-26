using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZoomManager : MonoBehaviour
{
    [System.Serializable]
    public struct SObjectCmd
    {
        public string hierarchyName;
        public string parentName;
        public string command;

        public SObjectCmd(string _hierarchyName, string _parentName, string _cmd)
        {
            hierarchyName = _hierarchyName;
            parentName = _parentName;
            command = _cmd;
        }
    }

    public static ZoomManager instance;

    [Header("Data")]
    public int zoomLevel = 0;
    public List<SObjectCmd> devices = new List<SObjectCmd>();
    public List<SObjectCmd> devicesAttributes = new List<SObjectCmd>();

    [Header("References")]
    [SerializeField] private TextMeshProUGUI uiText = null;
    [SerializeField] private Slider slider = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        SetZoom(slider.value);
    }

    ///
    public bool IsListed(string _path)
    {
        foreach (SObjectCmd obj in devices)
        {
            if (obj.hierarchyName == _path)
                return true;
        }
        return false;
    }

    ///<summary>
    /// Change zoom value.
    ///</summary>
    ///<param name="_value"></param>
    public void SetZoom(float _value)
    {
        zoomLevel = Mathf.Clamp(zoomLevel, 0, 3);

        slider.value = _value;
        uiText.text = $"Zoom level = {_value}";
        zoomLevel = (int)_value;
        PopObjects();
    }

    ///<summary>
    /// .
    ///</summary>
    public void PopObjects()
    {
        // For debug purpose, should be a parameter
        // string target = GameObject.FindObjectOfType<Customer>()?.name;

        if (GameManager.gm.currentItems.Count == 0)
            return;
        string target = GameManager.gm.currentItems[0].GetComponent<HierarchyName>().GetHierarchyName();

        foreach (SObjectCmd obj in devices)
        {
            if (GameManager.gm.FindByAbsPath(obj.hierarchyName) == null
                && obj.parentName.Contains(target))
                GameManager.gm.consoleController.CreateDevice(obj.command);
        }
    }
}
