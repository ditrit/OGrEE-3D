using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    private bool editMode = false;
    private bool front = true;

    [Header("Butons")]
    [SerializeField] private GameObject buttonWrapper;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ChangeOrientationEvent>(OnChangeOrientation);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
        buttonWrapper.SetActive(false);
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
        for (int i = 0; i < GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.childCount; i++)
        {
            Transform ithChild = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.GetChild(i);
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
        buttonWrapper.transform.SetParent(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform);
        Vector3 parentSize = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.GetChild(0).lossyScale;

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
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] == GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1])
        {
            GameManager.gm.UnfocusItem();
            GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<FocusHandler>().ToggleCollider(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1], true);

        }
        else
        {
            GameManager.gm.FocusItem(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]);
        }
    }
    ///<summary>
    /// Select the selected object's parent if the selected object is not a rack, deselect if it is
    ///</summary>
    public async void ButtonSelectParent()
    {
        if (GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == null || (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] == GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]))
        {
            return;
        }
        if (GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.parent.GetComponent<OObject>() != null)
        {
            GameManager.gm.SetCurrentItem(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.parent.gameObject);
            StartCoroutine(SelectionDelay());
            await GetComponent<OgreeObject>().LoadChildren("0");
            GetComponent<FocusHandler>().ogreeChildMeshRendererList.Clear();
            GetComponent<FocusHandler>().ogreeChildObjects.Clear();
        }
        else
        {
            GameManager.gm.SetCurrentItem(null);
        }
    }

    private IEnumerator SelectionDelay()
    {
        HandInteractionHandler.canSelect = false;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1.5f);
        HandInteractionHandler.canSelect = true;
    }

    public void ButtonResetPosition()
    {
        ResetAllPositions(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]);
    }

    public void ButtonEditMode()
    {
        if (GameManager.gm.focus.Count == 0)
            return;
        GameObject focusedObject = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
        CustomRendererOutline rendererOutline = focusedObject.GetComponent<CustomRendererOutline>();
        if (!editMode)
        {
            editMode = true;
            rendererOutline.SetMaterial(rendererOutline.editMaterial);

            //enable collider used for manipulation
            focusedObject.transform.GetChild(0).GetComponent<Collider>().enabled = true;

            //disable rack colliders used for selection
            if (focusedObject.GetComponent<OgreeObject>().category != "rack")
            {
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchable>().enabled = false;
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>().enabled = true;
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = false;
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
                focusedObject.transform.GetChild(i).GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = false;
            }

        }
        else
        {
            editMode = false;
            rendererOutline.SetMaterial(rendererOutline.focusMaterial);

            focusedObject.transform.GetChild(0).GetComponent<Collider>().enabled = false;

            if (focusedObject.GetComponent<OgreeObject>().category != "rack")
            {
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionTouchable>().enabled = true;
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.Input.NearInteractionGrabbable>().enabled = false;
                focusedObject.transform.GetChild(0).GetComponent<Microsoft.MixedReality.Toolkit.UI.MoveAxisConstraint>().enabled = true;
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
