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
    private Vector3 previousMousePositionScreen;
    private Vector3 offset;
    public void Move()
    {
        if (!Input.GetMouseButton(0))
        {
            active = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(Camera.main.transform.position, ray.direction, out RaycastHit hit);

        Vector3 axisAlong = axis switch
        {
            WhichOne.X => Rescaler.instance.realDisplacement.right,
            WhichOne.Y => Rescaler.instance.realDisplacement.up,
            _ => Rescaler.instance.realDisplacement.forward,
        };
        if (isRotation)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider == collider)
                {
                    active = true;
                    previousMousePositionScreen = Input.mousePosition;
                }
            }
            if (!active)
                return;
            Vector3 aaah = Camera.main.transform.InverseTransformVector(Vector3.ProjectOnPlane(axisAlong, Camera.main.transform.forward));
            float distance =Mathf.Sin(Mathf.Deg2Rad * Vector3.SignedAngle(aaah, Input.mousePosition - previousMousePositionScreen, Camera.main.transform.forward)) *(Input.mousePosition - previousMousePositionScreen).magnitude;
            Rescaler.instance.realDisplacement.Rotate(distance * Rescaler.instance.realDisplacement.InverseTransformVector(axisAlong));
            previousMousePositionScreen = Input.mousePosition;
        }            
        else
        {
            // https://fr.wikipedia.org/wiki/Plan_(math%C3%A9matiques)#Dans_un_cadre_affine
            // https://wolfreealpha.gitlab.io/input/?i=solve+m*x+%2B+n*y+%2B+o*z+%3D+0%2C++%28v*o+-+w*n%29*x+%2B+%28w*m+-+u*o%29*y+%2B+%28u*n+-+v*m%29*z+-+a*%28v*o+-+w*n%29+-+b*%28w*m+-+u*o%29+-+c*%28u*n+-+v*m%29+%3D+0%2Cx%2By%2Bz%3D1+for+x%2Cy%2Cz
            // with the camera forward vector as (u,v,w), the axis along which the object moves as (m,n,o), the position of the object as (a,b,c), we want a plane containing (m,n,o) facing the camera as much as possible. So we look for a vector orthogonal
            // to (m,n,o) in the plane P containing (m,n,o) and (u,v,w). First we compute the cartesian equation of P then we compute a vector of P orthogonal to (m,n,o). We also look for a normalised vector in order to have only one solution
            // It's complicated
            // But it works
            float a = Rescaler.instance.realDisplacement.position.x;
            float b = Rescaler.instance.realDisplacement.position.y;
            float c = Rescaler.instance.realDisplacement.position.z;
            float u = Camera.main.transform.forward.x;
            float v = Camera.main.transform.forward.y;
            float w = Camera.main.transform.forward.z;
            float m = axisAlong.x;
            float n = axisAlong.y;
            float o = axisAlong.z;
            float x = -((n - o) * (-a * n * w + a * o * v + b * m * w - b * o * u - c * m * v + c * n * u + m * v - n * u) - o * (-m * v - m * w + n * u + o * u)) / ((n - o) * (-m * v + n * u + n * w - o * v) - (m - o) * (-m * v - m * w + n * u + o * u));
            float y = -(a * m * n * w - a * m * o * v - a * n * o * w + a * o * o * v - b * m * m * w + b * m * o * u + b * m * o * w - b * o * o * u + c * m * m * v - c * m * n * u - c * m * o * v + c * n * o * u - m * m * v + m * n * u + n * o * w - o * o * v) / (m * m * v + m * m * w - m * n * u - m * n * v - m * o * u - m * o * w + n * n * u + n * n * w - n * o * v - n * o * w + o * o * u + o * o * v);
            float z = -(-a * m * n * w + a * m * o * v + a * n * n * w - a * n * o * v + b * m * m * w - b * m * n * w - b * m * o * u + b * n * o * u - c * m * m * v + c * m * n * u + c * m * n * v - c * n * n * u - m * m * w + m * o * u - n * n * w + n * o * v) / (m * m * v + m * m * w - m * n * u - m * n * v - m * o * u - m * o * w + n * n * u + n * n * w - n * o * v - n * o * w + o * o * u + o * o * v);
            Plane rightPlane = new(new Vector3(x, y, z), Rescaler.instance.realDisplacement.position);
            rightPlane.Raycast(ray, out float rightDistance);
            Vector3 mousePosition = ray.GetPoint(rightDistance);

            if (Input.GetMouseButtonDown(0))
            {
                if (hit.collider == collider)
                {
                    active = true;
                    offset = Vector3.Project(mousePosition - Rescaler.instance.realDisplacement.position, axisAlong);
                }
            }
            if (!active)
                return;
            Rescaler.instance.realDisplacement.position += Vector3.Project(mousePosition - Rescaler.instance.realDisplacement.position, axisAlong) - offset;
        }
    }
}
