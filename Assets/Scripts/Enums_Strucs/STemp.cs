using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Structure containing all informations relative to an object's temperature : its average, standard deviation, minimum, maximum, unit and its hottest child.
/// </summary>
[System.Serializable]
public struct STemp
{
    public float mean;
    public float std;
    public float min;
    public float max;
    public string hottestChild;
    public string unit;

    public STemp(float _mean, float _std, float _min, float _max, string _hottestChild, string _unit)
    {
        mean = _mean;
        std = _std;
        min = _min;
        max = _max;
        hottestChild = _hottestChild;
        unit = _unit;
    }
}
