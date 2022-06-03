using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class DeltaPositionManager : MonoBehaviour
{
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;
    public bool isFirstMove = true;

    public void OnHoverIn()
    {
        if (transform.parent.GetComponent<DeltaPositionManager>())
        {
            DeltaPositionManager delta = transform.parent.GetComponent<DeltaPositionManager>();
            if (isFirstMove)
            {
                initialYPosition = delta.initialYPosition + (transform.position.y - transform.parent.position.y);
                isFirstMove = false;
            }
        }
        else
        {
            if (isFirstMove)
            {
                initialYPosition = transform.position.y;
                isFirstMove = false;
            }
        }
    }

    public void OnHoverOut()
    {
        yPositionDelta = transform.position.y - initialYPosition;
    }
}
