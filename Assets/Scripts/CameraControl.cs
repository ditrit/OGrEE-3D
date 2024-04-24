using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [System.Serializable]
    private struct SCameraTrans
    {
        public string label;
        public Vector3 pos;
        public Vector3 rot;
    }

    [Header("References")]
    [SerializeField] private TMP_InputField infosTMP = null;

    [Header("Parameters")]
    [SerializeField] private bool humanMode = false;

    [SerializeField] private List<Vector3> targetPos = new();
    [SerializeField] private List<Vector3> targetRot = new();
    [SerializeField] private List<SCameraTrans> labeledTransforms = new();
    private bool isReady = true;

    private void Start()
    {
        EventManager.instance.OnFocus.Add(OnFocus);
        EventManager.instance.OnUnFocus.Add(OnUnFocus);
    }

    private void OnDestroy()
    {
        EventManager.instance.OnFocus.Remove(OnFocus);
        EventManager.instance.OnUnFocus.Remove(OnUnFocus);
    }

    private void Update()
    {
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
            UpdateHumanModeHeight();
            transform.localEulerAngles = new(0, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }
        else
            transform.GetChild(0).localEulerAngles = Vector3.zero;//apply to wrapper and reset!
        UpdateGUIInfos();
    }

    /// <summary>
    /// Set localPosition.y to <see cref="GameManager.configLoader.config.humanHeight"/> if <see cref="humanMode"/> is enabled.
    /// </summary>
    public void UpdateHumanModeHeight()
    {
        if (humanMode)
            transform.localPosition = new(transform.localPosition.x, GameManager.instance.configHandler.config.HumanHeight, transform.localPosition.z);
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
        transform.localEulerAngles = new(_rot.x, _rot.y, 0);
    }

    ///<summary>
    /// Register given coordinates in targetPos and targetRot
    ///</summary>
    ///<param name="_pos">Position of the destination</param>
    ///<param name="_rot">Rotation of the destination</param>
    public void TranslateCamera(Vector3 _pos, Vector2 _rot)
    {
        targetPos.Add(_pos);
        targetRot.Add(new(_rot.x, _rot.y, 0));
    }

    ///<summary>
    /// Create a false targetRot with rot.x=999 and rot.y = time to wait.
    ///</summary>
    ///<param name="_time">The time to wait</param>
    public void WaitCamera(float _time)
    {
        targetRot.Add(new(999, _time, 0));
    }

    ///<summary>
    /// Move camera to targetPos[0] and targetRot[0].
    ///</summary>
    private void MoveToTarget()
    {
        if (humanMode)
            SwitchCameraMode(false);
        float speed = 10f * Time.deltaTime;
        transform.SetLocalPositionAndRotation(Vector3.MoveTowards(transform.localPosition, targetPos[0], speed),
                                                Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetRot[0]),
                                                                speed / Vector3.Distance(transform.localPosition, targetPos[0])));
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
    /// Control the camera from mouse & keyboard inputs
    ///</summary>
    public void InputControls()
    {
        if (humanMode)
            FPSControls();
        else
            FreeModeControls();
    }

    ///<summary>
    /// Controls for "AntMan" mode.
    ///</summary>
    private void FreeModeControls()
    {
        float moveSpeed = GameManager.instance.configHandler.config.MoveSpeed;
        float rotationSpeed = GameManager.instance.configHandler.config.RotationSpeed;
        if (GameManager.instance.focusMode)
            moveSpeed = Vector3.Distance(Camera.main.transform.position, GameManager.instance.GetFocused()[^1].transform.position);

        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
            else
                transform.Translate(Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime * Vector3.forward);
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0, Space.World);
            else
                transform.Translate(Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime * Vector3.right);
        }

        // Right click
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * rotationSpeed, 0, Space.World);
            transform.Rotate(-Input.GetAxis("Mouse Y") * Time.deltaTime * rotationSpeed, 0, 0);
        }
        // Scrollwheel click
        else if (Input.GetMouseButton(2))
            transform.Translate(-moveSpeed * Time.deltaTime * new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0));
        // Scrollwheel
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (Physics.Raycast(transform.position,
                                transform.GetChild(0).GetComponent<Camera>().ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit))
                transform.Translate(hit.distance * Input.GetAxis("Mouse ScrollWheel") * Vector3.forward);
            else
                transform.Translate(Input.GetAxis("Mouse ScrollWheel") * moveSpeed * Vector3.forward);
        }
    }

    ///<summary>
    /// Controls for "Human" mode.
    ///</summary>
    private void FPSControls()
    {
        float moveSpeed = GameManager.instance.configHandler.config.MoveSpeed;
        float rotationSpeed = GameManager.instance.configHandler.config.RotationSpeed;

        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.GetChild(0).Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
            else
                transform.Translate((moveSpeed / 2) * Input.GetAxis("Vertical") * Time.deltaTime * Vector3.forward);
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0);
            else
                transform.Translate((moveSpeed / 2) * Input.GetAxis("Horizontal") * Time.deltaTime * Vector3.right);
        }

        // Right click
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

        infosTMP.text = $"[{Utils.FloatToRefinedStr(transform.localPosition.x)},{Utils.FloatToRefinedStr(transform.localPosition.z)},{Utils.FloatToRefinedStr(transform.localPosition.y)}]@[{rotX:0},{rotY:0}]";
        if (!isReady)
            infosTMP.text += " (Waiting)";
    }

    ///<summary>
    /// Called when an OnFocusEvent is raised
    ///</summary>
    ///<param name="_e">The raised event</param>
    private void OnFocus(OnFocusEvent _e)
    {
        SCameraTrans target = GetRegisteredCameraTrans(_e.obj.name);
        if (string.IsNullOrEmpty(target.label))
        {
            RegisterTransform();
            MoveToObject(_e.obj.transform);
        }
        else
        {
            transform.position = target.pos;
            transform.eulerAngles = target.rot;
            labeledTransforms.RemoveAt(labeledTransforms.Count - 1);
        }
        UpdateGUIInfos();
    }

    ///<summary>
    /// Called when an OnUnFocusEvent is raised
    ///</summary>
    ///<param name="_e">The raised event</param>
    private void OnUnFocus(OnUnFocusEvent _e)
    {
        if (!GameManager.instance.focusMode)
        {
            transform.position = labeledTransforms[0].pos;
            transform.eulerAngles = labeledTransforms[0].rot;
            labeledTransforms.Clear();
        }
        UpdateGUIInfos();
    }

    ///<summary>
    /// Register camera position with a label in labeledTransforms.
    ///</summary>
    private void RegisterTransform()
    {
        SCameraTrans newTrans = new();
        if (labeledTransforms.Count == 0)
        {
            newTrans.label = "NoFocus";
            newTrans.pos = transform.position;
            newTrans.rot = transform.eulerAngles;
        }
        else
        {
            newTrans.label = GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 2].name;
            newTrans.pos = transform.position;
            newTrans.rot = transform.eulerAngles;
        }
        labeledTransforms.Add(newTrans);
    }

    ///<summary>
    /// Get a SCameraTrans by its label in labeledTranforms.
    ///</summary>
    ///<param name="_label">The label to look for</param>
    ///<returns>Wanted SCameraTrans or an empty one if not found</returns>
    private SCameraTrans GetRegisteredCameraTrans(string _label)
    {
        foreach (SCameraTrans ct in labeledTransforms)
        {
            Debug.Log($"{ct.label}/{_label}");
            if (ct.label.Equals(_label))
                return ct;
        }
        return new();
    }

    ///<summary>
    /// Move the camera in front of given _target.
    ///</summary>
    ///<param name="_target">The object to look at</param>
    public void MoveToObject(Transform _target)
    {
        float offset = 3f;
        OgreeObject obj = _target.GetComponent<OgreeObject>();

        switch (obj.category)
        {
            case Category.Rack:
                transform.position = _target.GetChild(0).position;
                offset = ((JArray)obj.attributes["size"]).ToVector2().y / 45;
                transform.eulerAngles = _target.eulerAngles + new Vector3(0, 180, 0);
                break;
            case Category.Device:
                transform.position = _target.GetChild(0).position;
                offset = ((JArray)obj.attributes["size"]).ToVector2().y / 450;
                transform.eulerAngles = _target.eulerAngles + new Vector3(0, 180, 0);
                break;
            default:
                transform.position = _target.position + new Vector3(0, 25, -10);
                transform.eulerAngles = new(45, 0, 0);
                break;
        }
        switch ((int)_target.eulerAngles.y)
        {
            case 0:
                transform.position += new Vector3(0, 0, offset);
                break;
            case 90:
                transform.position += new Vector3(offset, 0, 0);
                break;
            case 180:
                transform.position += new Vector3(0, 0, -offset);
                break;
            case 270:
                transform.position += new Vector3(-offset, 0, 0);
                break;
            default:
                Debug.Log("default: " + _target.rotation.y);
                break;
        }
    }
}
