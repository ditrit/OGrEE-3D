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

    private Vector3 previousMousePosition;

    private void Update()
    {
        print(previousMousePosition);
        if (!Input.GetMouseButton(0))
        {
            print("houhou");
            previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return;
        }

        Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit);
        if (hit.collider != collider)
            return;
        Vector3 offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - previousMousePosition;
        print(offset);
        switch (axis)
        {
            case WhichOne.X:
                if (isRotation)
                    transform.parent.parent.localEulerAngles += offset.x * Vector3.right;
                else
                    transform.parent.parent.position = offset.x * Vector3.right;
                break;
            case WhichOne.Y:
                if (isRotation)
                    transform.parent.parent.localEulerAngles += offset.x * Vector3.up;
                else
                    transform.parent.parent.localPosition += offset.x * Vector3.up;
                break;
            case WhichOne.Z:
                if (isRotation)
                    transform.parent.parent.localEulerAngles += offset.x * Vector3.forward;
                else
                    transform.parent.parent.localPosition += offset.x * Vector3.forward;
                break;
        }
        previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }
}
