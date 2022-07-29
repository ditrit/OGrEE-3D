using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

public class ButtonManager : MonoBehaviour
{
    private bool front = true;

    [Header("Buttons")]
    [SerializeField] private GameObject buttonWrapperFront;
    [SerializeField] private GameObject buttonEdit;
    [SerializeField] private GameObject buttonSelectParent;
    [SerializeField] private GameObject buttonResetPosition;
    [SerializeField] private GameObject buttonToggleFocus;
    [SerializeField] private GameObject buttonWrapperBack;
    [SerializeField] private ParentConstraint parentConstraintButtonWrapperFront;
    [SerializeField] private ParentConstraint parentConstraintButtonWrapperBack;
    [SerializeField] private float verticalOffset = 0.06f;
    [SerializeField] private float horizontalOffset = 0f;
    [SerializeField] private float rackScale = 2f;
    [SerializeField] private float deviceScale = 1f;

    private Color defaultBackplateColor;
    private Vector3 editScale;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ChangeOrientationEvent>(OnChangeOrientation);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);
        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
        buttonWrapperFront.SetActive(false);
        buttonWrapperBack.SetActive(false);
        defaultBackplateColor = buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.color;
    }

    private void Update()
    {
        if (GameManager.gm.editMode)
        {
            if (GameManager.gm.focus[GameManager.gm.focus.Count - 1].GetComponent<OgreeObject>().category != "rack")
            {
                GameManager.gm.focus[GameManager.gm.focus.Count - 1].GetComponent<OgreeObject>().ResetTransform(OgreeObject.TransformComponent.Position);
            }
            if (GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale != editScale)
            {
                float scaleDiff = GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale.x / editScale.x;
                parentConstraintButtonWrapperFront.SetTranslationOffset(0, parentConstraintButtonWrapperFront.GetTranslationOffset(0) * scaleDiff);
                parentConstraintButtonWrapperBack.SetTranslationOffset(0, parentConstraintButtonWrapperBack.GetTranslationOffset(0) * scaleDiff);
                editScale = GameManager.gm.focus[GameManager.gm.focus.Count - 1].transform.localScale;
            }
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
            ogree.ResetTransform();
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
            GameObject objectSelected = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1];
            OgreeObject ogree = ithChild.GetComponent<OgreeObject>();
            if (ogree == null)
            {
                continue;
            }
            if (ithChild.localPosition.z < ogree.originalLocalPosition.z)
            {
                ogree.ResetTransform();
            }
        }
    }


    ///<summary>
    /// When called set the buttonWrapper active, set the selected object as its parent (via a parent constraint) and initialise it
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (GameManager.gm.currentItems.Count > 0)
        {
            PlaceButton();

            if (GameManager.gm.focus.Count > 0 && GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
            {
                buttonResetPosition.SetActive(true);
                buttonSelectParent.SetActive(false);
                buttonEdit.SetActive(true);
            }
            else
            {
                buttonResetPosition.SetActive(false);
                buttonSelectParent.SetActive(true);
                buttonEdit.SetActive(false);
            }
        }
        else
        {
            buttonEdit.SetActive(true);
            buttonWrapperFront.SetActive(false);
            buttonWrapperBack.SetActive(false);
        }

    }


    ///<summary>
    /// When called set the edit button active
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent e)
    {
        buttonResetPosition.SetActive(true);
        buttonEdit.SetActive(true);
        if (GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
            buttonSelectParent.SetActive(false);
    }


    ///<summary>
    /// When called set the edit button inactive
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        buttonEdit.SetActive(false);
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
        {
            buttonSelectParent.SetActive(false);
            buttonResetPosition.SetActive(true);
        }
        else
        {
            buttonSelectParent.SetActive(true);
            buttonResetPosition.SetActive(false);
        }

    }

    ///<summary>
    /// When called change the front boolean accordingly
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnChangeOrientation(ChangeOrientationEvent e)
    {
        front = e.front;
    }


    ///<summary>
    /// When called set the focus button and the select parent button inactive and change the color of the edit button
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnEditModeIn(EditModeInEvent e)
    {
        buttonToggleFocus.SetActive(false);
        buttonSelectParent.SetActive(false);
        buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        GameManager.gm.editMode = true;
    }

    ///<summary>
    /// When called set the focus button and the select parent button active and change the color of the edit button
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnEditModeOut(EditModeOutEvent e)
    {
        buttonToggleFocus.SetActive(true);
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
            buttonSelectParent.SetActive(false);
        else
            buttonSelectParent.SetActive(true);
        buttonEdit.transform.GetChild(3).GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", defaultBackplateColor);
        GameManager.gm.editMode = false;
    }

    ///<summary>
    /// Focus the selected object or defocus according to the current state (selection, focus)
    ///</summary>
    public async void ButtonToggleFocus()
    {
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] == GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1])
        {
            GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<FocusHandler>().ToggleCollider(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1], true);
            await GameManager.gm.UnfocusItem();
        }
        else
        {
            await GameManager.gm.FocusItem(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]);
        }
        StartCoroutine(SelectionDelay());
    }

    ///<summary>
    /// Select the selected object's parent if the selected object is not a rack, deselect if it is
    /// if we deselect a rack, unload its children
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
            await GameManager.gm.SetCurrentItem(previousSelected.transform.parent.gameObject);
            if (GameManager.gm.focus.Count > 0 && GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1] == GameManager.gm.focus[GameManager.gm.focus.Count - 1])
            {
                buttonEdit.SetActive(true);
            }
        }
        else
        {
            await GameManager.gm.SetCurrentItem(null);
        }

        StartCoroutine(SelectionDelay());
    }

    ///<summary>
    /// Prevents the user from reselecting too quickly
    ///</summary>
    private IEnumerator SelectionDelay()
    {
        HandInteractionHandler.canSelect = false;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1.5f);
        HandInteractionHandler.canSelect = true;
    }

    ///<summary>
    /// See ResetAllPositions
    ///</summary>
    public void ButtonResetPosition()
    {
        ResetAllPositions(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1]);
        GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<OgreeObject>().ResetTransform();
        if (!GameManager.gm.editMode)
            UHelpersManager.um.ResetUHelpers();
    }


    ///<summary>
    /// Toggle the edit mode (raise an EditModeInEvent or an EditModdeOutEvent) if we already are focused on a object
    ///</summary>
    public void ButtonEditMode()
    {
        if (GameManager.gm.focus.Count == 0)
            return;
        GameObject focusedObject = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
        if (!GameManager.gm.editMode)
        {
            editScale = focusedObject.transform.localScale;
            EventManager.Instance.Raise(new EditModeInEvent { obj = focusedObject });
        }
        else
        {
            EventManager.Instance.Raise(new EditModeOutEvent { obj = focusedObject });
        }
    }

    private void PlaceButton()
    {
        if (GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<OgreeObject>().category == "rack")
        {
            buttonWrapperFront.transform.localScale = rackScale * Vector3.one;
            buttonWrapperBack.transform.localScale = rackScale * Vector3.one;
        }
        else
        {
            buttonWrapperFront.transform.localScale = deviceScale * Vector3.one;
            buttonWrapperBack.transform.localScale = deviceScale * Vector3.one;
        }

        buttonWrapperFront.transform.SetParent(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform);
        Vector3 parentSize = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform.GetChild(0).lossyScale;

        //Placing buttons
        buttonWrapperFront.transform.localPosition = Vector3.zero;
        buttonWrapperFront.transform.localRotation = Quaternion.Euler(0, front ? 0 : 180, 0);
        buttonWrapperFront.transform.localPosition += new Vector3(front ? -parentSize.x - horizontalOffset : parentSize.x + horizontalOffset, parentSize.y + verticalOffset, front ? parentSize.z : -parentSize.z) / 2;
        ConstraintSource source = new ConstraintSource
        {
            weight = 1,
            sourceTransform = GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform
        };
        parentConstraintButtonWrapperFront.SetSource(0, source);
        parentConstraintButtonWrapperFront.SetTranslationOffset(0, buttonWrapperFront.transform.localPosition);
        parentConstraintButtonWrapperFront.SetRotationOffset(0, buttonWrapperFront.transform.localEulerAngles);
        buttonWrapperFront.transform.SetParent(null);

        parentConstraintButtonWrapperFront.constraintActive = true;
        buttonWrapperFront.SetActive(true);


        buttonWrapperBack.transform.SetParent(GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].transform);

        //Placing buttons
        buttonWrapperBack.transform.localPosition = Vector3.zero;
        buttonWrapperBack.transform.localRotation = Quaternion.Euler(0, front ? 0 : 180, 0);
        buttonWrapperBack.transform.localPosition += new Vector3(!front ? -parentSize.x - horizontalOffset : parentSize.x + horizontalOffset, parentSize.y + verticalOffset, !front ? parentSize.z : -parentSize.z) / 2;

        parentConstraintButtonWrapperBack.SetSource(0, source);
        parentConstraintButtonWrapperBack.SetTranslationOffset(0, buttonWrapperBack.transform.localPosition);
        parentConstraintButtonWrapperBack.SetRotationOffset(0, buttonWrapperBack.transform.localEulerAngles);
        buttonWrapperBack.transform.SetParent(null);

        parentConstraintButtonWrapperBack.constraintActive = true;
        buttonWrapperBack.SetActive(true);
    }

    public void ButtonChangeOrientation()
    {
        front = !front;
        PlaceButton();
        GameManager.gm.currentItems[GameManager.gm.currentItems.Count - 1].GetComponent<FocusHandler>().ChangeOrientationFromRack(front);
    }
}