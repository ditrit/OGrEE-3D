using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inputs : MonoBehaviour
{
    private bool coroutineAllowed = true;
    private int clickCount = 0;
    private float clickTime;
    private float authorizedDrag = 100;
    private CameraControl camControl;
    [SerializeField] private bool camControlAllowed = true;
    [SerializeField] private Transform target;
    [SerializeField] private CoordModeController coordModeController;

    // Drag
    private bool isDraggingObj = false;
    private Vector3 screenSpace;
    private Vector3 offsetPos;

    // Rotate
    private bool isRotatingObj = false;
    private Vector3 mouseRef;

    // Scale
    private bool isScalingObj = false;

    private GameObject savedObjectThatWeHover;
    private Vector3 savedMousePos;
    public bool lockMouseInteract = false;
    private Vector3 previousClic;
    public bool twoClics = false;
    private void Start()
    {
        camControl = Camera.main.transform.parent.GetComponent<CameraControl>();
    }

    private void Update()
    {
        if (!Application.isFocused)
            return;
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && GameManager.instance.selectMode)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>())));
#endif
        if (camControlAllowed && !EventSystem.current.IsPointerOverGameObject())
            camControl.InputControls();

        if (!isDraggingObj && !isRotatingObj && !isScalingObj)
            target = Utils.RaycastFromCameraToMouse()?.transform;

        if (GameManager.instance.getCoordsMode && !lockMouseInteract)
            GetCoordsModeControls();
        else if (!GameManager.instance.getCoordsMode)
        {
            if (!lockMouseInteract)
                MouseControls();
            MouseHover();
        }

        RightClickMenu();
    }

    ///<summary>
    /// Mouse controls in GetCoordsMode
    ///</summary>
    private void GetCoordsModeControls()
    {
        coordModeController.RescaleTexts();
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit);
        if (hit.collider && hit.collider.transform.parent == GameManager.instance.GetSelected()[0].transform)
            coordModeController.MoveCSToHit(hit);

        if (Input.GetMouseButtonDown(0))
            AppendDistFromClick();
    }

    ///<summary>
    /// Mouse controls "classic" modes
    ///</summary>
    private void MouseControls()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            isDraggingObj = false;
            isRotatingObj = false;
            isScalingObj = false;
            return;
        }

        if (GameManager.instance.editMode)
        {
            if (Input.GetMouseButtonDown(0) && target)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    isRotatingObj = true;
                    mouseRef = Input.mousePosition;
                }
                else if (target.GetComponent<Rack>())
                {
                    isDraggingObj = true;
                    screenSpace = Camera.main.WorldToScreenPoint(target.position);
                    offsetPos = target.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                }
                else
                    isScalingObj = true;
            }
            else if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.LeftControl))
            {
                isDraggingObj = false;
                isRotatingObj = false;
                isScalingObj = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && clickTime == 0)
            {
                savedMousePos = Input.mousePosition;
                clickTime = Time.time;
            }
            else if (Input.GetMouseButton(0) && target && !isDraggingObj && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                && GameManager.instance.focusMode && GameManager.instance.GetFocused()[^1] == target.parent.gameObject)
            {
                isDraggingObj = true;
                screenSpace = Camera.main.WorldToScreenPoint(target.position);
                offsetPos = target.position - Camera.main.ScreenToWorldPoint(new(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!isDraggingObj)
                    clickCount++;
                isDraggingObj = false;
                clickTime = 0;
                if (clickCount == 1 && coroutineAllowed)
                    StartCoroutine(DoubleClickDetection(Time.realtimeSinceStartup));
            }
        }

        if (isDraggingObj)
            DragObject();
        if (isRotatingObj)
            RotateObject();
        if (isScalingObj)
            ScaleObject();
    }


    ///<summary>
    /// Right click handling for display / hide right click menu
    ///</summary>
    private void RightClickMenu()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(1))
        {
            savedMousePos = Input.mousePosition;
            if (!GameManager.instance.editMode)
            {
                foreach (GameObject go in GameManager.instance.GetSelected())
                {
                    if (go.GetComponent<OgreeObject>() is Item && go.GetComponent<ObjectDisplayController>().Shown)
                        go.transform.GetChild(0).GetComponent<Collider>().enabled = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (Mathf.Abs(Vector3.Distance(savedMousePos, Input.mousePosition)) < authorizedDrag)
            {
                UiManager.instance.menuTarget = target?.gameObject;
                EventManager.instance.Raise(new RightClickEvent());
                UiManager.instance.DisplayRightClickMenu();
                lockMouseInteract = true;
            }
            if (!GameManager.instance.editMode)
            {
                foreach (GameObject go in GameManager.instance.GetSelected())
                {
                    if (go.GetComponent<OgreeObject>() is Item)
                        go.transform.GetChild(0).GetComponent<Collider>().enabled = false;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            UiManager.instance.HideRightClickMenu();
            lockMouseInteract = false;
        }
    }

    ///<summary>
    /// Check if simple or double click and call corresponding method.
    ///</summary>
    ///<param name="_firstClickTime">The time of the first click</param>
    private IEnumerator DoubleClickDetection(float _firstClickTime)
    {
        Transform savedTarget = target;
        coroutineAllowed = false;
        while (Time.realtimeSinceStartup < _firstClickTime + GameManager.instance.configHandler.config.DoubleClickDelay)
        {
            if (clickCount == 2 && !GameManager.instance.editMode && Mathf.Abs(Vector3.Distance(savedMousePos, Input.mousePosition)) < authorizedDrag)
            {
                if (savedTarget == target)
                    DoubleClick();
                else
                    SingleClick(savedTarget);
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (clickCount == 1 && !GameManager.instance.editMode && Mathf.Abs(Vector3.Distance(savedMousePos, Input.mousePosition)) < authorizedDrag)
            SingleClick(savedTarget);
        clickCount = 0;
        coroutineAllowed = true;
    }

    ///<summary>
    /// Method called when single clicking on a gameObject.
    ///</summary>
    ///<param name="_target">The single click target (can be null)</param>
    private async void SingleClick(Transform _target)
    {
        if (_target)
        {
            if (_target.CompareTag("Selectable"))
            {
                bool canSelect = true;
                if (GameManager.instance.focusMode)
                    canSelect = GameManager.instance.IsInFocus(_target.gameObject);

                if (canSelect)
                {
                    if (Input.GetKey(KeyCode.LeftControl) && GameManager.instance.selectMode)
                        await GameManager.instance.UpdateCurrentItems(_target.gameObject);
                    else
                        await GameManager.instance.SetCurrentItem(_target.gameObject);
                }
            }
            else if (_target.CompareTag("UHelper"))
                ClickOnU(_target);
        }
        else if (GameManager.instance.GetFocused().Count > 0)
            await GameManager.instance.SetCurrentItem(GameManager.instance.GetFocused()[^1]);
        else
            await GameManager.instance.SetCurrentItem(null);
    }

    ///<summary>
    /// Method called when double clicking on a gameObject.
    ///</summary>
    private async void DoubleClick()
    {
        if (target && target.CompareTag("Selectable") && target.GetComponent<Item>())
        {
            if (target.GetComponent<Group>())
                target.GetComponent<Group>().ToggleContent(true);
            else
            {
                await GameManager.instance.SetCurrentItem(target.gameObject);
                await GameManager.instance.FocusItem(target.gameObject);
            }
        }
        else if (GameManager.instance.focusMode)
            await GameManager.instance.UnfocusItem();
    }

    ///<summary>
    /// Method called when clicking on a U helper.
    ///</summary>
    private void ClickOnU(Transform _target)
    {
        Rack rack = _target.GetComponentInParent<Rack>();
        rack.GetComponent<GridCell>().ToggleGrid(_target);
    }

    ///<summary>
    /// Raise OnMouseHoverEvent or OnMouseUnHoverEvent depending of what's under the mouse.
    ///</summary>
    private void MouseHover()
    {
        if (target)
        {
            if (target.gameObject != savedObjectThatWeHover)
            {
                if (savedObjectThatWeHover)
                    EventManager.instance.Raise(new OnMouseUnHoverEvent(savedObjectThatWeHover));

                savedObjectThatWeHover = target.gameObject;
                EventManager.instance.Raise(new OnMouseHoverEvent(target.gameObject));
            }
        }
        else
        {
            if (savedObjectThatWeHover)
                EventManager.instance.Raise(new OnMouseUnHoverEvent(savedObjectThatWeHover));
            savedObjectThatWeHover = null;
        }
    }

    ///<summary>
    /// Drag an OgreeObject using the mouse position with specific rules depending of the object's category.
    ///</summary>
    private void DragObject()
    {
        Vector3 curScreenSpace = new(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offsetPos;
        if (target.GetComponent<Rack>())
            target.position = new(target.position.x, curPosition.y, target.position.z);
        else if (target.GetComponent<Device>())
            target.position = curPosition;
    }

    ///<summary>
    /// Rotate an OgreeObject using the mouse position with specific rules depending of the object's category.
    ///</summary>
    private void RotateObject()
    {
        float sensitivity = 0.2f;

        Vector3 mouseOffset = Input.mousePosition - mouseRef;
        Vector3 rotation = Vector3.zero;
        if (target.GetComponent<Rack>())
        {
            rotation.y = -(mouseOffset.x + mouseOffset.y) * sensitivity;
            target.Rotate(rotation);
        }
        else if (target.GetComponent<Device>())
        {
            rotation.y = -mouseOffset.x * sensitivity;
            rotation.x = -mouseOffset.y * sensitivity;
            target.eulerAngles += rotation;
        }
        mouseRef = Input.mousePosition;
    }

    ///<summary>
    /// Rescale an OgreeObject using the mouse position.
    ///</summary>
    private void ScaleObject()
    {
        float sensitivity = 0.2f;
        float scale = target.localScale.x;
        scale += Input.GetAxis("Mouse Y") * sensitivity;
        scale = Mathf.Clamp(scale, 0.8f, 2f);
        target.localScale = scale * Vector3.one;
    }

    ///<summary>
    /// Display the distance between the origin of the selected objet and the hit point in the logger
    ///</summary>
    private void AppendDistFromClick()
    {
        Room targetRoom = GameManager.instance.GetSelected()[0].GetComponent<Room>();
        Vector3 localPos = targetRoom.transform.InverseTransformPoint(UiManager.instance.coordSystem.transform.position);

        List<string> distFromLocalCS = new()
        {
            // meters values
            Utils.FloatToRefinedStr(localPos.x),
            Utils.FloatToRefinedStr(localPos.z),
            // tiles values
            Utils.FloatToRefinedStr(localPos.x / UnitValue.Tile),
            Utils.FloatToRefinedStr(localPos.z / UnitValue.Tile)
        };
        GameManager.instance.AppendLogLine($"Distance from {targetRoom.name}'s localCS: [{distFromLocalCS[0]},{distFromLocalCS[1]}] m / [{distFromLocalCS[2]},{distFromLocalCS[3]}] t", ELogTarget.logger, ELogtype.info);

        if (UiManager.instance.previousClick)
            GameManager.instance.AppendLogLine($"Distance between last two clicks: {Utils.FloatToRefinedStr(Vector3.Distance(localPos, previousClic))} m", ELogTarget.logger, ELogtype.info);
        UiManager.instance.previousClick = true;
        previousClic = localPos;

        if (targetRoom.isSquare)
        {
            List<string> distFromOri = new();
            float minusX;
            float minusY;
            switch (targetRoom.attributes["axisOrientation"])
            {
                case AxisOrientation.Default:
                    return;
                case AxisOrientation.XMinus:
                    minusX = targetRoom.technicalZone.localScale.x * 10 - localPos.x;
                    // meters values
                    distFromOri.Add(Utils.FloatToRefinedStr(minusX));
                    distFromOri.Add(distFromLocalCS[1]);
                    // tiles values
                    distFromOri.Add(Utils.FloatToRefinedStr(minusX / UnitValue.Tile));
                    distFromOri.Add(distFromLocalCS[3]);
                    break;
                case AxisOrientation.YMinus:
                    minusY = targetRoom.technicalZone.localScale.z * 10 - localPos.z;
                    // meters values
                    distFromOri.Add(distFromLocalCS[0]);
                    distFromOri.Add(Utils.FloatToRefinedStr(minusY));
                    // tiles values
                    distFromOri.Add(distFromLocalCS[2]);
                    distFromOri.Add(Utils.FloatToRefinedStr(minusY / UnitValue.Tile));
                    break;
                case AxisOrientation.BothMinus:
                    minusX = targetRoom.technicalZone.localScale.x * 10 - localPos.x;
                    minusY = targetRoom.technicalZone.localScale.z * 10 - localPos.z;
                    // meters values
                    distFromOri.Add(Utils.FloatToRefinedStr(minusX));
                    distFromOri.Add(Utils.FloatToRefinedStr(minusY));
                    // tiles values
                    distFromOri.Add(Utils.FloatToRefinedStr(minusX / UnitValue.Tile));
                    distFromOri.Add(Utils.FloatToRefinedStr(minusY / UnitValue.Tile));
                    break;
            }
            GameManager.instance.AppendLogLine($"Distance from {targetRoom.name}'s tiles origin: [{distFromOri[0]},{distFromOri[1]}] m / [{distFromOri[2]},{distFromOri[3]}] t", ELogTarget.logger, ELogtype.info);
        }
    }

    ///<summary>
    /// Toggle value of <see cref="camControlAllowed"/>.
    ///</summary>
    ///<param name="_value">The value to assign</param>
    public void ToggleCameraControls(bool _value)
    {
        camControlAllowed = _value;
    }
}
