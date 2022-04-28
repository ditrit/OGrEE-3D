using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private bool focused = false;
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
        if (buttonWrapper.activeInHierarchy && selectedObject != null)
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
        buttonWrapper.transform.localPosition += new Vector3(front ? -parentSize.x + 0.18f : parentSize.x - 0.18f, parentSize.y + 0.06f, front ? parentSize.z + 0.06f : -parentSize.z - 0.06f) / 2;
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
    public async void ButtonToggleFocusAsync()
    {
        if (focused && focusedObject == selectedObject)
        {
            GameObject temp = selectedObject;
            await selectedObject.GetComponent<OgreeObject>().LoadChildren("0");
            temp.GetComponent<FocusHandler>().ogreeChildMeshRendererList.Clear();
            temp.GetComponent<FocusHandler>().ogreeChildObjects.Clear();
            GameManager.gm.UnfocusItem();
            temp.GetComponent<FocusHandler>().ToggleCollider(temp, true);

        }
        else
        {
            await selectedObject.GetComponent<OgreeObject>().LoadChildren("1");
            GameManager.gm.FocusItem(selectedObject);
        }
    }
    ///<summary>
    /// Select the selected object's parent if the selected object is not a rack, deselect if it is
    ///</summary>
    public void ButtonSelectParent()
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
