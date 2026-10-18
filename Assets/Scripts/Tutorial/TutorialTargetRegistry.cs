
using System.Collections.Generic;
using UnityEngine;

public class TutorialTargetRegistry : MonoBehaviour
{
    public static TutorialTargetRegistry Instance { get; private set; }

    private readonly Dictionary<TutorialTargetID, TutorialTarget> targets =
        new Dictionary<TutorialTargetID, TutorialTarget>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TutorialTargetRegistry objects found.");
            return;
        }

        Instance = this;
        RebuildRegistry();
    }

    public void RebuildRegistry()
    {
        targets.Clear();

        TutorialTarget[] sceneTargets =
            FindObjectsByType<TutorialTarget>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (TutorialTarget target in sceneTargets)
        {
            if (targets.ContainsKey(target.TargetID))
            {
                Debug.LogWarning(
                    "Duplicate tutorial target ID: " + target.TargetID
                );
                continue;
            }

            targets.Add(target.TargetID, target);
        }
    }

    public Transform FindTarget(TutorialTargetID targetID)
    {
        if (targets.TryGetValue(targetID, out TutorialTarget target)
            && target != null)
        {
            return target.transform;
        }

        Debug.LogWarning("Tutorial target not found: " + targetID);
        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
