using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TriLibCore.Playables
{

    /// <summary>Represents a Playable used to play Animations from its Animation Clip List using names or indices as parameters.</summary>
    public class SimpleAnimationPlayer : MonoBehaviour
    {
        /// <summary>
        /// Source animation clips.
        /// </summary>
        public IList<AnimationClip> AnimationClips;

        private PlayableGraph _playableGraph;
        private AnimationPlayableOutput _playableOutput;
        private AnimationClipPlayable _clipPlayable;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
        }

        /// <summary>Plays the Animation Clip with the given index.</summary>
        /// <param name="index">The Animation Clip index.</param>
        public void PlayAnimation(int index)
        {
            if (_animator == null || AnimationClips == null || index < 0 || index >= AnimationClips.Count)
            {
                return;
            }
            var animationClip = AnimationClips[index];
            if (_clipPlayable.IsValid())
            {
                _clipPlayable.Destroy();
            }
            _clipPlayable = AnimationPlayableUtilities.PlayClip(_animator, animationClip, out _playableGraph);
            _clipPlayable.SetApplyFootIK(false);
            _clipPlayable.SetApplyPlayableIK(false);
        }

        /// <summary>Plays the Animation Clip with the given index.</summary>
        /// <param name="name">The Animation Clip name.</param>
        public void PlayAnimation(string name)
        {
            if (_animator == null || AnimationClips == null)
            {
                return;
            }
            for (var i = 0; i < AnimationClips.Count; i++)
            {
                var animationClip = AnimationClips[i];
                if (animationClip.name == name)
                {
                    PlayAnimation(i);
                }
            }
        }
    }
}
