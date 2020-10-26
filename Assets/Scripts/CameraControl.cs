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

    [SerializeField] private List<Vector3> targetPos = new List<Vector3>();
    [SerializeField] private List<Vector3> targetRot = new List<Vector3>();
    private bool isReady = true;

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject)
            return;

        if (humanMode)
            FPSControls();
        else
            FreeModeControls();

        if (isReady && targetPos.Count > 0)
        {
            // Code for Waiting targetPos.y seconds
            if (targetRot[0].x == 999)
                StartCoroutine(Wait(targetRot[0].y));
            else
                MoveToTarget();
        }

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
            transform.GetChild(0).localEulerAngles = Vector3.zero;//apply to wrapper and reset!
        UpdateGUIInfos();
    }

    ///<summary>
    /// Move Camera to given coordinates
    ///</summary>
    ///<param name="_pos">Position of the destination</param>
    ///<param name="_rot">Rotation of the destination</param>
    public void MoveCamera(Vector3 _pos, Vector2 _rot)
    {
        if (humanMode)
            SwitchCameraMode(false);
        transform.localPosition = _pos;
        transform.localEulerAngles = new Vector3(_rot.x, _rot.y, 0);
    }

    ///<summary>
    /// Register given coordinates in targetPos and targetRot
    ///</summary>
    ///<param name="_pos">Position of the destination</param>
    ///<param name="_rot">Rotation of the destination</param>
    public void TranslateCamera(Vector3 _pos, Vector2 _rot)
    {
        targetPos.Add(_pos);
        targetRot.Add(new Vector3(_rot.x, _rot.y, 0));
    }

    ///<summary>
    /// Create a false targetRot with rot.x=999 and rot.y = time to wait.
    ///</summary>
    ///<param name="_time">The time to wait</param>
    public void WaitCamera(float _time)
    {
        targetRot.Add(new Vector3(999, _time, 0));
    }

    ///<summary>
    /// Move camera to targetPos[0] and targetRot[0].
    ///</summary>
    private void MoveToTarget()
    {
        if (humanMode)
            SwitchCameraMode(false);
        float speed = 10f * Time.deltaTime;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos[0], speed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetRot[0]),
                                                    speed / Vector3.Distance(transform.localPosition, targetPos[0]));
        if (Vector3.Distance(transform.localPosition, targetPos[0]) < 0.1f)
        {
            targetPos.RemoveAt(0);
            targetRot.RemoveAt(0);
        }
    }

    ///<summary>
    /// Wait _f seconds before authorising MoveToTarget(). 
    ///</summary>
    ///<param name="_f">The time to wait in seconds</param>
    private IEnumerator Wait(float _f)
    {
        isReady = false;
        targetRot.RemoveAt(0);
        yield return new WaitForSeconds(_f);
        isReady = true;
    }

    ///<summary>
    /// Controls for "AntMan" mode.
    ///</summary>
    private void FreeModeControls()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
            else
                transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed);
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0, Space.World);
            else
                transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed);
        }

        // Right click
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed, 0, Space.World);
            transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed, 0, 0);
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
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.GetChild(0).Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
            else
                transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * Time.deltaTime * (moveSpeed / 2));
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0);
            else
                transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * Time.deltaTime * (moveSpeed / 2));
        }

        if (Input.GetMouseButton(1))
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed, 0);
            transform.GetChild(0).Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed, 0, 0);
        }
    }

    ///<summary>
    /// Update GUI infos about the camera
    ///</summary>
    private void UpdateGUIInfos()
    {
        float rotX;
        float rotY = transform.localEulerAngles.y;
        if (humanMode)
            rotX = transform.GetChild(0).localEulerAngles.x;
        else
            rotX = transform.localEulerAngles.x;

        if (rotX < 0 || rotX > 180)
            rotX -= 360;
        if (rotY < 0 || rotY > 180)
            rotY -= 360;

        infosTMP.text = $"Camera pos: [{transform.localPosition.x.ToString("0.##")};{transform.localPosition.z.ToString("0.##")};{transform.localPosition.y.ToString("0.##")}]";
        if (!isReady)
            infosTMP.text += " (Waiting)";
        infosTMP.text += $"\nCamera angle: [{rotX.ToString("0")};{rotY.ToString("0")}]";
    }
}
