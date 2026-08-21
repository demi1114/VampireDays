using UnityEngine;

/// <summary>
/// 召喚マスター
///
/// 所持している召喚系スキルのCTを短縮する。
/// 実際のCT計算はSkillManagerから行う。
/// </summary>
public class SummonMasterSkill : MonoBehaviour
{
    [Header("対象スキル")]
    [SerializeField]
    private SkillData skillData;

    private SkillManager skillManager;


    //==================================================
    // Unity
    //==================================================

    private void Awake()
    {
        skillManager =
            GetComponent<SkillManager>();

        if (skillManager == null)
        {
            Debug.LogError(
                "SummonMasterSkill : " +
                "同じGameObjectにSkillManagerがありません。"
            );
        }
    }


    //==================================================
    // CT倍率
    //==================================================

    /// <summary>
    /// 現在の召喚マスターによるCT倍率を取得する。
    ///
    /// 1.0 = 変化なし
    /// 0.8 = CT20%短縮
    /// 0.5 = CT50%短縮
    /// </summary>
    public float GetMultiplier()
    {
        if (skillManager == null)
            return 1f;

        if (skillData == null)
            return 1f;

        RuntimeSkill runtimeSkill =
            skillManager.GetRuntimeSkill(
                skillData
            );

        // 未取得
        if (runtimeSkill == null)
            return 1f;

        SkillVariantData variant =
            runtimeSkill.Variant;

        if (variant == null)
            return 1f;

        return Mathf.Clamp(
            variant.summonCoolTimeReduction,
            0.01f,
            1f
        );
    }


    //==================================================
    // CT計算
    //==================================================

    /// <summary>
    /// 基本CTから召喚マスター適用後のCTを計算する。
    /// </summary>
    public float CalculateCoolTime(
        float baseCoolTime)
    {
        if (baseCoolTime <= 0f)
            return 0f;

        return baseCoolTime *
               GetMultiplier();
    }
}