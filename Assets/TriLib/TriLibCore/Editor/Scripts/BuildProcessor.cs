using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

namespace TriLibCore.Editor
{
    public class BuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            var waitingMappers = false;
            string materialMapper = null;
            var arguments = Environment.GetCommandLineArgs();
            foreach (var argument in arguments)
            {
                if (waitingMappers)
                {
                    materialMapper = argument;
                    continue;
                }
                switch (argument)
                {
                    case "-trilib_mappers":
                        {
                            waitingMappers = true;
                            break;
                        }
                }
            }
            if (materialMapper != null)
            {
                Debug.Log($"Using the given material mapper:{materialMapper}.");
                CheckMappers.SelectMapper(materialMapper);
            }
            else
            {
                CheckMappers.Initialize();
            }
        }
    }
}