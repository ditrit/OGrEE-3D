using UnityEngine;

namespace TriLibCore
{
    /// <summary>Represents the Viseme to Blend-Shape Keys mapped indices. The indices are generated from the Lip Sync Mappers.</summary>
    public class LipSyncMapping : MonoBehaviour
    {
        /// <summary>
        /// Viseme to blend-targets mapped indices.
        /// </summary>
        public int[] VisemeToBlendTargets;
    }
}