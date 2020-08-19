using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyName : MonoBehaviour
{
    public string fullname;
    
    private void OnEnable()
    {
        UpdateHierarchyName();
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

        parentsName.Add(parent.name);
        while (parent)
        {
            parent = parent.parent;
            if (parent)
                parentsName.Add(parent.name);
        }
        
        foreach (string str in parentsName)
            fullname = $"{str}.{fullname}";
    }

}
