using UnityEngine;

public class BloodCageSkillObject :MonoBehaviour,ISkillObject,ITargetEnemySkillObject
{
    private RuntimeSkill runtimeSkill;

    // çSë©ÇµÇΩêlä‘
    private HumanController targetHuman;

    private bool hasTarget;

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;
    }

    public void SetTarget(HumanController human)
    {
        if (hasTarget || human == null)
            return;

        targetHuman = human;
        targetHuman.BeginBloodCage(transform);

        hasTarget = targetHuman.IsBloodCaged;

        if (!hasTarget)
            targetHuman = null;
    }

    private void OnDisable()
    {
        ReleaseTarget();
    }

    private void OnDestroy()
    {
        ReleaseTarget();
    }

    private void ReleaseTarget()
    {
        if (targetHuman != null)
        {
            targetHuman.EndBloodCage(transform);
            targetHuman = null;
        }

        hasTarget = false;
    }
}