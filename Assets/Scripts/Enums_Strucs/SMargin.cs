using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

    public SMargin(string _N, string _S, string _E, string _W)
    {
        top = float.Parse(_N, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        bottom = float.Parse(_S, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        right = float.Parse(_E, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        left = float.Parse(_W, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
    }

    public SMargin(int[] _data)
    {
        if (_data.Length != 4)
        {
            top = 0;
            bottom = 0;
            right = 0;
            left = 0;
        }
        else
        {
            top = _data[0];
            bottom = _data[1];
            right = _data[2];
            left = _data[3];
        }
    }
}
