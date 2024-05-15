using Newtonsoft.Json.Linq;
using TMPro;
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
        EventManager.instance.PositionMode.Add(OnPositionMode);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (this is not Room)
            EventManager.instance.ImportFinished.Remove(OnImportFinihsed);
        EventManager.instance.UpdateDomain.Remove(UpdateColorByDomain);
        EventManager.instance.PositionMode.Remove(OnPositionMode);
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

    /// <summary>
    /// When called, set all children colliders according to <see cref="GameManager.positionMode"/> 
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnPositionMode(PositionModeEvent _e)
    {
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer(_e.toggled ? "Ignore Raycast" : "Default");
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
            UpdateColorByDomain(_src.domain);

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
    ///<param name="_domain">An optionnal domain to use</param>
    public void UpdateColorByDomain(string _domain = null)
    {
        string domainToUse = string.IsNullOrEmpty(_domain) ? domain : _domain;
        if (!GameManager.instance.allItems.Contains(domainToUse))
        {
            GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Domain doesn't exist", domainToUse), ELogTarget.both, ELogtype.error);
            return;
        }

        Domain domainObject = ((GameObject)GameManager.instance.allItems[domainToUse]).GetComponent<Domain>();

        Color color = Utils.ParseHtmlColor($"#{domainObject.attributes["color"]}");

        foreach (Transform child in walls)
            // walls & pillars || separators
            if (child.TryGetComponent(out Renderer rend) || child.GetChild(0).TryGetComponent(out rend))
                rend.material.color = color;

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
        Vector2 posXY = ((JArray)_apiObj.attributes["posXY"]).ToVector2();
        posXY *= GetUnitFromAttributes(_apiObj);
        transform.localPosition = new(posXY.x, 0, posXY.y);
        transform.localEulerAngles = new(0, float.Parse(_apiObj.attributes["rotation"].ToString()), 0);
    }
}
