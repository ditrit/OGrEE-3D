using UnityEngine.Rendering;

namespace TriLibCore.Utils
{
    /// <summary>
    /// Represents a series of graphic settings utility methods.
    /// </summary>
    public static class GraphicsSettingsUtils
    {
        /// <summary>Returns <c>true</c> if the project is using the Standard Rendering Pipeline.</summary>
        public static bool IsUsingStandardPipeline => GraphicsSettings.renderPipelineAsset == null;

        /// <summary>Returns <c>true</c> if the project is using the Universal Rendering Pipeline.</summary>
        public static bool IsUsingUniversalPipeline => GraphicsSettings.renderPipelineAsset != null && (GraphicsSettings.renderPipelineAsset.name.StartsWith("UniversalRenderPipeline") || GraphicsSettings.renderPipelineAsset.name.StartsWith("UniversalRP"));

        /// <summary>Returns <c>true</c> if the project is using the HDRP Rendering Pipeline.</summary>
        public static bool IsUsingHDRPPipeline => GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.name.StartsWith("HD");
    }
}
