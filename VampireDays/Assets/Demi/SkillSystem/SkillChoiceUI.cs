
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// レベルアップ時のスキル候補UIを管理する。
/// </summary>
public class SkillChoiceUI : MonoBehaviour
{
    [System.Serializable]
    public class ChoiceSlot
    {
        [Header("スロット")]
        public GameObject root;

        [Header("アイコン")]
        public Image icon;

        [Header("スキル名")]
        public TextMeshProUGUI skillNameText;

        [Header("説明")]
        public TextMeshProUGUI descriptionText;

        [Header("CT")]
        public TextMeshProUGUI coolTimeText;

        [Header("召喚寿命表示")]
        [Tooltip("召喚スキルのオブジェクト寿命を表示する欄")]
        public TextMeshProUGUI durationText;

        [Header("出現数")]
        public TextMeshProUGUI spawnCountText;

        [Header("選択ボタン")]
        public Button selectButton;
    }

    [Header("候補スロット")]
    [SerializeField]
    private List<ChoiceSlot> choiceSlots = new();


    //==================================================
    // 有効化
    //==================================================

    private void OnEnable()
    {
        Refresh();
    }


    //==================================================
    // UI更新
    //==================================================

    /// <summary>
    /// 現在のスキル候補をUIへ反映する。
    /// </summary>
    public void Refresh()
    {
        if (LevelUpManager.Instance == null)
            return;

        IReadOnlyList<SkillChoiceData> choices =
            LevelUpManager.Instance.CurrentChoices;

        // すべてのスロットを一旦非表示にする。
        for (int i = 0; i < choiceSlots.Count; i++)
        {
            ChoiceSlot slot = choiceSlots[i];

            if (slot == null)
                continue;

            if (slot.root != null)
                slot.root.SetActive(false);

            if (slot.selectButton != null)
                slot.selectButton.onClick.RemoveAllListeners();
        }

        // 候補を表示する。
        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceSlots.Count)
                break;

            SkillChoiceData choice = choices[i];

            if (choice == null)
                continue;

            SetupChoice(choiceSlots[i], choice, i);
        }
    }


    //==================================================
    // 候補1つ分の設定
    //==================================================

    private void SetupChoice(
        ChoiceSlot slot,
        SkillChoiceData choice,
        int index)
    {
        if (slot == null || choice == null)
            return;

        if (slot.root != null)
            slot.root.SetActive(true);

        if (choice.skillData == null)
            return;

        SkillVariantData variant = choice.Variant;

        if (variant == null)
            return;

        SkillType skillType = choice.skillData.skillType;


        //==================================================
        // アイコン
        //==================================================

        if (slot.icon != null)
        {
            slot.icon.sprite = variant.icon;
            slot.icon.enabled = variant.icon != null;
        }


        //==================================================
        // スキル名
        //==================================================

        if (slot.skillNameText != null)
        {
            slot.skillNameText.text =
                choice.skillData.skillName;
        }


        //==================================================
        // 説明
        //==================================================

        if (slot.descriptionText != null)
        {
            slot.descriptionText.text =
                variant.description;
        }


        //==================================================
        // CT
        //==================================================

        if (slot.coolTimeText != null)
        {
            if (skillType == SkillType.Passive)
            {
                slot.coolTimeText.text = "";
            }
            else
            {
                slot.coolTimeText.text =
                    $"{variant.coolTime:0.0}秒";
            }
        }


        //==================================================
        // 召喚オブジェクトの寿命
        //==================================================

        if (slot.durationText != null)
        {
            // summonLifetimeは召喚オブジェクト自体の寿命。
            // 0以下は無制限として扱うため、表示しない。
            if (skillType == SkillType.Summon &&
                variant.summonLifetime > 0f)
            {
                slot.durationText.text =
                    $"{variant.summonLifetime:0.0}秒";

                slot.durationText.gameObject.SetActive(true);
            }
            else
            {
                slot.durationText.text = "";
                slot.durationText.gameObject.SetActive(false);
            }
        }


        //==================================================
        // 出現数
        //==================================================

        if (slot.spawnCountText != null)
        {
            if (skillType != SkillType.Passive &&
                variant.spawnCount > 0)
            {
                slot.spawnCountText.text =
                    $"{variant.spawnCount}";

                slot.spawnCountText.gameObject.SetActive(true);
            }
            else
            {
                slot.spawnCountText.text = "";
                slot.spawnCountText.gameObject.SetActive(false);
            }
        }


        //==================================================
        // 選択ボタン
        //==================================================

        if (slot.selectButton != null)
        {
            slot.selectButton.onClick.RemoveAllListeners();

            int choiceIndex = index;

            slot.selectButton.onClick.AddListener(
                () => OnSelectChoice(choiceIndex)
            );
        }
    }


    //==================================================
    // スキル選択
    //==================================================

    private void OnSelectChoice(int index)
    {
        if (LevelUpManager.Instance == null)
            return;

        LevelUpManager.Instance.SelectSkill(index);
    }
}
