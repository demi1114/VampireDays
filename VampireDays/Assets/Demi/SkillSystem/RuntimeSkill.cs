using UnityEngine;

/// <summary>
/// ゲーム中のみ保持するスキル情報
/// </summary>
[System.Serializable]
public class RuntimeSkill
{
    /// <summary>
    /// 元になるスキルデータ
    /// </summary>
    public SkillData skillData;

    /// <summary>
    /// 現在選択中の強化形態
    /// </summary>
    public EnhancementType enhancementType =
        EnhancementType.Normal;

    public bool hasActivated = false;

    /// <summary>
    /// 現在のCT
    /// </summary>
    public float currentCoolTime;
    public float currentCoolTimeDuration;

    /// <summary>
    /// 現在使用しているデータ
    /// </summary>
    public SkillVariantData Variant
    {
        get
        {
            if (skillData == null)
                return null;

            return skillData.GetVariant(
                enhancementType
            );
        }
    }


    public RuntimeSkill(SkillData skillData)
    {
        this.skillData = skillData;
        enhancementType = EnhancementType.Normal;

        // 初期CTを設定
        currentCoolTime = Variant.coolTime;

        // まだ初回発動していない
        hasActivated = false;
    }

    public void ChangeEnhancement(EnhancementType newType)
    {
        enhancementType = newType;

        // 強化後のCTに変更
        currentCoolTime = Variant.coolTime;

        // 強化形態への変更後は初回発動を行う
        hasActivated = false;
    }
}