using System.Collections.Generic;
using TriLibCore.General;
using TriLibCore.Interfaces;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that search Visemes by searching Blend-Shape Keys names.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/LypSync/By Name Lip Sync Mapper", fileName = "ByNameLipSyncMapper")]
    public class ByNameLipSyncMapper : LipSyncMapper
    {
        /// <summary>
        /// String comparison mode to use on the mapping.
        /// </summary>
        [Header("Left = Blend-Shape Key Name, Right = Viseme Name")]
        public StringComparisonMode StringComparisonMode;

        /// <summary>
        /// Is the string comparison case insensitive?
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// The viseme candidates.
        /// A viseme candidate is a reference between visemes and valid blend-shape names for the viseme.
        /// </summary>
        public List<VisemeCandidate> VisemeCandidates;

        /// <inheritdoc />
        protected override int MapViseme(AssetLoaderContext assetLoaderContext, LipSyncViseme viseme, IGeometryGroup geometryGroup)
        {
            for (var i = 0; i < VisemeCandidates.Count; i++)
            {
                var visemeCandidate = VisemeCandidates[i];
                if (visemeCandidate.Viseme == viseme)
                {
                    foreach (var candidateName in visemeCandidate.CandidateNames)
                    {
                        for (var j = 0; j < geometryGroup.BlendShapeKeys.Count; j++)
                        {
                            var blendShapeGeometryBinding = geometryGroup.BlendShapeKeys[j];
                            if (Utils.StringComparer.Matches(StringComparisonMode, CaseInsensitive, blendShapeGeometryBinding.Name, candidateName))
                            {
                                return j;
                            }
                        }
                    }
                }
            }

            return -1;
        }
    }
}
