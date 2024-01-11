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

    private void Start()
    {
        EventManager.instance.ConnectApi.Add(OnApiConnect);
        EventManager.instance.ImportFinished.Add(OnImportFinished);
    }

    private void OnDestroy()
    {
        EventManager.instance.ImportFinished.Remove(OnImportFinished);
        EventManager.instance.ConnectApi.Remove(OnApiConnect);
    }

    private async void OnApiConnect(ConnectApiEvent _e)
    {
        await ApiManager.instance.GetObject("layers", ApiManager.instance.CreateLayer);
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
                    if (obj.GetLayer("racks") == null)
                        layers.Add(AutoLayerByCategory("rack", obj));
                    if (obj.GetLayer("corridors") == null)
                        layers.Add(AutoLayerByCategory("corridor", obj));
                    if (obj.GetLayer("groups") == null)
                        layers.Add(AutoLayerByCategory("group", obj));
                    break;
                case Category.Rack:
                case Category.Device:
                    CreateAutoLayersItem((Item)obj);
                    break;
            }
        }

        // Link each layer to related object(s)
        for (int i = 0; i < layers.Count; i++)
            layers[i].FindObjects();

        StartCoroutine(WaitAndRebuildLayersMenu());
    }

    private IEnumerator WaitAndRebuildLayersMenu()
    {
        yield return new WaitForEndOfFrame();
        UiManager.instance.layersList.RebuildMenu(UiManager.instance.BuildLayersList);
    }

    public void CreateAutoLayersItem(Item _item)
    {
        List<string> deviceTypes = new();
        foreach (Transform child in _item.transform)
        {
            if (child.GetComponent<Device>() is Device dv && dv.attributes.ContainsKey("type") && !deviceTypes.Contains(dv.attributes["type"]))
                deviceTypes.Add(dv.attributes["type"]);
        }

        foreach (string type in deviceTypes)
        {
            if (_item.GetLayer(type.EndsWith("s") ? type : $"{type}s") == null)
                layers.Add(AutoLayerByDeviceType(type, _item));
        }
    }

    private Layer AutoLayerByCategory(string _cat, OgreeObject _obj)
    {
        AutoLayer newLayer = new($"{_cat}s", _obj.id);
        newLayer.filters.Add("category", _cat);
        return newLayer;
    }

    private Layer AutoLayerByDeviceType(string _type, OgreeObject _obj)
    {
        AutoLayer newLayer = new(_type.EndsWith("s") ? _type : $"{_type}s", _obj.id);
        newLayer.filters.Add("category", "device");
        newLayer.filters.Add("type", _type);
        return newLayer;
    }

    public void CreateLayerFromSApiLayer(SApiLayer _apiLayer)
    {
        Layer layer = new(_apiLayer);
        if (!layers.Contains(layer))
            layers.Add(layer);
    }

    public Layer GetLayer(string _slug)
    {
        foreach (Layer l in layers)
        {
            if (l.slug == _slug)
                return l;
        }
        return null;
    }
}
