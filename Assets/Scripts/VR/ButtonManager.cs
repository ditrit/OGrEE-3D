using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

public class ButtonManager : MonoBehaviour
{
    private bool editMode = false;
    private bool front = true;

    [Header("Buttons")]
    [SerializeField] private GameObject buttonWrapper;
    [SerializeField] private GameObject buttonEdit;
    [SerializeField] private GameObject buttonSelectParent;
    [SerializeField] private GameObject buttonToggleFocus;
    [SerializeField] private ParentConstraint parentConstraint;
    [SerializeField] private float verticalOffset = 0.06f;
    [SerializeField] private float horizontalOffset = 0f;

    private Color defaultBackplateColor;
    private Vector3 editScale;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ChangeOrientationEvent>(OnChangeOrientation);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);
        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
        buttonWrapper.SetActive(false);
        defaultBackplateColor = buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.color;
    }

    private void Update()
    {
        if (editMode && GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale != editScale)
        {
            float scaleDiff = GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale.x / editScale.x;
            parentConstraint.SetTranslationOffset(0, parentConstraint.GetTranslationOffset(0) * scaleDiff);
            editScale = GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale;
        }
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
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        buttonWrapper.transform.SetParent(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform);
        Vector3 parentSize = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.GetChild(0).lossyScale;

        //Placing buttons
        buttonWrapper.transform.localPosition = Vector3.zero;
        buttonWrapper.transform.localRotation = Quaternion.Euler(0, front ? 0 : 180, 0);
        buttonWrapper.transform.localPosition += new Vector3(front ? -parentSize.x - horizontalOffset: parentSize.x + horizontalOffset, parentSize.y + verticalOffset, front ? parentSize.z : -parentSize.z) / 2;
        ConstraintSource source = new ConstraintSource
        {
            weight = 1,
            sourceTransform = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform
        };
        parentConstraint.SetSource(0, source);
        parentConstraint.SetTranslationOffset(0, buttonWrapper.transform.localPosition);
        parentConstraint.SetRotationOffset(0, buttonWrapper.transform.localEulerAngles);
        buttonWrapper.transform.SetParent(null);

        parentConstraint.constraintActive = true;
        buttonWrapper.SetActive(true);
        buttonEdit.SetActive(false);

    }

    ///<summary>
    /// When called set the button inactive
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent e)
    {
        buttonEdit.SetActive(true);
        buttonWrapper.SetActive(false);
    }

    private void OnFocusItem(OnFocusEvent e)
    {
        buttonEdit.SetActive(true);
    }
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        buttonEdit.SetActive(false);
    }

    ///<summary>
    /// When called change the front boolean accordingly
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnChangeOrientation(ChangeOrientationEvent e)
    {
        front = e.front;
    }

    private void OnEditModeIn(EditModeInEvent e)
    {
        buttonToggleFocus.SetActive(false);
        buttonSelectParent.SetActive(false);
        buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.green);
    }
    private void OnEditModeOut(EditModeOutEvent e)
    {
        buttonToggleFocus.SetActive(true);
        buttonSelectParent.SetActive(true);
        buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", defaultBackplateColor);
    }

    ///<summary>
    /// Focus the selected object or defocus according to the current state (selection, focus)
    ///</summary>
    public void ButonToggleFocus()
    {
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] == GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1])
        {
            GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<FocusHandler>().ToggleCollider(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1], true);
            GameManager.gm.UnfocusItem();

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

        GameObject previousSelected = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1];

        if (previousSelected.transform.parent.GetComponent<OObject>() != null)
        {
            GameManager.gm.SetCurrentItem(previousSelected.transform.parent.gameObject);
            if (GameManager.gm.focus.Count > 0 && GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
            {
                buttonEdit.SetActive(true);
            }
        }
        else
        {
            await previousSelected.GetComponent<OgreeObject>().LoadChildren("0");
            previousSelected.GetComponent<FocusHandler>().ogreeChildMeshRendererList.Clear();
            previousSelected.GetComponent<FocusHandler>().ogreeChildObjects.Clear();
            GameManager.gm.SetCurrentItem(null);
        }

        StartCoroutine(SelectionDelay());
    }

    ///<summary>
    /// Prevents the user from reselecting too quickly
    ///</summary>
    private IEnumerator SelectionDelay()
    {
        HandInteractionHandler.canSelect = false;
        print("CanSelect : " + HandInteractionHandler.canSelect);
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1.5f);
        HandInteractionHandler.canSelect = true;
        print("CanSelect : " + HandInteractionHandler.canSelect);
    }

    ///<summary>
    /// See ResetAllPositions
    ///</summary>
    public void ButtonResetPosition()
    {
        ResetAllPositions(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]);
        GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<OgreeObject>().ResetPosition();


    }

    public void ButtonEditMode()
    {
        if (GameManager.gm.focus.Count == 0)
            return;
        GameObject focusedObject = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
        if (!editMode)
        {
            editMode = true;

            editScale = focusedObject.transform.localScale;
                EventManager.Instance.Raise(new EditModeInEvent { obj = focusedObject });
        }
        else
        {
            editMode = false;
            EventManager.Instance.Raise(new EditModeOutEvent { obj = focusedObject });
        }
    }
}
