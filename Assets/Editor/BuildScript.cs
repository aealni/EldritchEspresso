using System;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public static class BuildScript
{
    public static void CompileOnly()
    {
        int errorCount = 0;

        void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var msg in messages)
            {
                if (msg.type == CompilerMessageType.Error)
                {
                    errorCount++;
                    Debug.LogError($"[Compile Error] {msg.message} ({msg.file}:{msg.line})");
                }
                else if (msg.type == CompilerMessageType.Warning)
                {
                    Debug.LogWarning($"[Compile Warning] {msg.message} ({msg.file}:{msg.line})");
                }
            }
        }

        CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

        try
        {
            // Kick off a refresh + compile
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();

            // Wait for compilation to complete 
            var start = DateTime.UtcNow;
            const int timeoutSeconds = 360; // 6 minutes
            while (EditorApplication.isCompiling)
            {
                Thread.Sleep(100);
                if ((DateTime.UtcNow - start).TotalSeconds > timeoutSeconds)
                {
                    Debug.LogError("Script compilation timed out.");
                    errorCount++;
                    break;
                }
            }

            if (errorCount > 0)
            {
                Debug.LogError($"Compilation failed with {errorCount} error(s).");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log("Compilation successful!");
                EditorApplication.Exit(0);
            }
        }
        finally
        {
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
        }
    }
}
