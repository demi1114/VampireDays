using UnityEngine;

/// <summary>
/// 移動速度UPパッシブスキル
///
/// 現在所持している移動速度UPスキルの
/// 倍率を取得する。
/// </summary>
public class MoveSpeedUpSkill : MonoBehaviour
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
                "MoveSpeedUpSkill : " +
                "同じGameObjectにSkillManagerがありません。"
            );
        }
    }


    //==================================================
    // 移動速度倍率
    //==================================================

    /// <summary>
    /// 現在の移動速度倍率を取得する
    /// </summary>
    public float GetMultiplier()
    {
        if (skillManager == null)
            return 1f;

        if (skillData == null)
            return 1f;

        RuntimeSkill runtimeSkill =
            skillManager.GetRuntimeSkill(skillData);

        // スキル未所持
        if (runtimeSkill == null)
            return 1f;

        SkillVariantData variant =
            runtimeSkill.Variant;

        if (variant == null)
            return 1f;

        return Mathf.Max(
            1f,
            variant.moveSpeedMultiplier
        );
    }


    /// <summary>
    /// 基本移動速度から最終移動速度を計算する
    /// </summary>
    public float CalculateMoveSpeed(
        float baseMoveSpeed)
    {
        if (baseMoveSpeed <= 0f)
            return 0f;

        return baseMoveSpeed *
               GetMultiplier();
    }
}