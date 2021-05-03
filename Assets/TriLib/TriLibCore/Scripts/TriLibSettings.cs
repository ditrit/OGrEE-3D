using System;
using System.Collections.Generic;
using TriLibCore.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TriLibCore
{
    /// <summary>
    /// Represents the TriLib project settings provider.
    /// You can override this behavior to store the settings in other places, in case you don't want to use Unity built-in PlayerPrefs.
    /// </summary>
    public class TriLibSettings : ScriptableObject, ISerializationCallbackReceiver
    {
        private Dictionary<string, bool> _boolPreferences;
        [SerializeField]
        [HideInInspector]
        private List<string> _boolKeys;
        [SerializeField]
        [HideInInspector]
        private List<bool> _boolValues;

        private static TriLibSettings GetTriLibPreferences()
        {
            var preferencesFiles = Resources.LoadAll<TriLibSettings>(string.Empty);
            TriLibSettings triLibSettings;
            if (preferencesFiles.Length == 0)
            {
#if UNITY_EDITOR
                var triLibDirectories = AssetDatabase.FindAssets("TriLibMainFolderPlaceholder");
                var triLibDirectory = triLibDirectories.Length > 0 ? FileUtils.GetFileDirectory(AssetDatabase.GUIDToAssetPath(triLibDirectories[0])) : "";
                triLibSettings = CreateInstance<TriLibSettings>();
                AssetDatabase.CreateAsset(triLibSettings, $"{triLibDirectory}/TriLibSettings.asset");
                AssetDatabase.SaveAssets();
#else
                throw new Exception("Could not find TriLib preferences file.");
#endif
            }
            else
            {
                if (preferencesFiles.Length > 1)
                {
                    Debug.LogWarning("There is more than one TriLibSettings asset, and there is only one allowed per project.");
                }
                triLibSettings = preferencesFiles[0];
            } 
            return triLibSettings;
        }

        public Dictionary<string, bool>.Enumerator GetKvp()
        {
            return _boolPreferences.GetEnumerator();
        }

        public static bool GetBool(string key)
        {
            var triLibPreferences = GetTriLibPreferences();
            if (triLibPreferences._boolPreferences == null || !triLibPreferences._boolPreferences.TryGetValue(key, out var value))
            {
                return false;
            }
            return value;
        }

        public static void SetBool(string key, bool value)
        {
            var triLibPreferences = GetTriLibPreferences();
            if (triLibPreferences._boolPreferences == null)
            {
                triLibPreferences._boolPreferences = new Dictionary<string, bool>();
            }
            triLibPreferences._boolPreferences[key] = value;
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.LogWarning("Can't save TriLib settings while in play mode. Please refer to the Project Settings/TriLib area.");
            }
            EditorUtility.SetDirty(triLibPreferences);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        public void OnBeforeSerialize()
        {
            if (_boolPreferences == null)
            {
                return;
            }
            if (_boolKeys == null || _boolValues == null)
            {
                _boolKeys = new List<string>();
                _boolValues = new List<bool>();
            }
            _boolKeys.Clear();
            _boolValues.Clear();
            foreach (var kvp in _boolPreferences)
            {
                _boolKeys.Add(kvp.Key);
                _boolValues.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            if (_boolKeys == null || _boolValues == null)
            {
                return;
            }
            if (_boolPreferences == null)
            {
                _boolPreferences = new Dictionary<string, bool>();
            }
            _boolPreferences.Clear();
            for (var i = 0; i < _boolKeys.Count; i++)
            {
                _boolPreferences.Add(_boolKeys[i], _boolValues[i]);
            }
        }
    }
}
