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
    private float timerSelectionVR = 0;
    private float leftHandTouchTimer = 0;
    private float rightHandTouchTimer = 0;
    private bool leftHandInContact = false;
    private bool rightHandInContact = false;
    private bool hasAlreadyFocused = false;
    private void Start()
    {
    }

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

    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData _eventData)
    {
        Debug.Log("SELECTION");
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

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData _eventData)
    {
        if (leftHandInContact && rightHandInContact && !hasAlreadyFocused)
        {
            FocusThis();
            hasAlreadyFocused = true;
        }
    }

    public void SelectThis()
    {
        GameManager.gm.SetCurrentItem(transform.parent.gameObject);
        timerSelectionVR = 0;
    }
    public void FocusThis()
    {
        GameManager.gm.FocusItem(transform.parent.gameObject);
        timerSelectionVR = 0;
    }
    private void Update()
    {
        timerSelectionVR += Time.deltaTime;
        rightHandTouchTimer += Time.deltaTime;
        leftHandTouchTimer += Time.deltaTime;
    }
}

