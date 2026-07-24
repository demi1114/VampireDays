using System.Collections.Generic;
using UnityEngine;

/// 全スキルを管理するデータベース
[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Game/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    [Header("全スキル")]
    [SerializeField]
    private List<SkillData> skillList = new();

    /// 全スキル取得
    public IReadOnlyList<SkillData> GetAllSkills()
    {
        return skillList;
    }

    /// IDから取得
    public SkillData GetSkill(int id)
    {
        foreach (SkillData skill in skillList)
        {
            if (skill.id == id)
                return skill;
        }

        return null;
    }

    /// 名前から取得
    public SkillData GetSkill(string skillName)
    {
        foreach (SkillData skill in skillList)
        {
            if (skill.skillName == skillName)
                return skill;
        }

        return null;
    }

    /// スキルを追加
    public void AddSkill(SkillData skill)
    {
        if (skill != null && !skillList.Contains(skill))
            skillList.Add(skill);
    }

    /// スキルを削除
    public void RemoveSkill(SkillData skill)
    {
        if (skill != null)
            skillList.Remove(skill);
    }
}