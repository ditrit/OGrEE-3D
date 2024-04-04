﻿using TMPro;
using UnityEngine;

public class Building : OgreeObject
{
    public bool isSquare = true;

    [Header("BD References")]
    public Transform walls;
    public TextMeshPro nameText;
    public bool displayWalls = true;

    private void Start()
    {
        if (this is not Room)
            EventManager.instance.ImportFinished.Add(OnImportFinihsed);
        EventManager.instance.UpdateDomain.Add(UpdateColorByDomain);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (this is not Room)
            EventManager.instance.ImportFinished.Remove(OnImportFinihsed);
        EventManager.instance.UpdateDomain.Remove(UpdateColorByDomain);
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
            nameText.transform.localPosition = new(nameText.transform.localPosition.x, roof.localPosition.y + 0.005f, nameText.transform.localPosition.z);
            roof.gameObject.SetActive(true);
        }
        else
        {
            nameText.gameObject.SetActive(false);
            nameText.transform.localPosition = new(nameText.transform.localPosition.x, 0.005f, nameText.transform.localPosition.z);
            roof.gameObject.SetActive(false);
        }
    }

    ///<summary>
    /// On an UpdateDomainEvent, update the building's color if it's the right domain
    ///</summary>
    ///<param name="_event">The event to catch</param>
    private void UpdateColorByDomain(UpdateDomainEvent _event)
    {
        if (_event.name == domain)
            UpdateColorByDomain();
    }

    ///<summary>
    /// Update the OgreeObject attributes with given SApiObject.
    ///</summary>
    ///<param name="_src">The SApiObject used to update attributes</param>
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if (domain != _src.domain)
        {
            domain = _src.domain;
            UpdateColorByDomain();
        }
        if ((HasAttributeChanged(_src, "posXY")
            || HasAttributeChanged(_src, "posXYUnit")
            || HasAttributeChanged(_src, "rotation"))
            && transform.parent)
            PlaceBuilding(_src);

        base.UpdateFromSApiObject(_src);
    }

    ///<summary>
    /// Update building's color according to its domain.
    ///</summary>
    public void UpdateColorByDomain()
    {
        if (string.IsNullOrEmpty(base.domain))
            return;

        if (!GameManager.instance.allItems.Contains(base.domain))
        {
            GameManager.instance.AppendLogLine($"Domain \"{base.domain}\" doesn't exist.", ELogTarget.both, ELogtype.error);
            return;
        }

        Domain domain = ((GameObject)GameManager.instance.allItems[base.domain]).GetComponent<Domain>();

        Color color = Utils.ParseHtmlColor($"#{domain.attributes["color"]}");

        foreach (Transform child in walls)
        {
            if (child.TryGetComponent(out Renderer rend))
                rend.material.color = color;
        }
        Transform roof = transform.Find("Roof");
        if (roof)
            roof.GetComponent<Renderer>().material.color = color;
    }

    /// <summary>
    /// Toggle walls, separators and pillars Renderer & Collider according to <see cref="displayWalls"/>.
    /// </summary>
    public virtual void ToggleWalls()
    {
        displayWalls = !displayWalls;
        foreach (Transform wall in walls)
        {
            wall.GetComponentInChildren<Renderer>().enabled = displayWalls;
            wall.GetComponentInChildren<Collider>().enabled = displayWalls;
        }
    }

    /// <summary>
    /// Move the given building/room to its position in a site/building according to the API data.
    /// </summary>
    /// <param name="_apiObj">The SApiObject containing relevant positionning data</param>
    public void PlaceBuilding(SApiObject _apiObj)
    {
        Vector2 posXY = Utils.ParseVector2(_apiObj.attributes["posXY"]);
        posXY *= _apiObj.attributes["posXYUnit"] switch
        {
            LengthUnit.Centimeter => 0.01f,
            LengthUnit.Millimeter => 0.001f,
            LengthUnit.Feet => UnitValue.Foot,
            _ => 1
        };
        transform.localPosition = new(posXY.x, 0, posXY.y);
        transform.localEulerAngles = new(0, Utils.ParseDecFrac(_apiObj.attributes["rotation"]), 0);
    }
}
