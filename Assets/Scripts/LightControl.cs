using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightControl : MonoBehaviour
{
    public void ToggleShadows(bool _value)
    {
        if (_value)
            GetComponent<Light>().shadows = LightShadows.Soft;
        else
            GetComponent<Light>().shadows = LightShadows.None;
    }
}
