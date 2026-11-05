using UnityEngine;

[CreateAssetMenu(fileName = "AppleAuthConfig", menuName = "Platu Salmon/Auth/Apple Auth Config")]
public sealed class AppleAuthConfig : ScriptableObject
{
    [SerializeField] private string appleClientId = "com.platusalmon.game";
    [SerializeField] private string backendUrl = "https://patu-salmon.onrender.com";

#if UNITY_EDITOR
    [SerializeField] private bool useMockInEditor = true;
#endif

    public string AppleClientId => appleClientId;
    public string BackendUrl => backendUrl.TrimEnd('/');

#if UNITY_EDITOR
    public bool UseMockInEditor => useMockInEditor;
#endif
}
