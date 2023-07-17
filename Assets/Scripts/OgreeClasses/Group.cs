using System.Collections.Generic;
using UnityEngine;

public class Group : OObject
{
    private List<GameObject> content;
    public bool isDisplayed = true;

    private void Start()
    {
        content = new List<GameObject>();
        string[] names = attributes["content"].Split(',');

        foreach (string rn in names)
        {
            GameObject go = GameManager.instance.FindByAbsPath($"{transform.parent.GetComponent<OgreeObject>().hierarchyName}.{rn}");
            if (go)
                content.Add(go);
        }
        DisplayContent(false);
    }

    protected override void OnDestroy()
    {
        ToggleContent(true);
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
            StartCoroutine(Utils.ImportFinished());
        }
        else
            GetComponent<ObjectDisplayController>().SubscribeEvents();
    }

    ///<summary>
    /// Enable or disable racks from attributes["content"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    private void DisplayContent(bool _value)
    {
        foreach (GameObject r in GetContent())
                r?.SetActive(_value);
    }

    ///<summary>
    /// Get all GameObjects listed in attributes["content"].
    ///</summary>
    ///<returns>The list of GameObject corresponding to attributes["content"]</returns>
    public List<GameObject> GetContent()
    {
        return content;
    }
}
