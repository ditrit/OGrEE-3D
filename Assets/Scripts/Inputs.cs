using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inputs : MonoBehaviour
{
    private readonly float doubleClickTimeLimit = 0.25f;
    private bool coroutineAllowed = true;
    private int clickCount = 0;
    private float clickTime;
    private readonly float delayUntilDrag = 0.2f;
    [SerializeField] Transform target;

    // Drag
    private bool isDragging = false;
    private Vector3 screenSpace;
    private Vector3 offsetPos;

    // Rotate
    private bool isRotating = false;
    private Vector3 mouseRef;

    // Scale
    private bool isScaling = false;

    private GameObject savedObjectThatWeHover;

    private void Update()
    {
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && GameManager.instance.currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(GameManager.instance.currentItems[0].GetComponent<OgreeObject>())));
#endif
        if (GameManager.instance.getCoordsMode)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit);
                if (hit.collider && hit.collider.transform.parent == GameManager.instance.GetSelected()[0].transform)
                {
                    UiManager.instance.MoveCSToHit(hit);
                    if (Input.GetMouseButtonDown(0))
                        AppendDistFromClick(hit);
                }
            }
        }
        else
        {
            if (!isDragging && !isRotating && !isScaling)
                target = Utils.RaycastFromCameraToMouse()?.transform;

            if (GameManager.instance.editMode)
            {
                if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
                {
                    if (target)
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            isRotating = true;
                            mouseRef = Input.mousePosition;
                        }
                        else if (target.GetComponent<OgreeObject>().category == "rack")
                        {
                            isDragging = true;
                            screenSpace = Camera.main.WorldToScreenPoint(target.position);
                            offsetPos = target.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                        }
                        else
                            isScaling = true;
                    }
                }
                if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.LeftControl))
                {
                    isDragging = false;
                    isRotating = false;
                    isScaling = false;
                }
            }
            else
            {
                if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0) && clickTime == 0)
                    clickTime = Time.time;

                if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
                {
                    if (!isDragging)
                        clickCount++;
                    isDragging = false;
                    clickTime = 0;
                }

                if (target && !isDragging && clickTime != 0 && Time.time > clickTime + delayUntilDrag)
                {
                    if (GameManager.instance.focusMode && GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1] == target.parent.gameObject)
                    {
                        isDragging = true;
                        screenSpace = Camera.main.WorldToScreenPoint(target.position);
                        offsetPos = target.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                    }
                }

                if (clickCount == 1 && target && target.CompareTag("UHelper"))
                {
                    ClickOnU();
                    clickCount = 0;
                }

                if (!isDragging && clickCount == 1 && coroutineAllowed)
                    StartCoroutine(DoubleClickDetection(Time.realtimeSinceStartup));
            }

            if (isDragging)
                DragObject();
            if (isRotating)
                RotateObject();
            if (isScaling)
                ScaleObject();
            MouseHover();
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
        while (Time.realtimeSinceStartup < _firstClickTime + doubleClickTimeLimit)
        {
            if (clickCount == 2 && !GameManager.instance.editMode)
            {
                ClickFocus();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (clickCount == 1 && !GameManager.instance.editMode)
            ClickSelect(savedTarget);
        clickCount = 0;
        coroutineAllowed = true;
    }

    ///<summary>
    /// Method called when single clicking on a gameObject.
    ///</summary>
    ///<param name="_target">the object clicked on</param>
    private async void ClickSelect(Transform _target)
    {
        if (_target && _target.CompareTag("Selectable"))
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
        else if (GameManager.instance.GetFocused().Count > 0)
            await GameManager.instance.SetCurrentItem(GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1]);
        else
            await GameManager.instance.SetCurrentItem(null);
    }

    ///<summary>
    /// Method called when double clicking on a gameObject.
    ///</summary>
    private async void ClickFocus()
    {
        if (target && target.CompareTag("Selectable") && target.GetComponent<OObject>())
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
    private void ClickOnU()
    {
        Transform rack = target.parent.parent;
        rack.GetComponent<GridCell>().ToggleGrid(target.position.y, target.name);
    }

    ///<summary>
    /// Raise OnMouseHoverEvent or OnMouseUnHoverEvent depending of what's under the mouse.
    ///</summary>
    private void MouseHover()
    {
        if (target)
        {
            if (!target.Equals(savedObjectThatWeHover))
            {
                if (savedObjectThatWeHover)
                    EventManager.instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });

                savedObjectThatWeHover = target.gameObject;
                EventManager.instance.Raise(new OnMouseHoverEvent { obj = target.gameObject });
            }
        }
        else
        {
            if (savedObjectThatWeHover)
                EventManager.instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });
            savedObjectThatWeHover = null;
        }
    }

    ///<summary>
    /// Drag an OgreeObject using the mouse position with specific rules depending of the object's category.
    ///</summary>
    private void DragObject()
    {
        Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offsetPos;
        if (target.GetComponent<OgreeObject>().category == "rack")
            target.position = new Vector3(target.position.x, curPosition.y, target.position.z);
        else if (target.GetComponent<OgreeObject>().category == "device")
            target.position = curPosition;
    }

    ///<summary>
    /// Rotate an OgreeObject using the mouse position with specific rules depending of the object's category.
    ///</summary>
    private void RotateObject()
    {
        float sensitivity = 0.2f;

        Vector3 mouseOffset = (Input.mousePosition - mouseRef);
        Vector3 rotation = Vector3.zero;
        if (target.GetComponent<OgreeObject>().category == "rack")
        {
            rotation.y = -(mouseOffset.x + mouseOffset.y) * sensitivity;
            target.Rotate(rotation);
        }
        else if (target.GetComponent<OgreeObject>().category == "device")
        {
            rotation.y = -(mouseOffset.x) * sensitivity;
            rotation.x = -(mouseOffset.y) * sensitivity;
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
    ///<param name="_clickPos">The hit data</param>
    private void AppendDistFromClick(RaycastHit _hit)
    {
        Transform hitObject = _hit.collider.transform.parent;
        string objName = hitObject.GetComponent<OgreeObject>().hierarchyName;
        Vector3 localHit = hitObject.InverseTransformPoint(_hit.point);
        GameManager.instance.AppendLogLine($"Distance from {objName}'s origin: [{Utils.FloatToRefinedStr(localHit.x)},{Utils.FloatToRefinedStr(localHit.z)}]", ELogTarget.logger, ELogtype.info);
    }
}
