using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("レベル")]
    public int level = 1;

    [Header("現在血液数")]
    public int currentBlood;

    [Header("次レベル必要量")]
    public int requiredBlood = 5;

    public void AddBlood(int amount)
    {
        currentBlood += amount;

        while (currentBlood >= requiredBlood)
        {
            currentBlood -= requiredBlood;

            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;

        requiredBlood += 3;

        Debug.Log($"Level Up : {level}");

        LevelUpManager.Instance.OpenLevelUp(level);
    }
}