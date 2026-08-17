using UnityEngine;

/// <summary>
/// プレイヤーのレベルと血液量を管理する
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    [Header("レベル")]
    [SerializeField]
    private int level = 1;

    [Header("現在血液数")]
    [SerializeField]
    private int currentBlood = 0;

    [Header("次レベル必要量")]
    [SerializeField]
    private int requiredBlood = 5;

    [Header("レベルアップごとの必要量増加")]
    [SerializeField]
    private int requiredBloodIncrease = 3;

    /// <summary>
    /// 現在のレベル
    /// </summary>
    public int Level => level;

    /// <summary>
    /// 現在の血液数
    /// </summary>
    public int CurrentBlood => currentBlood;

    /// <summary>
    /// 次のレベルに必要な血液量
    /// </summary>
    public int RequiredBlood => requiredBlood;


    /// <summary>
    /// 血液を取得
    /// </summary>
    public void AddBlood(int amount)
    {
        if (amount <= 0)
            return;

        currentBlood += amount;

        CheckLevelUp();
    }


    /// <summary>
    /// レベルアップ判定
    /// </summary>
    private void CheckLevelUp()
    {
        while (currentBlood >= requiredBlood)
        {
            currentBlood -= requiredBlood;

            LevelUp();
        }
    }


    /// <summary>
    /// レベルアップ
    /// </summary>
    private void LevelUp()
    {
        level++;

        requiredBlood += requiredBloodIncrease;

        // レベルアップUIを開く
        if (LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.OpenLevelUp(level);
        }
    }
}