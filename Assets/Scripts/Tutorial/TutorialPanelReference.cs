
using UnityEngine;

public class TutorialPanelReference : MonoBehaviour
{
    [SerializeField] private GameObject TutorialPanel;
    [SerializeField] private GameObject TutorialPanelancle;

    private void Awake()
    {
        TutorialSession.TutorialPanel = TutorialPanel;
        TutorialSession.TutorialPanelancle = TutorialPanelancle;
    }
}
