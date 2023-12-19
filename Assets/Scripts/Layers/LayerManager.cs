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
        // Create automatic layers if needed
        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            OgreeObject obj = ((GameObject)de.Value).GetComponent<OgreeObject>();
            switch (obj.category)
            {
                case Category.Room:
                    // if (!obj.layerSlugs.Contains("racks"))
                    if (obj.GetLayer("racks") == null)
                        layers.Add(AutoLayerByCategory("rack", obj));
                    if (obj.GetLayer("corridors") == null)
                        layers.Add(AutoLayerByCategory("corridor", obj));
                    if (obj.GetLayer("groups") == null)
                        layers.Add(AutoLayerByCategory("group", obj));
                    break;
                case Category.Rack:
                case Category.Device:
                    break;
            }

        }

        // Link each layer to related object(s)
        foreach (Layer l in layers)
            l.FindObjects();
    }

    public void CreateAutoLayersItem(Item _item, List<string> _deviceTypes)
    {
        foreach (string type in _deviceTypes)
            layers.Add(AutoLayerByDeviceType(type, _item));
    }

    private Layer AutoLayerByCategory(string _cat, OgreeObject _obj)
    {
        AutoLayer newLayer = new($"{_cat}s", _obj.id);
        newLayer.filters.Add("category", _cat);
        return newLayer;
    }

    private Layer AutoLayerByDeviceType(string _type, OgreeObject _obj)
    {
        AutoLayer newLayer = new($"{_type}s", _obj.id);
        newLayer.filters.Add("category", "device");
        newLayer.filters.Add("type", _type);
        return newLayer;
    }

}
