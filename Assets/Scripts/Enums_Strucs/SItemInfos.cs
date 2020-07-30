using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SRackInfos
{
    public string name;
    public string parentName; // To be deleted
    public Transform parent;
    public string orient;
    public Vector2 pos; // tile
    public Vector2 size; // cm 
    public int height; // U = 44.5mm
    public string comment; // To be deleted
    public string row; // To be deleted
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
    public Transform parent;
    public string orient;
    public string address;
    public string zipcode;
    public string city;
    public string country;
    public string description;
}

[System.Serializable]
public struct SBuildingInfos
{
    public string name;
    public Transform parent;
    public Vector3 pos;
    public Vector3 size;
}

[System.Serializable]
public struct SRoomInfos
{
    public string name;
    public Transform parent;
    public Vector3 pos; // tile
    public Vector3 size; // tile
    public Vector2 margin; // tile, should be deleted
    public string orient;
}
