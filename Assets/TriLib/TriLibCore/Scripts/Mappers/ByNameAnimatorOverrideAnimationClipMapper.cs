using System.Collections.Generic;
using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that finds Animator Override Animation Clips by name-matching.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Animation Clip/By Name Animator Override Animation Clip Mapper", fileName = "ByNameAnimatorOverrideAnimationClipMapper")]
    public class ByNameAnimatorOverrideAnimationClipMapper : AnimatorOverrideAnimationClipMapper
    {
        /// <summary>
        /// String comparison mode to use on the mapping.
        /// </summary>
        [Header("Left = Animator Override Clip Names, Right = Loaded Clip Names")]
        public StringComparisonMode StringComparisonMode;

        /// <summary>
        /// Is the string comparison case insensitive?
        /// </summary>
        public bool CaseInsensitive = true;

        /// <inheritdoc />
        public override AnimationClip[] MapArray(AssetLoaderContext assetLoaderContext, AnimationClip[] sourceAnimationClips)
        {
            if (AnimatorOverrideController != null)
            {
                for (var i = 0; i < sourceAnimationClips.Length; i++)
                {
                    var animationClip = sourceAnimationClips[i];
                    var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(AnimatorOverrideController.overridesCount);
                    AnimatorOverrideController.GetOverrides(overrides);
                    for (var j = 0; j < overrides.Count; j++)
                    {
                        var kvp = overrides[j];
                        var keyName = kvp.Key.name;
                        var clipName = animationClip.name;
                        if (StringComparer.Matches(StringComparisonMode, CaseInsensitive, keyName, clipName))
                        {
                            overrides[j] = new KeyValuePair<AnimationClip, AnimationClip>(kvp.Key, animationClip);
                        }
                    }

                    AnimatorOverrideController.ApplyOverrides(overrides);
                }
            }
            return base.MapArray(assetLoaderContext, sourceAnimationClips);
        }
    }
}