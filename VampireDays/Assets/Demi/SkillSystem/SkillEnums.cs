
/// スキルの種類
public enum SkillType
{
    Summon,    // 召喚系
    Magic,     // 魔法系
    Passive    // 強化系（常時効果）
}

/// 強化形態
public enum EnhancementType
{
    Normal,

    Emerald,
    Sapphire,
    Ruby,
    Opal
}

/// スポーン位置
public enum SpawnPositionType
{
    Player,        // プレイヤー
    Cursor,        // カーソル
    RandomAround,  // プレイヤー周囲ランダム
    TargetEnemy,   // 対象の敵
    Forward        // プレイヤー前方
}
