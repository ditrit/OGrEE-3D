using TMPro;
using UnityEngine;

public class Building : OgreeObject
{
    public bool isSquare = true;

    [Header("BD References")]
    public Transform walls;
    public TextMeshPro nameText;

    private void Start()
    {
        if (!(this is Room))
            EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinihsed);
        EventManager.instance.AddListener<UpdateDomainEvent>(UpdateColorByDomain);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!(this is Room))
            EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinihsed);
        EventManager.instance.RemoveListener<UpdateDomainEvent>(UpdateColorByDomain);
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
        name = _src.name;
        hierarchyName = _src.hierarchyName;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        if (domain != _src.domain)
        {
            domain = _src.domain;
            UpdateColorByDomain();
        }
        description = _src.description;
        attributes = _src.attributes;
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

        OgreeObject domain = ((GameObject)GameManager.instance.allItems[base.domain]).GetComponent<OgreeObject>();

        Color color = Utils.ParseHtmlColor($"#{domain.attributes["color"]}");

        foreach (Transform child in walls)
        {
            Renderer rend = child.GetComponent<Renderer>();
            if (rend)
                rend.material.color = color;
        }
        Transform roof = transform.Find("Roof");
        if (roof)
            roof.GetComponent<Renderer>().material.color = color;
    }
}
