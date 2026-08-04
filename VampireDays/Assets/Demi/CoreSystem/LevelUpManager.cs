using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("レベルアップパネル")]
    public GameObject levelUpPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OpenLevelUp(int level)
    {
        GameManager.Instance.Pause();

        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);
    }

    public void CloseLevelUp()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        GameManager.Instance.Resume();
    }
}