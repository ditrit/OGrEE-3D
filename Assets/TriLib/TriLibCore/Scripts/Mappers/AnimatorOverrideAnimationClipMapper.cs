using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper used to fill Animator Override Animation Clips.</summary>
    public class AnimatorOverrideAnimationClipMapper : AnimationClipMapper
    {
        /// <summary>
        /// Animator controller override to use on the animator.
        /// </summary>
        public AnimatorOverrideController AnimatorOverrideController;

        ///<inheritdoc />
        public override AnimationClip[] MapArray(AssetLoaderContext assetLoaderContext, AnimationClip[] sourceAnimationClips)
        {
            var animator = assetLoaderContext.RootGameObject.GetComponent<Animator>();
            if (animator == null || AnimatorOverrideController == null)
            {
                if (assetLoaderContext.Options.ShowLoadingWarnings)
                {
                    Debug.LogWarning("Tried to execute an AnimatorOverrideController Mapper on a GameObject without an Animator or without setting an AnimatorOverrideController.");
                }
                return sourceAnimationClips;
            }
            animator.runtimeAnimatorController = AnimatorOverrideController;
            return sourceAnimationClips;
        }
    }
}