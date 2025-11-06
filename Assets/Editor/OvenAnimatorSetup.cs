using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
#endif

/// <summary>
/// Editor utility to generate a simple Animator Controller for Ovens and assign it.
/// Creates three states: Idle (no clip), Baking (pulse), Ready (gentle pulse) and
/// drives transitions with bools IsBaking and HasReady (which Oven.cs sets).
/// </summary>
public static class OvenAnimatorSetup
{
#if UNITY_EDITOR
    [MenuItem("Tools/Eldritch Espresso/Generate & Assign Oven Animator", priority = 10)]
    public static void GenerateAndAssign()
    {
        // Ensure folders
        string animFolder = "Assets/Animations";
        if (!AssetDatabase.IsValidFolder(animFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        // Create clips
        var bakingClip = CreatePulseClip(animFolder + "/Oven_Baking.anim", 1.06f, 0.6f);
        var readyClip  = CreatePulseClip(animFolder + "/Oven_Ready.anim", 1.03f, 0.8f);

        // Create controller
        string controllerPath = animFolder + "/Oven.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // Ensure parameters
        EnsureBool(controller, "IsBaking");
        EnsureBool(controller, "HasReady");

        // Build states
        var sm = controller.layers[0].stateMachine;
        var idle  = EnsureState(sm, "Idle", null, new Vector3(200, 100));
        var bake  = EnsureState(sm, "Baking", bakingClip, new Vector3(450, 40));
        var ready = EnsureState(sm, "Ready",  readyClip,  new Vector3(450, 170));
        sm.defaultState = idle;

        // Transitions:
        // Idle -> Baking when IsBaking
        var t1 = idle.AddTransition(bake);
        t1.hasExitTime = false; t1.hasFixedDuration = false; t1.duration = 0; t1.interruptionSource = TransitionInterruptionSource.None;
        t1.AddCondition(AnimatorConditionMode.If, 0, "IsBaking");

        // Baking -> Ready when !IsBaking && HasReady
        var t2 = bake.AddTransition(ready);
        t2.hasExitTime = false; t2.hasFixedDuration = false; t2.duration = 0;
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsBaking");
        t2.AddCondition(AnimatorConditionMode.If,    0, "HasReady");

        // Ready -> Baking when IsBaking
        var t3 = ready.AddTransition(bake);
        t3.hasExitTime = false; t3.hasFixedDuration = false; t3.duration = 0;
        t3.AddCondition(AnimatorConditionMode.If, 0, "IsBaking");

        // Ready -> Idle when !HasReady
        var t4 = ready.AddTransition(idle);
        t4.hasExitTime = false; t4.hasFixedDuration = false; t4.duration = 0;
        t4.AddCondition(AnimatorConditionMode.IfNot, 0, "HasReady");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign to selected Oven(s) or scene ovens
        var ovens = Selection.gameObjects;
        if (ovens == null || ovens.Length == 0)
        {
            ovens = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        int assigned = 0;
        foreach (var go in ovens)
        {
            if (go == null) continue;
            var oven = go.GetComponent<Oven>();
            if (oven == null) continue;
            var animator = go.GetComponent<Animator>();
            if (animator == null) animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            assigned++;
        }

        Debug.Log($"Oven Animator ready. Assigned to {assigned} Oven object(s). Controller at {controllerPath}.");
    }

    private static void EnsureBool(AnimatorController controller, string name)
    {
        if (controller.parameters.Any(p => p.name == name)) return;
        controller.AddParameter(name, AnimatorControllerParameterType.Bool);
    }

    private static AnimatorState EnsureState(AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos)
    {
        foreach (var s in sm.states)
        {
            if (s.state.name == name)
            {
                s.state.motion = clip;
                return s.state;
            }
        }
        var st = sm.AddState(name);
        st.motion = clip;
        return st;
    }

    private static AnimationClip CreatePulseClip(string path, float scale, float duration)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null) return clip;
        clip = new AnimationClip();
        clip.frameRate = 30f;

        // Simple scale pulse on the root transform (x and y)
        var curveUp = AnimationCurve.EaseInOut(0f, 1f, duration * 0.5f, scale);
        var curveDown = AnimationCurve.EaseInOut(duration * 0.5f, scale, duration, 1f);
        var curve = new AnimationCurve();
        foreach (var k in curveUp.keys) curve.AddKey(k);
        foreach (var k in curveDown.keys) curve.AddKey(k);

        var bindingX = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x");
        var bindingY = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y");
        AnimationUtility.SetEditorCurve(clip, bindingX, curve);
        AnimationUtility.SetEditorCurve(clip, bindingY, curve);

        clip.wrapMode = WrapMode.Loop;
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }
#endif
}