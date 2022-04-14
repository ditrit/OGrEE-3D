using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private bool focused = false;
    private GameObject selectedObject = null;
    private GameObject focusedObject = null;

    [Header("Butons")]
    [SerializeField] private GameObject buttonResetPosition;
    [SerializeField] private GameObject buttonDeselect;
    [SerializeField] private GameObject buttonToggleFocus;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonResetPosition.activeInHierarchy && selectedObject != null)
        {
            CheckChildrenPositions();
        }
    }

    ///<summary>
    /// Reset all children's positions to their original value
    ///</summary>
    public void ResetAllPositions()
    {
        for (int i = 0; i < selectedObject.transform.childCount; i++)
        {
            OgreeObject ogree = selectedObject.transform.GetChild(i).GetComponent<OgreeObject>();
            if (ogree == null)
            {
                continue;
            }
            ogree.ResetPosition();
        }
    }

    ///<summary>
    /// Ensure that devices can not be moved in the wrong direction
    ///</summary>
    private void CheckChildrenPositions()
    {
        for (int i = 0; i < selectedObject.transform.childCount; i++)
        {
            Transform ithChild = selectedObject.transform.GetChild(i);
            OgreeObject ogree = ithChild.GetComponent<OgreeObject>();
            if (ogree == null)
            {
                continue;
            }
            if (ithChild.localPosition.z < ogree.originalLocalPosition.z)
            {
                ogree.ResetPosition();
            }
        }
    }


    ///<summary>
    /// When called set the buton active, set the selected object as its parent and initialise it
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        selectedObject = _e.obj;
        Vector3 parentSize = selectedObject.transform.GetChild(0).lossyScale;

        //Placing buttonResetPosition
        buttonResetPosition.transform.position = selectedObject.transform.GetChild(0).position;
        buttonResetPosition.transform.localRotation = Quaternion.Euler(0, -90, 0);
        buttonResetPosition.transform.position += new Vector3(parentSize.z + 0.06f, parentSize.y + 0.06f, parentSize.x - 0.06f) / 2;
        buttonResetPosition.SetActive(true);

        //Placing buttonDeselect
        buttonDeselect.transform.position = selectedObject.transform.GetChild(0).position;
        buttonDeselect.transform.localRotation = Quaternion.Euler(0, -90, 0);
        buttonDeselect.transform.position += new Vector3(parentSize.z + 0.06f, parentSize.y + 0.06f, parentSize.x - 0.12f) / 2;
        buttonDeselect.SetActive(true);

        //Placing buttonToggleFocus
        buttonToggleFocus.transform.position = selectedObject.transform.GetChild(0).position;
        buttonToggleFocus.transform.localRotation = Quaternion.Euler(0, -90, 0);
        buttonToggleFocus.transform.position += new Vector3(parentSize.z + 0.06f, parentSize.y + 0.06f, parentSize.x - 0.18f) / 2;
        buttonToggleFocus.SetActive(true);


    }

    ///<summary>
    /// When called set the buton inactive
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        buttonResetPosition.SetActive(false);
        buttonDeselect.SetActive(false);
        buttonToggleFocus.SetActive(false);
        selectedObject = null;
    }
    private void OnFocusItem(OnFocusEvent _e)
    {
        focused = true;
        focusedObject = _e.obj;
    }
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        focused = false;
        focusedObject = null;
    }

    ///<summary>
    /// I should move this (WIP)
    ///</summary>
    public void ButonFocus()
    {
        if (focused && focusedObject == selectedObject)
        {
            GameManager.gm.UnfocusItem();
            GameManager.gm.SetCurrentItem(selectedObject);

        }
        else
        {
            GameManager.gm.FocusItem(selectedObject);
        }
    }
    ///<summary>
    /// I should move this (WIP)
    ///</summary>
    public void ButonDeselect()
    {
        if (selectedObject == null || (focused && focusedObject == selectedObject))
        {
            return;
        }
        if (selectedObject.transform.parent.GetComponent<OObject>() != null)
        {
            GameManager.gm.SetCurrentItem(selectedObject.transform.parent.gameObject);
        }
        else
        {
            GameManager.gm.SetCurrentItem(null);
        }
    }
}
