using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SRackInfos
{
    public string name;
    public Transform parent;
    public string orient;
    public Vector2 pos; // tile
    public Vector3 size; // cm 
    public int height; // U
    public string template;
}

[System.Serializable]
public struct SDeviceInfos
{
    public string name;
    public Transform parent;
    // public int posU;
    public float posU; // should be int, authorize until non IT objects can be created
    public string slot;
    public float sizeU;
    public string template;
    public string side;

}

public struct SSeparatorInfos
{
    public string name;
    public Vector2 pos1XYm;
    public Vector2 pos2XYm;
    public Transform parent;
}

