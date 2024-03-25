using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    private enum WhichOne
    {
        X,
        Y,
        Z
    }
    [SerializeField]
    private WhichOne axis;
    [SerializeField]
    private bool isRotation;
    [SerializeField]
    private new MeshCollider collider;
    private bool active = false;
    private Vector3 previousMousePosition;
    private Vector3 previousMousePositionScreen;

    private void Update()
    {
        if (!Input.GetMouseButton(0))
        {
            active = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(Camera.main.transform.position,ray.direction, out RaycastHit hit);
        Plane plane = new(-ray.direction, transform.position);
        plane.Raycast(ray, out float distance);
        if (Input.GetMouseButtonDown(0))
        {
            if (hit.collider == collider)
            {
                active = true;
                previousMousePosition = ray.GetPoint(distance);
                previousMousePositionScreen = Input.mousePosition;
            }
        }
        if (!active)
            return;
        Vector3 mousePosition = ray.GetPoint(distance);
        Vector3 offset = mousePosition - previousMousePosition;
        Vector3 offsetScreen = Input.mousePosition - previousMousePositionScreen;
        switch (axis)
        {
            case WhichOne.X:
                if (isRotation)
                    transform.parent.parent.Rotate((offsetScreen.x + offsetScreen.y) * Vector3.right);
                else
                    transform.parent.parent.position += transform.parent.parent.rotation *( offset.x * Vector3.right);
                break;
            case WhichOne.Y:
                if (isRotation)
                    transform.parent.parent.Rotate((offsetScreen.x + offsetScreen.y) * Vector3.up);
                else
                    transform.parent.parent.position += transform.parent.parent.rotation * (offset.y * Vector3.up);
                break;
            case WhichOne.Z:
                if (isRotation)
                    transform.parent.parent.Rotate((offsetScreen.x + offsetScreen.y) * Vector3.forward);
                else
                    transform.parent.parent.position += transform.parent.parent.rotation * (offset.z * Vector3.forward);
                break;
        }
        previousMousePosition = mousePosition;
        previousMousePositionScreen = Input.mousePosition;
    }
}
