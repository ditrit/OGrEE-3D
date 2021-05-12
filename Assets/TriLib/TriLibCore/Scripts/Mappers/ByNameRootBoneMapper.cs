using System.Collections.Generic;
using TriLibCore.General;
using TriLibCore.Interfaces;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that searches for a root bone on the Models by the bone names.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Root Bone/By Name Root Bone Mapper", fileName = "ByNameRootBoneMapper")]
    public class ByNameRootBoneMapper : RootBoneMapper
    {
        /// <summary>
        /// String comparison mode to use on the mapping.
        /// </summary>
        [Header("Left = Loaded GameObjects Names, Right = Names in RootBoneNames")]
        public StringComparisonMode StringComparisonMode;

        /// <summary>
        /// Is the string comparison case insensitive?
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// Root bone names to be searched.
        /// </summary>
        public string[] RootBoneNames = { "Hips", "Bip01", "Pelvis" };

        /// <inheritdoc />        
        public override Transform Map(AssetLoaderContext assetLoaderContext, IList<Transform> bones)
        {
            if (RootBoneNames != null)
            {
                for (var i = 0; i < RootBoneNames.Length; i++)
                {
                    var rootBoneName = RootBoneNames[i];
                    var found = FindDeepChild(bones, rootBoneName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return base.Map(assetLoaderContext, bones);
        }

        private Transform FindDeepChild(IList<Transform> transforms, string right)
        {
            foreach (var transform in transforms)
            {
                if (StringComparer.Matches(StringComparisonMode, CaseInsensitive, transform.name, right))
                {
                    return transform;
                }
            }
            return null;
        }
    }
}