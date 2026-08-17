/// <summary>
/// レベルアップ時に表示するスキル候補
/// </summary>
[System.Serializable]
public class SkillChoiceData
{
    //==================================================
    // 基本情報
    //==================================================

    /// <summary>
    /// 元になるスキル
    /// </summary>
    public SkillData skillData;

    /// <summary>
    /// 選択される強化形態
    /// </summary>
    public EnhancementType enhancementType;


    //==================================================
    // Variant
    //==================================================

    /// <summary>
    /// この候補で使用されるVariant
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
    // 判定
    //==================================================

    /// <summary>
    /// 新規スキル取得かどうか
    /// </summary>
    public bool IsNewSkill
    {
        get
        {
            return enhancementType ==
                   EnhancementType.Normal;
        }
    }

    /// <summary>
    /// 強化形態変更かどうか
    /// </summary>
    public bool IsEnhancement
    {
        get
        {
            return enhancementType !=
                   EnhancementType.Normal;
        }
    }


    //==================================================
    // コンストラクタ
    //==================================================

    public SkillChoiceData(
        SkillData skillData,
        EnhancementType enhancementType)
    {
        this.skillData = skillData;
        this.enhancementType = enhancementType;
    }
}