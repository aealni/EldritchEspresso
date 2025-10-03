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
        DateTime lastMessageTime = DateTime.UtcNow;

        void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            assembliesFinished++;
            lastMessageTime = DateTime.UtcNow;
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

            // Wait for compilation to complete
            var start = DateTime.UtcNow;
            const int hardTimeoutSeconds = 600; // 10 minutes for first cold import on CI
            const int inactivityTimeoutSeconds = 90; // no compiler callbacks for 90s => stuck
            while (EditorApplication.isCompiling)
            {
                Thread.Sleep(250);
                var now = DateTime.UtcNow;
                if ((now - start).TotalSeconds > hardTimeoutSeconds)
                {
                    Debug.LogError("[CompileOnly] Hard timeout exceeded. Aborting.");
                    errorCount++;
                    break;
                }
                if ((now - lastMessageTime).TotalSeconds > inactivityTimeoutSeconds && assembliesFinished == 0)
                {
                    // Likely waiting on asset import or license
                    Debug.LogWarning("[CompileOnly] No compilation progress yet (still importing assets?). Waiting...");
                    lastMessageTime = now; 
                }
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
