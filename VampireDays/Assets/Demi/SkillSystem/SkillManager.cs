using System.Collections.Generic;
using UnityEngine;

/// プレイヤーが所持するスキルを管理
public class SkillManager : MonoBehaviour
{
    [Header("Skill Database")]
    [SerializeField]
    private SkillDatabase skillDatabase;

    [Header("カーソル")]
    [SerializeField]
    private Transform cursorTransform;

    /// 所持スキル
    private readonly List<RuntimeSkill> ownedSkills = new();

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        UpdateCooldown();
    }

    /// CT更新
    private void UpdateCooldown()
    {
        foreach (RuntimeSkill runtime in ownedSkills)
        {
            // 強化系は常時発動
            if (runtime.skillData.skillType == SkillType.Passive)
                continue;

            runtime.currentCoolTime -= Time.deltaTime;

            if (runtime.currentCoolTime <= 0f)
            {
                ExecuteSkill(runtime);

                runtime.currentCoolTime = runtime.Variant.coolTime;
            }
        }
    }

    /// スキル発動
    private void ExecuteSkill(RuntimeSkill runtime)
    {
        SkillVariantData variant = runtime.Variant;

        Vector3 spawnPosition = GetSpawnPosition(variant);

        // 発動エフェクト
        if (variant.castEffect != null)
        {
            Instantiate(
                variant.castEffect,
                spawnPosition,
                Quaternion.identity);
        }

        // 発動SE
        if (variant.castSE != null)
        {
            audioSource.PlayOneShot(variant.castSE);
        }

        // オブジェクト生成
        if (variant.prefab == null)
            return;

        for (int i = 0; i < variant.spawnCount; i++)
        {
            GameObject obj =
                Instantiate(
                    variant.prefab,
                    spawnPosition,
                    Quaternion.identity);

            // 初期化
            ISkillObject skillObject =
                obj.GetComponent<ISkillObject>();

            if (skillObject != null)
            {
                skillObject.Initialize(runtime);
            }

            // 自動削除
            if (variant.lifeTime > 0f)
            {
                Destroy(obj, variant.lifeTime);
            }
        }
    }

    /// スポーン位置取得
    private Vector3 GetSpawnPosition(SkillVariantData variant)
    {
        switch (variant.spawnPosition)
        {
            case SpawnPositionType.Player:

                return transform.position;

            case SpawnPositionType.Cursor:

                if (cursorTransform != null)
                    return cursorTransform.position;

                return transform.position;

            case SpawnPositionType.Forward:

                return transform.position +
                       transform.up * variant.forwardDistance;

            case SpawnPositionType.RandomAround:

                Vector2 random =
                    Random.insideUnitCircle *
                    variant.randomRadius;

                return transform.position +
                       new Vector3(random.x, random.y);

            default:

                return transform.position;
        }
    }

    /// スキル取得
    public bool AddSkill(int id)
    {
        SkillData data = skillDatabase.GetSkill(id);

        if (data == null)
            return false;

        if (HasSkill(id))
            return false;

        ownedSkills.Add(new RuntimeSkill(data));

        return true;
    }

    /// スキル削除
    public bool RemoveSkill(int id)
    {
        RuntimeSkill runtime = GetRuntimeSkill(id);

        if (runtime == null)
            return false;

        ownedSkills.Remove(runtime);

        return true;
    }

    /// 強化変更
    public void ChangeEnhancement(
        int id,
        EnhancementType type)
    {
        RuntimeSkill runtime = GetRuntimeSkill(id);

        if (runtime == null)
            return;

        runtime.ChangeEnhancement(type);
    }

    /// 所持判定
    public bool HasSkill(int id)
    {
        return GetRuntimeSkill(id) != null;
    }

    /// RuntimeSkill取得
    private RuntimeSkill GetRuntimeSkill(int id)
    {
        foreach (RuntimeSkill runtime in ownedSkills)
        {
            if (runtime.skillData.id == id)
                return runtime;
        }

        return null;
    }

    /// 所持スキル一覧取得
    public IReadOnlyList<RuntimeSkill> GetOwnedSkills()
    {
        return ownedSkills;
    }
}