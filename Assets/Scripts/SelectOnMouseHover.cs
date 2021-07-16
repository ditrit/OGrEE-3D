using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectOnMouseHover : MonoBehaviour
{
    public Transform savedObjectThatWeHover;
    

    // Update is called once per frame
    void Update()
    {
       Transform objectHit =  RaycastFromCameraToMouse();

        if(objectHit) {
            Debug.Log(objectHit.name);
            if(objectHit.Equals(savedObjectThatWeHover)) {

            } else {
                if(savedObjectThatWeHover)
                    EventManager.Instance.Raise(new OnDeselectItemEvent { _obj = savedObjectThatWeHover.gameObject });

                savedObjectThatWeHover = objectHit;
                EventManager.Instance.Raise(new OnSelectItemEvent { _obj = objectHit.gameObject });

            }
        }
        
    }

    private Transform RaycastFromCameraToMouse() {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit)) {
            Transform objectHit = hit.transform;

            return objectHit;
            // Do something with the object that was hit by the raycast.
        } else {
            return null;
        }

        
    }
}
