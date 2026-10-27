
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// パッシブスキルの常時エフェクトを管理する。
///
/// ・取得時にエフェクトを表示
/// ・強化形態の変更時にエフェクトを置き換える
/// ・複数のパッシブスキルを個別に管理
/// ・不要になったエフェクトを破棄
/// </summary>
public class PassiveSkillEffectManager : MonoBehaviour
{
    // RuntimeSkillごとに表示中のエフェクトを管理する
    private readonly Dictionary<RuntimeSkill, GameObject>
        activeEffects = new();

    /// <summary>
    /// パッシブエフェクトを表示・更新する
    /// </summary>
    public void RefreshEffect(RuntimeSkill runtimeSkill)
    {
        if (runtimeSkill == null ||
            runtimeSkill.skillData == null)
        {
            return;
        }

        // 既存エフェクトがあれば先に破棄する
        RemoveEffect(runtimeSkill);

        // パッシブスキル以外は対象外
        if (runtimeSkill.skillData.skillType != SkillType.Passive)
            return;

        SkillVariantData variant = runtimeSkill.Variant;

        if (variant == null)
        {
            Debug.LogWarning(
                $"パッシブのVariantがありません: " +
                runtimeSkill.skillData.skillName,
                this
            );
            return;
        }

        // エフェクト未設定なら何も表示しない
        if (variant.passiveEffectPrefab == null)
        {
            Debug.LogWarning(
                $"パッシブエフェクト未設定: " +
                $"{runtimeSkill.skillData.skillName} / " +
                $"{runtimeSkill.enhancementType}",
                this
            );
            return;
        }

        // プレイヤーの子オブジェクトとして生成し、追従させる
        GameObject effect = Instantiate(
            variant.passiveEffectPrefab,
            transform
        );

        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = Quaternion.identity;

        activeEffects[runtimeSkill] = effect;
    }

    /// <summary>
    /// 指定したパッシブのエフェクトを破棄する
    /// </summary>
    public void RemoveEffect(RuntimeSkill runtimeSkill)
    {
        if (runtimeSkill == null)
            return;

        if (!activeEffects.TryGetValue(
            runtimeSkill,
            out GameObject effect))
        {
            return;
        }

        if (effect != null)
        {
            Destroy(effect);
        }

        activeEffects.Remove(runtimeSkill);
    }

    /// <summary>
    /// すべてのパッシブエフェクトを破棄する
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (GameObject effect in activeEffects.Values)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }

        activeEffects.Clear();
    }

    private void OnDestroy()
    {
        ClearAllEffects();
    }
}
