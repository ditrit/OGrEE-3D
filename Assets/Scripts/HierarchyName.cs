using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyName : MonoBehaviour
{
    public string fullname;

    private void OnEnable()
    {
        UpdateHierarchyName();
        // Should also add to allItems?
    }

    private void OnDestroy()
    {
        GameManager.gm.allItems.Remove(fullname);
    }

    ///<summary>
    /// Update hierarchyName and returns it
    ///</summary>
    ///<returns>The updated hierarchy name</returns>
    public string GetHierarchyName()
    {
        UpdateHierarchyName();
        return fullname;
    }

    ///<summary>
    /// Build fullname with all the parents
    ///</summary>
    public void UpdateHierarchyName()
    {
        fullname = name;

        List<string> parentsName = new List<string>();
        Transform parent = transform.parent;
        if (!parent)
            return;

        if (parent.GetComponent<HierarchyName>())
            parentsName.Add(parent.name);
        while (parent)
        {
            parent = parent.parent;
            if (parent && parent.GetComponent<HierarchyName>())
                parentsName.Add(parent.name);
        }

        foreach (string str in parentsName)
            fullname = $"{str}.{fullname}";
    }

}
