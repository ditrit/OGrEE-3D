using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SApiObject
{
    public string name;
    public string id;
    public string parentId;
    public string category;
    public string description;
    public string domain;
    public Dictionary<string, string> attributes;
}

// [System.Serializable]
// public struct STnFromJson
// {
//     public string name;
//     public string color;
//     public string mainContact;
//     public string mainPhone;
//     public string mainEmail;
//     public string id;
// }

[System.Serializable]
public struct SSiteFromJson
{
    public string name;
    public string orient;
    public string comment;
    public string address;
    public string zipcode;
    public string city;
    public string country;
    public Vector3 gps;
    public string usableColor;
    public string reservedColor;
    public string technicalColor;
    public string parentId;
    public string id;
}