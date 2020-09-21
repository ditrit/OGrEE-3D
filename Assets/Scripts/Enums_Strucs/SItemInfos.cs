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
    public Vector2 size; // cm 
    public int height; // U = 44.5mm
    public string template;
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
}

[System.Serializable]
public struct SDataCenterInfos
{
    public string name;
    public Transform parent;
    public string orient;
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
    public string orient;
}

[System.Serializable]
public struct SRoomTemplate
{
    public SRoomInfos infos;
    public SMargin reserved;
    public SMargin technical;
}
