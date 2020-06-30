using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class OverviewControl : MonoBehaviour
{
    [Range(1, 15)]
    public float moveSpeed = 10;
    [Range(20, 100)]
    public float rotationSpeed = 50;


    private void Start()
    {
        GetComponent<ParentConstraint>().enabled = false;
    }

    private void Update()
    {
        // if (Input.GetAxis("Mouse ScrollWheel") != 0)
        //     transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * moveSpeed);

        if (Input.GetAxis("Vertical") != 0)
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed/*, Space.World*/);
        if (Input.GetAxis("Horizontal") != 0)
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed/*, Space.World*/);

        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            transform.Rotate(0, h, 0, Space.World);

            float v = -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
            transform.Rotate(v, 0, 0);
        }
    }
}
