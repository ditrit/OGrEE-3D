using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AutoLayer : Layer
{
    public AutoLayer(string _slug, string _applicability)
    {
        slug = _slug;
        applicability = _applicability;
        filters = new();
    }

    /// <summary>
    /// Use <see cref="Layer.FindObjects"/>, then if <see cref="targetObjects"/> is empty, delete this layer.
    /// </summary>
    public async override void FindObjects()
    {
        await Task.Delay(10);
        base.FindObjects();
        if (targetObjects.Count == 0)
            LayerManager.instance.layers.Remove(this);
    }

    /// <summary>
    /// Get objects corresponding to this layer, using <see cref="filters"/>
    /// </summary>
    /// <returns>All related GameObjects</returns>
    public Task<List<GameObject>> GetRelatedObjects()
    {
        List<GameObject> objects = new();
        // Case for catching Devices (only one with type key)
        if (filters.ContainsKey("type"))
        {
            foreach (Transform child in targetObjects[0].transform)
            {
                if (child.GetComponent<OgreeObject>() is OgreeObject obj && obj.category == filters["category"] && obj.attributes.ContainsKey("type") && obj.attributes["type"] == filters["type"])
                    objects.Add(child.gameObject);
            }
        }
        // Case for Racks, Corridors or Groups
        else
        {
            foreach (Transform child in targetObjects[0].transform)
            {
                if (child.GetComponent<OgreeObject>() is OgreeObject obj && obj.category == filters["category"])
                    objects.Add(child.gameObject);
            }
        }
        return Task.FromResult(objects);
    }
}
