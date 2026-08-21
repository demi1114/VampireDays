using UnityEngine;

/// <summary>
/// ‹zŒŒ‘¬“xUPƒXƒLƒ‹
///
/// lŠÔ‚ğ‹zŒŒ‚µI‚í‚é‚Ü‚Å‚ÌŠÔ‚ğ’Zk‚·‚éB
///
/// —áF
/// ’Êí       1.0•b
/// Emerald    0.8•b
/// Sapphire   0.6•b
/// Ruby       0.4•b
/// Opal       0.25•b
/// </summary>
public class DrainSpeedUpSkill : MonoBehaviour
{
    [Header("‘ÎÛƒXƒLƒ‹")]
    [Tooltip("‹zŒŒ‘¬“xUP‚Ég—p‚·‚éSkillData")]
    [SerializeField]
    private SkillData skillData;


    //==================================================
    // ƒLƒƒƒbƒVƒ…
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
                "DrainSpeedUpSkill : " +
                "“¯‚¶GameObject‚ÉSkillManager‚ª‚ ‚è‚Ü‚¹‚ñB"
            );
        }
    }


    //==================================================
    // ”{—¦æ“¾
    //==================================================

    /// <summary>
    /// Œ»İ‚Ì‹zŒŒŠÔ”{—¦‚ğæ“¾‚·‚é
    /// </summary>
    public float GetMultiplier()
    {
        if (skillManager == null)
            return 1f;

        if (skillData == null)
        {
            Debug.LogWarning(
                "DrainSpeedUpSkill : " +
                "SkillData‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB"
            );

            return 1f;
        }


        RuntimeSkill runtimeSkill =
            skillManager.GetRuntimeSkill(skillData);


        // ƒXƒLƒ‹–¢Š
        if (runtimeSkill == null)
            return 1f;


        SkillVariantData variant =
            runtimeSkill.Variant;

        if (variant == null)
            return 1f;


        float multiplier =
            Mathf.Clamp(
                variant.drainTimeMultiplier,
                0.1f,
                1f
            );


        Debug.Log(
            $"DrainSpeedUp : " +
            $"ƒXƒLƒ‹={skillData.skillName} / " +
            $"‹­‰»={runtimeSkill.enhancementType} / " +
            $"”{—¦={multiplier:F2}"
        );


        return multiplier;
    }


    //==================================================
    // ‹zŒŒŠÔŒvZ
    //==================================================

    /// <summary>
    /// Šî–{‹zŒŒŠÔ‚©‚çÅI‹zŒŒŠÔ‚ğŒvZ‚·‚é
    /// </summary>
    public float CalculateDrainTime(
        float baseTime)
    {
        if (baseTime <= 0f)
            return 0f;


        float multiplier =
            GetMultiplier();


        float result =
            baseTime *
            multiplier;


        return Mathf.Max(
            0.05f,
            result
        );
    }
}