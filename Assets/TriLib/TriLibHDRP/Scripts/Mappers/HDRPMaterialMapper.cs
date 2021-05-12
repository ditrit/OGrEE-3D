using System;
using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace TriLibCore.HDRP.Mappers
{
    /// <summary>Represents a Material Mapper that converts TriLib Materials into Unity HDRP Materials.</summary>
    [Serializable]
    [CreateAssetMenu(menuName = "TriLib/Mappers/Material/HDRP Material Mapper", fileName = "HDRPMaterialMapper")]
    public class HDRPMaterialMapper : MaterialMapper
    {
        #region Standard
        public override Material MaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRP");

        public override Material CutoutMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlphaCutout");

        public override Material TransparentMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlpha");

        public override Material TransparentComposeMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlpha");
        #endregion

        #region Autodesk
        public override Material AutodeskMaterialPreset => Resources.Load<Material>("Materials/HDRP/Autodesk/TriLibAutodeskHDRP");

        public override Material AutodeskCutoutMaterialPreset => Resources.Load<Material>("Materials/HDRP/Autodesk/TriLibAutodeskHDRPAlphaCutout");

        public override Material AutodeskTransparentMaterialPreset => Resources.Load<Material>("Materials/HDRP/Autodesk/TriLibAutodeskHDRPAlpha");

        public override Material AutodeskTransparentComposeMaterialPreset => Resources.Load<Material>("Materials/HDRP/Autodesk/TriLibAutodeskHDRPAlpha");
        #endregion
        public override Material LoadingMaterial => Resources.Load<Material>("Materials/HDRP/TriLibHDRPLoading");


        public override bool IsCompatible(MaterialMapperContext materialMapperContext)
        {
            return TriLibSettings.GetBool("HDRPMaterialMapper");
        }


        public override void Map(MaterialMapperContext materialMapperContext)
        {
            materialMapperContext.VirtualMaterial = new HDRPVirtualMaterial();

            CheckDiffuseColor(materialMapperContext);
            CheckDiffuseMapTexture(materialMapperContext);
            CheckNormalMapTexture(materialMapperContext);
            CheckEmissionColor(materialMapperContext);
            CheckEmissionMapTexture(materialMapperContext);
            CheckOcclusionMapTexture(materialMapperContext);

            if (materialMapperContext.Material.MaterialShadingSetup == MaterialShadingSetup.Specular)
            {
                CheckMetallicValue(materialMapperContext);
                CheckMetallicGlossMapTexture(materialMapperContext);
                CheckGlossinessValue(materialMapperContext);
                CheckSpecularTexture(materialMapperContext);
            }
            else
            {
                CheckGlossinessValue(materialMapperContext);
                CheckSpecularTexture(materialMapperContext);
                CheckMetallicValue(materialMapperContext);
                CheckMetallicGlossMapTexture(materialMapperContext);
            }

            Dispatcher.InvokeAsyncAndWait(BuildMaterial, materialMapperContext);
			if (!materialMapperContext.Material.IsAutodeskInteractive) {
				Dispatcher.InvokeAsyncAndWait(BuildHDRPMask, materialMapperContext);
			}
        }

        private void CheckDiffuseMapTexture(MaterialMapperContext materialMapperContext)
        {
            var diffuseTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.DiffuseTexture);
            var texture = LoadTexture(materialMapperContext, TextureType.Diffuse, materialMapperContext.Material.GetTextureValue(diffuseTexturePropertyName));
            ApplyDiffuseMapTexture(materialMapperContext, TextureType.Diffuse, texture);
        }

        private void ApplyDiffuseMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty(GetDiffuseTextureName(materialMapperContext), texture);
            if (texture != null && materialMapperContext.Context.Options.ApplyTexturesOffsetAndScaling)
            {
                var diffuseTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.DiffuseTexture);
                var diffuseTexture = materialMapperContext.Material.GetTextureValue(diffuseTexturePropertyName);
                materialMapperContext.VirtualMaterial.Tiling = diffuseTexture.Tiling;
                materialMapperContext.VirtualMaterial.Offset = diffuseTexture.Offset;
            }
        }

        private void CheckGlossinessValue(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.Glossiness, materialMapperContext.Material.GetGenericFloatValue(GenericMaterialProperty.Glossiness));
            materialMapperContext.VirtualMaterial.SetProperty("_Smoothness", value);
        }

        private void CheckMetallicValue(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.Metallic, materialMapperContext.Material.GetGenericFloatValue(GenericMaterialProperty.Metallic));
            materialMapperContext.VirtualMaterial.SetProperty("_Metallic", value);
        }

        private void CheckEmissionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var emissionTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.EmissionTexture);
            var texture = LoadTexture(materialMapperContext, TextureType.Emission, materialMapperContext.Material.GetTextureValue(emissionTexturePropertyName));
            ApplyEmissionMapTexture(materialMapperContext, TextureType.Emission, texture);
        }

        private void ApplyEmissionMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_EmissiveColorMap", texture);
            if (texture)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_EMISSIVE_COLOR_MAP");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                materialMapperContext.VirtualMaterial.SetProperty("_EmissiveIntensity", materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.EmissionColor, 1f));
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_EMISSIVE_COLOR_MAP");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckNormalMapTexture(MaterialMapperContext materialMapperContext)
        {
            var normalMapTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.NormalTexture);
            var texture = LoadTexture(materialMapperContext, TextureType.NormalMap, materialMapperContext.Material.GetTextureValue(normalMapTexturePropertyName));
            ApplyNormalMapTexture(materialMapperContext, TextureType.NormalMap, texture);
        }

        private void ApplyNormalMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_NormalMap", texture);
            if (texture != null)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP");
                materialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
                materialMapperContext.VirtualMaterial.SetProperty("_NormalScale", materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.NormalTexture, 1f));
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP");
                materialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP_TANGENT_SPACE");
            }
        }

        private void CheckSpecularTexture(MaterialMapperContext materialMapperContext)
        {
            var specularTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.SpecularTexture);
            var texture = LoadTexture(materialMapperContext, TextureType.Specular, materialMapperContext.Material.GetTextureValue(specularTexturePropertyName));
            ApplySpecGlossMapTexture(materialMapperContext, TextureType.Specular, texture);
        }

        private void ApplySpecGlossMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            ((HDRPVirtualMaterial)materialMapperContext.VirtualMaterial).SmoothnessTexture = texture;
        }

        private void CheckOcclusionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var occlusionMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.OcclusionTexture);
            var texture = LoadTexture(materialMapperContext, TextureType.Occlusion, materialMapperContext.Material.GetTextureValue(occlusionMapTextureName));
            ApplyOcclusionMapTexture(materialMapperContext, TextureType.Occlusion, texture);
        }

        private void ApplyOcclusionMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            ((HDRPVirtualMaterial)materialMapperContext.VirtualMaterial).OcclusionTexture = texture;
        }

        private void CheckMetallicGlossMapTexture(MaterialMapperContext materialMapperContext)
        {
            var metallicGlossMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.MetallicGlossMap);
            var texture = LoadTexture(materialMapperContext, TextureType.Metalness, materialMapperContext.Material.GetTextureValue(metallicGlossMapTextureName));
            ApplyMetallicGlossMapTexture(materialMapperContext, TextureType.Metalness, texture);
        }

        private void ApplyMetallicGlossMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            ((HDRPVirtualMaterial)materialMapperContext.VirtualMaterial).MetallicTexture = texture;
        }

        private void CheckEmissionColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValue(GenericMaterialProperty.EmissionColor);
            materialMapperContext.VirtualMaterial.SetProperty("_EmissiveColor", value);
            if (value != Color.black)
            {
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                materialMapperContext.VirtualMaterial.SetProperty("_EmissiveIntensity", materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.EmissionColor, 1f));
            }
            else
            {
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckDiffuseColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValue(GenericMaterialProperty.DiffuseColor) * materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.DiffuseColor, 1f);
            value.a *= materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.AlphaValue, materialMapperContext.Material.GetGenericFloatValue(GenericMaterialProperty.AlphaValue));
            if (!materialMapperContext.VirtualMaterial.HasAlpha && value.a < 1f)
            {
                materialMapperContext.VirtualMaterial.HasAlpha = true;
            }
            materialMapperContext.VirtualMaterial.SetProperty("_BaseColor", value);
        }

        private void BuildHDRPMask(MaterialMapperContext materialMapperContext)
        {
            if (materialMapperContext.UnityMaterial == null)
            {
                return;
            }
            var hdrpVirtualMaterial = (HDRPVirtualMaterial)materialMapperContext.VirtualMaterial;
            var maskBaseTexture = hdrpVirtualMaterial.MetallicTexture ?? hdrpVirtualMaterial.OcclusionTexture ?? hdrpVirtualMaterial.DetailMaskTexture ?? hdrpVirtualMaterial.SmoothnessTexture;
            if (maskBaseTexture == null)
            {
                if (materialMapperContext.Context.Options.UseMaterialKeywords)
                {
                    materialMapperContext.UnityMaterial.DisableKeyword("_MASKMAP");
                }
                return;
            }
            var graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            var renderTexture = new RenderTexture(maskBaseTexture.width, maskBaseTexture.height, 0, graphicsFormat);
            renderTexture.name = $"{(string.IsNullOrWhiteSpace(maskBaseTexture.name) ? "Unnamed" : maskBaseTexture.name)}_Mask";
            renderTexture.useMipMap = materialMapperContext.Context.Options.GenerateMipmaps;
            renderTexture.autoGenerateMips = false;
            var material = new Material(Shader.Find("Hidden/TriLib/BuildHDRPMask"));
            if (hdrpVirtualMaterial.MetallicTexture != null)
            {
                material.SetTexture("_MetallicTex", hdrpVirtualMaterial.MetallicTexture);
            }
            if (hdrpVirtualMaterial.OcclusionTexture != null)
            {
                material.SetTexture("_OcclusionTex", hdrpVirtualMaterial.OcclusionTexture);
            }
            if (hdrpVirtualMaterial.DetailMaskTexture != null)
            {
                material.SetTexture("_DetailMaskTex", hdrpVirtualMaterial.DetailMaskTexture);
            }
            if (hdrpVirtualMaterial.SmoothnessTexture != null)
            {
                material.SetTexture("_SmoothnessTex", hdrpVirtualMaterial.SmoothnessTexture);
            }
            Graphics.Blit(null, renderTexture, material);
            if (materialMapperContext.Context.Options.GenerateMipmaps)
            {
                renderTexture.GenerateMips();
            }
            if (materialMapperContext.Context.Options.UseMaterialKeywords)
            {
                materialMapperContext.UnityMaterial.EnableKeyword("_MASKMAP");
            }
            materialMapperContext.UnityMaterial.SetTexture("_MaskMap", renderTexture);
            materialMapperContext.VirtualMaterial.TextureProperties.Add("_MaskMap", renderTexture);
            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }
		
		public override string GetDiffuseTextureName(MaterialMapperContext materialMapperContext) 
		{
            return "_BaseColorMap";
		}
    }
}