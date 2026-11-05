#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class BackendConfigPreloader
{
    private const string AssetPath = "Assets/Config/BackendConfig.asset";

    static BackendConfigPreloader()
    {
        EditorApplication.delayCall += EnsurePreloaded;
    }

    [MenuItem("Platu Salmon/Backend/Ensure Config Is Included In Builds")]
    private static void EnsurePreloaded()
    {
        BackendConfig config = AssetDatabase.LoadAssetAtPath<BackendConfig>(AssetPath);
        if (config == null)
        {
            Debug.LogError("BackendConfig asset not found at " + AssetPath);
            return;
        }

        Object[] currentAssets = PlayerSettings.GetPreloadedAssets();
        foreach (Object currentAsset in currentAssets)
        {
            if (currentAsset == config)
                return;
        }

        var updatedAssets = new List<Object>(currentAssets) { config };
        PlayerSettings.SetPreloadedAssets(updatedAssets.ToArray());
        Debug.Log("BackendConfig added to Player Settings preloaded assets.");
    }
}
#endif
