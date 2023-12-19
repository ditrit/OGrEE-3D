using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLayer : Layer
{
    public AutoLayer(string _slug, string _applicability)
    {
        slug = _slug;
        applicability = _applicability;
        filters = new();
    }

    public override void FindObjects()
    {
        base.FindObjects();
        if (targetObjects.Count == 0)
            LayerManager.instance.layers.Remove(this);
    }

    public List<GameObject> GetRelatedObjects(Transform _target)
    {
        List<GameObject> objects = new();
        foreach (Transform child in _target)
        {
            if (child.GetComponent<OgreeObject>() is OgreeObject obj)
            {
                bool canBeAdded = false;
                foreach (KeyValuePair<string, string> kvp in filters)
                {
                    switch (kvp.Key)
                    {
                        case "category":
                            if (obj.category == kvp.Value)
                                canBeAdded = true;
                            break;
                        case "type": 
                            if (obj.attributes["type"] == kvp.Value)
                                canBeAdded = true;
                            break;
                    }
                }
                if (canBeAdded)
                    objects.Add(child.gameObject);
            }
        }

        return objects;
    }
}
