using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

/// <summary>
/// プレイヤーのスキルを管理する
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
    /// 現在所持しているスキル
    /// </summary>
    private readonly List<RuntimeSkill> currentSkills = new();

    /// <summary>
    /// 所持スキル一覧
    /// </summary>
    public IReadOnlyList<RuntimeSkill> CurrentSkills =>
        currentSkills;


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
    /// スキルを所持しているか
    /// </summary>
    public bool HasSkill(SkillData skillData)
    {
        if (skillData == null)
            return false;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null)
                continue;

            if (skill.skillData == skillData)
                return true;
        }

        return false;
    }


    /// <summary>
    /// RuntimeSkillを取得
    /// </summary>
    public RuntimeSkill GetRuntimeSkill(
        SkillData skillData)
    {
        if (skillData == null)
            return null;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null)
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
    /// 新しいスキルを取得
    /// </summary>

    /// <summary>
    /// 新しいスキルを取得
    /// </summary>
    public bool AcquireSkill(SkillData skillData)
    {
        if (skillData == null)
            return false;

        if (HasSkill(skillData))
            return false;

        // RuntimeSkill生成
        RuntimeSkill runtimeSkill =
            new RuntimeSkill(skillData);

        currentSkills.Add(runtimeSkill);

        Debug.Log(
            $"スキル取得 : {skillData.skillName}"
        );

        // パッシブ取得時は、既存の魔法・召喚スキルのCTを再計算
        if (skillData.skillType == SkillType.Passive)
        {
            RefreshSkillCoolTimes();
        }

        // 始祖の鮮血なら取得時に即時発動
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
                    "AncestralBloodSkillが見つかりません。"
                );
            }
        }

        return true;
    }


    /// <summary>
    /// 始祖の鮮血の効果。
    /// 所持中の召喚・魔法スキルを即時発動し、
    /// CTをリセットした後、自身を所持リストから削除する。
    /// </summary>
    /// <summary>
    /// 始祖の鮮血の効果。
    /// 所持中の召喚・魔法スキルを即時発動し、
    /// CTをリセットした後、自身を所持リストから削除する。
    /// </summary>
    public void ActivateAncestralBloodEffect()
    {
        Debug.Log("【始祖の鮮血】効果開始");

        RuntimeSkill[] skills = currentSkills.ToArray();

        int targetCount = 0;

        foreach (RuntimeSkill skill in skills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            // 始祖の鮮血自身は対象外
            if (skill.skillData == ancestralBloodSkillData)
                continue;

            SkillType type = skill.skillData.skillType;

            // 召喚・魔法以外は対象外
            if (type != SkillType.Summon &&
                type != SkillType.Magic)
            {
                Debug.Log(
                    $"【始祖の鮮血】対象外: " +
                    $"{skill.skillData.skillName} / {type}"
                );

                continue;
            }

            targetCount++;

            SkillVariantData variant = skill.Variant;

            if (variant == null)
            {
                Debug.LogError(
                    $"【始祖の鮮血】Variantがありません: " +
                    $"{skill.skillData.skillName}"
                );

                continue;
            }

            if (variant.prefab == null)
            {
                Debug.LogError(
                    $"【始祖の鮮血】Prefab未設定: " +
                    $"{skill.skillData.skillName}"
                );

                continue;
            }

            Debug.Log(
                $"【始祖の鮮血】強制発動: " +
                $"{skill.skillData.skillName} / " +
                $"種類={type} / " +
                $"Prefab={variant.prefab.name}"
            );

            // 通常の発動処理を利用
            ActivateSkill(skill);

            // 発動後にCTをリセット
            float coolTime = GetActualCoolTime(skill);

            skill.currentCoolTimeDuration = coolTime;
            skill.currentCoolTime = coolTime;

            Debug.Log(
                $"【始祖の鮮血】発動処理終了: " +
                $"{skill.skillData.skillName} / " +
                $"次回CT={coolTime:F2}秒"
            );
        }

        Debug.Log(
            $"【始祖の鮮血】発動対象数: {targetCount}"
        );

        // 始祖の鮮血自身を削除
        RuntimeSkill ancestralBlood =
            GetRuntimeSkill(ancestralBloodSkillData);

        if (ancestralBlood != null)
        {
            currentSkills.Remove(ancestralBlood);

            Debug.Log(
                "【始祖の鮮血】所持リストから削除しました"
            );
        }
        else
        {
            Debug.LogWarning(
                "【始祖の鮮血】削除対象が見つかりません"
            );
        }

        Debug.Log("【始祖の鮮血】効果終了");
    }

    //==================================================
    // 強化
    //==================================================

    /// <summary>
    /// 所持スキルの強化形態を変更
    /// </summary>
    public bool ChangeEnhancement(
        SkillData skillData,
        EnhancementType enhancementType)
    {
        RuntimeSkill runtimeSkill =
            GetRuntimeSkill(skillData);

        if (runtimeSkill == null)
            return false;

        SkillVariantData variant =
            skillData.GetVariant(enhancementType);

        if (variant == null)
            return false;

        runtimeSkill.ChangeEnhancement(
    enhancementType
);

        Debug.Log(
            $"スキル変更 : " +
            $"{skillData.skillName} → " +
            $"{enhancementType}"
        );

        // 召喚CT短縮率が変化する可能性があるため即時反映
        if(skillData.skillType == SkillType.Passive)
{
            // 強化後の短縮率を使って再計算
            RefreshSkillCoolTimes();
        }

        return true;
    }


    //==================================================
    // ドロップ倍率
    //==================================================

    /// <summary>
    /// 現在のPassiveスキルによる
    /// 血液ドロップ倍率を取得する。
    ///
    /// 何も所持していない場合は1倍。
    /// 複数のドロップUP系Passiveを持っている場合は
    /// それぞれの倍率を乗算する。
    /// </summary>
    public float GetDropMultiplier()
    {
        float multiplier = 1f;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null)
                continue;

            if (skill.skillData == null)
                continue;

            // Passive以外は対象外
            if (skill.skillData.skillType !=
                SkillType.Passive)
            {
                continue;
            }

            SkillVariantData variant =
                skill.Variant;

            if (variant == null)
                continue;

            // 1未満にならないようにする
            float value =
                Mathf.Max(
                    1f,
                    variant.dropMultiplier
                );

            multiplier *= value;
        }

        return multiplier;
    }


    //==================================================
    // 抽選候補作成
    //==================================================

    /// <summary>
    /// 現在の状態から抽選可能な全候補を作成
    /// </summary>
    public List<SkillChoiceData> CreateChoicePool()
    {
        List<SkillChoiceData> pool = new();

        if (skillDatabase == null)
        {
            Debug.LogWarning(
                "SkillManager : " +
                "SkillDatabaseが設定されていません。"
            );

            return pool;
        }

        IReadOnlyList<SkillData> allSkills =
            skillDatabase.GetAllSkills();

        foreach (SkillData skillData in allSkills)
        {
            if (skillData == null)
                continue;


            //==================================================
            // 所持済みか確認
            //==================================================

            RuntimeSkill runtimeSkill =
                GetRuntimeSkill(skillData);


            //==================================================
            // 未所持
            //==================================================

            if (runtimeSkill == null)
            {
                // Normalのみ新規取得候補にする
                if (skillData.HasVariant(
                    EnhancementType.Normal))
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


            //==================================================
            // ここから所持済み
            //==================================================

            EnhancementType currentType =
                runtimeSkill.enhancementType;


            //==================================================
            // 強化候補
            //==================================================

            foreach (
                EnhancementType type
                in System.Enum.GetValues(
                    typeof(EnhancementType)))
            {
                //==============================================
                // Normalは絶対に候補にしない
                //==============================================

                if (type == EnhancementType.Normal)
                    continue;


                //==============================================
                // 現在使用中の強化は候補にしない
                //==============================================

                if (type == currentType)
                    continue;


                //==============================================
                // 存在する強化だけ候補
                //==============================================

                if (!skillData.HasVariant(type))
                    continue;


                pool.Add(
                    new SkillChoiceData(
                        skillData,
                        type
                    )
                );
            }
        }

        return pool;
    }


    //==================================================
    // ランダム抽選
    //==================================================

    /// <summary>
    /// 抽選可能候補からランダムに指定数取得
    /// </summary>
    /// <summary>
    /// レア度に応じた重み付き抽選でスキル候補を取得する
    /// </summary>
    public List<SkillChoiceData> DrawSkillChoices(int count = 3)
    {
        List<SkillChoiceData> pool = CreateChoicePool();
        List<SkillChoiceData> result = new List<SkillChoiceData>();

        if (pool.Count == 0)
        {
            Debug.LogWarning("抽選可能なスキルがありません。");
            return result;
        }

        count = Mathf.Min(count, pool.Count);

        while (result.Count < count && pool.Count > 0)
        {
            float totalWeight = 0f;

            // 候補全体のウェイトを合計する
            foreach (SkillChoiceData choice in pool)
            {
                totalWeight += GetRarityWeight(choice);
            }

            // すべてのウェイトが0なら、残り候補から均等に選ぶ
            if (totalWeight <= 0f)
            {
                int randomIndex = Random.Range(0, pool.Count);
                result.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex);
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
    /// スキルのレア度に対応した抽選ウェイトを返す
    /// 強化形態ではなくSkillData本体のレア度を参照する
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
    /// レベルアップ画面で選択された候補を適用
    /// </summary>
    public bool ApplySkillChoice(
        SkillChoiceData choice)
    {
        if (choice == null ||
            choice.skillData == null)
        {
            return false;
        }

        // 新規取得
        if (choice.enhancementType ==
            EnhancementType.Normal)
        {
            return AcquireSkill(
                choice.skillData
            );
        }

        // 強化
        return ChangeEnhancement(
            choice.skillData,
            choice.enhancementType
        );
    }


    //==================================================
    // スキル更新
    //==================================================

    /// <summary>
    /// 所持スキルのCTを更新する
    /// </summary>
    /// <summary>
    /// 所持スキルのCTを更新する
    /// </summary>
    private void UpdateSkills()
    {
        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            SkillVariantData variant = skill.Variant;

            if (variant == null)
                continue;

            // パッシブ系はCTによる発動を行わない
            if (skill.skillData.skillType == SkillType.Passive)
                continue;

            // 取得直後、または強化形態に変更した直後
            if (!skill.hasActivated)
            {
                ActivateSkill(skill);
                continue;
            }

            // CTを減少させる
            skill.currentCoolTime -= Time.deltaTime;

            // CT終了時に再発動
            if (skill.currentCoolTime <= 0f)
            {
                Debug.Log(
                    $"CT終了 : {skill.skillData.skillName}"
                );

                ActivateSkill(skill);
            }
        }
    }


    //==================================================
    // スキル発動
    //==================================================

    /// <summary>
    /// スキルを発動する
    /// </summary>
    private void ActivateSkill(RuntimeSkill skill)
    {
        if (skill == null || skill.skillData == null)
            return;

        // パッシブ系は発動処理を行わない
        if (skill.skillData.skillType == SkillType.Passive)
            return;

        SkillVariantData variant = skill.Variant;

        if (variant == null)
        {
            Debug.LogError(
                $"Variantがありません : {skill.skillData.skillName}"
            );
            return;
        }

        Debug.Log(
            $"【スキル発動】{skill.skillData.skillName}"
        );

        // 召喚系・魔法系ともに、設定されたPrefabを生成する。
        // 魔法の効果はPrefab側の個別スクリプトで実装する。
        SpawnSkillObject(skill, variant);

        // 召喚マスターの効果を考慮してCTをリセット
        skill.currentCoolTimeDuration =
    GetActualCoolTime(skill);

        skill.currentCoolTime =
            skill.currentCoolTimeDuration;

        // 取得直後の初回発動を完了
        skill.hasActivated = true;

        Debug.Log(
            $"次回CT : {skill.currentCoolTime:F2}秒"
        );
    }


    //==================================================
    // スキルオブジェクト生成
    //==================================================

    private void SpawnSkillObject(
        RuntimeSkill skill,
        SkillVariantData variant)
    {
        if (variant == null)
            return;

        if (variant.prefab == null)
        {
            Debug.LogError(
                $"Prefabが設定されていません : " +
                $"{skill.skillData.skillName}"
            );

            return;
        }

        int spawnCount =
            Mathf.Max(
                1,
                variant.spawnCount
            );

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition =
                GetSpawnPosition(variant);

            GameObject obj =
                Instantiate(
                    variant.prefab,
                    spawnPosition,
                    Quaternion.identity
                );

            ISkillObject skillObject =
                obj.GetComponent<ISkillObject>();

            if (skillObject != null)
            {
                skillObject.Initialize(skill);
            }
            else
            {
                Debug.LogWarning(
                    $"ISkillObjectが付いていません : " +
                    $"{obj.name}"
                );
            }

            if (variant.lifeTime > 0f)
            {
                Destroy(
                    obj,
                    variant.lifeTime
                );
            }
        }
    }


    //==================================================
    // スポーン位置
    //==================================================

    private Vector3 GetSpawnPosition(
        SkillVariantData variant)
    {
        Vector3 playerPosition =
            playerTransform != null
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
                    Random.insideUnitCircle *
                    variant.randomRadius;

                return playerPosition +
                       new Vector3(
                           random.x,
                           0f,
                           random.y
                       );


            case SpawnPositionType.Cursor:

                return GetCursorWorldPosition();


            case SpawnPositionType.TargetEnemy:

                return GetTargetEnemyPosition();


            default:

                return playerPosition;
        }
    }



    /// <summary>
    /// マウスカーソルのワールド座標を取得する
    /// 新Input System対応
    /// </summary>
    private Vector3 GetCursorWorldPosition()
    {
        Camera cam = Camera.main;

        // カメラが存在しない場合はプレイヤー位置を返す
        if (cam == null)
        {
            return playerTransform != null
                ? playerTransform.position
                : transform.position;
        }

        // マウスが使用できない場合もプレイヤー位置を返す
        if (Mouse.current == null)
        {
            return playerTransform != null
                ? playerTransform.position
                : transform.position;
        }

        // 新Input Systemからマウス位置を取得
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // マウス位置からカメラのレイを作成
        Ray ray =
            cam.ScreenPointToRay(mousePosition);

        // プレイヤーと同じ高さの地面を設定
        float groundY =
            playerTransform != null
                ? playerTransform.position.y
                : transform.position.y;

        Plane groundPlane =
            new Plane(
                Vector3.up,
                new Vector3(0f, groundY, 0f)
            );

        // レイと地面の交点を取得
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // 交点がない場合はプレイヤー位置を返す
        return playerTransform != null
            ? playerTransform.position
            : transform.position;
    }


    //==================================================
    // 召喚CT
    //==================================================


    //==================================================
    // 魔法・召喚CT短縮
    //==================================================

    /// <summary>
    /// 指定した種類のスキルに対する
    /// パッシブのCT短縮率を取得する。
    /// </summary>
    private float GetCoolTimeReduction(SkillType targetType)
    {
        float reduction = 0f;

        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null || skill.skillData == null)
                continue;

            // パッシブのみ対象
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

        // 短縮率の上限は100%
        return Mathf.Clamp01(reduction);
    }

    /// <summary>
    /// 元のCTから、現在の短縮率を反映したCTを計算する。
    /// </summary>
    private float GetActualCoolTime(RuntimeSkill skill)
    {
        if (skill == null || skill.skillData == null)
            return 0f;

        SkillVariantData variant = skill.Variant;

        if (variant == null)
            return 0f;

        // 強化形態に設定されている元のCT
        float baseCoolTime = Mathf.Max(0f, variant.coolTime);

        SkillType type = skill.skillData.skillType;

        // 魔法・召喚以外は短縮しない
        if (type != SkillType.Magic &&
            type != SkillType.Summon)
        {
            return baseCoolTime;
        }

        float reduction = GetCoolTimeReduction(type);

        // 元のCTから毎回計算する
        return baseCoolTime * (1f - reduction);
    }

    /// <summary>
    /// パッシブの取得・強化時に、
    /// 所持中の魔法・召喚スキルのCTを再計算する。
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

            // まだ初回発動していないスキルは、
            // ActivateSkillで最新のCTを設定する
            if (!skill.hasActivated)
                continue;

            float newDuration = GetActualCoolTime(skill);

            // 発動待ちなら、残りCTは0のままにする
            if (skill.currentCoolTime <= 0f)
            {
                skill.currentCoolTimeDuration = newDuration;
                continue;
            }

            // 変更前のCTに対する残り時間の割合を維持
            float remainingRatio = 1f;

            if (skill.currentCoolTimeDuration > 0f)
            {
                remainingRatio = Mathf.Clamp01(
                    skill.currentCoolTime /
                    skill.currentCoolTimeDuration
                );
            }

            // 新しいCTを元のCTから再計算
            skill.currentCoolTimeDuration = newDuration;

            // 変更前の進行度を維持して残りCTを更新
            skill.currentCoolTime =
                newDuration * remainingRatio;

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
        return playerTransform != null
            ? playerTransform.position
            : transform.position;
    }
}