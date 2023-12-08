using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    static public LayerManager instance;

    public List<Layer> layers = new();


    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }
    // Start is called before the first frame update
    private void Start()
    {
        EventManager.instance.ImportFinished.Add(OnImportFinished);
    }

    private void OnDestroy()
    {
        EventManager.instance.ImportFinished.Remove(OnImportFinished);
    }

    private void OnImportFinished(ImportFinishedEvent _e)
    {
        foreach (Layer l in layers)
            l.FindObjects();
    }

}
