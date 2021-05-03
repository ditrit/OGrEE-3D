using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that converts legacy Animation Clips into humanoid Animation Clips.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Animation Clip/Legacy To Humanoid Animation Clip Mapper", fileName = "LegacyToHumanoidAnimationClipMapper")]

    public class LegacyToHumanoidAnimationClipMapper : AnimationClipMapper
    {
        /// <summary>
        /// Template mecanim animation clip.
        /// Unity runtime API can't access mecanim animation clip settings as root motion baking, animation loop mode, etc.
        /// So we get these settings from the template animation clip.
        /// </summary>
        public AnimationClip MecanimAnimationClipTemplate;

        /// <inheritdoc/>
        public override AnimationClip[] MapArray(AssetLoaderContext assetLoaderContext, AnimationClip[] sourceAnimationClips)
        {
            var animator = assetLoaderContext.RootGameObject.GetComponent<Animator>();
            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                animator.enabled = false;
                if (MecanimAnimationClipTemplate == null)
                {
                    if (assetLoaderContext.Options.ShowLoadingWarnings)
                    {
                        Debug.LogWarning("No MecanimAnimationClipTemplate specified when using the LegacyToHumanoidAnimationClipMapper.");
                    }
                    MecanimAnimationClipTemplate = new AnimationClip();
                    assetLoaderContext.Allocations.Add(MecanimAnimationClipTemplate);
                }
                for (var i = 0; i < sourceAnimationClips.Length; i++)
                {
                    var animationClip = HumanoidRetargeter.ConvertLegacyIntoHumanoidAnimationClip(assetLoaderContext.RootGameObject, animator.avatar, sourceAnimationClips[i], MecanimAnimationClipTemplate);
                    if (animationClip != null)
                    {
                        sourceAnimationClips[i] = animationClip;
                    }
                }
                animator.enabled = true;
            }
            return sourceAnimationClips;
        }
    }
}