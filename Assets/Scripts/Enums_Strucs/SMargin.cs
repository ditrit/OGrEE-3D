using UnityEngine;

[System.Serializable]
public struct SMargin
{
    public float left;
    public float right;
    public float front;
    public float back;

    public SMargin(SMargin _data)
    {
        front = _data.front;
        back = _data.back;
        right = _data.right;
        left = _data.left;
    }

    public SMargin(Vector4 _data)
    {
        front = _data.x;
        back = _data.y;
        right = _data.z;
        left = _data.z;
    }
}
