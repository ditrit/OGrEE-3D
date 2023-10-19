using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Tag : IComparable<Tag>
{
    public string slug;
    public string name;
    public string colorCode;
    public Color color;
    public List<string> linkedObjects;

    public Tag(SApiTag _src)
    {
        slug = _src.slug;
        name = _src.name;
        colorCode = _src.color;
        color = Utils.ParseHtmlColor($"#{colorCode}");
        linkedObjects = new();
    }

    public int CompareTo(Tag _other)
    {
        // A null value means that this object is greater.
        if (_other == null)
            return 1;
        else
            return name.CompareTo(_other.name);
    }

    /// <summary>
    /// Update properties of the Tag with given <paramref name="_src"/>.
    /// </summary>
    /// <param name="_src">Data from API</param>
    public void UpdateFromSApiTag(SApiTag _src)
    {
        if (slug != _src.slug)
        {
            foreach (GameObject go in GetLinkedObjects())
            {
                OgreeObject obj = go.GetComponent<OgreeObject>();
                obj.tags.Remove(slug);
                obj.tags.Add(_src.slug);
            }
        }
        slug = _src.slug;

        if (colorCode != _src.color)
            color = Utils.ParseHtmlColor($"#{_src.color}");
        colorCode = _src.color;

        UiManager.instance.tagsList.RebuildMenu(UiManager.instance.BuildTagButtons);
    }

    /// <summary>
    /// Select all objects listed in <see cref="linkedObjects"/>.
    /// </summary>
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

    /// <summary>
    /// Get a list of GameObjects corresponding to <see cref="linkedObjects"/>
    /// </summary>
    /// <returns>All GameObjects from linkedObjects</returns>
    public List<GameObject> GetLinkedObjects()
    {
        List<GameObject> list = new();
        foreach (string id in linkedObjects)
            list.Add(Utils.GetObjectById(id));
        return list;
    }
}