using TriLibCore.General;
using TriLibCore.Playables;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that creates a Simple Animation Player used to play Animation Clips by their index or name.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Animation Clip/Simple Animation Player Animation Clip Mapper", fileName = "SimpleAnimationPlayerAnimationClipMapper")]
    public class SimpleAnimationPlayerAnimationClipMapper : AnimationClipMapper
    {
        ///<inheritdoc />
        public override AnimationClip[] MapArray(AssetLoaderContext assetLoaderContext, AnimationClip[] sourceAnimationClips)
        {
            if ((assetLoaderContext.Options.AnimationType == AnimationType.Generic || assetLoaderContext.Options.AnimationType == AnimationType.Humanoid) && sourceAnimationClips.Length > 0)
            {
                var simpleAnimationPlayer = assetLoaderContext.RootGameObject.AddComponent<SimpleAnimationPlayer>();
                simpleAnimationPlayer.AnimationClips = sourceAnimationClips;
                simpleAnimationPlayer.enabled = false;
            }
            return sourceAnimationClips;
        }
    }
}