
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 魅了の瞳
/// エフェクト再生中に接触した人間を3秒間魅了する。
/// エフェクトの寿命管理はSkillManagerに任せる。
/// </summary>
public class CharmEyeSkillObject : MonoBehaviour, ISkillObject
{
    [Header("魅了設定")]
    [SerializeField]
    private float stopDuration = 3f;

    // 同じ人間に何度も適用しない
    private readonly HashSet<HumanController> affectedHumans =
        new HashSet<HumanController>();

    private RuntimeSkill runtimeSkill;

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;
    }

    private void OnTriggerEnter(Collider other)
    {
        HumanController human =
            other.GetComponentInParent<HumanController>();

        if (human == null)
            return;

        if (human.IsBeingDrained)
            return;

        if (!affectedHumans.Add(human))
            return;

        human.ApplyCharm(stopDuration);
    }
}
