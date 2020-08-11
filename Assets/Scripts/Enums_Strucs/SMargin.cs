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

    public SMargin(SMargin _data)
    {
        top = _data.top;
        bottom = _data.bottom;
        right = _data.right;
        left = _data.left;
    }

    public SMargin(float _N, float _S, float _E, float _W)
    {
        top = _N;
        bottom = _S;
        right = _E;
        left = _W;
    }
}
