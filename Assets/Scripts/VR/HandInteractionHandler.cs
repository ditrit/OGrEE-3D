using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.UI;

public class HandInteractionHandler : MonoBehaviour, IMixedRealityTouchHandler
{

    #region Event handlers
    public TouchEvent OnTouchCompleted;
    public TouchEvent OnTouchStarted;
    public TouchEvent OnTouchUpdated;
    #endregion


    private void Start()
    {
    }

    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData _eventData)
    {
    }

    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData _eventData)
    {
        GameManager.gm.WaitBeforeNewSelection(transform.parent.gameObject);
    }

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData _eventData)
    {
    }

    public void SelectThis()
    {

    }

    public void UnselectThis()
    {
        GameManager.gm.SetCurrentItem(null);
    }
}

