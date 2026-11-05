using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魅了の瞳
/// エフェクト再生中、一定範囲内の人間を停止させ、
/// この瞳の方向を向かせる。
/// エフェクトの寿命管理はSkillManagerに任せる。
/// </summary>
public class CharmEyeSkillObject : MonoBehaviour, ISkillObject
{
    [Header("魅了の瞳の設定")]
    [SerializeField]
    private float effectRange = 5f;

    [SerializeField]
    private LayerMask humanLayer = ~0;

    // この瞳が制御している人間
    private readonly HashSet<HumanController> affectedHumans =
        new HashSet<HumanController>();

    private RuntimeSkill runtimeSkill;

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;
    }

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            effectRange,
            humanLayer
        );

        foreach (Collider col in colliders)
        {
            HumanController human =
                col.GetComponentInParent<HumanController>();

            if (human == null)
                continue;

            if (human.IsBeingDrained)
                continue;

            // すでに別の瞳に制御されている人間には適用しない
            if (human.IsCharmed)
                continue;

            // 同じ人間に何度も適用しない
            if (!affectedHumans.Add(human))
                continue;

            human.BeginCharmEye(transform);
        }
    }

    private void OnDisable()
    {
        ReleaseAllHumans();
    }

    private void OnDestroy()
    {
        ReleaseAllHumans();
    }

    private void ReleaseAllHumans()
    {
        foreach (HumanController human in affectedHumans)
        {
            if (human != null)
                human.EndCharmEye(transform);
        }

        affectedHumans.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            effectRange
        );
    }
}