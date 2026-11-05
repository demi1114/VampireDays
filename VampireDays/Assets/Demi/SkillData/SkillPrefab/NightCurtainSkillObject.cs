using UnityEngine;

/// <summary>
/// 夜のとばり
/// 画面を暗転させ、画面内の人間の視線を消去する。
/// </summary>
public class NightCurtainSkillObject : MonoBehaviour, ISkillObject
{
    private RuntimeSkill runtimeSkill;

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;

        NightCurtainScreenEffect effect =
            FindFirstObjectByType<NightCurtainScreenEffect>();

        if (effect == null)
        {
            Debug.LogWarning(
                "NightCurtainScreenEffectが見つかりません。"
            );

            return;
        }

        effect.PlayEffect();
    }
}