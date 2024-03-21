using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

/// <summary>
/// An extention of <see cref="LocalizedString"/> to provide simplier constructor
/// </summary>
public class ExtendedLocalizedString : LocalizedString
{
    /// <summary>
    /// Create a <see cref="LocalizedString"/> with a <see cref="StringVariable"/> named "str".
    /// </summary>
    /// <param name="_tableReference">Reference to the String Table Collection. This can either be the name of the collection as a string or the Collection Guid as a <see cref="System.Guid"/>.</param>
    /// <param name="_entryReference">Reference to the String Table Collection entry. This can either be the name of the Key as a string or the long Key Id.</param>
    /// <param name="_strVariable">Value of the created "str" variable.</param>
    public ExtendedLocalizedString(TableReference _tableReference, TableEntryReference _entryReference, string _strVariable)
    {
        TableReference = _tableReference;
        TableEntryReference = _entryReference;
        Add("str", new StringVariable { Value = _strVariable });
    }

    /// <summary>
    /// Create a <see cref="LocalizedString"/> with a <see cref="StringVariable"/> named "str".
    /// </summary>
    /// <param name="_tableReference">Reference to the String Table Collection. This can either be the name of the collection as a string or the Collection Guid as a <see cref="System.Guid"/>.</param>
    /// <param name="_entryReference">Reference to the String Table Collection entry. This can either be the name of the Key as a string or the long Key Id.</param>
    /// <param name="_strVariables">A list of values used for creating "strX" variables, where X is the index in the list.</param>
    public ExtendedLocalizedString(TableReference _tableReference, TableEntryReference _entryReference, List<string> _strVariables)
    {
        TableReference = _tableReference;
        TableEntryReference = _entryReference;
        for (int i = 0; i < _strVariables.Count; i++)
            Add($"str{i}", new StringVariable { Value = _strVariables[i] });

    }
}
