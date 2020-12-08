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
    /// Build fullname with all the parents and returns it.
    ///</summary>
    ///<returns>The updated hierarchy name</returns>
    public string UpdateHierarchyName()
    {
        fullname = name;

        List<string> parentsName = new List<string>();
        Transform parent = transform.parent;
        if (parent)
        {
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
        return fullname;
    }

}
