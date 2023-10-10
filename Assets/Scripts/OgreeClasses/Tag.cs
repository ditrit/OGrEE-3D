using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tag
{
    public string slug;
    public string colorCode;
    public Color color;
    public List<string> linkedObjects;

    public Tag(SApiTag _src)
    {
        slug = _src.slug;
        colorCode = _src.color;
        color = Utils.ParseHtmlColor($"#{colorCode}");
        linkedObjects = new();
    }

    /// <summary>
    /// Update properties of the Tag with given <paramref name="_src"/>.
    /// </summary>
    /// <param name="_src">Data from API</param>
    public void UpdateFromSApiTag(SApiTag _src)
    {
        slug = _src.slug;
        colorCode = _src.color;
        color = Utils.ParseHtmlColor($"#{colorCode}");
    }
}