using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

public class HandInteractionHandler : MonoBehaviour, IMixedRealityTouchHandler
{

    #region Event handlers
    public TouchEvent OnTouchCompleted;
    public TouchEvent OnTouchStarted;
    public TouchEvent OnTouchUpdated;
    #endregion
    private bool leftHandInContact = false;
    private bool rightHandInContact = false;
    private bool hasAlreadyFocused = false;

    ///<summary>
    /// Check which hand is currently exiting the object's collider and update the booleans
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData _eventData)
    {
        switch (_eventData.Handedness)
        {
            case Handedness.Left:
                leftHandInContact = false; break;
            case Handedness.Right:
                rightHandInContact = false; break;
            default: break;
        }
        hasAlreadyFocused = false;
    }

    ///<summary>
    /// Check which hand is currently entering the object's collider and update the booleans
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData _eventData)
    {
        SelectThis();
        switch (_eventData.Handedness)
        {
            case Handedness.Left:
                leftHandInContact = true; break;
            case Handedness.Right:
                rightHandInContact = true; break;
            default: break;
        }
    }

    ///<summary>
    /// Check which hand is currently touching the object's collider and focus on the object if both hands are in contact
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData _eventData)
    {
        if (leftHandInContact && rightHandInContact && !hasAlreadyFocused)
        {
            FocusThis();
            hasAlreadyFocused = true;
        }
    }

    ///<summary>
    /// Select this object
    ///</summary>
    public void SelectThis()
    {
        GameManager.gm.SetCurrentItem(transform.parent.gameObject);
    }
    ///<summary>
    /// Focus this object
    ///</summary>
    public void FocusThis()
    {
        GameManager.gm.FocusItem(transform.parent.gameObject);
    }

}

