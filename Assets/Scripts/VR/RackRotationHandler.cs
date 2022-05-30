using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RackRotationHandler : MonoBehaviour, IMixedRealityPointerHandler
{
    public Microsoft.MixedReality.Toolkit.UI.RotationAxisConstraint rackRotationAxisConstraint;
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer || eventData.Pointer is ShellHandRayPointer)
        {
            Debug.Log($"Grab start from {eventData.Pointer.PointerName}");
            rackRotationAxisConstraint.ConstraintOnRotation = Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.XAxis | Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.YAxis | Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.ZAxis;
        }
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer || eventData.Pointer is ShellHandRayPointer)
        {
            Debug.Log($"Grab end from {eventData.Pointer.PointerName}");
            rackRotationAxisConstraint.ConstraintOnRotation = Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.XAxis | Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.ZAxis;
        }
    }
}
