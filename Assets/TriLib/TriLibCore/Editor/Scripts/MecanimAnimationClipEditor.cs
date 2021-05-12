using TriLibCore.Utils;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public class MecanimAnimationClipEditor : EditorWindow
    {
        private AnimationClip _animationClip;
        private SerializedObject _serializedObject;
        private SerializedProperty _animationClipSettings;

        [MenuItem("Assets/Create/TriLib/MecanimAnimationClipTemplate")]
        public static void CreateMecanimAnimationClipTemplate()
        {
            var editorWindow = (EditorWindow)GetWindow<MecanimAnimationClipEditor>();
            editorWindow.Show();
        }

        private void Awake()
        {
            _animationClip = new AnimationClip();
            _animationClip.name = "NewMecanimAnimationClipTemplate";
            _serializedObject = new SerializedObject(_animationClip);
            _animationClipSettings = _serializedObject.FindProperty("m_AnimationClipSettings");
        }

        private void OnDestroy()
        {
            if (!AssetDatabase.Contains(_animationClip))
            {
                DestroyImmediate(_animationClip);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("m_Name"));

            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_StartTime"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_StopTime"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_OrientationOffsetY"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_Level"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_CycleOffset"));

            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_LoopTime"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_LoopBlend"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_LoopBlendOrientation"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_LoopBlendPositionY"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_LoopBlendPositionXZ"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_KeepOriginalOrientation"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_KeepOriginalPositionY"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_KeepOriginalPositionXZ"));
            EditorGUILayout.PropertyField(_animationClipSettings.FindPropertyRelative("m_HeightFromFeet"));

            if (GUILayout.Button("Create Template with given Settings"))
            {
                AssetDatabase.CreateAsset(_animationClip, $"{FileUtils.GetFileDirectory(AssetDatabase.GetAssetPath(Selection.activeObject))}/{FileUtils.SanitizePath(_animationClip.name)}.asset");
            }
        }
    }
}
