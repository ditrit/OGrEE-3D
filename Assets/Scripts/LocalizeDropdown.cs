using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// Base script from https://gist.github.com/alisahanyalcin/c9512807d6d418019ca87c8454938110
/// </summary>
public class LocalizeDropdown : MonoBehaviour
{
    [SerializeField] private List<LocalizedString> dropdownOptions;
    private TMP_Dropdown tmpDropdown;

    private void Awake()
    {
        if (!tmpDropdown)
            tmpDropdown = GetComponent<TMP_Dropdown>();

        LocalizationSettings.SelectedLocaleChanged += ChangedLocale;
        ChangedLocale(LocalizationSettings.SelectedLocale);
    }

    private void ChangedLocale(Locale _newLocale)
    {
        List<TMP_Dropdown.OptionData> tmpDropdownOptions = new();
        foreach (LocalizedString dropdownOption in dropdownOptions)
            tmpDropdownOptions.Add(new TMP_Dropdown.OptionData(dropdownOption.GetLocalizedString()));
        tmpDropdown.options = tmpDropdownOptions;
    }
}

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