using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public class ImporterOption : GUIContent
    {
        public readonly string Namespace;
        public readonly PluginImporter PluginImporter;

        public ImporterOption(string name, string @namespace, PluginImporter pluginImporter) : base(name)
        {
            Namespace = @namespace;
            PluginImporter = pluginImporter;
        }
    }
}