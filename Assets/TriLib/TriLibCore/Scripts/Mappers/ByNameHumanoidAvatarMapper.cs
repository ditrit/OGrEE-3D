using System.Collections.Generic;
using TriLibCore.General;
using TriLibCore.Extensions;
using UnityEngine;
using HumanLimit = TriLibCore.General.HumanLimit;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a Mapper that finds humanoid Avatar bones by name-matching.</summary>
    [CreateAssetMenu(menuName = "TriLib/Mappers/Humanoid/By Name Humanoid Avatar Mapper", fileName = "ByNameHumanoidAvatarMapper")]
    public class ByNameHumanoidAvatarMapper : HumanoidAvatarMapper
    {
        /// <summary>
        /// String comparison mode to use on the mapping.
        /// </summary>
        [Header("Left = Loaded GameObjects Names, Right = Names in BonesMapping.BoneNames")]
        public StringComparisonMode stringComparisonMode;

        /// <summary>
        /// Is the string comparison case insensitive?
        /// </summary>
        public bool CaseInsensitive = true;

        /// <summary>
        /// The human bone to Unity bone relationship list.
        /// </summary>
        public List<BoneMapping> BonesMapping;

        /// <inheritdoc />
        public override Dictionary<BoneMapping, Transform> Map(AssetLoaderContext assetLoaderContext)
        {
            var mapping = new Dictionary<BoneMapping, Transform>();
            for (var i = 0; i < BonesMapping.Count; i++)
            {
                var boneMapping = BonesMapping[i];
                if (boneMapping.BoneNames != null)
                {
                    var found = false;
                    for (var j = 0; j < boneMapping.BoneNames.Length; j++)
                    {
                        var boneName = boneMapping.BoneNames[j];
                        var transform = assetLoaderContext.RootGameObject.transform.FindDeepChild(boneName, stringComparisonMode, CaseInsensitive);
                        if (transform == null)
                        {
                            continue;
                        }

                        var model = assetLoaderContext.Models[transform.gameObject];
                        if (!model.IsBone)
                        {
                            continue;
                        }

                        mapping.Add(boneMapping, transform);
                        found = true;
                        break;
                    }

                    if (!found && !IsBoneOptional(boneMapping.HumanBone))
                    {
                        if (assetLoaderContext.Options.ShowLoadingWarnings)
                        {
                            Debug.LogWarning($"Could not find bone '{boneMapping.HumanBone}'");
                        }

                        mapping.Clear();
                        return mapping;
                    }
                }
            }

            return mapping;
        }

        private static bool IsBoneOptional(HumanBodyBones humanBodyBones)
        {
            return !HumanTrait.RequiredBone((int)humanBodyBones);
        }

        /// <summary>Adds a new mapping item, containing the humanoid bone type, the limits, and the list of names to look for to the query.</summary>
        /// <param name="humanBodyBones">The Human Body Bones (Humanoid Bone type).</param>
        /// <param name="humanLimit">The bone Human Limit.</param>
        /// <param name="boneNames">The bones Transform names.</param>
        public void AddMapping(HumanBodyBones humanBodyBones, HumanLimit humanLimit, params string[] boneNames)
        {
            if (BonesMapping == null)
            {
                BonesMapping = new List<BoneMapping>();
            }
            BonesMapping.Add(new BoneMapping(humanBodyBones, humanLimit, boneNames));
        }
    }
}