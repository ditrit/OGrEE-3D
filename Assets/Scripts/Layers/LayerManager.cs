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
            if (obj is Room)
            {
                if (obj.GetComponentInChildren<Rack>() && obj.GetLayer("racks") == null)
                    layers.Add(AutoLayerByCategory("rack", obj));
                if (obj.GetComponentInChildren<Corridor>() && obj.GetLayer("corridors") == null)
                    layers.Add(AutoLayerByCategory("corridor", obj));
                if (obj.GetComponentInChildren<Group>() && obj.GetLayer("groups") == null)
                    layers.Add(AutoLayerByCategory("group", obj));
            }
            else if (obj is Rack || obj is Device)
                CreateAutoLayersItem((Item)obj);
        }

        // Link each layer to related object(s)
        for (int i = 0; i < layers.Count; i++)
            layers[i].FindObjects();

        StartCoroutine(WaitAndRebuildLayersMenu());
    }

    /// <summary>
    /// Wait until the end of current frame and call <see cref="UiManager.layerList.RebuildMenu"/>
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAndRebuildLayersMenu()
    {
        yield return new WaitForEndOfFrame();
        UiManager.instance.layersList.RebuildMenu(UiManager.instance.BuildLayersList);
    }

    /// <summary>
    /// Get a list of all devices types in an <paramref name="_item"/> and generate corresponding <see cref="AutoLayer"/>
    /// </summary>
    /// <param name="_item">The root Item</param>
    private void CreateAutoLayersItem(Item _item)
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

    /// <summary>
    /// Create an <see cref="AutoLayer"/> for given category
    /// </summary>
    /// <param name="_cat">The value of the "category" filter</param>
    /// <param name="_obj">The object targeted by the Layer</param>
    /// <returns>The newly created Layer</returns>
    private Layer AutoLayerByCategory(string _cat, OgreeObject _obj)
    {
        AutoLayer newLayer = new($"{_cat}s", _obj.id);
        newLayer.filters.Add("category", _cat);
        return newLayer;
    }

    /// <summary>
    /// Create an <see cref="AutoLayer"/> for given device type
    /// </summary>
    /// <param name="_type">The value of the "type" filter</param>
    /// <param name="_obj">The object targeted by the Layer</param>
    /// <returns>The newly created Layer</returns>
    private Layer AutoLayerByDeviceType(string _type, OgreeObject _obj)
    {
        AutoLayer newLayer = new(_type.EndsWith("s") ? _type : $"{_type}s", _obj.id);
        newLayer.filters.Add("category", "device");
        newLayer.filters.Add("type", _type);
        return newLayer;
    }

    /// <summary>
    /// Create a <see cref="Layer"/> from given <see cref="SApiLayer"/>
    /// </summary>
    /// <param name="_apiLayer">The <see cref="SApiLayer"/> used for creation</param>
    public void CreateLayerFromSApiLayer(SApiLayer _apiLayer)
    {
        Layer layer = new(_apiLayer);
        if (!layers.Contains(layer))
            layers.Add(layer);
    }

    /// <summary>
    /// Get a Layer using its slug
    /// </summary>
    /// <param name="_slug">The slug to look for</param>
    /// <returns>The asked Layer or null</returns>
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
