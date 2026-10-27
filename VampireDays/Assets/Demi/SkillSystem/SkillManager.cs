
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーのスキルを管理する。
/// スキルの取得・強化・抽選・CT・オブジェクト生成を担当する。
/// 個別の攻撃効果や人間への効果付与は各スキル側で処理する。
/// </summary>
public class SkillManager : MonoBehaviour
{
    [Header("スキルデータベース")]
    [SerializeField]
    private SkillDatabase skillDatabase;

    [Header("レア度別抽選ウェイト")]
    [Min(0f)]
    [SerializeField] private float commonWeight = 55f;

    [Min(0f)]
    [SerializeField] private float rareWeight = 30f;

    [Min(0f)]
    [SerializeField] private float epicWeight = 15f;

    [Header("プレイヤー")]
    [Tooltip("スキルの生成位置・向きの基準となるPlayer")]
    [SerializeField]
    private Transform playerTransform;

    [Header("特殊パッシブスキル")]
    [Tooltip("取得時に所持スキルを一斉発動するスキル")]
    [SerializeField]
    private SkillData ancestralBloodSkillData;

    /// <summary>
    /// 現在所持しているスキル。
    /// </summary>
    private readonly List<RuntimeSkill> currentSkills = new();

    public IReadOnlyList<RuntimeSkill> CurrentSkills => currentSkills;


    //==================================================
    // Unity
    //==================================================

    private void Update()
    {
        UpdateSkills();
    }


    //==================================================
    // 所持スキル
    //==================================================

    /// <summary>
    /// 指定したスキルを所持しているか。
    /// </summary>
    public bool HasSkill(SkillData skillData)
    {
        return GetRuntimeSkill(skillData) != null;
    }

    /// <summary>
    /// 指定したスキルのRuntimeSkillを取得する。
    /// </summary>
    public RuntimeSkill GetRuntimeSkill(SkillData skillData)
    {
        if (skillData == null)
            return null;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            if (skill.skillData == skillData)
                return skill;
        }

        return null;
    }


    //==================================================
    // スキル取得
    //==================================================

    /// <summary>
    /// 新しいスキルを取得する。
    /// </summary>
    public bool AcquireSkill(SkillData skillData)
    {
        if (skillData == null)
            return false;

        if (HasSkill(skillData))
            return false;

        RuntimeSkill runtimeSkill = new RuntimeSkill(skillData);
        currentSkills.Add(runtimeSkill);

        Debug.Log($"スキル取得 : {skillData.skillName}");

        // パッシブ取得時は、既存の魔法・召喚スキルのCTを再計算する。
        if (skillData.skillType == SkillType.Passive)
        {
            RefreshSkillCoolTimes();
        }

        // 始祖の鮮血は取得時に即時発動する。
        if (skillData == ancestralBloodSkillData)
        {
            AncestralBloodSkill ancestralBlood =
                GetComponent<AncestralBloodSkill>();

            if (ancestralBlood != null)
            {
                ancestralBlood.Activate();
            }
            else
            {
                Debug.LogError(
                    "AncestralBloodSkillが見つかりません。",
                    this
                );
            }
        }

        return true;
    }


    //==================================================
    // 始祖の鮮血
    //==================================================

    /// <summary>
    /// 所持中の召喚・魔法スキルを即時発動する。
    /// 始祖の鮮血自身は発動対象から除外し、
    /// 発動後に所持リストから削除する。
    /// </summary>
    public void ActivateAncestralBloodEffect()
    {
        Debug.Log("【始祖の鮮血】効果開始");

        // 発動中に所持リストが変わっても安全なようにコピーする。
        RuntimeSkill[] skills = currentSkills.ToArray();

        int targetCount = 0;

        foreach (RuntimeSkill skill in skills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            if (skill.skillData == ancestralBloodSkillData)
                continue;

            SkillType type = skill.skillData.skillType;

            if (type != SkillType.Summon &&
                type != SkillType.Magic)
            {
                continue;
            }

            SkillVariantData variant = skill.Variant;

            if (variant == null)
            {
                Debug.LogError(
                    $"Variantがありません : {skill.skillData.skillName}",
                    this
                );
                continue;
            }

            GameObject prefab = GetSkillPrefab(skill, variant);

            if (prefab == null)
            {
                Debug.LogError(
                    $"Prefabが設定されていません : " +
                    $"{skill.skillData.skillName} / {type}",
                    this
                );
                continue;
            }

            targetCount++;

            Debug.Log(
                $"【始祖の鮮血】強制発動 : " +
                $"{skill.skillData.skillName} / {type}"
            );

            // 通常の発動処理を利用する。
            ActivateSkill(skill);

            // 発動後にCTをリセットする。
            float coolTime = GetActualCoolTime(skill);

            skill.currentCoolTimeDuration = coolTime;
            skill.currentCoolTime = coolTime;
        }

        Debug.Log($"【始祖の鮮血】発動対象数 : {targetCount}");

        RuntimeSkill ancestralBlood =
            GetRuntimeSkill(ancestralBloodSkillData);

        if (ancestralBlood != null)
        {
            currentSkills.Remove(ancestralBlood);
            Debug.Log("【始祖の鮮血】所持リストから削除しました");
        }

        Debug.Log("【始祖の鮮血】効果終了");
    }


    //==================================================
    // 強化
    //==================================================

    /// <summary>
    /// 所持スキルの強化形態を変更する。
    /// </summary>
    public bool ChangeEnhancement(
        SkillData skillData,
        EnhancementType enhancementType)
    {
        RuntimeSkill runtimeSkill = GetRuntimeSkill(skillData);

        if (runtimeSkill == null || skillData == null)
            return false;

        if (!skillData.HasVariant(enhancementType))
            return false;

        runtimeSkill.ChangeEnhancement(enhancementType);

        Debug.Log(
            $"スキル変更 : {skillData.skillName} → {enhancementType}"
        );

        if (skillData.skillType == SkillType.Passive)
        {
            RefreshSkillCoolTimes();
        }

        return true;
    }


    //==================================================
    // パッシブ：血液ドロップ倍率
    //==================================================

    /// <summary>
    /// パッシブスキルによる血液ドロップ倍率を取得する。
    /// 複数の倍率は乗算する。
    /// </summary>
    public float GetDropMultiplier()
    {
        float multiplier = 1f;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            if (skill.skillData.skillType != SkillType.Passive)
                continue;

            SkillVariantData variant = skill.Variant;

            if (variant == null)
                continue;

            multiplier *= Mathf.Max(1f, variant.dropMultiplier);
        }

        return multiplier;
    }


    //==================================================
    // 抽選候補作成
    //==================================================

    /// <summary>
    /// 現在の状態から抽選可能な候補を作成する。
    /// 未所持スキルはNormalのみ。
    /// 所持済みスキルは、現在と異なる所持可能な強化形態のみ。
    /// </summary>
    public List<SkillChoiceData> CreateChoicePool()
    {
        List<SkillChoiceData> pool = new();

        if (skillDatabase == null)
        {
            Debug.LogWarning(
                "SkillDatabaseが設定されていません。",
                this
            );
            return pool;
        }

        IReadOnlyList<SkillData> allSkills =
            skillDatabase.GetAllSkills();

        foreach (SkillData skillData in allSkills)
        {
            if (skillData == null)
                continue;

            RuntimeSkill runtimeSkill = GetRuntimeSkill(skillData);

            // 未所持：Normalのみ候補にする。
            if (runtimeSkill == null)
            {
                if (skillData.HasVariant(EnhancementType.Normal))
                {
                    pool.Add(
                        new SkillChoiceData(
                            skillData,
                            EnhancementType.Normal
                        )
                    );
                }

                continue;
            }

            EnhancementType currentType =
                runtimeSkill.enhancementType;

            foreach (EnhancementType type in
                     System.Enum.GetValues(typeof(EnhancementType)))
            {
                if (type == EnhancementType.Normal)
                    continue;

                if (type == currentType)
                    continue;

                if (!skillData.HasVariant(type))
                    continue;

                pool.Add(new SkillChoiceData(skillData, type));
            }
        }

        return pool;
    }


    //==================================================
    // ランダム抽選
    //==================================================

    /// <summary>
    /// レア度に応じた重み付き抽選を行う。
    /// 同じ候補は1回の抽選で重複しない。
    /// </summary>
    public List<SkillChoiceData> DrawSkillChoices(int count = 3)
    {
        List<SkillChoiceData> pool = CreateChoicePool();
        List<SkillChoiceData> result = new();

        if (pool.Count == 0)
        {
            Debug.LogWarning("抽選可能なスキルがありません。");
            return result;
        }

        count = Mathf.Clamp(count, 0, pool.Count);

        while (result.Count < count && pool.Count > 0)
        {
            float totalWeight = 0f;

            foreach (SkillChoiceData choice in pool)
            {
                totalWeight += GetRarityWeight(choice);
            }

            // 全候補のウェイトが0なら、残り候補から均等に選ぶ。
            if (totalWeight <= 0f)
            {
                int index = Random.Range(0, pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
                continue;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < pool.Count; i++)
            {
                currentWeight += GetRarityWeight(pool[i]);

                if (randomValue < currentWeight)
                {
                    result.Add(pool[i]);
                    pool.RemoveAt(i);
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// スキルのレア度に対応する抽選ウェイトを返す。
    /// </summary>
    private float GetRarityWeight(SkillChoiceData choice)
    {
        if (choice == null || choice.skillData == null)
            return 0f;

        float weight;

        switch (choice.skillData.rarity)
        {
            case SkillRarity.Common:
                weight = commonWeight;
                break;

            case SkillRarity.Rare:
                weight = rareWeight;
                break;

            case SkillRarity.Epic:
                weight = epicWeight;
                break;

            default:
                weight = commonWeight;
                break;
        }

        return Mathf.Max(0f, weight);
    }


    //==================================================
    // 選択確定
    //==================================================

    /// <summary>
    /// レベルアップ画面で選択された候補を適用する。
    /// </summary>
    public bool ApplySkillChoice(SkillChoiceData choice)
    {
        if (choice == null || choice.skillData == null)
            return false;

        if (choice.enhancementType == EnhancementType.Normal)
        {
            return AcquireSkill(choice.skillData);
        }

        return ChangeEnhancement(
            choice.skillData,
            choice.enhancementType
        );
    }


    //==================================================
    // スキル更新・CT
    //==================================================

    private void UpdateSkills()
    {
        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            if (skill.Variant == null)
                continue;

            // パッシブは自動発動CTを持たない。
            if (skill.skillData.skillType == SkillType.Passive)
                continue;

            // 取得直後、または強化変更後の初回発動。
            if (!skill.hasActivated)
            {
                ActivateSkill(skill);
                continue;
            }

            skill.currentCoolTime -= Time.deltaTime;

            if (skill.currentCoolTime <= 0f)
            {
                ActivateSkill(skill);
            }
        }
    }


    //==================================================
    // スキル発動
    //==================================================

    private void ActivateSkill(RuntimeSkill skill)
    {
        if (skill == null || skill.skillData == null)
            return;

        SkillType type = skill.skillData.skillType;

        if (type == SkillType.Passive)
            return;

        SkillVariantData variant = skill.Variant;

        if (variant == null)
        {
            Debug.LogError(
                $"Variantがありません : {skill.skillData.skillName}",
                this
            );
            return;
        }

        GameObject prefab = GetSkillPrefab(skill, variant);

        // Prefabが未設定なら発動済みにせず、エラーを出す。
        if (prefab == null)
        {
            Debug.LogError(
                $"Prefabが設定されていません : " +
                $"{skill.skillData.skillName} / {type}",
                this
            );
            return;
        }

        Debug.Log($"【スキル発動】{skill.skillData.skillName}");

        SpawnSkillObject(skill, variant, prefab);

        // パッシブによるCT短縮を反映する。
        skill.currentCoolTimeDuration = GetActualCoolTime(skill);
        skill.currentCoolTime = skill.currentCoolTimeDuration;
        skill.hasActivated = true;

        Debug.Log(
            $"次回CT : {skill.currentCoolTime:F2}秒"
        );
    }


    //==================================================
    // スキル種別ごとのPrefab
    //==================================================

    /// <summary>
    /// スキル種別に対応するPrefabを返す。
    /// SkillData側でPrefabを種別ごとに分けて管理する。
    /// </summary>
    private GameObject GetSkillPrefab(
        RuntimeSkill skill,
        SkillVariantData variant)
    {
        if (skill == null ||
            skill.skillData == null ||
            variant == null)
        {
            return null;
        }

        switch (skill.skillData.skillType)
        {
            case SkillType.Summon:
                return variant.summonPrefab;

            case SkillType.Magic:
                return variant.magicEffectPrefab;

            // Passiveは自動発動するPrefabとして扱わない。
            // パッシブ効果の数値処理は従来どおり別途行う。
            case SkillType.Passive:
            default:
                return null;
        }
    }


    //==================================================
    // スキルオブジェクト生成
    //==================================================

    private void SpawnSkillObject(
        RuntimeSkill skill,
        SkillVariantData variant,
        GameObject prefab)
    {
        if (skill == null || variant == null || prefab == null)
            return;

        int spawnCount = Mathf.Max(1, variant.spawnCount);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(variant);

            GameObject obj = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            // 個別スキルの初期化。
            ISkillObject skillObject = obj.GetComponent<ISkillObject>();

            if (skillObject != null)
            {
                skillObject.Initialize(skill);
            }
            else
            {
                Debug.LogWarning(
                    $"ISkillObjectが付いていません : {obj.name}",
                    obj
                );
            }

            switch (skill.skillData.skillType)
            {
                case SkillType.Summon:
                    // 召喚オブジェクト自体の寿命。
                    // 0以下なら自動破棄しない。
                    if (variant.summonLifetime > 0f)
                    {
                        Destroy(obj, variant.summonLifetime);
                    }
                    break;

                case SkillType.Magic:
                    // 魔法は召喚用寿命を使用しない。
                    // Particle Systemの再生終了を監視して破棄する。
                    MagicEffectLifetime lifetime =
                        obj.GetComponent<MagicEffectLifetime>();

                    if (lifetime == null)
                    {
                        lifetime = obj.AddComponent<MagicEffectLifetime>();
                    }

                    lifetime.Initialize();
                    break;
            }
        }
    }


    //==================================================
    // スポーン位置
    //==================================================

    private Vector3 GetSpawnPosition(SkillVariantData variant)
    {
        Vector3 playerPosition = playerTransform != null
            ? playerTransform.position
            : transform.position;

        switch (variant.spawnPosition)
        {
            case SpawnPositionType.Player:
                return playerPosition;

            case SpawnPositionType.Forward:
                if (playerTransform == null)
                    return playerPosition;

                return playerPosition +
                       playerTransform.forward *
                       variant.forwardDistance;

            case SpawnPositionType.RandomAround:
                Vector2 random =
                    Random.insideUnitCircle * variant.randomRadius;

                return playerPosition +
                       new Vector3(random.x, 0f, random.y);

            case SpawnPositionType.Cursor:
                return GetCursorWorldPosition();

            case SpawnPositionType.TargetEnemy:
                return GetTargetEnemyPosition();

            default:
                return playerPosition;
        }
    }


    /// <summary>
    /// 新Input Systemからマウス位置を取得し、
    /// プレイヤーと同じ高さの地面との交点を返す。
    /// </summary>
    private Vector3 GetCursorWorldPosition()
    {
        Vector3 fallbackPosition = playerTransform != null
            ? playerTransform.position
            : transform.position;

        Camera cam = Camera.main;

        if (cam == null || Mouse.current == null)
            return fallbackPosition;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePosition);

        float groundY = fallbackPosition.y;

        Plane groundPlane = new Plane(
            Vector3.up,
            new Vector3(0f, groundY, 0f)
        );

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return fallbackPosition;
    }


    //==================================================
    // パッシブ：魔法・召喚CT短縮
    //==================================================

    /// <summary>
    /// パッシブスキルによるCT短縮率を取得する。
    /// 各パッシブの短縮率を加算し、合計100%を上限とする。
    /// </summary>
    private float GetCoolTimeReduction(SkillType targetType)
    {
        float reduction = 0f;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            if (skill.skillData.skillType != SkillType.Passive)
                continue;

            SkillVariantData variant = skill.Variant;

            if (variant == null)
                continue;

            switch (targetType)
            {
                case SkillType.Magic:
                    reduction += variant.magicCoolTimeReduction;
                    break;

                case SkillType.Summon:
                    reduction += variant.summonCoolTimeReduction;
                    break;
            }
        }

        return Mathf.Clamp01(reduction);
    }

    /// <summary>
    /// 現在のパッシブ効果を反映したCTを計算する。
    /// </summary>
    private float GetActualCoolTime(RuntimeSkill skill)
    {
        if (skill == null || skill.skillData == null)
            return 0f;

        SkillVariantData variant = skill.Variant;

        if (variant == null)
            return 0f;

        float baseCoolTime = Mathf.Max(0f, variant.coolTime);
        SkillType type = skill.skillData.skillType;

        if (type != SkillType.Magic &&
            type != SkillType.Summon)
        {
            return baseCoolTime;
        }

        float reduction = GetCoolTimeReduction(type);

        return baseCoolTime * (1f - reduction);
    }

    /// <summary>
    /// パッシブの取得・強化時にCTを再計算する。
    /// 進行中のCTは、変更前の進行割合を維持する。
    /// </summary>
    private void RefreshSkillCoolTimes()
    {
        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            SkillType type = skill.skillData.skillType;

            if (type != SkillType.Magic &&
                type != SkillType.Summon)
            {
                continue;
            }

            if (!skill.hasActivated)
                continue;

            float newDuration = GetActualCoolTime(skill);

            // すでに発動待ちなら、残り時間は0のままにする。
            if (skill.currentCoolTime <= 0f)
            {
                skill.currentCoolTimeDuration = newDuration;
                continue;
            }

            float remainingRatio = 1f;

            if (skill.currentCoolTimeDuration > 0f)
            {
                remainingRatio = Mathf.Clamp01(
                    skill.currentCoolTime /
                    skill.currentCoolTimeDuration
                );
            }

            skill.currentCoolTimeDuration = newDuration;
            skill.currentCoolTime = newDuration * remainingRatio;

            Debug.Log(
                $"CT再計算 : {skill.skillData.skillName} / " +
                $"残りCT {skill.currentCoolTime:F2}秒"
            );
        }
    }


    //==================================================
    // 対象敵位置
    //==================================================

    private Vector3 GetTargetEnemyPosition()
    {
        // 現段階ではプレイヤー位置を返す。
        // 対象人間を指定する処理は、必要なスキル側で実装する。
        return playerTransform != null
            ? playerTransform.position
            : transform.position;
    }
}
