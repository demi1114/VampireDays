using UnityEngine;

/// ゲーム中のみ保持するスキル情報
[System.Serializable]
public class RuntimeSkill
{
    /// 元になるスキルデータ
    public SkillData skillData;

    /// 現在選択中の強化形態
    public EnhancementType enhancementType = EnhancementType.Normal;

    /// 現在のCT
    public float currentCoolTime;

    /// 現在使用しているデータ
    public SkillVariantData Variant
    {
        get
        {
            return skillData.GetVariant(enhancementType);
        }
    }

    /// コンストラクタ
    public RuntimeSkill(SkillData skillData)
    {
        this.skillData = skillData;

        enhancementType = EnhancementType.Normal;
        currentCoolTime = Variant.coolTime;
    }

    /// 強化形態変更
    public void ChangeEnhancement(EnhancementType type)
    {
        enhancementType = type;

        //CTをリセット
        currentCoolTime = Variant.coolTime;
    }
}