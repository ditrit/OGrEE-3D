using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisMover : MonoBehaviour
{
    private enum EAxis
    {
        X,
        Y,
        Z
    }
    [SerializeField] Color baseColor;
    [SerializeField] private new Renderer renderer;
    [SerializeField] private EAxis axis;
    [SerializeField] private bool isRotation;
    [SerializeField] private new MeshCollider collider;
    public bool active = false;
    private Vector3 previousMousePos;

    /// <summary>
    /// Move <see cref="Positionner.realDisplacement"/> along <see cref="axis"/>  in rotation or translation
    ///<br/> according to <see cref="isRotation"/>
    /// </summary>
    public void Move()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(Camera.main.transform.position, ray.direction, out RaycastHit hit);
        Vector3 axisAlong = axis switch
        {
            EAxis.X => Positionner.instance.realDisplacement.right,
            EAxis.Y => Positionner.instance.realDisplacement.up,
            _ => Positionner.instance.realDisplacement.forward,
        };

        if (hit.collider == collider && (isRotation || Vector3.Angle(Camera.main.transform.forward, axisAlong) == Mathf.Clamp(Vector3.Angle(Camera.main.transform.forward, axisAlong), 5, 175)))
            renderer.material.color = Color.white;
        else if (!active)
        {
            renderer.material.color = baseColor;
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            active = false;
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            active = true;
            previousMousePos = Input.mousePosition;
        }

        if (isRotation)
        {
            Vector3 projected = Camera.main.transform.InverseTransformVector(Vector3.ProjectOnPlane(axisAlong, Camera.main.transform.forward));
            float distance = Mathf.Sin(Mathf.Deg2Rad * Vector3.SignedAngle(projected, Input.mousePosition - previousMousePos, Camera.main.transform.forward)) * (Input.mousePosition - previousMousePos).magnitude;
            Positionner.instance.realDisplacement.Rotate(distance * Positionner.instance.realDisplacement.InverseTransformVector(axisAlong));
        }
        else
        {
            Vector3 projected = Vector3.ProjectOnPlane(axisAlong, Camera.main.transform.forward);
            Vector3 realMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.Distance(Camera.main.transform.position, transform.position) * Vector3.forward);
            Vector3 realPreviousMousePos = Camera.main.ScreenToWorldPoint(previousMousePos + Vector3.Distance(Camera.main.transform.position, transform.position) * Vector3.forward);
            Vector3 displacement = Vector3.Project(realMousePos - realPreviousMousePos, projected);
            float angle = Vector3.SignedAngle(axisAlong, displacement, Camera.main.transform.forward);
            if (Mathf.Abs(Mathf.Abs(angle) - 90) > 5)
                Positionner.instance.realDisplacement.position += displacement.magnitude / Mathf.Cos(Mathf.Deg2Rad * Vector3.SignedAngle(axisAlong, displacement, Camera.main.transform.forward)) * axisAlong;
            else
                active = false;
        }
        previousMousePos = Input.mousePosition;
    }
}
