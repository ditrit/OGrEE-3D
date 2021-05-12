using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace TriLibCore.Editor
{
    public static class TriLibDeprecationWarnings
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            CompilationPipeline.assemblyCompilationFinished -= CompilationPipelineOnassemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnassemblyCompilationFinished;
        }

        private static void CompilationPipelineOnassemblyCompilationFinished(string arg1, CompilerMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error && message.message.Contains("'SFB'"))
                {
                    Debug.LogWarning("Since TriLib 2.0.12, the 'SFB' namespace has been replaced by the 'TriLibCore.SFB' namespace to avoid conflicts with the vanilla StandaloneFileBrowser.\n\nTo fix errors related to the 'SFB' namespace, replace the 'SFB' namespace from your code with the 'TriLibCore.SFB' namespace.\n\nIf you need any guidance, send a message to 'contato@ricardoreis.net'.");
                }
            }
        }
    }
}
