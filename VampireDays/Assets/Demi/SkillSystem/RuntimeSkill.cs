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

    /// <summary>
    /// 現在のCT
    /// </summary>
    public float currentCoolTime;

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


    //==================================================
    // コンストラクタ
    //==================================================

    public RuntimeSkill(
        SkillData skillData)
    {
        this.skillData = skillData;

        enhancementType =
            EnhancementType.Normal;

        SkillVariantData variant =
            Variant;

        currentCoolTime =
            variant != null
                ? variant.coolTime
                : 0f;
    }


    //==================================================
    // 強化形態変更
    //==================================================

    public void ChangeEnhancement(
        EnhancementType type)
    {
        if (skillData == null)
            return;

        SkillVariantData variant =
            skillData.GetVariant(type);

        if (variant == null)
            return;

        enhancementType = type;

        // CTリセット
        currentCoolTime =
            variant.coolTime;
    }
}