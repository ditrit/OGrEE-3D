using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SCuFromJson
{
    public string name;
    public string mainContact;
    public string mainPhone;
    public string mainEmail;
    public int id;
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
    public int[] gps;
    public string usableColor;
    public string reservecColor;
    public string technicalColor;
    public int customer_id;
    public int id;
}