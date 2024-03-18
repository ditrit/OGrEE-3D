using System.Collections.Generic;
using TMPro;
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

    /// <summary>
    /// Called when the current Locale is changed. Translate options according to used Locale
    /// </summary>
    /// <param name="_newLocale">The new Locale</param>
    private void ChangedLocale(Locale _newLocale)
    {
        List<TMP_Dropdown.OptionData> tmpDropdownOptions = new();
        foreach (LocalizedString dropdownOption in dropdownOptions)
            tmpDropdownOptions.Add(new TMP_Dropdown.OptionData(dropdownOption.GetLocalizedString()));
        tmpDropdown.options = tmpDropdownOptions;
    }
}