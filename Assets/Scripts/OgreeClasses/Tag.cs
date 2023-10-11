using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    /// <summary>
    /// Select all objects listed in <see cref="linkedObjects"/>.
    /// </summary>
    /// <returns></returns>
    public async Task SelectLinkedObjects()
    {
        if (linkedObjects.Count > 0)
        {
            List<GameObject> objsToSelect = Utils.GetObjectsById(linkedObjects);
            await GameManager.instance.SetCurrentItem(objsToSelect[0]);
            for (int i = 1; i < linkedObjects.Count; i++)
                await GameManager.instance.UpdateCurrentItems(objsToSelect[i]);
        }
    }
}