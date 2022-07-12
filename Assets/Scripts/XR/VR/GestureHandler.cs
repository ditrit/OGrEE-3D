using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class GestureHandler : MonoBehaviour, IMixedRealityGestureHandler
{
    public void OnGestureCanceled(InputEventData eventData)
    {
    }

    public void OnGestureCompleted(InputEventData eventData)
    {
        Debug.Log(eventData.ToString());
    }

    public void OnGestureStarted(InputEventData eventData)
    {
    }

    public void OnGestureUpdated(InputEventData eventData)
    {
    }
}

