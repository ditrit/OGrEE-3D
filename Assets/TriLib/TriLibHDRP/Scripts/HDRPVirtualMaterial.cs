using TriLibCore.General;
using UnityEngine;

namespace TriLibCore.HDRP
{
    /// <summary>
    /// Represents a container to hold HDRP Material properties temporarily.
    /// </summary>
    public class HDRPVirtualMaterial : VirtualMaterial
    {
        public Texture MetallicTexture;
        public Texture OcclusionTexture;
        public Texture DetailMaskTexture;
        public Texture SmoothnessTexture;
    }
}