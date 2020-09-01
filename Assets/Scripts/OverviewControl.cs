using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class OverviewControl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infosTMP = null;

    [Range(1, 15)]
    public float moveSpeed = 10;
    [Range(20, 100)]
    public float rotationSpeed = 50;

    private void Update()
    {
        // if (Input.GetAxis("Mouse ScrollWheel") != 0)
        //     transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * moveSpeed);

        if (EventSystem.current.currentSelectedGameObject)
            return;

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

        infosTMP.text = $"Camera pos: [{transform.localPosition.x.ToString("0")},{transform.localPosition.y.ToString("0")},{transform.localPosition.z.ToString("0")}]";
        float rot;
        if (0 <= transform.localEulerAngles.x && transform.localEulerAngles.x < 180)
            rot = transform.localEulerAngles.x;
        else
            rot = transform.localEulerAngles.x - 360;
        infosTMP.text += $"\nCamera angle: {rot.ToString("0")}°";
    }
}
