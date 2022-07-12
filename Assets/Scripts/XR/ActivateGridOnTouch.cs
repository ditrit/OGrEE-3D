using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;

public class ActivateGridOnTouch : MonoBehaviour, IMixedRealityTouchHandler
{

    #region Event handlers
    public TouchEvent OnTouchCompleted;
    public TouchEvent OnTouchStarted;
    public TouchEvent OnTouchUpdated;
    #endregion


    ///<summary>
    /// Called when a hand is exiting the object's collider
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
    {
    }

    ///<summary>
    /// Called when a hand is entering the object's collider. Create a Grid at the height of the object.
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
    {
        Transform t = GameManager.gm.currentItems[0].transform;
        while(t != null)
        {
            if (t.GetComponent<OgreeObject>().category == "rack")
            {
                int lenght = gameObject.name.Length;
                string name = gameObject.name.Substring(lenght - 4);
                t.GetComponent<Grid>().CreateGrid(gameObject.transform.position.y, gameObject.name);
                return;
            }
            t = t.parent.transform;
        }
    }

    ///<summary>
    /// Called when a hand is in the object's collider 
    ///</summary>
    ///<param name="_eventData">The HandTrackingInputEventData</param>
    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
    {
    }
}

