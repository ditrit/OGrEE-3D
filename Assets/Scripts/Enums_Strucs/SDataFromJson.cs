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
    public List<string> description;
    public string domain;
    public Dictionary<string, string> attributes;
}
