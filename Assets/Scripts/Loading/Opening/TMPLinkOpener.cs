using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Opens HTTP(S) links embedded in a TextMeshPro text component.</summary>
public sealed class TMPLinkOpener : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text targetText;

    private void Awake()
    {
        targetText = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetText == null)
            return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            targetText,
            eventData.position,
            eventData.pressEventCamera);
        if (linkIndex < 0)
            return;

        string url = targetText.textInfo.linkInfo[linkIndex].GetLinkID();
        if (System.Uri.TryCreate(url, System.UriKind.Absolute, out System.Uri uri) &&
            (uri.Scheme == System.Uri.UriSchemeHttp || uri.Scheme == System.Uri.UriSchemeHttps))
        {
            Application.OpenURL(uri.AbsoluteUri);
        }
    }
}
