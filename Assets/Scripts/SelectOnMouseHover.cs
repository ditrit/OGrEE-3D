using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectOnMouseHover : MonoBehaviour
{
    [SerializeField] private Transform savedObjectThatWeHover;

    private void Update()
    {
        Transform objectHit = RaycastFromCameraToMouse();

        if (objectHit)
        {
            // Debug.Log(objectHit.name);
            if (!objectHit.Equals(savedObjectThatWeHover))
            {
                if (savedObjectThatWeHover)
                    EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover.gameObject });

                savedObjectThatWeHover = objectHit;
                EventManager.Instance.Raise(new OnMouseHoverEvent { obj = objectHit.gameObject });
            }
        }
    }

    private Transform RaycastFromCameraToMouse()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            return objectHit;
        }
        else
            return null;
    }
}
