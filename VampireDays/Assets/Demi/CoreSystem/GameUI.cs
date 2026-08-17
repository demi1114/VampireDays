using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲーム中のUI管理
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HP")]
    [SerializeField]
    private Slider hpSlider;

    [Header("時間")]
    [SerializeField]
    private TextMeshProUGUI timeText;

    [Header("人間数")]
    [SerializeField]
    private TextMeshProUGUI humanCountText;

    [Header("視線検知")]
    [SerializeField]
    private TextMeshProUGUI visionText;

    [Header("吸血状態")]
    [SerializeField]
    private TextMeshProUGUI drainText;

    [Header("レベル")]
    [SerializeField]
    private TextMeshProUGUI levelText;

    [Header("血液")]
    [SerializeField]
    private TextMeshProUGUI bloodText;

    [Header("ゲーム終了UI")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private GameObject gameClearPanel;

    private PlayerStatus player;
    private PlayerVampire vampire;
    private PlayerLevel level;

    private void Start()
    {
        // プレイヤー関連コンポーネント取得
        player = FindFirstObjectByType<PlayerStatus>();
        vampire = FindFirstObjectByType<PlayerVampire>();
        level = FindFirstObjectByType<PlayerLevel>();

        // ゲーム終了UIは最初は非表示
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

        humanCountText.text =
            $"Humans : {HumanManager.Instance.HumanCount}";
    }

    /// <summary>
    /// 視線検知更新
    /// </summary>
    private void UpdateVision()
    {
        if (visionText == null)
            return;

        VisionController[] visions =
            FindObjectsByType<VisionController>(
                FindObjectsSortMode.None
            );

        bool detected = false;

        foreach (VisionController vision in visions)
        {
            if (vision == null)
                continue;

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

        drainText.text =
            vampire.IsDraining ? "Draining..." : "";
    }

    /// <summary>
    /// レベル・血液更新
    /// </summary>
    private void UpdateLevel()
    {
        if (level == null)
            return;

        // レベル
        if (levelText != null)
        {
            levelText.text = $"Lv {level.Level}";
        }

        // 血液
        if (bloodText != null)
        {
            bloodText.text =
                $"Blood : {level.CurrentBlood}/{level.RequiredBlood}";
        }
    }

    /// <summary>
    /// ゲーム状態更新
    /// </summary>
    private void UpdateGameState()
    {
        if (GameManager.Instance == null)
            return;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                GameManager.Instance.State ==
                GameManager.GameState.GameOver
            );
        }

        if (gameClearPanel != null)
        {
            gameClearPanel.SetActive(
                GameManager.Instance.State ==
                GameManager.GameState.GameClear
            );
        }
    }
}