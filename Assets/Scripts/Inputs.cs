using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inputs : MonoBehaviour
{
    private float doubleClickTimeLimit = 0.25f;
    private bool coroutineAllowed = true;
    private int clickCount = 0;

    [SerializeField] private GameObject savedObjectThatWeHover;

    private void Update()
    {
#if !PROD
        if (Input.GetKeyDown(KeyCode.Insert) && currentItems.Count > 0)
            Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(new SApiObject(currentItems[0].GetComponent<OgreeObject>())));
#endif

        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
            clickCount++;

        if (clickCount == 1 && coroutineAllowed)
            StartCoroutine(DoubleClickDetection(Time.time));

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
            if (clickCount == 2)
            {
                DoubleClick();
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        if (clickCount == 1)
            SingleClick();
        clickCount = 0;
        coroutineAllowed = true;

    }

    ///<summary>
    /// Method called when single click on a gameObject.
    ///</summary>
    private async void SingleClick()
    {
        GameObject objectHit = Utils.RaycastFromCameraToMouse();
        if (objectHit && objectHit.tag == "Selectable")
        {
            bool canSelect = false;
            if (GameManager.gm.focus.Count > 0)
                canSelect = GameManager.gm.IsInFocus(objectHit);
            else
                canSelect = true;

            if (canSelect)
            {
                if (Input.GetKey(KeyCode.LeftControl) && GameManager.gm.currentItems.Count > 0)
                    await GameManager.gm.UpdateCurrentItems(objectHit);
                else
                    await GameManager.gm.SetCurrentItem(objectHit);
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
    private async void DoubleClick()
    {
        GameObject objectHit = Utils.RaycastFromCameraToMouse();
        if (objectHit && objectHit.tag == "Selectable" && objectHit.GetComponent<OObject>())
        {
            if (objectHit.GetComponent<Group>())
                objectHit.GetComponent<Group>().ToggleContent("true");
            else
            {
                await GameManager.gm.SetCurrentItem(objectHit);
                await GameManager.gm.FocusItem(objectHit);
            }
        }
        else if (GameManager.gm.focus.Count > 0)
            await GameManager.gm.UnfocusItem();
    }

    ///
    private void MouseHover()
    {
        GameObject objectHit = Utils.RaycastFromCameraToMouse();
        if (objectHit)
        {
            if (!objectHit.Equals(savedObjectThatWeHover))
            {
                if (savedObjectThatWeHover)
                    EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });

                savedObjectThatWeHover = objectHit;
                EventManager.Instance.Raise(new OnMouseHoverEvent { obj = objectHit });
            }
        }
        else
        {
            if (savedObjectThatWeHover)
                EventManager.Instance.Raise(new OnMouseUnHoverEvent { obj = savedObjectThatWeHover });
            savedObjectThatWeHover = null;
        }
    }
}
