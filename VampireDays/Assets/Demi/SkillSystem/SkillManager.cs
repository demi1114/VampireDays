using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーのスキルを管理する
/// </summary>
public class SkillManager : MonoBehaviour
{
    [Header("スキルデータベース")]
    [SerializeField]
    private SkillDatabase skillDatabase;

    [Header("プレイヤー")]
    [Tooltip("スキルの生成位置・向きの基準となるPlayer")]
    [SerializeField]
    private Transform playerTransform;

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
    public bool AcquireSkill(
        SkillData skillData)
    {
        if (skillData == null)
            return false;

        // すでに所持している場合は取得できない
        if (HasSkill(skillData))
            return false;

        RuntimeSkill runtimeSkill =
            new RuntimeSkill(skillData);

        currentSkills.Add(runtimeSkill);

        Debug.Log(
            $"スキル取得 : {skillData.skillName}"
        );

        return true;
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

            //==========================================
            // 未所持スキル
            //==========================================

            if (!HasSkill(skillData))
            {
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

            //==========================================
            // 所持スキル
            //==========================================

            RuntimeSkill runtimeSkill =
                GetRuntimeSkill(skillData);

            if (runtimeSkill == null)
                continue;

            EnhancementType currentType =
                runtimeSkill.enhancementType;

            foreach (
                EnhancementType type
                in System.Enum.GetValues(
                    typeof(EnhancementType)))
            {
                // Normalは新規取得用
                if (type == EnhancementType.Normal)
                    continue;

                // 現在使用中の強化は除外
                if (type == currentType)
                    continue;

                // 存在する強化だけ候補
                if (skillData.HasVariant(type))
                {
                    pool.Add(
                        new SkillChoiceData(
                            skillData,
                            type
                        )
                    );
                }
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
    public List<SkillChoiceData> DrawSkillChoices(
        int count = 3)
    {
        List<SkillChoiceData> pool =
            CreateChoicePool();

        List<SkillChoiceData> result = new();

        if (pool.Count == 0)
        {
            Debug.LogWarning(
                "SkillManager : " +
                "抽選可能なスキルがありません。"
            );

            return result;
        }

        count =
            Mathf.Min(
                count,
                pool.Count
            );

        // Fisher-Yates
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            SkillChoiceData temp =
                pool[i];

            pool[i] =
                pool[randomIndex];

            pool[randomIndex] =
                temp;
        }

        for (int i = 0; i < count; i++)
        {
            result.Add(pool[i]);
        }

        return result;
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
    private void UpdateSkills()
    {
        foreach (RuntimeSkill skill in currentSkills)
        {
            if (skill == null)
                continue;

            if (skill.skillData == null)
                continue;

            SkillVariantData variant =
                skill.Variant;

            if (variant == null)
                continue;

            // PassiveはCTを使用しない
            if (skill.skillData.skillType ==
                SkillType.Passive)
            {
                continue;
            }

            skill.currentCoolTime -=
                Time.deltaTime;

            if (skill.currentCoolTime <= 0f)
            {
                skill.currentCoolTime = 0f;

                Debug.Log(
                    $"CT終了 : " +
                    $"{skill.skillData.skillName}"
                );

                ActivateSkill(skill);
            }
        }
    }


    //==================================================
    // スキル発動
    //==================================================

    private void ActivateSkill(
        RuntimeSkill skill)
    {
        if (skill == null)
            return;

        if (skill.skillData == null)
            return;

        SkillVariantData variant =
            skill.Variant;

        if (variant == null)
        {
            Debug.LogError(
                $"Variantがありません : " +
                $"{skill.skillData.skillName}"
            );

            return;
        }

        Debug.Log(
            $"【スキル発動】" +
            $"{skill.skillData.skillName}"
        );

        SpawnSkillObject(
            skill,
            variant
        );

        skill.currentCoolTime =
            variant.coolTime;
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


    //==================================================
    // カーソル位置
    //==================================================

    private Vector3 GetCursorWorldPosition()
    {
        Camera cam =
            Camera.main;

        if (cam == null)
        {
            return playerTransform != null
                ? playerTransform.position
                : transform.position;
        }

        Ray ray =
            cam.ScreenPointToRay(
                Input.mousePosition
            );

        float groundY =
            playerTransform != null
                ? playerTransform.position.y
                : transform.position.y;

        Plane groundPlane =
            new Plane(
                Vector3.up,
                new Vector3(
                    0f,
                    groundY,
                    0f
                )
            );

        if (groundPlane.Raycast(
            ray,
            out float distance))
        {
            return ray.GetPoint(distance);
        }

        return playerTransform != null
            ? playerTransform.position
            : transform.position;
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