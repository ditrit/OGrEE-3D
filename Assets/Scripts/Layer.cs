using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    /// <summary>
    /// Custom constructor from a <see cref="SApiLayer"/>
    /// </summary>
    /// <param name="_apiLayer">Data to use for creation</param>
    public Layer(SApiLayer _apiLayer)
    {
        slug = _apiLayer.slug;
        applicability = _apiLayer.applicability.Replace("/", ".");
        filters = _apiLayer.filters;
    }

    /// <summary>
    /// Fill <see cref="targetObjects"/> from <see cref="applicability"/> and register this layer in each <see cref="targetObjects"/>
    /// </summary>
    public void FindObjects()
    {
        targetObjects.Clear();

        // Fill targetObjects
        if (applicability.EndsWith(".**"))
        {
            // Remove ".**" from applicability
            string pattern = applicability.Remove(applicability.Length - 3);
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                if (((string)de.Key).StartsWith(pattern))
                    targetObjects.Add((GameObject)de.Value);
            }
        }
        else if (applicability.EndsWith(".*"))
        {
            // Remove ".*" from applicability
            string pattern = applicability.Remove(applicability.Length - 2);
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                string key = (string)de.Key;
                if (key.StartsWith(pattern) && key.Count(x => x == '.') <= pattern.Count(x => x == '.') + 1)
                    targetObjects.Add((GameObject)de.Value);
            }
        }
        else
        {
            if (Utils.GetObjectById(applicability) is GameObject go)
                targetObjects.Add(go);
        }

        // Register the layer in each OgreeObject
        foreach (GameObject go in targetObjects)
        {
            OgreeObject obj = go.GetComponent<OgreeObject>();
            if (!obj.layers.ContainsKey(this))
                obj.layers.Add(this, true);
        }
    }


    public async Task<List<GameObject>> GetRelatedObjects(string _rootId)
    {
        List<GameObject> relatedObjects = new();

        string apiCall = $"objects?id={applicability}.%2A&namespace=physical.hierarchy";
        foreach (KeyValuePair<string, string> kvp in filters)
            apiCall += $"&{kvp.Key}={kvp.Value}";

        List<GameObject> apiResultObjects = await ApiManager.instance.GetObject(apiCall, ApiManager.instance.GetLayerContent);
        Debug.Log($"Got {apiResultObjects.Count} objects");

        await Task.Delay(10);
        foreach (GameObject go in apiResultObjects)
        {
            if (go.GetComponent<OgreeObject>().id.Contains(_rootId))
                relatedObjects.Add(go);
        }

        // foreach (GameObject go in relatedObjects)
        //     Debug.Log($"[] {go.name}");
        return relatedObjects;
    }
}
