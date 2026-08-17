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
            if (skill.skillData == skillData)
                return true;
        }

        return false;
    }


    /// <summary>
    /// RuntimeSkillを取得
    /// </summary>
    public RuntimeSkill GetRuntimeSkill(SkillData skillData)
    {
        if (skillData == null)
            return null;

        foreach (RuntimeSkill skill in currentSkills)
        {
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
    public bool AcquireSkill(SkillData skillData)
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
                "SkillManager : SkillDatabaseが設定されていません。"
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
                // 未所持スキルはNormalのみ候補
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

            // 所持しているスキルの強化版を候補にする
            foreach (
                EnhancementType type
                in System.Enum.GetValues(
                    typeof(EnhancementType)))
            {
                // Normalは新規取得用なので除外
                if (type == EnhancementType.Normal)
                    continue;

                // 現在使用している強化版は除外
                if (type == currentType)
                    continue;

                // 実際に存在する強化版だけ候補にする
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
                "SkillManager : 抽選可能なスキルがありません。"
            );

            return result;
        }

        // 候補数より多く要求しない
        count = Mathf.Min(
            count,
            pool.Count
        );

        // Fisher-Yatesシャッフル
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

        // 必要数だけ取得
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

        //==========================================
        // 新規取得
        //==========================================

        if (choice.enhancementType ==
            EnhancementType.Normal)
        {
            return AcquireSkill(
                choice.skillData
            );
        }

        //==========================================
        // 強化
        //==========================================

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

            // CTを減少
            skill.currentCoolTime -=
                Time.deltaTime;

            // CT終了
            if (skill.currentCoolTime <= 0f)
            {
                skill.currentCoolTime = 0f;

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

        Debug.Log(
            $"Prefab : {variant.prefab}"
        );

        // Prefab生成
        SpawnSkillObject(
            skill,
            variant
        );

        // 発動後にCTをリセット
        skill.currentCoolTime =
            variant.coolTime;

        Debug.Log(
            $"CTリセット : " +
            $"{skill.currentCoolTime}"
        );
    }


    //==================================================
    // スキルオブジェクト生成
    //==================================================

    /// <summary>
    /// スキルPrefabを生成する
    /// </summary>
    private void SpawnSkillObject(
        RuntimeSkill skill,
        SkillVariantData variant)
    {
        if (variant == null)
        {
            Debug.LogError(
                "SpawnSkillObject : Variantがnullです。"
            );

            return;
        }

        if (variant.prefab == null)
        {
            Debug.LogError(
                $"Prefabが設定されていません : " +
                $"{skill.skillData.skillName}"
            );

            return;
        }

        // SpawnCountが0以下なら1回生成
        int spawnCount =
            Mathf.Max(1, variant.spawnCount);

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

            Debug.Log(
                $"Prefab生成 : " +
                $"{obj.name} / " +
                $"位置 : {spawnPosition}"
            );

            // ISkillObjectを取得
            ISkillObject skillObject =
                obj.GetComponent<ISkillObject>();

            if (skillObject != null)
            {
                skillObject.Initialize(skill);

                Debug.Log(
                    $"ISkillObject初期化 : " +
                    $"{obj.name}"
                );
            }
            else
            {
                Debug.LogWarning(
                    $"ISkillObjectが付いていません : " +
                    $"{obj.name}"
                );
            }

            // LifeTimeが設定されている場合
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

    /// <summary>
    /// スキルの生成位置を取得する
    /// </summary>
    private Vector3 GetSpawnPosition(
        SkillVariantData variant)
    {
        // Playerが設定されていればPlayerを使用
        // 未設定の場合はSkillManager自身を使用
        Vector3 playerPosition =
            playerTransform != null
                ? playerTransform.position
                : transform.position;

        switch (variant.spawnPosition)
        {
            //==========================================
            // Player
            //==========================================

            case SpawnPositionType.Player:

                return playerPosition;


            //==========================================
            // Forward
            //==========================================

            case SpawnPositionType.Forward:

                if (playerTransform == null)
                    return playerPosition;

                return playerPosition +
                       playerTransform.forward *
                       variant.forwardDistance;


            //==========================================
            // RandomAround
            //==========================================

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


            //==========================================
            // Cursor
            //==========================================

            case SpawnPositionType.Cursor:

                return GetCursorWorldPosition();


            //==========================================
            // TargetEnemy
            //==========================================

            case SpawnPositionType.TargetEnemy:

                return GetTargetEnemyPosition();


            default:

                return playerPosition;
        }
    }


    //==================================================
    // カーソル位置
    //==================================================

    /// <summary>
    /// マウスカーソルのワールド座標を取得
    /// </summary>
    private Vector3 GetCursorWorldPosition()
    {
        Camera cam =
            Camera.main;

        if (cam == null)
            return playerTransform != null
                ? playerTransform.position
                : transform.position;

        Ray ray =
            cam.ScreenPointToRay(
                Input.mousePosition
            );

        // 地面の高さをPlayerのY座標に合わせる
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

        // Raycastできなかった場合
        return playerTransform != null
            ? playerTransform.position
            : transform.position;
    }


    //==================================================
    // 対象敵位置
    //==================================================

    /// <summary>
    /// 対象となる敵の位置を取得
    /// 現在は仮実装
    /// </summary>
    private Vector3 GetTargetEnemyPosition()
    {
        // TODO:
        // 今後、最も近い人間などを取得する処理を実装する

        return playerTransform != null
            ? playerTransform.position
            : transform.position;
    }
}