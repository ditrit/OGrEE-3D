using UnityEngine;

public class Domain : OgreeObject
{
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        name = _src.name;
        id = _src.id;
        parentId = _src.parentId;
        category = _src.category;
        domain = _src.domain;
        description = _src.description;
        attributes = _src.attributes;
    }

    /// <summary>
    /// Get the color stored in attributes["color"]
    /// </summary>
    /// <returns>The Color value</returns>
    public Color GetColor()
    {
        return Utils.ParseHtmlColor($"#{attributes["color"]}");
    }
}
