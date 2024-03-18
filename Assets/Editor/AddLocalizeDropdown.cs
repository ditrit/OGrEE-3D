using UnityEditor;
using UnityEngine;

/// <summary>
/// Base script from https://gist.github.com/alisahanyalcin/c9512807d6d418019ca87c8454938110
/// </summary>
public abstract class AddLocalizeDropdown
{
    [MenuItem("CONTEXT/TMP_Dropdown/Localize", false, 1)]
    private static void AddLocalizeComponent()
    {
        // add localize dropdown component to selected gameobject
        if (Selection.activeGameObject is GameObject selected)
            selected.AddComponent<LocalizeDropdown>();
    }
}