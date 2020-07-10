using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SRackInfos
{
    public string name;
    public string parentName;
    public string orient;
    public Vector2 pos; // tile
    public Vector2 size; // cm 
    public int height; // U = 44.5mm
    public string comment;
    public string row;
}

[System.Serializable]
public struct SDeviceInfos
{
    public string name;
    public string parentName;
    public Vector3 pos; // mm? U ?
    public Vector3 size; // mm? U ?
    public string type;
    public string orient;
    public string model;
    public string serial; // ?
    public string vendor;
    public string comment;
}

[System.Serializable]
public struct SDataCenterInfos
{
    public string name;
    public string address;
    public string zipcode;
    public string city;
    public string country;
    public string description;
}

[System.Serializable]
public struct SItRoomInfos
{
    public string name;
    public string parentName;
    public Vector2 size; // tile
    public Vector2 margin; // tile
    public Vector2 pos; // tile
    public string orient;
}
