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
    public float yRotation;

    ///<summary>
    /// When an object is moved, keep track of the original position on the y axis
    ///</summary>
    public void OnMovingObject()
    {
        if (transform.parent.GetComponent<DeltaPositionManager>())
        {
            DeltaPositionManager delta = transform.parent.GetComponent<DeltaPositionManager>();
            if (isFirstMove)
            {
                if (delta.isFirstMove)
                {
                    initialYPosition = transform.position.y; //Problème ici pour les i helpers, déplcement sans focus du parent.
                    isFirstMove = false;
                }
                else
                {
                    initialYPosition = delta.initialYPosition + (transform.position.y - transform.parent.position.y); //Problème ici pour les i helpers, déplcement sans focus du parent.
                    isFirstMove = false;
                }
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

    ///<summary>
    /// When an object is not moving anymore, update the total translation on the y axis
    ///</summary>
    public void OnStoppingObject()
    {
        yPositionDelta = transform.position.y - initialYPosition;
    }
}
