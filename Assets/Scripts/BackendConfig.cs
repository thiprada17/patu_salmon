using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>Single source of truth for backend URLs used by Unity clients.</summary>
[CreateAssetMenu(fileName = "BackendConfig", menuName = "Platu Salmon/Backend Config")]
public sealed class BackendConfig : ScriptableObject
{
    private const string AssetPath = "Assets/Config/BackendConfig.asset";
    private static BackendConfig cached;

    [SerializeField] private string productionBaseUrl = "https://patu-salmon.onrender.com";
    [SerializeField] private string developmentBaseUrl = "http://127.0.0.1:8000";
#if UNITY_EDITOR
    [SerializeField] private bool useDevelopmentBackendInEditor = true;
#endif

    public static string BaseUrl
    {
        get
        {
            if (cached == null)
                cached = LoadConfigAsset();
            if (cached == null)
            {
                Debug.LogError("BackendConfig asset is missing or was not preloaded: " + AssetPath);
                return string.Empty;
            }
            return cached.ResolveBaseUrl();
        }
    }

    private static BackendConfig LoadConfigAsset()
    {
#if UNITY_EDITOR
        BackendConfig editorAsset = AssetDatabase.LoadAssetAtPath<BackendConfig>(AssetPath);
        if (editorAsset != null)
            return editorAsset;
#endif

        // Player builds cannot use AssetDatabase. The editor registrar adds this
        // ScriptableObject to PlayerSettings.preloadedAssets before a build.
        BackendConfig[] loadedConfigs = Resources.FindObjectsOfTypeAll<BackendConfig>();
        return loadedConfigs.Length > 0 ? loadedConfigs[0] : null;
    }

    private string ResolveBaseUrl()
    {
#if UNITY_EDITOR
        if (useDevelopmentBackendInEditor)
            return developmentBaseUrl.TrimEnd('/');
#endif
        return productionBaseUrl.TrimEnd('/');
    }
}
