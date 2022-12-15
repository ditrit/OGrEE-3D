using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusHandlerEventRaiser : MonoBehaviour
{
    ///<summary>
    /// Called by GUI button
    ///</summary>
    public void FocusHandlerUpdateArrayButtonPressed()
    {
        EventManager.instance.Raise(new ImportFinishedEvent());
    }

}
