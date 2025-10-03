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
    int assembliesFinished = 0;

        void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            assembliesFinished++;
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
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();

            // maybe setup timeout later???
            while (EditorApplication.isCompiling)
            {
                Thread.Sleep(250);
            }

            if (errorCount > 0)
            {
                Debug.LogError($"Compilation failed with {errorCount} error(s).");
                EditorApplication.Exit(1);
            }
            else
            {
                Debug.Log($"Compilation successful! Assemblies finished: {assembliesFinished}");
                EditorApplication.Exit(0);
            }
        }
        finally
        {
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
        }
    }
    [MenuItem("Build/Run Compile Only")]
    public static void CompileFromMenu()
    {
        CompileOnly();
    }
}
