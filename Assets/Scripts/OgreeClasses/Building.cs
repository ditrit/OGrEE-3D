using TMPro;
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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!(this is Room))
            EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinihsed);
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
            nameText.transform.localPosition = new Vector3(nameText.transform.localPosition.x,roof.localPosition.y + 0.005f, nameText.transform.localPosition.z);
            transform.Find("Roof").gameObject.SetActive(true);
        }
        else
        {
            nameText.gameObject.SetActive(false);
            nameText.transform.localPosition = new Vector3(nameText.transform.localPosition.x, 0.005f, nameText.transform.localPosition.z);
            transform.Find("Roof").gameObject.SetActive(false);
        }
    }
}
