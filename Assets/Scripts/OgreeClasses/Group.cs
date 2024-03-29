﻿using System.Collections.Generic;
using UnityEngine;

public class Group : Item
{
    private List<GameObject> content;
    public bool isDisplayed = true;

    protected override void Start()
    {
        base.Start();
        content = new();

        string[] names = attributes["content"].Split(',');
        foreach (string rn in names)
        {
            if (Utils.GetObjectById($"{parentId}.{rn}") is GameObject go)
            {
                content.Add(go);
                go.GetComponent<Item>().group = this;
            }
        }
        DisplayContent(false);
    }

    protected override void OnDestroy()
    {
        ToggleContent(true);
        foreach (GameObject gameObject in content)
            gameObject.GetComponent<Item>().group = null;
        UiManager.instance.openedGroups.Remove(this);
        UiManager.instance.groupsList.RebuildMenu(UiManager.instance.BuildGroupButtons);
        base.OnDestroy();
    }

    ///<summary>
    /// Display or hide the rackGroup and its content.
    ///</summary>
    ///<param name="_value">true or false value</param>
    public void ToggleContent(bool _value)
    {
        isDisplayed = !_value;
        GetComponent<ObjectDisplayController>().Display(!_value, !_value, !_value);
        DisplayContent(_value);
        if (_value)
        {
            GetComponent<ObjectDisplayController>().UnsubscribeEvents();
            if (!UiManager.instance.openedGroups.Contains(this))
            {
                UiManager.instance.openedGroups.Add(this);
                UiManager.instance.openedGroups.Sort();
            }
        }
        else
        {
            ObjectDisplayController objectDisplayController = GetComponent<ObjectDisplayController>();
            objectDisplayController.SubscribeEvents();
            objectDisplayController.HandleMaterial();
            if (UiManager.instance.openedGroups.Contains(this))
                UiManager.instance.openedGroups.Remove(this);
        }
        UiManager.instance.groupsList.RebuildMenu(UiManager.instance.BuildGroupButtons);
    }

    ///<summary>
    /// Enable or disable GameObjects in <see cref="content"/>.
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    private void DisplayContent(bool _value)
    {
        foreach (GameObject go in content)
        {
            if (go && !go.GetComponent<OgreeObject>().isDoomed)
            {
                ObjectDisplayController itemsOdc = go.GetComponent<ObjectDisplayController>();
                itemsOdc.Display(_value, _value, _value);
                itemsOdc.isHiddenInGroup = !_value;
                itemsOdc.ForceHighlightCube();
                if (_value)
                    itemsOdc.SubscribeEvents();
                else
                    itemsOdc.UnsubscribeEvents();
            }
        }
    }

    ///<summary>
    /// Get all GameObjects listed in <see cref="content"/>.
    ///</summary>
    ///<returns>The list of GameObject corresponding to <see cref="content"/></returns>
    public List<GameObject> GetContent()
    {
        return content;
    }
}
