using UnityEngine;

/// <summary>
/// ドロップUPスキル
///
/// 人間から獲得できる血液量を増加させる。
/// 現在所持しているドロップUPスキルの
/// 強化形態から倍率を取得する。
/// </summary>
public class DropUpSkill : MonoBehaviour
{
    [Header("対象スキル")]
    [Tooltip("ドロップUPに使用するSkillData")]
    [SerializeField]
    private SkillData skillData;


    //==================================================
    // キャッシュ
    //==================================================

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
                "DropUpSkill : " +
                "同じGameObjectにSkillManagerがありません。"
            );
        }
    }


    //==================================================
    // 倍率取得
    //==================================================

    /// <summary>
    /// 現在のドロップUP倍率を取得する
    /// </summary>
    public float GetMultiplier()
    {
        // SkillManagerがない
        if (skillManager == null)
            return 1f;

        // SkillDataが設定されていない
        if (skillData == null)
        {
            Debug.LogWarning(
                "DropUpSkill : " +
                "SkillDataが設定されていません。"
            );

            return 1f;
        }


        //==============================================
        // RuntimeSkill取得
        //==============================================

        RuntimeSkill runtimeSkill =
            skillManager.GetRuntimeSkill(skillData);


        // スキル未所持
        if (runtimeSkill == null)
            return 1f;


        //==============================================
        // 現在のVariant取得
        //==============================================

        SkillVariantData variant =
            runtimeSkill.Variant;

        if (variant == null)
            return 1f;


        //==============================================
        // 倍率取得
        //==============================================

        float multiplier =
            Mathf.Max(
                1f,
                variant.dropMultiplier
            );


        Debug.Log(
            $"DropUp : " +
            $"スキル={skillData.skillName} / " +
            $"強化={runtimeSkill.enhancementType} / " +
            $"倍率={multiplier:F2}"
        );


        return multiplier;
    }


    //==================================================
    // ドロップ量計算
    //==================================================

    /// <summary>
    /// 基本血液量から最終血液量を計算する
    /// </summary>
    public int CalculateDropAmount(
        int baseAmount)
    {
        if (baseAmount <= 0)
            return 0;


        float multiplier =
            GetMultiplier();


        float result =
            baseAmount *
            multiplier;


        // 小数部分を確率で繰り上げ
        int amount =
            Mathf.FloorToInt(result);

        float remainder =
            result -
            amount;


        if (Random.value < remainder)
        {
            amount++;
        }


        amount =
            Mathf.Max(
                1,
                amount
            );


        Debug.Log(
            $"DropUp計算 : " +
            $"基本={baseAmount} / " +
            $"倍率={multiplier:F2} / " +
            $"最終={amount}"
        );


        return amount;
    }
}