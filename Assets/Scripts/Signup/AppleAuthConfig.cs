using UnityEngine;

[CreateAssetMenu(fileName = "AppleAuthConfig", menuName = "Platu Salmon/Auth/Apple Auth Config")]
public sealed class AppleAuthConfig : ScriptableObject
{
    [SerializeField] private string appleClientId = "com.platusalmon.game";

#if UNITY_EDITOR
    [SerializeField] private bool useMockInEditor = true;
#endif

    public string AppleClientId => appleClientId;

#if UNITY_EDITOR
    public bool UseMockInEditor => useMockInEditor;
#endif
}
