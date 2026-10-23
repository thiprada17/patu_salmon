
using UnityEngine;

public class TutorialPanelReference : MonoBehaviour
{
    [SerializeField] private GameObject TutorialPanel;

    private void Awake()
    {
        TutorialSession.TutorialPanel = TutorialPanel;
    }
}
