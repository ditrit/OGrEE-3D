using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SApiObject
{
    public string name;
    public string hierarchyName;
    public string id;
    public string parentId;
    public string category;
    public List<string> description;
    public string domain;
    public Dictionary<string, string> attributes;

    public SApiObject(OgreeObject _src)
    {
        name = _src.name;
        hierarchyName = _src.hierarchyName;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        description = _src.description;
        domain = _src.domain;
        attributes = _src.attributes;
    }
}
