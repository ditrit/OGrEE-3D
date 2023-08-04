using System.Collections;
using System.Collections.Generic;

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
}
