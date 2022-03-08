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

    public float doubleClickTimeLimit = 0.25f;
    private float timerSelectionVR = 0;
    private bool hasAlreadyFocused = false;
    private bool isAlreadySelected = false;
    private void Start()
    {
    }

    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData _eventData)
    {
    }

    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData _eventData)
    {
        SelectThis();
    }

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData _eventData)
    {
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData _eventData)
    {
        FocusThis();
    }

    void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData _eventData)
    {
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData _eventData)
    {

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
    private void Update()
    {
        timerSelectionVR += Time.deltaTime;
    }
}

