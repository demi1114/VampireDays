using UnityEngine;

/// <summary>
/// ゲーム全体を管理するマネージャー
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        GameClear
    }

    [Header("制限時間（秒）")]
    public float gameTime = 300f;

    [Header("時間経過ダメージ（1秒あたり）")]
    public float damagePerSecond = 2f;

    /// <summary>
    /// 現在のゲーム状態
    /// </summary>
    public GameState State { get; private set; } = GameState.Playing;

    /// <summary>
    /// 現在の残り時間
    /// </summary>
    public float CurrentTime => currentTime;

    private float currentTime;
    private PlayerStatus player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentTime = gameTime;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerStatus>();
    }

    private void Update()
    {
        if (State != GameState.Playing)
            return;

        if (player == null)
            return;

        // 制限時間更新
        currentTime -= Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;

        // 時間経過ダメージ
        player.Damage(damagePerSecond * Time.deltaTime);

        // HP0でゲームオーバー
        if (player.IsDead())
        {
            GameOver();
            return;
        }

        // 時間切れでゲームクリア
        if (currentTime <= 0f)
        {
            GameClear();
        }
    }

    /// <summary>
    /// ゲームオーバー
    /// </summary>
    public void GameOver()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.GameOver;

        Debug.Log("Game Over");

        Time.timeScale = 0f;
    }

    /// <summary>
    /// ゲームクリア
    /// </summary>
    public void GameClear()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.GameClear;

        Debug.Log("Game Clear");

        Time.timeScale = 0f;
    }

    /// <summary>
    /// 一時停止
    /// </summary>
    public void Pause()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 一時停止解除
    /// </summary>
    public void Resume()
    {
        if (State != GameState.Paused)
            return;

        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// ゲームリセット（将来シーンリロード用）
    /// </summary>
    public void ResetGame()
    {
        State = GameState.Playing;
        currentTime = gameTime;
        Time.timeScale = 1f;
    }
}