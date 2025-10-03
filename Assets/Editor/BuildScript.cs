using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    public static void CompileOnly()
    {
        AssetDatabase.Refresh();

        if (UnityEditor.Compilation.CompilationPipeline.GetAssemblyCompilationErrors().Length > 0)
        {
            Debug.LogError("Compilation failed!");
            EditorApplication.Exit(1); 
        }
        else
        {
            Debug.Log("Compilation successful!");
            EditorApplication.Exit(0);// Success
        }
    }
}
