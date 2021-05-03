using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEditor;
using UnityEngine;

namespace TriLibCore.Editor
{
    public class MapperContextActions : MonoBehaviour
    {
        [MenuItem("Assets/Create Mapper Instance")]
        private static void CreateInstance()
        {
            var monoScript = Selection.activeObject as MonoScript;
            if (monoScript != null)
            {
                var scriptableObject = ScriptableObject.CreateInstance(monoScript.GetClass());
                var assetPath = AssetDatabase.GetAssetPath(monoScript);
                var directory = FileUtils.GetFileDirectory(assetPath);
                var name = FileUtils.GetFilenameWithoutExtension(assetPath);
                AssetDatabase.CreateAsset(scriptableObject, $"{directory}/{name}.asset");
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Assets/Create Mapper Instance", true)]
        private static bool Validate()
        {
            if (Selection.activeObject is MonoScript monoScript)
            {
                var @class = monoScript.GetClass();
                return
                    typeof(AnimationClipMapper).IsAssignableFrom(@class) ||
                    typeof(MaterialMapper).IsAssignableFrom(@class) ||
                    typeof(TextureMapper).IsAssignableFrom(@class) ||
                    typeof(HumanoidAvatarMapper).IsAssignableFrom(@class) ||
                    typeof(RootBoneMapper).IsAssignableFrom(@class) ||
                    typeof(LipSyncMapper).IsAssignableFrom(@class) ||
                    typeof(UserPropertiesMapper).IsAssignableFrom(@class) ||
                    typeof(ExternalDataMapper).IsAssignableFrom(@class);
            }
            return false;
        }
    }
}
