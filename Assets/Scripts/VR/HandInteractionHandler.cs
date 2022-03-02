using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.UI;

public class HandInteractionHandler : MonoBehaviour, IMixedRealityTouchHandler, IMixedRealityPointerHandler
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
        UnselectThis();
    }

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData _eventData)
    {
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData _eventData)
    {
        SelectThis();
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData _eventData)
    {
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData _eventData)
    {
        FocusThis();
    }

    void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData _eventData)
    {

    }
    public void SelectThis()
    {
        GameManager.gm.SetCurrentItem(gameObject);
    }

    public void UnselectThis()
    {
        GameManager.gm.SetCurrentItem(null);
    }

    public void FocusThis()
    {
        GameManager.gm.FocusItem(gameObject);
    }

}

