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
            return pool;

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

            // 所持スキルの別強化形態を候補にする
            foreach (
                EnhancementType type
                in System.Enum.GetValues(
                    typeof(EnhancementType)))
            {
                // Normalは新規取得用なので除外
                if (type == EnhancementType.Normal)
                    continue;

                // 現在使用中の形態は除外
                if (type == currentType)
                    continue;

                // 実際に存在する強化形態だけ候補にする
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
        // 強化形態変更
        //==========================================

        return ChangeEnhancement(
            choice.skillData,
            choice.enhancementType
        );
    }
}