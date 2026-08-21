using UnityEngine;

/// <summary>
/// 回復量UPスキル
///
/// 吸血によってプレイヤーが回復する量を増加させる。
/// 強化形態による倍率はSkillVariantDataから取得する。
/// </summary>
public class RecoveryUpSkill : MonoBehaviour
{
    [Header("対象スキル")]
    [SerializeField]
    private SkillData skillData;

    /// <summary>
    /// SkillManager
    /// </summary>
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
                "RecoveryUpSkill : " +
                "同じGameObjectにSkillManagerがありません。"
            );
        }
    }


    //==================================================
    // 回復倍率
    //==================================================

    /// <summary>
    /// 現在の回復倍率を取得する
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

        // スキル未所持
        if (runtimeSkill == null)
            return 1f;

        SkillVariantData variant =
            runtimeSkill.Variant;

        if (variant == null)
            return 1f;

        return Mathf.Max(
            1f,
            variant.recoveryMultiplier
        );
    }


    //==================================================
    // 回復量計算
    //==================================================

    /// <summary>
    /// 基本回復量から最終回復量を計算する
    /// </summary>
    public float CalculateRecoveryAmount(
        float baseAmount)
    {
        if (baseAmount <= 0f)
            return 0f;

        float multiplier =
            GetMultiplier();

        return baseAmount *
               multiplier;
    }
}