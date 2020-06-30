using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Range(1, 5)]
    public float moveSpeed = 1;
    [Range(50, 150)]
    public float rotationSpeed = 100;

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetAxis("Vertical") != 0)
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed);
        if (Input.GetAxis("Horizontal") != 0)
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed);

        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            transform.Rotate(0, h, 0);

        }
    }
}
