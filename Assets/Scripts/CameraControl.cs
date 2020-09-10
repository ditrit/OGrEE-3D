using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI infosTMP = null;

    [Header("Parameters")]
    [Range(5, 20)]
    public float moveSpeed = 15;
    [Range(20, 100)]
    public float rotationSpeed = 50;
    [Range(1.60f, 1.90f)]
    public float humanHeight = 1.75f;
    [SerializeField] private bool humanMode = false;


    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject)
            return;

        if (humanMode)
            FPSControls();
        else
            FreeModeControls();

        UpdateGUIInfos();
    }


    ///<summary>
    /// Linked to cameraSwitch Toggle.
    ///</summary>
    ///<param name="_value">Value given by the Toggle</param>
    public void SwitchCameraMode(bool _value)
    {
        humanMode = _value;
        if (humanMode)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, humanHeight, transform.localPosition.z);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        else
            transform.GetChild(0).localEulerAngles = Vector3.zero;
        UpdateGUIInfos();
    }

    ///<summary>
    /// Controls for "AntMan" mode.
    ///</summary>
    private void FreeModeControls()
    {
        if (Input.GetAxis("Vertical") != 0)
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed/*, Space.World*/);
        if (Input.GetAxis("Horizontal") != 0)
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed/*, Space.World*/);

        // Right click
        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            transform.Rotate(0, h, 0, Space.World);
            float v = -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
            transform.Rotate(v, 0, 0);
        }
        // Scrollwheel click
        else if (Input.GetMouseButton(2))
            transform.Translate(new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * Time.deltaTime * -moveSpeed);
        // Scrollwheel
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
            transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * moveSpeed);
    }

    ///<summary>
    /// Controls for "Human" mode.
    ///</summary>
    private void FPSControls()
    {
        if (Input.GetAxis("Vertical") != 0)
            transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * (moveSpeed / 2));
        if (Input.GetAxis("Horizontal") != 0)
            transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * (moveSpeed / 2));

        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed;
            transform.Rotate(0, h, 0);
            float v = -Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed;
            transform.GetChild(0).Rotate(v, 0, 0);
        }
    }

    ///<summary>
    /// Update GUI infos about the camera
    ///</summary>
    private void UpdateGUIInfos()
    {
        float rot;
        if (humanMode)
            rot = transform.GetChild(0).localEulerAngles.x;
        else
            rot = transform.localEulerAngles.x;

        if (rot < 0 || rot > 180)
            rot -= 360;
        
        infosTMP.text = $"Camera pos: [{transform.localPosition.x.ToString("F2")},{transform.localPosition.y.ToString("F2")},{transform.localPosition.z.ToString("F2")}]";
        infosTMP.text += $"\nCamera angle: {rot.ToString("0")}°";
    }
}
