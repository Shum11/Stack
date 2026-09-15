using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildScript
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string ApkPath = "Builds/Stack.apk";

    public static void BuildAndroid()
    {
        CreateScene();

        PlayerSettings.companyName = "StackCo";
        PlayerSettings.productName = "Stack";
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.stackco.stack");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.X86_64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
        PlayerSettings.Android.forceSDCardPermission = false;
        PlayerSettings.Android.useCustomKeystore = false;

        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
        {
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3
        });

        // Always include shaders used at runtime via Shader.Find
        foreach (var shaderName in new[] { "Unlit/Color", "Unlit/Texture" })
        {
            var sh = Shader.Find(shaderName);
            if (sh == null) continue;
            var gs = new SerializedObject(UnityEngine.Rendering.GraphicsSettings.GetGraphicsSettings());
            var prop = gs.FindProperty("m_AlwaysIncludedShaders");
            bool found = false;
            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == sh) { found = true; break; }
            }
            if (!found)
            {
                prop.InsertArrayElementAtIndex(prop.arraySize);
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = sh;
                gs.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = ApkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Build failed: " + report.summary.result + " errors=" + report.summary.totalErrors);
    }

    static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("Game");
        root.AddComponent<Bootstrap>();
        EditorSceneManager.SaveScene(scene, ScenePath);
    }
}