using UnityEngine;

/// <summary>
/// ゲーム全体を管理するマネージャー
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //==================================================
    // ゲーム状態
    //==================================================

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        GameClear
    }

    public GameState State { get; private set; }


    //==================================================
    // ゲーム設定
    //==================================================

    [Header("制限時間")]
    [SerializeField]
    private float gameTime = 300f;

    [Header("時間経過ダメージ")]
    [SerializeField]
    private float damagePerSecond = 2f;


    //==================================================
    // 時間
    //==================================================

    private float currentTime;

    public float CurrentTime => currentTime;


    //==================================================
    // プレイヤー
    //==================================================

    private PlayerStatus player;
    private PlayerVampire vampire;


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
            return;
        }

        State = GameState.Playing;

        currentTime = gameTime;

        Time.timeScale = 1f;
    }

    private void Start()
    {
        FindPlayer();
    }


    private void FindPlayer()
    {
        player = FindFirstObjectByType<PlayerStatus>();
        vampire = FindFirstObjectByType<PlayerVampire>();
    }


    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        if (State != GameState.Playing)
            return;

        if (player == null || vampire == null)
        {
            FindPlayer();

            if (player == null || vampire == null)
                return;
        }

        // 時間更新
        UpdateGameTime();

        // 時間経過ダメージ
        UpdateTimeDamage();

        // GameOver判定
        CheckGameOver();

        if (State != GameState.Playing)
            return;

        // GameClear判定
        CheckGameClear();
    }


    //==================================================
    // 時間
    //==================================================

    private void UpdateGameTime()
    {
        currentTime -= Time.deltaTime;

        if (currentTime < 0f)
            currentTime = 0f;
    }


    //==================================================
    // 時間経過ダメージ
    //==================================================

    private void UpdateTimeDamage()
    {
        if (damagePerSecond <= 0f)
            return;

        player.Damage(
            damagePerSecond * Time.deltaTime
        );
    }


    //==================================================
    // GameOver判定
    //==================================================

    private void CheckGameOver()
    {
        //==============================================
        // ① HPが0
        //==============================================

        if (player.IsDead())
        {
            GameOver();
            return;
        }


        //==============================================
        // ② 吸血中でなければ視線判定不要
        //==============================================

        if (!vampire.IsDraining)
            return;


        //==============================================
        // ③ 全人間の視線を確認
        //==============================================

        VisionController[] visions =
            FindObjectsByType<VisionController>(
                FindObjectsSortMode.None
            );

        foreach (VisionController vision in visions)
        {
            if (vision == null)
                continue;


            //==========================================
            // 吸血対象自身の視線を除外
            //==========================================

            HumanController visionHuman =
                vision.GetComponent<HumanController>();

            if (visionHuman != null &&
                visionHuman == vampire.CurrentDrainTarget)
            {
                continue;
            }


            //==========================================
            // 他の人間の視線
            //==========================================

            if (!vision.IsPlayerVisible)
                continue;


            Debug.Log(
                "吸血中に別の人間の視線に発見されました！"
            );

            GameOver();

            return;
        }
    }


    //==================================================
    // GameClear
    //==================================================

    private void CheckGameClear()
    {
        if (currentTime <= 0f)
        {
            GameClear();
        }
    }


    //==================================================
    // GameOver
    //==================================================

    public void GameOver()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.GameOver;

        Debug.Log("Game Over");

        Time.timeScale = 0f;
    }


    //==================================================
    // GameClear
    //==================================================

    public void GameClear()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.GameClear;

        Debug.Log("Game Clear");

        Time.timeScale = 0f;
    }


    //==================================================
    // Pause
    //==================================================

    public void Pause()
    {
        if (State != GameState.Playing)
            return;

        State = GameState.Paused;

        Time.timeScale = 0f;
    }


    //==================================================
    // Resume
    //==================================================

    public void Resume()
    {
        if (State != GameState.Paused)
            return;

        State = GameState.Playing;

        Time.timeScale = 1f;
    }


    //==================================================
    // リセット
    //==================================================

    public void ResetGame()
    {
        State = GameState.Playing;

        currentTime = gameTime;

        FindPlayer();

        Time.timeScale = 1f;
    }
}