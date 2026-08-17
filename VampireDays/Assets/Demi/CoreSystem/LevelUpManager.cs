using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レベルアップ画面を管理する
/// </summary>
public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("レベルアップパネル")]
    [SerializeField]
    private GameObject levelUpPanel;

    [Header("スキル管理")]
    [SerializeField]
    private SkillManager skillManager;

    [Header("スキル候補UI")]
    [SerializeField]
    private SkillChoiceUI skillChoiceUI;

    [Header("候補数")]
    [SerializeField]
    private int choiceCount = 3;

    /// <summary>
    /// 現在表示している候補
    /// </summary>
    private List<SkillChoiceData> currentChoices = new();

    /// <summary>
    /// 現在の候補を外部から取得
    /// </summary>
    public IReadOnlyList<SkillChoiceData> CurrentChoices =>
        currentChoices;


    //==================================================
    // 初期化
    //==================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    //==================================================
    // レベルアップ
    //==================================================

    /// <summary>
    /// レベルアップ画面を開く
    /// </summary>
    public void OpenLevelUp(int level)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Pause();

        // スキル候補を抽選
        DrawChoices();

        // パネル表示
        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);

        // UI更新
        if (skillChoiceUI != null)
            skillChoiceUI.Refresh();
    }


    /// <summary>
    /// スキル候補を抽選する
    /// </summary>
    private void DrawChoices()
    {
        currentChoices.Clear();

        if (skillManager == null)
        {
            Debug.LogWarning(
                "LevelUpManager : SkillManagerが設定されていません。"
            );

            return;
        }

        currentChoices =
            skillManager.DrawSkillChoices(choiceCount);

        Debug.Log(
            $"スキル候補を {currentChoices.Count} 個抽選しました。"
        );
    }


    //==================================================
    // スキル選択
    //==================================================

    /// <summary>
    /// UIから選択されたスキルを適用する
    /// </summary>
    public void SelectSkill(int index)
    {
        if (index < 0 ||
            index >= currentChoices.Count)
        {
            Debug.LogWarning(
                "LevelUpManager : 無効なスキル候補です。"
            );

            return;
        }

        SkillChoiceData choice =
            currentChoices[index];

        if (choice == null)
        {
            Debug.LogWarning(
                "LevelUpManager : SkillChoiceDataがnullです。"
            );

            return;
        }

        // スキルを適用
        bool success =
            skillManager.ApplySkillChoice(choice);

        if (!success)
        {
            Debug.LogWarning(
                "スキルの適用に失敗しました。"
            );

            return;
        }

        Debug.Log(
            $"スキル選択 : " +
            $"{choice.skillData.skillName} / " +
            $"{choice.enhancementType}"
        );

        // レベルアップ画面を閉じる
        CloseLevelUp();
    }


    //==================================================
    // 終了
    //==================================================

    /// <summary>
    /// レベルアップ画面を閉じる
    /// </summary>
    public void CloseLevelUp()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.Resume();
    }
}