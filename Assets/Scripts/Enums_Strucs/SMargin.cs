using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SMargin
{
    public float left;
    public float right;
    public float top;
    public float bottom;

    public SMargin(float _left, float _right, float _top, float _bottom)
    {
        left = _left;
        right = _right;
        top = _top;
        bottom = _bottom;
    }
}
