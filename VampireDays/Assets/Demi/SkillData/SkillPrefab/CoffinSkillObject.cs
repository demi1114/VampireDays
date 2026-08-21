using UnityEngine;

/// <summary>
/// 棺桶召喚スキルによって生成される棺桶
/// </summary>
public class CoffinSkillObject : MonoBehaviour, ISkillObject
{
    [Header("移動速度")]
    [SerializeField]
    private float moveSpeed = 2f;

    [Header("生成地点からの移動範囲")]
    [SerializeField]
    private float moveRange = 5f;

    [Header("目的地到着距離")]
    [SerializeField]
    private float arrivalDistance = 0.2f;

    [Header("人間捕獲範囲")]
    [SerializeField]
    private float captureRadius = 0.8f;

    [Header("プレイヤー回収判定範囲")]
    [SerializeField]
    private float collectRadius = 0.8f;

    [Header("デバッグ")]
    [SerializeField]
    private bool showDebugLog = true;

    //==================================================
    // スキル情報
    //==================================================

    private RuntimeSkill runtimeSkill;

    //==================================================
    // 移動
    //==================================================

    private Vector3 startPosition;

    private Vector3 targetPosition;

    //==================================================
    // 捕獲した人間
    //==================================================

    private int bloodAmount = 1;

    private bool hasCaptured;

    //==================================================
    // ライフタイム
    //==================================================

    private float lifeTimer;

    private float lifeTime;

    //==================================================
    // プレイヤー
    //==================================================

    private Transform player;

    //==================================================
    // 初期化
    //==================================================

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;

        startPosition =
            transform.position;

        // スキルデータからLifeTimeを取得
        if (runtimeSkill != null &&
            runtimeSkill.Variant != null)
        {
            lifeTime =
                runtimeSkill.Variant.lifeTime;
        }

        // プレイヤー取得
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }

        // 最初の目的地
        SetRandomTarget();

        if (showDebugLog)
        {
            Debug.Log(
                "CoffinSkillObject 初期化"
            );
        }
    }

    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        UpdateLifeTime();

        // 人間を捕獲するまでは移動
        if (!hasCaptured)
        {
            MoveToTarget();

            SearchHuman();

            return;
        }

        // 捕獲後はその場に留まる
        CheckPlayerCollection();
    }

    //==================================================
    // LifeTime
    //==================================================

    private void UpdateLifeTime()
    {
        // 0以下なら無制限
        if (lifeTime <= 0f)
            return;

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    //==================================================
    // ランダム移動
    //==================================================

    private void MoveToTarget()
    {
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed *
                Time.deltaTime
            );

        if (Vector3.Distance(
                transform.position,
                targetPosition)
            <= arrivalDistance)
        {
            SetRandomTarget();
        }
    }

    private void SetRandomTarget()
    {
        Vector2 random =
            Random.insideUnitCircle *
            moveRange;

        targetPosition =
            startPosition +
            new Vector3(
                random.x,
                0f,
                random.y
            );
    }

    //==================================================
    // 人間検索
    //==================================================

    private void SearchHuman()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                captureRadius
            );

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            HumanController human =
                hit.GetComponent<HumanController>();

            if (human == null)
                continue;

            // 吸血中の人間は対象外
            if (human.IsBeingDrained)
                continue;

            // 人間を捕獲
            if (CaptureHuman(human))
            {
                break;
            }
        }
    }

    //==================================================
    // 人間捕獲
    //==================================================

    private bool CaptureHuman(
        HumanController human)
    {
        if (human == null)
            return false;

        if (hasCaptured)
            return false;

        // 血液量を保存
        bloodAmount =
            human.humanData != null
                ? human.humanData.bloodAmount
                : 1;

        if (bloodAmount <= 0)
            bloodAmount = 1;

        // 人間を削除
        Destroy(human.gameObject);

        // 捕獲済みにする
        hasCaptured = true;

        if (showDebugLog)
        {
            Debug.Log(
                $"棺桶が人間を捕獲。" +
                $"血液量 : {bloodAmount}"
            );
        }

        return true;
    }

    //==================================================
    // プレイヤーによる血液回収
    //==================================================

    private void CheckPlayerCollection()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance > collectRadius)
            return;

        CollectBlood();
    }

    //==================================================
    // 血液回収
    //==================================================

    private void CollectBlood()
    {
        if (!hasCaptured)
            return;

        if (player == null)
            return;


        PlayerLevel playerLevel =
            player.GetComponent<PlayerLevel>();

        if (playerLevel == null)
        {
            Debug.LogWarning(
                "CoffinSkillObject : " +
                "PlayerLevelが見つかりません。"
            );

            return;
        }


        //==============================================
        // ドロップUP適用
        //==============================================

        int finalAmount =
            bloodAmount;


        DropUpSkill dropUpSkill =
            player.GetComponent<DropUpSkill>();


        if (dropUpSkill != null)
        {
            finalAmount =
                dropUpSkill.CalculateDropAmount(
                    bloodAmount
                );
        }


        //==============================================
        // 血液追加
        //==============================================

        playerLevel.AddBlood(
            finalAmount
        );


        if (showDebugLog)
        {
            Debug.Log(
                $"棺桶から血液取得 : " +
                $"基本量={bloodAmount} / " +
                $"最終量={finalAmount}"
            );
        }


        //==============================================
        // 棺桶削除
        //==============================================

        Destroy(gameObject);
    }

    //==================================================
    // Gizmos
    //==================================================

    private void OnDrawGizmosSelected()
    {
        // 移動範囲
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            Application.isPlaying
                ? startPosition
                : transform.position,
            moveRange
        );

        // 人間捕獲範囲
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            captureRadius
        );

        // プレイヤー回収範囲
        Gizmos.color =
            Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            collectRadius
        );
    }
}