using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaPositionManager : MonoBehaviour
{
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;

    public void OnHoverIn()
    {
        initialYPosition = transform.position.y;
    }

    public void OnHoverOut()
    {
        yPositionDelta = transform.position.y - initialYPosition;
    }
}
