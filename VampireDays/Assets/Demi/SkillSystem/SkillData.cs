using System.Collections.Generic;
using UnityEngine;

/// スキルのScriptableObject
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

    /// 指定した強化形態のデータを取得
    public SkillVariantData GetVariant(EnhancementType type)
    {
        if (type == EnhancementType.Normal)
            return normalData;

        foreach (var data in enhancementList)
        {
            if (data.enhancementType == type)
                return data;
        }

        return normalData;
    }
}

#region Skill Variant

/// 通常版・強化版共通データ
[System.Serializable]
public class SkillVariantData
{
    [Header("強化形態")]
    public EnhancementType enhancementType = EnhancementType.Normal;

    // 表示
    [Header("表示")]
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;

    // 基本性能
    [Header("CT/持続時間/生成数/リセット確率")]
    public float coolTime = 1f;
    public float duration = 0f;
    public int spawnCount = 1;

    [Range(0, 100)]
    public float resetChance = 0f;

    // スポーン設定
    [Header("スポーン設定")]
    public SpawnPositionType spawnPosition = SpawnPositionType.Player;

    [Tooltip("Forward時の前方距離")]
    public float forwardDistance = 2f;

    [Tooltip("RandomAround時の半径")]
    public float randomRadius = 3f;

    // 生成物
    [Header("生成物")]
    public GameObject prefab;

    [Tooltip("0以下で無制限")]
    public float lifeTime = 0f;

    // 演出
    [Header("演出")]
    public GameObject castEffect;
    public AudioClip castSE;
    public AudioClip destroySE;
}

#endregion