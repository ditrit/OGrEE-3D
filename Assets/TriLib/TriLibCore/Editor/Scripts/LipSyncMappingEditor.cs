using TriLibCore.General;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    [CustomEditor(typeof(LipSyncMapping))]
    public class LipSyncMappingEditor : UnityEditor.Editor
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;

        private void OnEnable()
        {
            _skinnedMeshRenderer = ((LipSyncMapping) target).GetComponent<SkinnedMeshRenderer>();
        }

        private void OnDisable()
        {
            _skinnedMeshRenderer = null;
        }

        public override void OnInspectorGUI()
        {
            if (_skinnedMeshRenderer == null)
            {
                base.OnInspectorGUI();
                return;
            }
            GUI.enabled = false;
            var visemeToBlendShapeTargets = serializedObject.FindProperty("VisemeToBlendTargets");
            for (var i = 0; i < 14; i++)
            {
                EditorGUILayout.TextField(((LipSyncViseme) i).ToString(), _skinnedMeshRenderer.sharedMesh.GetBlendShapeName(visemeToBlendShapeTargets.GetArrayElementAtIndex(i).intValue));
            }
            GUI.enabled = true;
        }
    }
}
