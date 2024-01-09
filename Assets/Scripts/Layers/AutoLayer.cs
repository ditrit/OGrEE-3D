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

    public async override void FindObjects()
    {
        await Task.Delay(10);
        base.FindObjects();
        if (targetObjects.Count == 0)
            LayerManager.instance.layers.Remove(this);
    }

    public Task<List<GameObject>> GetRelatedObjects(Transform _target)
    {
        List<GameObject> objects = new();
        // Case for catching Devices (only one with type key)
        if (filters.ContainsKey("type"))
        {
            foreach (Transform child in _target)
            {
                if (child.GetComponent<OgreeObject>() is OgreeObject obj && obj.category == filters["category"]
                                                                        && obj.attributes["type"] == filters["type"])
                    objects.Add(child.gameObject);
            }
        }
        // Case for Racks, Corridors or Groups
        else
        {
            foreach (Transform child in _target)
            {
                if (child.GetComponent<OgreeObject>() is OgreeObject obj && obj.category == filters["category"])
                    objects.Add(child.gameObject);
            }
        }
        return Task.FromResult(objects);
    }
}
