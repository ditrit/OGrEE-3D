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

    private void OnImportFinihsed(ImportFinishedEvent _e)
    {
        nameText.gameObject.SetActive(!GetComponentInChildren<Room>());
    }
}
