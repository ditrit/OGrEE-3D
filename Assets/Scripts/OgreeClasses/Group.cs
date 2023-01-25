using System.Collections.Generic;
using UnityEngine;

public class Group : OObject
{
    public bool isDisplayed = true;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ToggleContent("true");
    }

    ///<summary>
    /// Display or hide the rackGroup and its content.
    ///</summary>
    ///<param name="_value">"true" or "false" value</param>
    public void ToggleContent(string _value)
    {
        if (_value != "true" && _value != "false")
            return;

        if (_value == "true")
        {
            isDisplayed = false;
            UpdateAlpha("true");
            DisplayContent(true);
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            StartCoroutine(Utils.ImportFinished());
        }
        else
        {
            isDisplayed = true;
            UpdateAlpha("false");
            DisplayContent(false);
            transform.GetChild(0).GetComponent<Collider>().enabled = true;
        }
    }

    ///<summary>
    /// Enable or disable racks from attributes["content"].
    ///</summary>
    ///<param name="_value">The bool value to apply</param>
    public void DisplayContent(bool _value)
    {
        foreach (GameObject r in GetContent())
            r.SetActive(_value);

        GetComponent<DisplayObjectData>().ToggleLabel(!_value);
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
