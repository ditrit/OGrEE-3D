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
    public SApiObject[] children;

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
        children = null;
    }
}

[System.Serializable]
public struct SBuildingFromJson
{
    public string slug;
    public string category;
    public float[] sizeWDHm;
    public List<List<float>> vertices;
}

#region Room
[System.Serializable]
public struct SRoomFromJson
{
    public string slug;
    public string axisOrientation;
    public float[] sizeWDHm;
    public string floorUnit;
    public List<List<float>> vertices;
    public List<float> tileOffset;
    public List<int> technicalArea;
    public List<int> reservedArea;
    public SSeparator[] separators;
    public SPillar[] pillars;
    public SColor[] colors;
    public STile[] tiles;
    public SRow[] rows;
    public float tileAngle;
}

[System.Serializable]
public struct SSeparator
{
    public float[] startPosXYm;
    public float[] endPosXYm;
    public string type;
}

[System.Serializable]
public struct SPillar
{
    public float[] centerXY;
    public float[] sizeXY;
    public float rotation;
}

[System.Serializable]
public struct STile
{
    public string location;
    public string name;
    public string label;
    public string texture;
    public string color;
}

[System.Serializable]
public struct SRow
{
    public string name;
    public string locationY; // should be posY
    public string orientation;
}
#endregion

#region Object
[System.Serializable]
public struct STemplate
{
    public string slug;
    public string description;
    public string category;
    public float[] sizeWDHmm;
    public string fbxModel;
    public Dictionary<string, string> attributes;
    public SColor[] colors;
    public STemplateChild[] components;
    public STemplateChild[] slots;
    public STemplateSensor[] sensors;
}

[System.Serializable]
public struct STemplateChild
{
    public string location;
    public string type;
    public string elemOrient;
    public float[] elemPos;
    public float[] elemSize;
    public string labelPos;
    public string color;
    public Dictionary<string, string> attributes;
}

[System.Serializable]
public struct STemplateSensor
{
    public string location;
    public string[] elemPos;
    public float[] elemSize;
}

[System.Serializable]
public struct SColor
{
    public string name;
    public string value;
}

[System.Serializable]
public struct STempUnit
{
    public string temperatureUnit;
}

#endregion