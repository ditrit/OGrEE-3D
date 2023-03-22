using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] private TextMeshProUGUI infosTMP = null;

    [Header("Parameters")]
    [Range(5, 20)]
    public float defaultMoveSpeed = 15;
    [Range(20, 100)]
    public float rotationSpeed = 50;
    [Range(1.60f, 1.90f)]
    public float humanHeight = 1.62f;
    [SerializeField] private bool humanMode = false;

    [SerializeField] private List<Vector3> targetPos = new List<Vector3>();
    [SerializeField] private List<Vector3> targetRot = new List<Vector3>();
    [SerializeField] private List<SCameraTrans> labeledTransforms = new List<SCameraTrans>();
    private bool isReady = true;

    private void Start()
    {
        EventManager.instance.AddListener<OnFocusEvent>(OnFocus);
        EventManager.instance.AddListener<OnUnFocusEvent>(OnUnFocus);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<OnFocusEvent>(OnFocus);
        EventManager.instance.RemoveListener<OnUnFocusEvent>(OnUnFocus);
    }

    private void Update()
    {
        // This check may be deleted when the build-in CLI will be removed
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
        float moveSpeed;
        if (GameManager.instance.focusMode)
        {
            moveSpeed = Vector3.Distance(Camera.main.transform.position,
                                                GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1].transform.position);
        }
        else
            moveSpeed = defaultMoveSpeed;

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
        if (Input.GetAxis("Vertical") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.GetChild(0).Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
            else
                transform.Translate((defaultMoveSpeed / 2) * Input.GetAxis("Vertical") * Time.deltaTime * Vector3.forward);
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0);
            else
                transform.Translate((defaultMoveSpeed / 2) * Input.GetAxis("Horizontal") * Time.deltaTime * Vector3.right);
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

        infosTMP.text = $"Camera p/a: [{transform.localPosition.x:0.##};{transform.localPosition.z:0.##};{transform.localPosition.y:0.##}] / [{rotX:0};{rotY:0}]";
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
        SCameraTrans newTrans = new SCameraTrans();
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
        return new SCameraTrans();
    }

    ///<summary>
    /// Move the camera in front of given _target.
    ///</summary>
    ///<param name="_target">The object to look at</param>
    public void MoveToObject(Transform _target)
    {
        transform.position = _target.position;
        float offset = 3f;
        OObject obj = _target.GetComponent<OObject>();
        if (obj && obj.category == "rack")
            offset = JsonUtility.FromJson<Vector2>(obj.attributes["size"]).y / 45;
        else if (obj && obj.category == "device")
            offset = JsonUtility.FromJson<Vector2>(obj.attributes["size"]).y / 450;
        switch ((int)_target.eulerAngles.y)
        {
            case 0:
                // Debug.Log("0");
                transform.position += new Vector3(0, 0, offset);
                transform.eulerAngles = new Vector3(0, 180, 0);
                break;
            case 90:
                // Debug.Log("90");
                transform.position += new Vector3(offset, 0, 0);
                transform.eulerAngles = new Vector3(0, 270, 0);
                break;
            case 180:
                // Debug.Log("180");
                transform.position += new Vector3(0, 0, -offset);
                transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case 270:
                // Debug.Log("270");
                transform.position += new Vector3(-offset, 0, 0);
                transform.eulerAngles = new Vector3(0, 90, 0);
                break;
            default:
                Debug.Log("default: " + _target.rotation.y);
                break;
        }
    }
}
