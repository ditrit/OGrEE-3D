using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TriLibCore.Mappers;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public class TriLibSettingsProvider : SettingsProvider
    {
        private class Styles
        {
            public static readonly GUIStyle Group = new GUIStyle { padding = new RectOffset(10, 10, 5, 5) };
        }

        private const string ReadersFileTemplate = "//Auto-generated: Do not modify this file!\n\nusing System.Collections;\nusing System.Collections.Generic;\n{0}\nnamespace TriLibCore\n{{\n    public class Readers\n    {{\n        public static IList<string> Extensions\n        {{\n            get\n            {{\n                var extensions = new List<string>();{1}\n                return extensions;\n            }}\n        }}\n        public static ReaderBase FindReaderForExtension(string extension)\n        {{\n\t\t\t{2}\n            return null;\n        }}\n    }}\n}}";

        private readonly List<ImporterOption> _importerOptions;
        private static string _settingsFilePath;

        public TriLibSettingsProvider(string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            var settingsAssetGuids = AssetDatabase.FindAssets("TriLibReaders");
            if (settingsAssetGuids.Length > 0)
            {
                _settingsFilePath = AssetDatabase.GUIDToAssetPath(settingsAssetGuids[0]);
            }
            else
            {
                throw new Exception("Could not find TriLibReaders.cs file. Please re-import TriLib package.");
            }
            _importerOptions = new List<ImporterOption>();
            var pluginImporters = PluginImporter.GetAllImporters();
            foreach (var pluginImporter in pluginImporters)
            {
                if (!pluginImporter.isNativePlugin && pluginImporter.assetPath.Contains("TriLibCore."))
                {
                    var assembly = Assembly.LoadFile(pluginImporter.assetPath);
                    foreach (var type in assembly.ExportedTypes)
                    {
                        if (type.BaseType == typeof(ReaderBase))
                        {
                            _importerOptions.Add(new ImporterOption(type.Name, type.Namespace, pluginImporter));
                        }
                    }
                }
            }
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            var contentWidth = GUILayoutUtility.GetLastRect().width * 0.5f;
            EditorGUIUtility.labelWidth = contentWidth;
            EditorGUIUtility.fieldWidth = contentWidth;
            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label("Enabled Readers", EditorStyles.boldLabel);
            GUILayout.Label("You can disable file formats you don't use here");
            EditorGUILayout.Space();
            var changed = false;
            foreach (var importerOption in _importerOptions)
            {
                var value = importerOption.PluginImporter.GetCompatibleWithAnyPlatform();
                var newValue = EditorGUILayout.Toggle(importerOption, value);
                if (newValue != value)
                {
                    importerOption.PluginImporter.SetCompatibleWithAnyPlatform(newValue);
                    changed = true;
                }
            }
            if (changed)
            {
                string usings = null;
                string extensions = null;
                string findReader = null;
                foreach (var importerOption in _importerOptions)
                {
                    if (importerOption.PluginImporter.GetCompatibleWithAnyPlatform())
                    {
                        extensions += $"\n\t\t\t\textensions.AddRange({importerOption.text}.GetExtensions());";
                        usings += $"using {importerOption.Namespace};\n";
                        findReader += $"\n\t\t\tif (((IList) {importerOption.text}.GetExtensions()).Contains(extension))\n\t\t\t{{\n\t\t\t\treturn new {importerOption.text}();\n\t\t\t}}";
                    }
                }
                var text = string.Format(ReadersFileTemplate, usings, extensions, findReader);
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(_settingsFilePath);
                File.WriteAllText(_settingsFilePath, text);
                EditorUtility.SetDirty(textAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUILayout.Space();
            GUILayout.Label("Material Mappers", EditorStyles.boldLabel);
            GUILayout.Label("Select the Material Mappers according your project rendering pipeline");
            EditorGUILayout.Space();
            foreach (var materialMapperName in MaterialMapper.RegisteredMappers)
            {
                var value = TriLibSettings.GetBool(materialMapperName);
                var newValue = EditorGUILayout.Toggle(materialMapperName, value);
                if (newValue != value)
                {
                    TriLibSettings.SetBool(materialMapperName, newValue);
                }
            }
            CheckMappers.Initialize();
            EditorGUILayout.Space();
            GUILayout.Label("Misc Options", EditorStyles.boldLabel);
            GUILayout.Label("Advanced Options");
            EditorGUILayout.Space();
            ShowConditionalToggle("Enable GLTF Draco Decompression (Experimental)", "TRILIB_DRACO");
            ShowConditionalToggle("Force synchronous loading", "TRILIB_FORCE_SYNC");
            ShowConditionalToggle("Change Thread names (Debug purposes only)", "TRILIB_USE_THREAD_NAMES");
            ShowConditionalToggle("Use Unity internal image loader instead of stb_image (Experimental)", "TRILIB_USE_UNITY_TEXTURE_LOADER");
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Version Notes"))
            {
                TriLibVersionNotes.ShowWindow();
            }
            if (GUILayout.Button("API Reference"))
            {
                Application.OpenURL("https://ricardoreis.net/trilib/trilib2/docs/");
            }
            if (GUILayout.Button("Support"))
            {
                Application.OpenURL("mailto:contato@ricardoreis.net");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            base.OnGUI(searchContext);
        }

        private void ShowConditionalToggle(string label, string symbol)
        {
            var currentValue = TriLibDefineSymbolsHelper.IsSymbolDefined(symbol);
            var newValue = EditorGUILayout.Toggle(label, currentValue);
            if (newValue != currentValue)
            {
                TriLibDefineSymbolsHelper.UpdateSymbol(symbol, newValue);
            }
        }

        [SettingsProvider]
        public static SettingsProvider TriLib()
        {
            var provider = new TriLibSettingsProvider("Project/TriLib", SettingsScope.Project)
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
            return provider;
        }
    }
}
