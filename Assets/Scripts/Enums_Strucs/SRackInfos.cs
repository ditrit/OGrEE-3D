using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SRackInfos
{
    public string name;
    public string orient;
    public Vector2 pos; // tile
    public Vector2 size; // cm 
    public int height; // U = 44.5mm
    public string comment;
    public string row;
}
