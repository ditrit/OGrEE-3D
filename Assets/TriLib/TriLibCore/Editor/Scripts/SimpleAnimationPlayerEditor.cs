using TriLibCore.Playables;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    [CustomEditor(typeof(SimpleAnimationPlayer))]
    public class SimpleAnimationPlayerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var simpleAnimationPlayer = (SimpleAnimationPlayer) target;
            if (simpleAnimationPlayer.AnimationClips != null)
            {
                for (var i = 0; i < simpleAnimationPlayer.AnimationClips.Count; i++)
                {
                    var animationClip = simpleAnimationPlayer.AnimationClips[i];
                    if (animationClip != null && GUILayout.Button(animationClip.name))
                    {
                        simpleAnimationPlayer.PlayAnimation(i);
                    }
                }
            }
        }
    }
}
