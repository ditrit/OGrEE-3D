using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class HandInteractionHandler : MonoBehaviour, IMixedRealityTouchHandler
{

    #region Event handlers
    public TouchEvent OnTouchCompleted;
    public TouchEvent OnTouchStarted;
    public TouchEvent OnTouchUpdated;
    #endregion

    public static bool canSelect = true;
    public bool canSelectDebug;
    public bool isARack = false;
    public bool front;

    private void Update()
    {
        canSelectDebug = canSelect;
    }

    ///<summary>
    /// Called when a hand is exiting the object's collider
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
    {
    }

    ///<summary>
    /// Called when a hand is entering the object's collider and select this object  
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
    {
        SelectThis();
    }

    ///<summary>
    /// Called when a hand is in the object's collider 
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
    {
    }

    ///<summary>
    /// Call the couroutine to select an object and update which face is selected
    ///</summary>
    public void SelectThis()
    {
        if (isARack)
        {
            if (front)
                FrontSelected();
            else
                BackSelected();
        }
        StartCoroutine(SelectThisCoroutine());
    }

    ///<summary>
    /// Select this object
    ///</summary>
    public IEnumerator SelectThisCoroutine()
    {
        if (canSelect)
        {
            canSelect = false;
            Task task = GameManager.gm.SetCurrentItem(transform.parent.gameObject);
            yield return new WaitUntil(() => task.IsCompleted);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1.5f);
            canSelect = true;
        }

    }

    ///<summary>
    /// Raise a ChangeOrientationEvent to update which face is currently selected in all objects
    ///</summary>
    public void FrontSelected()
    {
        EventManager.Instance.Raise(new ChangeOrientationEvent() { front = true });
        transform.parent.GetComponent<FocusHandler>().ChangeOrientation(true, false);
    }

    ///<summary>
    /// Raise a ChangeOrientationEvent to update which face is currently selected in all objects
    ///</summary>
    public void BackSelected()
    {
        EventManager.Instance.Raise(new ChangeOrientationEvent() { front = false });
        transform.parent.GetComponent<FocusHandler>().ChangeOrientation(false, false);
    }

}

