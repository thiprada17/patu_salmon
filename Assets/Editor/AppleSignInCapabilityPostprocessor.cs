#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

internal static class AppleSignInCapabilityPostprocessor
{
    [PostProcessBuild(900)]
    private static void AddAppleSignInCapability(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
            return;

        string projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        string mainTarget = project.GetUnityMainTargetGuid();

        string entitlementsPath = project.GetBuildPropertyForAnyConfig(mainTarget, "CODE_SIGN_ENTITLEMENTS");
        if (string.IsNullOrEmpty(entitlementsPath))
            entitlementsPath = "PlatuSalmon.entitlements";

        try
        {
            var capabilities = new ProjectCapabilityManager(
                projectPath,
                entitlementsPath,
                targetGuid: mainTarget
            );
            capabilities.AddSignInWithApple();
            capabilities.WriteToFile();
        }
        catch (System.Exception exception)
        {
            UnityEngine.Debug.LogError("Failed to add Sign in with Apple capability: " + exception);
            throw;
        }
    }
}
#endif
