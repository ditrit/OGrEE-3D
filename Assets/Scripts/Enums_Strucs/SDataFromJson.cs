using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct SApiObject
{
    public string name;
    // public string hierarchyName;
    public string id;
    public string parentId;
    public string category;
    public List<string> description;
    public string domain;
    public Dictionary<string, string> attributes;
    public SApiObject[] children;

    public SApiObject(OgreeObject _src)
    {
        name = _src.name;
        // hierarchyName = _src.hierarchyName;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        description = _src.description;
        domain = _src.domain;
        attributes = _src.attributes;
        children = null;
    }
    ///<summary>
    /// Avoid requestsToSend 
    /// Get an Object from the api. Create an ogreeObject with response.
    ///</summary>
    ///<param name="_response">The response for API GET request</param>
    ///<returns>A string containing the name of the first object created by the _response</returns>
    public string GetName(SApiObject _obj)
    {
        return _obj.name;
    }
}
