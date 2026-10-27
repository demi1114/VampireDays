using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキルの共通データ。
/// スキルの基本情報と、通常版・強化版の設定を保持する。
/// </summary>
[CreateAssetMenu(
    fileName = "SkillData",
    menuName = "Game/Skill Data"
)]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public int id;
    public string skillName;

    [Header("スキルタイプ")]
    public SkillType skillType;

    [Header("レア度")]
    public SkillRarity rarity = SkillRarity.Common;

    [Header("通常版")]
    public SkillVariantData normalData = new();

    [Header("強化版")]
    public List<SkillVariantData> enhancementList = new();

    /// <summary>
    /// 指定した強化形態のデータを取得する。
    /// </summary>
    public SkillVariantData GetVariant(EnhancementType type)
    {
        if (type == EnhancementType.Normal)
            return normalData;

        foreach (SkillVariantData data in enhancementList)
        {
            if (data == null)
                continue;

            if (data.enhancementType == type)
                return data;
        }

        return null;
    }

    /// <summary>
    /// 指定した強化形態が存在するか確認する。
    /// </summary>
    public bool HasVariant(EnhancementType type)
    {
        return GetVariant(type) != null;
    }
}

/// <summary>
/// 通常版・強化版で共通して使用するスキル設定。
/// </summary>
[System.Serializable]
public class SkillVariantData
{
    [Header("強化形態")]
    public EnhancementType enhancementType =
        EnhancementType.Normal;

    [Header("表示情報")]
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;

    [Header("発動設定")]
    [Min(0f)]
    public float coolTime = 1f;

    [Min(1)]
    public int spawnCount = 1;

    [Range(0f, 100f)]
    public float resetChance = 0f;

    [Header("生成位置")]
    public SpawnPositionType spawnPosition =
        SpawnPositionType.Player;

    [Min(0f)]
    public float forwardDistance = 2f;

    [Min(0f)]
    public float randomRadius = 3f;

    //==================================================
    // 生成物
    //==================================================

    [Header("召喚系：召喚物Prefab")]
    public GameObject summonPrefab;

    [Header("魔法系：魔法エフェクトPrefab")]
    public GameObject magicEffectPrefab;

    [Header("パッシブ系：専用エフェクトPrefab")]
    public GameObject passiveEffectPrefab;

    [Header("召喚系：召喚物の持続時間")]
    [Tooltip("0以下の場合、時間による自動消滅を行わない")]
    public float summonLifetime = 0f;

    [Header("発動時の演出")]
    [Tooltip("スキル発動時に別途再生する演出。不要なら未設定")]
    public GameObject castEffect;

    public AudioClip castSE;

    public AudioClip destroySE;

    //==================================================
    // パッシブ系の効果設定
    //==================================================

    [Header("パッシブ：ドロップ倍率")]
    [Min(1f)]
    public float dropMultiplier = 1f;

    [Header("パッシブ：回復倍率")]
    [Min(1f)]
    public float recoveryMultiplier = 1f;

    [Header("パッシブ：吸血時間倍率")]
    [Tooltip("小さいほど吸血が速くなる")]
    [Range(0.1f, 1f)]
    public float drainTimeMultiplier = 1f;

    [Header("パッシブ：移動速度倍率")]
    [Min(1f)]
    public float moveSpeedMultiplier = 1f;

    [Header("パッシブ：魔法CT短縮率")]
    [Tooltip("0.2なら基本CTを20%短縮する設定")]
    [Range(0f, 1f)]
    public float magicCoolTimeReduction = 0f;

    [Header("パッシブ：召喚CT短縮率")]
    [Tooltip("0.2なら基本CTを20%短縮する設定")]
    [Range(0f, 1f)]
    public float summonCoolTimeReduction = 0f;
}