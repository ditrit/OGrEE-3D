using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private bool focused = false;
    private bool editMode = false;
    private GameObject selectedObject = null;
    private GameObject focusedObject = null;
    private bool front = true;

    [Header("Butons")]
    [SerializeField] private GameObject buttonWrapper;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ChangeOrientationEvent>(OnChangeOrientation);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);
        buttonWrapper.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    ///<summary>
    /// Reset all children's positions to their original value
    ///</summary>
    ///<param name="_obj">The parent</param>
    public void ResetAllPositions(GameObject _obj)
    {
        for (int i = 0; i < _obj.transform.childCount; i++)
        {
            OgreeObject ogree = _obj.transform.GetChild(i).GetComponent<OgreeObject>();
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
    /// When called set the button active, set the selected object as its parent and initialise it
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        selectedObject = _e.obj;
        buttonWrapper.transform.SetParent(selectedObject.transform);
        Vector3 parentSize = selectedObject.transform.GetChild(0).lossyScale;

        //Placing buttons
        buttonWrapper.transform.localPosition = Vector3.zero;
        buttonWrapper.transform.localRotation = Quaternion.Euler(0, front ? 0 : 180, 0);
        buttonWrapper.transform.localPosition += new Vector3(front ? -parentSize.x : parentSize.x, parentSize.y + 0.06f, front ? parentSize.z : -parentSize.z) / 2;
        buttonWrapper.transform.SetParent(null);
        buttonWrapper.SetActive(true);

    }

    ///<summary>
    /// When called set the button inactive
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        buttonWrapper.SetActive(false);
        selectedObject = null;
    }
    ///<summary>
    /// When called set the focus variables accordingly
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent _e)
    {
        focused = true;
        focusedObject = _e.obj;
    }

    ///<summary>
    /// When called set the focus variables accordingly
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        focused = false;
        focusedObject = null;
    }

    ///<summary>
    /// When called change the front boolean accordingly
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnChangeOrientation(ChangeOrientationEvent _e)
    {
        front = _e.front;
    }

    ///<summary>
    /// Focus the selected object or defocus according to the current state (selection, focus)
    ///</summary>
    public void ButonToggleFocus()
    {
        if (focused && focusedObject == selectedObject)
        {
            GameManager.gm.UnfocusItem();
            selectedObject.GetComponent<FocusHandler>().ToggleCollider(selectedObject, true);

        }
        else
        {
            GameManager.gm.FocusItem(selectedObject);
        }
    }
    ///<summary>
    /// Select the selected object's parent if the selected object is not a rack, deselect if it is
    ///</summary>
    public async void ButtonSelectParent()
    {
        if (selectedObject == null || (focused && focusedObject == selectedObject))
        {
            return;
        }
        if (selectedObject.transform.parent.GetComponent<OObject>() != null)
        {
            GameManager.gm.SetCurrentItem(selectedObject.transform.parent.gameObject);
            await GetComponent<OgreeObject>().LoadChildren("0");
            GetComponent<FocusHandler>().ogreeChildMeshRendererList.Clear();
            GetComponent<FocusHandler>().ogreeChildObjects.Clear();
        }
        else
        {
            GameManager.gm.SetCurrentItem(null);
        }
    }

    public void ButtonResetPosition()
    {
        ResetAllPositions(selectedObject);
    }

    public void ButtonEditMode()
    {
        if (!focused)
            return;
        if (!editMode)
        {
            editMode = true;
            ResetAllPositions(focusedObject);

            //enable collider used for manipulation
            focusedObject.transform.GetChild(0).GetComponent<Collider>().enabled = true;

            //disable rack colliders used for selection
            if (focusedObject.GetComponent<OgreeObject>().category == "rack")
            {
                focusedObject.transform.GetChild(1).GetComponent<Collider>().enabled = false;
                focusedObject.transform.GetChild(2).GetComponent<Collider>().enabled = false;
            }
            else //disable selection on the devices collider
            {
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchable>().enabled = false;
            }

            //disable children colliders
            focusedObject.GetComponent<FocusHandler>().UpdateChildMeshRenderers(true);

            for (int i = 0; i < focusedObject.transform.childCount; i++)
            {
                OgreeObject ogree = focusedObject.transform.GetChild(i).GetComponent<OgreeObject>();
                if (ogree == null)
                {
                    continue;
                }
                focusedObject.transform.GetChild(i).GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = true;
            }

            transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = false;
        }
        else
        {
            editMode = false;
            focusedObject.GetComponent<OgreeObject>().ResetPosition();

            if (focusedObject.GetComponent<OgreeObject>().category == "rack")
            {
                focusedObject.transform.GetChild(0).GetComponent<Collider>().enabled = false;
                focusedObject.transform.GetChild(1).GetComponent<Collider>().enabled = true;
                focusedObject.transform.GetChild(2).GetComponent<Collider>().enabled = true;
            }
            else
            {
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchable>().enabled = true;
            }
            focusedObject.GetComponent<FocusHandler>().UpdateChildMeshRenderers(true, true);

            for (int i = 0; i < focusedObject.transform.childCount; i++)
            {
                OgreeObject ogree = focusedObject.transform.GetChild(i).GetComponent<OgreeObject>();
                if (ogree == null)
                {
                    continue;
                }
                focusedObject.transform.GetChild(i).GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = true;
            }
        }
    }
}
