using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tenant
{
    public string name;
    public string color; // or public Color color;
    public string mainContact;
    public string mainPhone;
    public string mainEmail;

    public Tenant(string _name, string _color)
    {
        name = _name;
        color = _color;
    }

}
