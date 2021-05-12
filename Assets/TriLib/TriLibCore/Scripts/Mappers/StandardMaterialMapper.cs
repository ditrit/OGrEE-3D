using System;
using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Material Mapper that converts TriLib Materials into Unity Standard Materials.</summary>
    [Serializable]
    [CreateAssetMenu(menuName = "TriLib/Mappers/Material/Standard Material Mapper", fileName = "StandardMaterialMapper")]
    public class StandardMaterialMapper : MaterialMapper
    {
        #region Standard
        public override Material MaterialPreset => Resources.Load<Material>("Materials/Standard/Standard/TriLibStandard");

        public override Material CutoutMaterialPreset => Resources.Load<Material>("Materials/Standard/Standard/TriLibStandardAlphaCutout");

        public override Material TransparentComposeMaterialPreset => Resources.Load<Material>("Materials/Standard/Standard/TriLibStandardAlphaCompose");

        public override Material TransparentMaterialPreset => Resources.Load<Material>("Materials/Standard/Standard/TriLibStandardAlpha");
        #endregion

        #region Specular
        public override Material SpecularMaterialPreset => Resources.Load<Material>("Materials/Standard/Specular/TriLibStandardSpecular");

        public override Material SpecularCutoutMaterialPreset => Resources.Load<Material>("Materials/Standard/Specular/TriLibStandardSpecularAlphaCutout");

        public override Material SpecularTransparentMaterialPreset => Resources.Load<Material>("Materials/Standard/Specular/TriLibStandardSpecularAlpha");

        public override Material SpecularTransparentComposeMaterialPreset => Resources.Load<Material>("Materials/Standard/Specular/TriLibStandardSpecularAlphaCompose");
        #endregion

        #region Autodesk
        public override Material AutodeskMaterialPreset => Resources.Load<Material>("Materials/Standard/Autodesk/TriLibStandardAutodesk");

        public override Material AutodeskCutoutMaterialPreset => Resources.Load<Material>("Materials/Standard/Autodesk/TriLibStandardAutodeskAlphaCutout");

        public override Material AutodeskTransparentMaterialPreset => Resources.Load<Material>("Materials/Standard/Autodesk/TriLibStandardAutodeskAlpha");

        public override Material AutodeskTransparentComposeMaterialPreset => Resources.Load<Material>("Materials/Standard/Autodesk/TriLibStandardAutodeskAlphaCompose");
        #endregion

        public override Material LoadingMaterial => Resources.Load<Material>("Materials/Standard/TriLibStandardLoading");


        public override bool IsCompatible(MaterialMapperContext materialMapperContext)
        {
            return TriLibSettings.GetBool("StandardMaterialMapper");
        }


        public override void Map(MaterialMapperContext materialMapperContext)
        {
            materialMapperContext.VirtualMaterial = new VirtualMaterial();

            CheckDiffuseColor(materialMapperContext);
            CheckDiffuseMapTexture(materialMapperContext);
            CheckNormalMapTexture(materialMapperContext);
            CheckEmissionColor(materialMapperContext);
            CheckEmissionMapTexture(materialMapperContext);
            CheckOcclusionMapTexture(materialMapperContext);
            CheckParallaxMapTexture(materialMapperContext);

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
        }

        private void CheckDiffuseMapTexture(MaterialMapperContext materialMapperContext)
        {
            var diffuseTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.DiffuseTexture);
            var textureValue = materialMapperContext.Material.GetTextureValue(diffuseTexturePropertyName);
            var texture = LoadTexture(materialMapperContext, TextureType.Diffuse, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
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
            materialMapperContext.VirtualMaterial.SetProperty("_Glossiness", value);
            materialMapperContext.VirtualMaterial.SetProperty("_GlossMapScale", value);
        }

        private void CheckMetallicValue(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.Metallic, materialMapperContext.Material.GetGenericFloatValue(GenericMaterialProperty.Metallic));
            materialMapperContext.VirtualMaterial.SetProperty("_Metallic", value);
        }

        private void CheckEmissionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var emissionTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.EmissionTexture);
            var textureValue = materialMapperContext.Material.GetTextureValue(emissionTexturePropertyName);
            var texture = LoadTexture(materialMapperContext, TextureType.Emission, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplyEmissionMapTexture(materialMapperContext, TextureType.Emission, texture);
        }

        private void ApplyEmissionMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_EmissionMap", texture);
            if (texture)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_EMISSION");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_EMISSION");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckNormalMapTexture(MaterialMapperContext materialMapperContext)
        {
            var normalMapTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.NormalTexture);
            var textureValue = materialMapperContext.Material.GetTextureValue(normalMapTexturePropertyName);
            var texture = LoadTexture(materialMapperContext, TextureType.NormalMap, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplyNormalMapTexture(materialMapperContext, TextureType.NormalMap, texture);
        }

        private void ApplyNormalMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_BumpMap", texture);
            if (texture != null)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP");
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP");
            }
        }

        private void CheckSpecularTexture(MaterialMapperContext materialMapperContext)
        {
            var specularTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.SpecularTexture);
            var textureValue = materialMapperContext.Material.GetTextureValue(specularTexturePropertyName);
            var texture = LoadTexture(materialMapperContext, TextureType.Specular, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplySpecGlossMapTexture(materialMapperContext, TextureType.Specular, texture);
        }

        private void ApplySpecGlossMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_SpecGlossMap", texture);
            if (texture != null)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_SPECGLOSSMAP");
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_SPECGLOSSMAP");
            }
        }

        private void CheckOcclusionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var occlusionMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.OcclusionTexture);
            var textureValue = materialMapperContext.Material.GetTextureValue(occlusionMapTextureName);
            var texture = LoadTexture(materialMapperContext, TextureType.Occlusion, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplyOcclusionMapTexture(materialMapperContext, TextureType.Occlusion, texture);
        }

        private void ApplyOcclusionMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_OcclusionMap", texture);
            if (texture != null)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_OCCLUSIONMAP");
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_OCCLUSIONMAP");
            }
        }

        private void CheckParallaxMapTexture(MaterialMapperContext materialMapperContext)
        {
            var parallaxMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.ParallaxMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(parallaxMapTextureName);
            var texture = LoadTexture(materialMapperContext, TextureType.Parallax, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplyParallaxMapTexture(materialMapperContext, TextureType.Parallax, texture);
        }

        private void ApplyParallaxMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_ParallaxMap", texture);
            if (texture)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_PARALLAXMAP");
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_PARALLAXMAP");
            }
        }

        private void CheckMetallicGlossMapTexture(MaterialMapperContext materialMapperContext)
        {
            var metallicGlossMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.MetallicGlossMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(metallicGlossMapTextureName);
            var texture = LoadTexture(materialMapperContext, TextureType.Metalness, textureValue);
            CheckTextureOffsetAndScaling(materialMapperContext, textureValue, texture);
            ApplyMetallicGlossMapTexture(materialMapperContext, TextureType.Metalness, texture);
        }

        private void ApplyMetallicGlossMapTexture(MaterialMapperContext materialMapperContext, TextureType textureType, Texture texture)
        {
            materialMapperContext.VirtualMaterial.SetProperty("_MetallicGlossMap", texture);
            if (texture != null)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_METALLICGLOSSMAP");
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_METALLICGLOSSMAP");
            }
        }

        private void CheckEmissionColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValue(GenericMaterialProperty.EmissionColor) * materialMapperContext.Material.GetGenericPropertyValueMultiplied(GenericMaterialProperty.EmissionColor, 1f);
            materialMapperContext.VirtualMaterial.SetProperty("_EmissionColor", value);
            if (value != Color.black)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_EMISSION");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_EMISSION");
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
            materialMapperContext.VirtualMaterial.SetProperty("_Color", value);
        }
		
		public override string GetDiffuseTextureName(MaterialMapperContext materialMapperContext) 
		{
            return "_MainTex";
		}
    }
}
