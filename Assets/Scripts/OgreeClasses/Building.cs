﻿using TMPro;
using UnityEngine;

public class Building : OgreeObject
{
    public bool isSquare = true;

    [Header("BD References")]
    public Transform walls;
    public TextMeshPro nameText;

    private void Awake()
    {
        if (!(this is Room))
            EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinihsed);
        EventManager.instance.AddListener<UpdateTenantEvent>(UpdateColorByTenant);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!(this is Room))
            EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinihsed);
        EventManager.instance.RemoveListener<UpdateTenantEvent>(UpdateColorByTenant);
    }

    /// <summary>
    /// Toggle the roof and the name depending on if the building have children
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void OnImportFinihsed(ImportFinishedEvent _e)
    {
        Transform roof = transform.Find("Roof");
        if (!GetComponentInChildren<Room>())
        {
            nameText.gameObject.SetActive(true);
            nameText.transform.localPosition = new Vector3(nameText.transform.localPosition.x, roof.localPosition.y + 0.005f, nameText.transform.localPosition.z);
            transform.Find("Roof").gameObject.SetActive(true);
        }
        else
        {
            nameText.gameObject.SetActive(false);
            nameText.transform.localPosition = new Vector3(nameText.transform.localPosition.x, 0.005f, nameText.transform.localPosition.z);
            transform.Find("Roof").gameObject.SetActive(false);
        }
    }

    ///<summary>
    /// On an UpdateTenantEvent, update the building's color if it's the right tenant
    ///</summary>
    ///<param name="_event">The event to catch</param>
    private void UpdateColorByTenant(UpdateTenantEvent _event)
    {
        if (_event.name == domain)
            UpdateColorByTenant();
    }

    ///<summary>
    /// Update building's color according to its domain.
    ///</summary>
    public void UpdateColorByTenant()
    {
        if (string.IsNullOrEmpty(domain))
            return;

        if (!GameManager.instance.allItems.Contains(domain))
        {
            GameManager.instance.AppendLogLine($"Tenant \"{domain}\" doesn't exist.", ELogTarget.both, ELogtype.error);
            return;
        }

        OgreeObject tenant = ((GameObject)GameManager.instance.allItems[domain]).GetComponent<OgreeObject>();

        Color color = Utils.ParseHtmlColor($"#{tenant.attributes["color"]}");

        foreach (Transform child in walls)
        {
            Material mat = child.GetComponent<Renderer>()?.material;
            if (mat)
                mat.color = color;
        }
        Transform roof = transform.Find("Roof");
        if (roof)
            roof.GetComponent<Renderer>().material.color = color;
    }


    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    public void ToggleCS()
    {
        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS)
        {
            localCS.SetActive(false); //for UI
            Destroy(localCS);
            GameManager.instance.AppendLogLine($"Hide local Coordinate System for {name}", ELogTarget.logger, ELogtype.success);
        }
        else
            PopLocalCS(csName);
    }

    ///<summary>
    /// Display or hide the local coordinate system
    ///</summary>
    ///<param name="_value">true of false value</param>
    public void ToggleCS(string _value)
    {
        _value = _value.ToLower();
        if (_value != "true" && _value != "false")
        {
            GameManager.instance.AppendLogLine($"[{hierarchyName}] Toggle local Coordinate System value has to be true or false", ELogTarget.both, ELogtype.warning);
            return;
        }

        string csName = "localCS";
        GameObject localCS = transform.Find(csName)?.gameObject;
        if (localCS && _value == "false")
        {
            Destroy(localCS);
            GameManager.instance.AppendLogLine($"Hide local Coordinate System for {name}", ELogTarget.logger, ELogtype.success);
        }
        else if (!localCS && _value == "true")
            PopLocalCS(csName);
    }

    ///<summary>
    /// Create a local Coordinate System for this object.
    ///</summary>
    ///<param name="_name">The name of the local CS</param>
    private void PopLocalCS(string _name)
    {
        GameObject localCS = Instantiate(GameManager.instance.coordinateSystemModel);
        localCS.name = _name;
        localCS.transform.parent = transform;
        localCS.transform.localScale = 7 * Vector3.one;
        localCS.transform.localEulerAngles = Vector3.zero;
        localCS.transform.localPosition = Vector3.zero;
        GameManager.instance.AppendLogLine($"Display local Coordinate System for {name}", ELogTarget.logger, ELogtype.success);
    }

}
