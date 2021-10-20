using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectOnMouseHover : MonoBehaviour
{
    [SerializeField] private GameObject savedObjectThatWeHover;

    private void Update()
    {
        GameObject objectHit = Utils.RaycastFromCameraToMouse();
        if (objectHit)
        {
            if (!objectHit.Equals(savedObjectThatWeHover))
            {
                if (savedObjectThatWeHover)
                    EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });

                savedObjectThatWeHover = objectHit;
                EventManager.Instance.Raise(new OnMouseHoverEvent { obj = objectHit });
            }
        }
        else
        {
            if (savedObjectThatWeHover)
                EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });
            savedObjectThatWeHover = null;
        }

    }
}
