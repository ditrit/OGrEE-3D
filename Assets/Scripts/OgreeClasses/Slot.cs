using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool used = false;
    public string orient;
    public string formFactor;
    public string labelPos;

    ///<summary>
    /// Disable slot renderer and disable its labels.
    ///</summary>
    ///<param name="_value">Is the slot taken ?</param>
    public void SlotTaken(bool _value)
    {
        used = _value;
        Display(!used);
    }

    ///<summary>
    /// Display or not the Renderer and all TMP
    ///<summary>
    ///<param name="_value">If the slot is hiden or not</param>
    public void Display(bool _value)
    {
        transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
        GetComponent<DisplayObjectData>().ToggleLabel(_value);
    }
}
