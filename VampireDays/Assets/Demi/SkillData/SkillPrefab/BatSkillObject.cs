using UnityEngine;

/// <summary>
/// バットスキルによって生成されるバット
/// </summary>
public class BatSkillObject : MonoBehaviour, ISkillObject
{
    [Header("移動")]
    [SerializeField]
    private float moveSpeed = 2f;

    [Header("ランダム移動")]
    [SerializeField]
    private float directionChangeInterval = 1.5f;

    [SerializeField]
    private float moveRange = 5f;

    [Header("視線誘導")]
    [SerializeField]
    private float attractionRange = 6f;

    private RuntimeSkill runtimeSkill;

    private Transform player;

    private Vector3 moveDirection;

    private float directionTimer;

    private Vector3 startPosition;


    //==================================================
    // 初期化
    //==================================================

    /// <summary>
    /// スキル生成時の初期化
    /// </summary>
    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;

        startPosition = transform.position;

        // プレイヤーを取得
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // 最初の移動方向を決定
        ChangeMoveDirection();

        Debug.Log(
            "BatSkillObject 初期化"
        );
    }


    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        MoveRandomly();

        AttractHumanVision();
    }


    //==================================================
    // ランダム移動
    //==================================================

    /// <summary>
    /// ランダムに移動する
    /// </summary>
    private void MoveRandomly()
    {
        directionTimer -= Time.deltaTime;

        // 一定時間ごとに方向変更
        if (directionTimer <= 0f)
        {
            ChangeMoveDirection();
        }

        transform.position +=
            moveDirection *
            moveSpeed *
            Time.deltaTime;

        // 移動範囲を超えたら方向を変更
        Vector3 offset =
            transform.position -
            startPosition;

        if (offset.magnitude > moveRange)
        {
            moveDirection =
                -offset.normalized;

            directionTimer =
                directionChangeInterval;
        }
    }


    /// <summary>
    /// ランダムな移動方向を設定
    /// </summary>
    private void ChangeMoveDirection()
    {
        Vector2 random =
            Random.insideUnitCircle.normalized;

        moveDirection =
            new Vector3(
                random.x,
                0f,
                random.y
            );

        directionTimer =
            directionChangeInterval;
    }


    //==================================================
    // 視線誘導
    //==================================================

    /// <summary>
    /// 周囲の人間の視線をバットへ誘導する
    /// </summary>
    /// <summary>
    /// バットの周囲にいる人間の視線を誘導する
    /// </summary>
    private void AttractHumanVision()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                attractionRange
            );

        foreach (Collider hit in hits)
        {
            HumanController human =
                hit.GetComponent<HumanController>();

            if (human == null)
                continue;

            VisionController vision =
                human.GetComponent<VisionController>();

            if (vision == null)
                continue;

            vision.SetBatAttraction(transform);
        }
    }

    //==================================================
    // Gizmos
    //==================================================

    private void OnDrawGizmosSelected()
    {
        // 移動範囲
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            Application.isPlaying
                ? startPosition
                : transform.position,
            moveRange
        );

        // 視線誘導範囲
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attractionRange
        );
    }
}