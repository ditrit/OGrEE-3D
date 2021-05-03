using TriLibCore.General;
using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using HumanLimit = TriLibCore.General.HumanLimit;

namespace TriLibCore.Editor
{
    public class PreBuiltResources : UnityEditor.Editor
    {

        [MenuItem("Assets/Create/TriLib/Asset Loader Options/Pre-built Asset Loader Options")]
        public static void CreatePreBuiltAssetLoaderOptions()
        {
            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            AssetDatabase.CreateAsset(assetLoaderOptions, $"{FileUtils.GetFileDirectory(AssetDatabase.GetAssetPath(Selection.activeObject))}/AssetLoaderOptions.asset");
        }
        
        [MenuItem("Assets/Create/TriLib/Mappers/Humanoid/Mixamo and Biped By Name Humanoid Avatar Mapper")]
        public static void CreateMixamoAndBipedMapper()
        {
            var humanLimit = new HumanLimit();
            var mapper = CreateInstance<ByNameHumanoidAvatarMapper>();
            mapper.AddMapping(HumanBodyBones.Head, humanLimit, "Head", "Head1");
            mapper.AddMapping(HumanBodyBones.Neck, humanLimit, "Neck", "Neck1");
            mapper.AddMapping(HumanBodyBones.Chest, humanLimit, "Spine1");
            mapper.AddMapping(HumanBodyBones.UpperChest, humanLimit, "Spine4", "Spine3", "Spine2", "Spine1");
            mapper.AddMapping(HumanBodyBones.Spine, humanLimit, "Spine");
            mapper.AddMapping(HumanBodyBones.Hips, humanLimit, "Hips", "Bip01", "Pelvis");
            mapper.AddMapping(HumanBodyBones.LeftShoulder, humanLimit, "LeftShoulder", "L Clavicle", "L_Clavicle");
            mapper.AddMapping(HumanBodyBones.LeftUpperArm, humanLimit, "LeftArm", "L UpperArm", "L_UpperArm");
            mapper.AddMapping(HumanBodyBones.LeftLowerArm, humanLimit, "LeftForeArm", "L Forearm", "L_Forearm");
            mapper.AddMapping(HumanBodyBones.LeftHand, humanLimit, "LeftHand", "L Hand", "L_Hand", "LeftWrist");
            mapper.AddMapping(HumanBodyBones.RightShoulder, humanLimit, "RightShoulder", "R Clavicle", "R_Clavicle");
            mapper.AddMapping(HumanBodyBones.RightUpperArm, humanLimit, "RightArm", "R UpperArm", "R_UpperArm");
            mapper.AddMapping(HumanBodyBones.RightLowerArm, humanLimit, "RightForeArm", "R Forearm", "R_Forearm");
            mapper.AddMapping(HumanBodyBones.RightHand, humanLimit, "RightHand", "R Hand", "R_Hand", "RightWrist");
            mapper.AddMapping(HumanBodyBones.LeftUpperLeg, humanLimit, "LeftUpLeg", "L Thigh", "L_Thigh");
            mapper.AddMapping(HumanBodyBones.LeftLowerLeg, humanLimit, "LeftLeg", "L Calf", "L_Calf");
            mapper.AddMapping(HumanBodyBones.LeftFoot, humanLimit, "LeftFoot", "L Foot", "L_Foot");
            mapper.AddMapping(HumanBodyBones.LeftToes, humanLimit, "LeftToeBase", "L Toe0", "L_Toe0");
            mapper.AddMapping(HumanBodyBones.RightUpperLeg, humanLimit, "RightUpLeg", "R Thigh", "R_Thigh");
            mapper.AddMapping(HumanBodyBones.RightLowerLeg, humanLimit, "RightLeg", "R Calf", "R_Calf");
            mapper.AddMapping(HumanBodyBones.RightFoot, humanLimit, "RightFoot", "R Foot", "R_Foot");
            mapper.AddMapping(HumanBodyBones.RightToes, humanLimit, "RightToeBase", "R Toe0", "R_Toe0");
            mapper.AddMapping(HumanBodyBones.LeftThumbProximal, humanLimit, "LeftHandThumb1", "L Finger0", "L_Finger0");
            mapper.AddMapping(HumanBodyBones.LeftThumbIntermediate, humanLimit, "LeftHandThumb2", "L Finger01", "L_Finger01");
            mapper.AddMapping(HumanBodyBones.LeftThumbDistal, humanLimit, "LeftHandThumb3", "L Finger02", "L_Finger02");
            mapper.AddMapping(HumanBodyBones.LeftIndexProximal, humanLimit, "LeftHandIndex1", "L Finger1", "L_Finger1");
            mapper.AddMapping(HumanBodyBones.LeftIndexIntermediate, humanLimit, "LeftHandIndex2", "L Finger11", "L_Finger11");
            mapper.AddMapping(HumanBodyBones.LeftIndexDistal, humanLimit, "LeftHandIndex3", "L Finger12", "L_Finger12");
            mapper.AddMapping(HumanBodyBones.LeftMiddleProximal, humanLimit, "LeftHandMiddle1", "L Finger2", "L_Finger2");
            mapper.AddMapping(HumanBodyBones.LeftMiddleIntermediate, humanLimit, "LeftHandMiddle2", "L Finger21", "L_Finger21");
            mapper.AddMapping(HumanBodyBones.LeftMiddleDistal, humanLimit, "LeftHandMiddle3", "L Finger22", "L_Finger22");
            mapper.AddMapping(HumanBodyBones.LeftRingProximal, humanLimit, "LeftHandRing1", "L Finger3", "L_Finger3");
            mapper.AddMapping(HumanBodyBones.LeftRingIntermediate, humanLimit, "LeftHandRing2", "L Finger31", "L_Finger31");
            mapper.AddMapping(HumanBodyBones.LeftRingDistal, humanLimit, "LeftHandRing3", "L Finger32", "L_Finger32");
            mapper.AddMapping(HumanBodyBones.LeftLittleProximal, humanLimit, "LeftHandPinky1", "L Finger4", "L_Finger4");
            mapper.AddMapping(HumanBodyBones.LeftLittleIntermediate, humanLimit, "LeftHandPinky2", "L Finger41", "L_Finger41");
            mapper.AddMapping(HumanBodyBones.LeftLittleDistal, humanLimit, "LeftHandPinky3", "L Finger42", "L_Finger42");
            mapper.AddMapping(HumanBodyBones.RightThumbProximal, humanLimit, "RightHandThumb1", "R Finger0", "R_Finger0");
            mapper.AddMapping(HumanBodyBones.RightThumbIntermediate, humanLimit, "RightHandThumb2", "R Finger01", "R_Finger01");
            mapper.AddMapping(HumanBodyBones.RightThumbDistal, humanLimit, "RightHandThumb3", "R Finger02", "R_Finger02");
            mapper.AddMapping(HumanBodyBones.RightIndexProximal, humanLimit, "RightHandIndex1", "R Finger1", "R_Finger1");
            mapper.AddMapping(HumanBodyBones.RightIndexIntermediate, humanLimit, "RightHandIndex2", "R Finger11", "R_Finger11");
            mapper.AddMapping(HumanBodyBones.RightIndexDistal, humanLimit, "RightHandIndex3", "R Finger12", "R_Finger12");
            mapper.AddMapping(HumanBodyBones.RightMiddleProximal, humanLimit, "RightHandMiddle1", "R Finger2", "R_Finger2");
            mapper.AddMapping(HumanBodyBones.RightMiddleIntermediate, humanLimit, "RightHandMiddle2", "R Finger21", "R_Finger21");
            mapper.AddMapping(HumanBodyBones.RightMiddleDistal, humanLimit, "RightHandMiddle3", "R Finger22", "R_Finger22");
            mapper.AddMapping(HumanBodyBones.RightRingProximal, humanLimit, "RightHandRing1", "R Finger3", "R_Finger3");
            mapper.AddMapping(HumanBodyBones.RightRingIntermediate, humanLimit, "RightHandRing2", "R Finger31", "R_Finger31");
            mapper.AddMapping(HumanBodyBones.RightRingDistal, humanLimit, "RightHandRing3", "R Finger32", "R_Finger32");
            mapper.AddMapping(HumanBodyBones.RightLittleProximal, humanLimit, "RightHandPinky1", "R Finger4", "R_Finger4");
            mapper.AddMapping(HumanBodyBones.RightLittleIntermediate, humanLimit, "RightHandPinky2", "R Finger41", "R_Finger41");
            mapper.AddMapping(HumanBodyBones.RightLittleDistal, humanLimit, "RightHandPinky3", "R Finger42", "R_Finger42");
            mapper.CaseInsensitive = true;
            mapper.stringComparisonMode = StringComparisonMode.LeftEndsWithRight;
            AssetDatabase.CreateAsset(mapper, $"{FileUtils.GetFileDirectory(AssetDatabase.GetAssetPath(Selection.activeObject))}/MixamoAndBipedByNameHumanoidAvatarMapper.asset");
        }
    }
}
