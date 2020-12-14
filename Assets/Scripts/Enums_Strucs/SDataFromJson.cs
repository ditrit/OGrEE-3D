using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SCuFromJson
{
    public string name;
    public string color;
    public string mainContact;
    public string mainPhone;
    public string mainEmail;
    public string id;
}

[System.Serializable]
public struct SDcFromJson
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