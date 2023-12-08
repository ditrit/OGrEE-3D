using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Layer
{
    [Header("Refs from API")]
    public string slug;
    public string applicability;
    public Dictionary<string, string> filters;

    [Header("Refs for 3D")]
    public List<GameObject> targetObjects = new();
    public List<GameObject> resultObjects = new();

    public Layer(SApiLayer _apiLayer)
    {
        slug = _apiLayer.slug;
        applicability = _apiLayer.applicability.Replace("/", ".");
        filters = _apiLayer.filters;
    }

    public async void FindObjects()
    {
        targetObjects.Clear();
        resultObjects.Clear();

        // Apply applicability
        if (applicability.EndsWith("**"))
        {

        }
        else if (applicability.EndsWith("*"))
        {

        }
        else
        {
            if (Utils.GetObjectById(applicability) is GameObject go)
                targetObjects.Add(go);
        }

        // Apply filters
        
        string apiCall = $"objects?id={applicability}.%2A&namespace=physical.hierarchy";
        foreach (KeyValuePair<string, string> kvp in filters)
            apiCall += $"&{kvp.Key}={kvp.Value}";

        resultObjects = await ApiManager.instance.GetObject(apiCall, ApiManager.instance.GetLayerContent);
    }
}
