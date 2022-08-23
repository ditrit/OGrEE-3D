using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inputs : MonoBehaviour
{
    private float doubleClickTimeLimit = 0.25f;
    private bool coroutineAllowed = true;
    private int clickCount = 0;
    private float clickTime;
    private float delayUntilDrag = 0.2f;
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

    [SerializeField] private GameObject savedObjectThatWeHover;

    private void Update()
    {
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && GameManager.gm.currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(GameManager.gm.currentItems[0].GetComponent<OgreeObject>())));
#endif
        if (!isDragging && !isRotating && !isScaling)
            target = Utils.RaycastFromCameraToMouse()?.transform;

        if (GameManager.gm.editMode)
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
                if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count -1] == target.parent.gameObject)
                {
                    isDragging = true;
                    screenSpace = Camera.main.WorldToScreenPoint(target.position);
                    offsetPos = target.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
                }
            }

            if (clickCount == 1 && target && target.tag == "UHelper")
            {
                ClickOnU();
                clickCount = 0;
            }

            if (!isDragging && clickCount == 1 && coroutineAllowed)
                StartCoroutine(DoubleClickDetection(Time.time));
        }

        if (isDragging)
            DragObject();
        if (isRotating)
            RotateObject();
        if (isScaling)
            ScaleObject();
        MouseHover();
    }

    ///<summary>
    /// Check if simple or double click and call corresponding method.
    ///</summary>
    ///<param name="_firstClickTime">The time of the first click</param>
    private IEnumerator DoubleClickDetection(float _firstClickTime)
    {
        coroutineAllowed = false;
        while (Time.time < _firstClickTime + doubleClickTimeLimit)
        {
            if (clickCount == 2 && !GameManager.gm.editMode)
            {
                ClickFocus();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (clickCount == 1 && !GameManager.gm.editMode)
            ClickSelect();
        clickCount = 0;
        coroutineAllowed = true;

    }

    ///<summary>
    /// Method called when single click on a gameObject.
    ///</summary>
    private async void ClickSelect()
    {
        if (target && target.tag == "Selectable")
        {
            bool canSelect = false;
            if (GameManager.gm.focus.Count > 0)
                canSelect = GameManager.gm.IsInFocus(target.gameObject);
            else
                canSelect = true;

            if (canSelect)
            {
                if (Input.GetKey(KeyCode.LeftControl) && GameManager.gm.currentItems.Count > 0)
                    await GameManager.gm.UpdateCurrentItems(target.gameObject);
                else
                    await GameManager.gm.SetCurrentItem(target.gameObject);
            }
        }
        else if (GameManager.gm.focus.Count > 0)
            await GameManager.gm.SetCurrentItem(GameManager.gm.focus[GameManager.gm.focus.Count - 1]);
        else
            await GameManager.gm.SetCurrentItem(null);
    }

    ///<summary>
    /// Method called when single click on a gameObject.
    ///</summary>
    private async void ClickFocus()
    {
        if (target && target.tag == "Selectable" && target.GetComponent<OObject>())
        {
            if (target.GetComponent<Group>())
                target.GetComponent<Group>().ToggleContent("true");
            else
            {
                await GameManager.gm.SetCurrentItem(target.gameObject);
                await GameManager.gm.FocusItem(target.gameObject);
            }
        }
        else if (GameManager.gm.focus.Count > 0)
            await GameManager.gm.UnfocusItem();
    }

    ///<summary>
    /// Method called when click on a U helper.
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
                    EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });

                savedObjectThatWeHover = target.gameObject;
                EventManager.Instance.Raise(new OnMouseHoverEvent { obj = target.gameObject });
            }
        }
        else
        {
            if (savedObjectThatWeHover)
                EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });
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
        {
            target.position = new Vector3(target.position.x, curPosition.y, target.position.z);
        }
        else if (target.GetComponent<OgreeObject>().category == "device")
        {
            target.position = curPosition;
        }
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
}
