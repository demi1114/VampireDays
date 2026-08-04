using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム中のUI管理
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HP")]
    public Slider hpSlider;

    [Header("時間")]
    public TextMeshProUGUI timeText;

    [Header("人間数")]
    public TextMeshProUGUI humanCountText;

    [Header("視線検知")]
    public TextMeshProUGUI visionText;

    [Header("吸血状態")]
    public TextMeshProUGUI drainText;

    [Header("レベル")]
    public TextMeshProUGUI levelText;

    [Header("血液")]
    public TextMeshProUGUI bloodText;

    [Header("ゲーム終了UI")]
    public GameObject gameOverPanel;
    public GameObject gameClearPanel;

    private PlayerStatus player;
    private PlayerVampire vampire;
    private PlayerLevel level;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerStatus>();
        vampire = FindFirstObjectByType<PlayerVampire>();
        level = FindFirstObjectByType<PlayerLevel>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameClearPanel != null)
            gameClearPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateHP();
        UpdateTime();
        UpdateHumanCount();
        UpdateVision();
        UpdateDrain();
        UpdateLevel();
        UpdateGameState();
    }

    /// <summary>
    /// HP更新
    /// </summary>
    private void UpdateHP()
    {
        if (player == null || hpSlider == null)
            return;

        hpSlider.maxValue = player.maxHP;
        hpSlider.value = player.currentHP;
    }

    /// <summary>
    /// 時間更新
    /// </summary>
    private void UpdateTime()
    {
        if (timeText == null || GameManager.Instance == null)
            return;

        float time = GameManager.Instance.CurrentTime;

        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);

        timeText.text = $"{min:00}:{sec:00}";
    }

    /// <summary>
    /// 人間数更新
    /// </summary>
    private void UpdateHumanCount()
    {
        if (humanCountText == null || HumanManager.Instance == null)
            return;

        humanCountText.text = $"Humans : {HumanManager.Instance.HumanCount}";
    }

    /// <summary>
    /// 視線検知更新
    /// </summary>
    private void UpdateVision()
    {
        if (visionText == null)
            return;

        VisionController[] visions = FindObjectsByType<VisionController>(FindObjectsSortMode.None);

        bool detected = false;

        foreach (VisionController vision in visions)
        {
            if (vision.IsPlayerVisible)
            {
                detected = true;
                break;
            }
        }

        visionText.text = detected ? "Detected" : "Hidden";
    }

    /// <summary>
    /// 吸血状態更新
    /// </summary>
    private void UpdateDrain()
    {
        if (drainText == null || vampire == null)
            return;

        drainText.text = vampire.IsDraining ? "Draining..." : "";
    }

    /// <summary>
    /// レベル・血液更新
    /// </summary>
    private void UpdateLevel()
    {
        if (level == null)
            return;

        if (levelText != null)
            levelText.text = $"Lv {level.level}";

        if (bloodText != null)
            bloodText.text = $"Blood : {level.currentBlood}/{level.requiredBlood}";
    }

    /// <summary>
    /// ゲーム状態更新
    /// </summary>
    private void UpdateGameState()
    {
        if (GameManager.Instance == null)
            return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(GameManager.Instance.State == GameManager.GameState.GameOver);

        if (gameClearPanel != null)
            gameClearPanel.SetActive(GameManager.Instance.State == GameManager.GameState.GameClear);
    }
}