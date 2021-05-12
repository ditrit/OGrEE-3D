using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public static class CheckMappers
    {
        [InitializeOnEnterPlayMode]
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            var hasAnyMapper = false;
            foreach (var materialMapperName in MaterialMapper.RegisteredMappers)
            {
                if (TriLibSettings.GetBool(materialMapperName))
                {
                    hasAnyMapper = true;
                    break;
                }
            }
            if (!hasAnyMapper)
            {
                string materialMapper;
                if (GraphicsSettingsUtils.IsUsingHDRPPipeline)
                {
                    materialMapper = "HDRPMaterialMapper";
                }
                else if (GraphicsSettingsUtils.IsUsingUniversalPipeline)
                {
                    materialMapper = "UniversalRPMaterialMapper";
                }
                else
                {
                    materialMapper = "StandardMaterialMapper";
                }
                Debug.Log($"TriLib is configured to use the '{materialMapper}' Material Mapper. If you want to use different Material Mappers, you can change this setting on the Project Settings/TriLib area.");
                TriLibSettings.SetBool(materialMapper, true);
            }
        }

        [MenuItem("TriLib/Select Material Mappers based on Rendering Pipeline")]
        public static void AutoSelect()
        {
            foreach (var materialMapperName in MaterialMapper.RegisteredMappers)
            {
                TriLibSettings.SetBool(materialMapperName, false);
            }
            string materialMapper;
            if (GraphicsSettingsUtils.IsUsingHDRPPipeline)
            {
                materialMapper = "HDRPMaterialMapper";
            }
            else if (GraphicsSettingsUtils.IsUsingUniversalPipeline)
            {
                materialMapper = "UniversalRPMaterialMapper";
            }
            else
            {
                materialMapper = "StandardMaterialMapper";
            }
            SelectMapper(materialMapper);
        }

        public static void SelectMapper(string materialMapper)
        {

            Debug.Log($"TriLib is configured to use the '{materialMapper}' Material Mapper. If you want to use different Material Mappers, you can change this setting on the Project Settings/TriLib area.");
            TriLibSettings.SetBool(materialMapper, true);
        }
    }
}