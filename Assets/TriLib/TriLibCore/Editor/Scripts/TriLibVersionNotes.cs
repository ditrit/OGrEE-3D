using System.IO;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public class TriLibVersionNotes : EditorWindow
    {
        private class Styles
        {
            public const int WindowWidth = 720;
            public const int WindowHeight = 500;
            public static readonly GUIStyle HeaderStyle = new GUIStyle("label") { fontSize = 19, fontStyle = FontStyle.Bold, margin = new RectOffset(10, 10, 5, 5) };
            public static readonly GUIStyle SubHeaderStyle = new GUIStyle("label") { margin = new RectOffset(10, 10, 5, 5), fontStyle = FontStyle.Bold };
            public static readonly GUIStyle TextStyle = new GUIStyle("label") { margin = new RectOffset(20, 20, 5, 5) };
            public static readonly GUIStyle TextAreaStyle = new GUIStyle(TextStyle) { wordWrap = true };
            public static readonly GUIStyle ButtonStyle = new GUIStyle("button") { margin = new RectOffset(10, 10, 5, 5) };
        }

        private const string SkipVersionInfoKey = "TriLibSkipVersionInfo";

        private string _text;
        private bool _loaded;
        private Vector2 _changeLogScrollPosition;
        private Vector2 _notesScrollPosition;

        private static TriLibVersionNotes Instance
        {
            get
            {
                var window = GetWindow<TriLibVersionNotes>();
                window.titleContent = new GUIContent("TriLib Version Notes");
                window.minSize = new Vector2(Styles.WindowWidth, Styles.WindowHeight);
                return window;
            }
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (!EditorPrefs.GetBool(SkipVersionInfoKey))
            {
                ShowWindow();
            }
        }

        public static void ShowWindow()
        {
            Instance.Show();
        }

        private void OnDestroy()
        {
            EditorPrefs.SetBool(SkipVersionInfoKey, true);
        }

        private void OnGUI()
        {
            if (!_loaded)
            {
                var guids = AssetDatabase.FindAssets("TriLibReleaseNotes");
                if (guids.Length > 0)
                {
                    var guid = guids[0];
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    if (textAsset == null || textAsset.text == null)
                    {
                        AssetDatabase.Refresh();
                        textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                        if (textAsset == null)
                        {
                            Close();
                        }
                        return;
                    }
                    _text = textAsset.text.Replace("\\n", "\n");
                }
                else
                {
                    Close();
                }
                _loaded = true;
            }
            EditorGUILayout.BeginVertical();
            using (var stringReader = new StringReader(_text))
            {
                var changeLogOpen = false;
                var version = stringReader.ReadLine();
                GUILayout.Label($"TriLib {version}", Styles.HeaderStyle);
                for (; ; )
                {
                    var line = stringReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (line.ToLowerInvariant() == "changelog:")
                    {
                        EditorGUILayout.Space();
                        GUILayout.Label("Changelog", Styles.SubHeaderStyle);
                        _changeLogScrollPosition = GUILayout.BeginScrollView(_changeLogScrollPosition, GUILayout.Height(260f));
                        changeLogOpen = true;
                    }
                    else if (line.ToLowerInvariant() == "version notes:")
                    {
                        if (changeLogOpen)
                        {
                            GUILayout.EndScrollView();
                            changeLogOpen = false;
                        }
                        EditorGUILayout.Space();
                        GUILayout.Label("Version Notes", Styles.SubHeaderStyle);
                        var versionInfo = stringReader.ReadToEnd();
                        _notesScrollPosition = EditorGUILayout.BeginScrollView(_notesScrollPosition);
                        EditorGUILayout.TextArea(versionInfo, Styles.TextAreaStyle);
                        EditorGUILayout.EndScrollView();
                        break;
                    }
                    else
                    {
                        GUILayout.Label(line, Styles.TextStyle);
                    }
                }
                if (changeLogOpen)
                {
                    GUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
                GUILayout.Label("You can show this window on the Project Settings/TriLib area", Styles.SubHeaderStyle);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Close", Styles.ButtonStyle))
                {
                    Close();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
