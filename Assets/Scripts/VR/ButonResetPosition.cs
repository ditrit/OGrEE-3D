using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButonResetPosition : MonoBehaviour
{
    private bool focused = false;
    private GameObject selectedObject;
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
        if (gameObject.activeInHierarchy)
        {
            CheckChildrenPositions();
        }
    }

    ///<summary>
    /// Reset all children's positions to their original value
    ///</summary>
    public void ResetAllPositions()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i).GetComponent<OgreeObject>() == null)
            {
                continue;
            }
            transform.parent.GetChild(i).GetComponent<OgreeObject>().ResetPosition();
        }
    }

    ///<summary>
    /// Ensure that devices can not be moved in the wrong direction
    ///</summary>
    private void CheckChildrenPositions()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform ithChild = transform.parent.GetChild(i);
            OgreeObject ogree = ithChild.GetComponent<OgreeObject>();
            if (ogree == null)
            {
                continue;
            }
            if (ithChild.localPosition.x < ogree.originalLocalPosition.x)
            {
                ogree.ResetPosition();
            }
        }
    }

    ///<summary>
    /// Set the ResetPosition buton to the correct position and rotation on selection
    ///</summary>
    ///<param name="_position">the position of the buton </param>
    ///<param name="_rotation">the rotation of the buton </param>
    private void InitButton(Vector3 _position, Quaternion _rotation)
    {
        gameObject.transform.localPosition = _position;
        gameObject.transform.localRotation = _rotation;
    }

    ///<summary>
    /// When called set the buton active, set the selected object as its parent and initialise it
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        gameObject.SetActive(true);
        gameObject.transform.parent = _e.obj.transform;
        InitButton(new Vector3(_e.obj.transform.localScale.x / 2, _e.obj.transform.localScale.y / 2, _e.obj.transform.localScale.z / 2), Quaternion.Euler(0, -180, 0));
        selectedObject = _e.obj;

    }

    ///<summary>
    /// When called set the buton inactive
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        gameObject.SetActive(false);
    }
    private void OnFocusItem(OnFocusEvent _e)
    {
        focused = true;
    }
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        focused = false;
    }

    ///<summary>
    /// I should move this (WIP)
    ///</summary>
    public void ButonFocus()
    {
        if (focused)
        {
            GameManager.gm.UnfocusItem();
        } else
        {
            GameManager.gm.FocusItem(selectedObject);
        }
    }
}
