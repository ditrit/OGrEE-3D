using TriLibCore.General;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    [CustomEditor(typeof(AssetLoaderOptions))]
    public class AssetLoaderOptionsEditor : UnityEditor.Editor
    {
        private int _currentTab;

        public override void OnInspectorGUI()
        {
            ShowInspectorGUI(serializedObject, ref _currentTab);
        }

        public static void ShowInspectorGUI(SerializedObject serializedObject, ref int currentTab)
        {
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(currentTab == 0, "Model", "LargeButtonLeft"))
            {
                currentTab = 0;
            }
            if (GUILayout.Toggle(currentTab == 1, "Rig", "LargeButtonMid"))
            {
                currentTab = 1;
            }
            if (GUILayout.Toggle(currentTab == 2, "Animations", "LargeButtonMid"))
            {
                currentTab = 2;
            }
            if (GUILayout.Toggle(currentTab == 3, "Materials", "LargeButtonMid"))
            {
                currentTab = 3;
            }
            if (GUILayout.Toggle(currentTab == 4, "Textures", "LargeButtonMid"))
            {
                currentTab = 4;
            }
            if (GUILayout.Toggle(currentTab == 5, "Misc.", "LargeButtonRight"))
            {
                currentTab = 5;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            switch (currentTab)
            {
                case 0:
                    GUILayout.Label(new GUIContent("Scene", "Scene import settings"), "BoldLabel");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ScaleFactor"), new GUIContent("Scale Factor", "Model scale multiplier."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UseFileScale"), new GUIContent("Use File Scale", "Turn on this flag to use the file original scale."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportVisibility"), new GUIContent("Import Visibility", "Turn on this field to apply the visibility property to Mesh Renderers/Skinned Mesh Renderers."));
                    //todo: cameras and lights
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Static"), new GUIContent("Import as Static", "Turn on this field to import the Model as a static Game Object."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SortHierarchyByName"), new GUIContent("Sort Hierarchy by Name", "Turn on this field to sort the Model hierarchy by name."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AddAssetUnloader"), new GUIContent("Add Asset Unloader", "Turn on this field to add the Asset Unloader Component to the loaded Game Object."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowLoadingWarnings"), new GUIContent("Show Loading Warnings", "Turn on this field to display Model loading warnings on the Console."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CloseStreamAutomatically"), new GUIContent("Close Stream Automatically", "Turn on this field to close the Model loading Stream automatically."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DestroyOnError"), new GUIContent("Destroy on Error", "Turn on this field to destroy the loaded Game Object automatically when there is any loading error."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Timeout"), new GUIContent("Timeout", "Model loading timeout in seconds (0=disabled)."));
                    var forceGcCollectionWhileLoadingProperty = serializedObject.FindProperty("ForceGCCollectionWhileLoading");
                    EditorGUILayout.PropertyField(forceGcCollectionWhileLoadingProperty, new GUIContent("Force GC Collection while loading", "Turn on this field to force the GC collection while loading the model and release memory promptly."));
                    if (forceGcCollectionWhileLoadingProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(forceGcCollectionWhileLoadingProperty, new GUIContent("GC Helper Collection Interval", "How many seconds to wait to release a loading model when using the GCHelper class."));
                    }
                    EditorGUILayout.Space();
                    GUILayout.Label(new GUIContent("Meshes", "Global settings for generated meshes"), "BoldLabel");
                    var importMeshesProperty = serializedObject.FindProperty("ImportMeshes");
                    EditorGUILayout.PropertyField(importMeshesProperty, new GUIContent("Import Meshes", "Turn on this field to import Model Meshes."));
                    if (importMeshesProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ReadAndWriteEnabled"), new GUIContent("Read and Write Enabled", "Turn on this field to optimize imported Meshes for reading/writing."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OptimizeMeshes"), new GUIContent("Optimize Meshes", "Turn on this field to optimize imported Meshes for GPU access."));
                        var generateCollidersProperty = serializedObject.FindProperty("GenerateColliders");
                        EditorGUILayout.PropertyField(generateCollidersProperty, new GUIContent("Generate Colliders", "Turn on this field to generate Colliders for imported Meshes."));
                        if (generateCollidersProperty.boolValue)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("ConvexColliders"), new GUIContent("Convex Colliders", "Turn on this field to generate convex Colliders when the GenerateColliders field is enabled."));
                        }
                        EditorGUILayout.Space();
                        GUILayout.Label(new GUIContent("Geometry", "Detailed mesh data"), "BoldLabel");
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("KeepQuads"), new GUIContent("Keep Quads", "Turn on this field to mantain Mesh quads. (Useful for DX11 tesselation)"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MergeVertices"), new GUIContent("Merge Vertices", "Turn on this field to merge model duplicated vertices where possible."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportNormals"), new GUIContent("Import Normals", "Turn on this field to import Mesh normals. If not enabled, normals will be calculated instead."));
                        var useUnityNativeNormalCalculatorProperty = serializedObject.FindProperty("UseUnityNativeNormalCalculator");
                        EditorGUILayout.PropertyField(useUnityNativeNormalCalculatorProperty, new GUIContent("Use Unity Native Normal Calculator", "Turn on this field to use the builtin Unity normal calculator."));
                        if (!useUnityNativeNormalCalculatorProperty.boolValue)
                        {
                            EditorGUILayout.Slider(serializedObject.FindProperty("SmoothingAngle"), 0f, 180f, new GUIContent("Smoothing Angle", "Normals calculation smoothing angle."));
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportBlendShapes"), new GUIContent("Import Blend Shapes", "Turn on this field to import Mesh Blend Shapes."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportColors"), new GUIContent("Import Colors", "Turn on this field to import Mesh Colors."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ImportTangents"), new GUIContent("Import Tangents", "Turn on this field to import Mesh tangents. If not enabled, tangents will be calculated instead."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SwapUVs"), new GUIContent("Swap UVs", "Turn on this field to swap Mesh UVs. (uv1 into uv2)"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("LODScreenRelativeTransitionHeightBase"), new GUIContent("LOD Screen Relative Transition Height Base", "Defines the initial screen relative transition height when creating LOD Groups."));
                    }
                    break;
                case 1:
                    var animationTypeProperty = serializedObject.FindProperty("AnimationType");
                    EditorGUILayout.PropertyField(animationTypeProperty, new GUIContent("Animation Type", "Model rigging type."));
                    var animationType = (AnimationType)animationTypeProperty.intValue;
                    switch (animationType)
                    {
                        case AnimationType.Generic:
                        case AnimationType.Humanoid:

                            var avatarDefinitionTypeProperty = serializedObject.FindProperty("AvatarDefinition");
                            EditorGUILayout.PropertyField(avatarDefinitionTypeProperty, new GUIContent("Avatar Definition", "Type of avatar creation for the Model."));
                            var avatarDefinitionType = (AvatarDefinitionType)avatarDefinitionTypeProperty.intValue;
                            switch (avatarDefinitionType)
                            {
                                case AvatarDefinitionType.CreateFromThisModel:
                                    if (animationType == AnimationType.Humanoid)
                                    {
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("HumanDescription"), new GUIContent("Human Description", "HHuman Description used to create the humanoid Avatar, when the humanoid rigging type is selected."));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("SampleBindPose"), new GUIContent("Sample Bind Pose", "Turn on this field to enforce the loaded Model to the bind-pose when rigging."));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnforceTPose"), new GUIContent("Enforce T-Pose", "Turn on this field to enforce the loaded Model to the t-pose when rigging."));
                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("HumanoidAvatarMapper"), new GUIContent("Humanoid Avatar Mapper", "Mapper used to map the humanoid Avatar, when the humanoid rigging type is selected."));
                                    }
                                    break;
                                case AvatarDefinitionType.CopyFromOtherAvatar:
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Avatar"), new GUIContent("Source", "Source Avatar to use when copying from other Avatar."));
                                    break;
                            }
                            break;
                    }
                    if (animationType != AnimationType.None)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("RootBoneMapper"), new GUIContent("Root Bone Mapper", "Mapper used to find the Model root bone."));
                    }
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("LimitBoneWeights"), new GUIContent("Limit Bone Weights", "Turn on this field to limit bones weight to 4 weights per bone."));
                    break;
                case 2:
                    animationTypeProperty = serializedObject.FindProperty("AnimationType");
                    animationType = (AnimationType)animationTypeProperty.intValue;
                    //todo: constraints
                    if (animationType != AnimationType.None)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("EnsureQuaternionContinuity"), new GUIContent("Ensure Quaternion Continuity", "Turn on this field to realign quaternion keys to ensure shortest interpolation paths."));
                        //todo: keyframe reduction
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationWrapMode"), new GUIContent("Wrap Mode", "Default wrap-mode to apply to Animations."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimationClipMappers"), new GUIContent("Animation Clip Mappers", "Mappers used to process Animation Clips."));
                    }
                    break;
                case 3:
                    var importMaterialsProperty = serializedObject.FindProperty("ImportMaterials");
                    EditorGUILayout.PropertyField(importMaterialsProperty, new GUIContent("Import Materials", "Turn on this field to import Materials."));
                    if (importMaterialsProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("UseMaterialKeywords"), new GUIContent("Use Material Keywords", "Turn on this field to enable/disable created Material Keywords based on the source native Materials."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("AlphaMaterialMode"), new GUIContent("Alpha Material Mode", "Chooses the way TriLib creates alpha materials."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MaterialMappers"), new GUIContent("Material Mappers", "Mappers used to create suitable Unity Materials from original Materials."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("UseAutodeskInteractiveMaterials"), new GUIContent("Use Autodesk Interactive Materials", "Turn on this field to use Autodesk Interactive Materials when possible."));
                    }
                    break;
                case 4:
                    var importTexturesProperty = serializedObject.FindProperty("ImportTextures");
                    EditorGUILayout.PropertyField(importTexturesProperty, new GUIContent("Import Textures", "Turn on this field to import Textures."));
                    if (importTexturesProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("TextureMapper"), new GUIContent("Texture Mapper", "Mapper used to find native Texture Streams from custom sources."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("TextureCompressionQuality"), new GUIContent("Texture Compression Quality", "Texture compression to apply on loaded Textures."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("GenerateMipmaps"), new GUIContent("Generate Mipmaps", "Turn on this field to enable Textures mip-map generation."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("FixNormalMaps"), new GUIContent("Fix Normal Maps", "Turn on this field to change normal map channels order to ABBR instead of RGBA."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("MarkTexturesNoLongerReadable"), new GUIContent("Mark Textures no longer readable", "Turn on this field to set textures as no longer readable and release memory resources."));
                        var alphaMaterialModeProperty = serializedObject.FindProperty("AlphaMaterialMode");
                        if (alphaMaterialModeProperty.enumValueIndex > 0)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScanForAlphaPixels"), new GUIContent("Scan for Alpha Pixels", "Turn on this field to scan Textures for alpha-blended pixels in order to generate transparent Materials."));
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadTexturesAsSRGB"), new GUIContent("Load Textures as sRGB", "Turn off this field to load textures as linear, instead of sRGB."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ApplyTexturesOffsetAndScaling"), new GUIContent("Apply Textures Offset and Scaling", "urn on this field to apply Textures offset and scaling."));
                    }
                    break;
                case 5:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ExternalDataMapper"), new GUIContent("External Data Mapper", "Mapper used to find data Streams on external sources."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UserPropertiesMapper"), new GUIContent("User Properties Mapper", " Mapper used to process User Properties from Models."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("LipSyncMappers"), new GUIContent("Lip Sync Mappers", "Mappers used to configure Lip-Sync Blend Shapes."));
                    break;
            }
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }
    }
}