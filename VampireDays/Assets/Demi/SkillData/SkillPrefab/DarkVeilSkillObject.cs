using System.Collections.Generic;
using UnityEngine;

public class DarkVeilSkillObject : MonoBehaviour, ISkillObject
{
    private readonly HashSet<VisionController> affectedHumans = new();

    public void Initialize(RuntimeSkill skill)
    {
        affectedHumans.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        HumanController human =
            other.GetComponentInParent<HumanController>();

        if (human == null) return;

        VisionController vision =
            human.GetComponent<VisionController>();

        if (vision == null) return;

        if (affectedHumans.Contains(vision)) return;

        affectedHumans.Add(vision);

        vision.SuppressVision();
    }

    private void OnDisable()
    {
        RestoreAllVision();
    }

    private void OnDestroy()
    {
        RestoreAllVision();
    }

    private void RestoreAllVision()
    {
        foreach (VisionController vision in affectedHumans)
        {
            if (vision == null) continue;

            vision.RestoreVision();
        }

        affectedHumans.Clear();
    }
}