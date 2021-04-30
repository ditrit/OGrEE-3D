using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelTemplate : MonoBehaviour
{
    private void Start()
    {
        GameManager.gm.rackTemplates.Add(name, gameObject);
#if !DEBUG
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif
        Destroy(this);
    }
}
