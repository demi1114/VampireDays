using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// レベルアップ時のスキル候補UIを管理する
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

        [Header("持続時間")]
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
    /// 現在のスキル候補をUIへ反映
    /// </summary>
    public void Refresh()
    {
        if (LevelUpManager.Instance == null)
            return;

        IReadOnlyList<SkillChoiceData> choices =
            LevelUpManager.Instance.CurrentChoices;

        // 全スロットを一旦非表示
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

        // 候補を表示
        for (int i = 0; i < choices.Count; i++)
        {
            if (i >= choiceSlots.Count)
                break;

            SkillChoiceData choice = choices[i];

            if (choice == null)
                continue;

            SetupChoice(
                choiceSlots[i],
                choice,
                i
            );
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
        if (slot == null)
            return;

        if (slot.root != null)
            slot.root.SetActive(true);

        if (choice.skillData == null)
            return;

        SkillVariantData variant =
            choice.Variant;

        if (variant == null)
            return;


        //==================================================
        // アイコン
        //==================================================

        if (slot.icon != null)
        {
            slot.icon.sprite = variant.icon;

            slot.icon.enabled =
                variant.icon != null;
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
            // 強化系はCTなし
            if (choice.skillData.skillType ==
                SkillType.Passive)
            {
                slot.coolTimeText.text = "";
            }
            else
            {
                slot.coolTimeText.text =
                    $"CT : {variant.coolTime:0.0}s";
            }
        }


        //==================================================
        // 持続時間
        //==================================================

        if (slot.durationText != null)
        {
            if (variant.duration > 0f)
            {
                slot.durationText.text =
                    $"Duration : {variant.duration:0.0}s";
            }
            else
            {
                slot.durationText.text = "";
            }
        }


        //==================================================
        // 出現数
        //==================================================

        if (slot.spawnCountText != null)
        {
            if (variant.spawnCount > 0)
            {
                slot.spawnCountText.text =
                    $"Count : {variant.spawnCount}";
            }
            else
            {
                slot.spawnCountText.text = "";
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
                () =>
                {
                    OnSelectChoice(choiceIndex);
                }
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