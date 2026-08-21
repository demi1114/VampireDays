using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキルのScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public int id;
    public string skillName;

    [Header("種類")]
    public SkillType skillType;

    [Header("通常版")]
    public SkillVariantData normalData = new();

    [Header("強化版")]
    public List<SkillVariantData> enhancementList = new();

    /// <summary>
    /// 指定した強化形態のデータを取得
    /// </summary>
    public SkillVariantData GetVariant(EnhancementType type)
    {
        // 通常版
        if (type == EnhancementType.Normal)
            return normalData;

        // 強化版を検索
        foreach (SkillVariantData data in enhancementList)
        {
            if (data == null)
                continue;

            if (data.enhancementType == type)
                return data;
        }

        // 存在しない場合
        return null;
    }

    /// <summary>
    /// 指定した強化形態が存在するか
    /// </summary>
    public bool HasVariant(EnhancementType type)
    {
        return GetVariant(type) != null;
    }
}

#region Skill Variant

/// <summary>
/// 通常版・強化版共通データ
/// </summary>
[System.Serializable]
public class SkillVariantData
{
    [Header("強化形態")]
    public EnhancementType enhancementType = EnhancementType.Normal;

    [Header("表示")]
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;

    [Header("基本性能")]
    public float coolTime = 1f;
    public float duration = 0f;
    public int spawnCount = 1;

    [Range(0, 100)]
    public float resetChance = 0f;

    [Header("スポーン設定")]
    public SpawnPositionType spawnPosition = SpawnPositionType.Player;

    [Tooltip("Forward時の前方距離")]
    public float forwardDistance = 2f;

    [Tooltip("RandomAround時の半径")]
    public float randomRadius = 3f;

    [Header("生成物")]
    public GameObject prefab;

    [Tooltip("0以下で無制限")]
    public float lifeTime = 0f;

    [Header("ドロップ効果")]
    [Min(1f)]
    public float dropMultiplier = 1f;

    [Header("回復効果")]
    [Min(1f)]
    public float recoveryMultiplier = 1f;

    [Header("吸血速度効果")]
    [Tooltip("吸血時間に掛ける倍率。小さいほど吸血が速い")]
    [Range(0.1f, 1f)]
    public float drainTimeMultiplier = 1f;

    [Header("パッシブ効果")]
    [Min(1f)]
    public float moveSpeedMultiplier = 1f;

    [Header("召喚CT短縮")]
    [Range(0f, 1f)]
    public float summonCoolTimeReduction = 0f;

    [Header("演出")]
    public GameObject castEffect;

    public AudioClip castSE;

    public AudioClip destroySE;
}

#endregion