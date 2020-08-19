using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightControl : MonoBehaviour
{
    ///<summary>
    /// Called by a GUI Checkbox.
    /// Change type of shadows according to _value.
    ///</summary>
    ///<param name="_value">The checkbox value</param>
    public void ToggleShadows(bool _value)
    {
        if (_value)
            GetComponent<Light>().shadows = LightShadows.Soft;
        else
            GetComponent<Light>().shadows = LightShadows.None;
    }
}
