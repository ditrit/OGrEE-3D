﻿using System.Collections.Generic;
using UnityEngine;

public class Group : OObject
{
    public bool isDisplayed = true;

    ///<summary>
    /// Display or hide the rackGroup and its content.
    ///</summary>
    ///<param name="_value">true or false value</param>
    public void ToggleContent(bool _value)
    {
        isDisplayed = !_value;
        GetComponent<ObjectDisplayController>().Display(!_value,!_value,!_value);
        DisplayContent(_value);
        if (_value)
        {
            GetComponent<ObjectDisplayController>().UnsubscribeEvents();
            StartCoroutine(Utils.ImportFinished());
        }
        else
            GetComponent<ObjectDisplayController>().SubscribeEvents();
    }

    ///<summary>
    /// Enable or disable racks from attributes["content"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayContent(bool _value)
    {
        foreach (GameObject r in GetContent())
            r.SetActive(_value);
    }

    ///<summary>
    /// Get all GameObjects listed in attributes["content"].
    ///</summary>
    ///<returns>The list of GameObject corresponding to attributes["content"]</returns>
    public List<GameObject> GetContent()
    {
        List<GameObject> content = new List<GameObject>();
        string[] names = attributes["content"].Split(',');

        foreach (string rn in names)
        {
            GameObject go = GameManager.instance.FindByAbsPath($"{transform.parent.GetComponent<OgreeObject>().hierarchyName}.{rn}");
            if (go)
                content.Add(go);
        }
        return content;
    }
}
