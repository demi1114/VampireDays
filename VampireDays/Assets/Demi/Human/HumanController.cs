using UnityEngine;

public class HumanController : MonoBehaviour
{
    [Header("データ")]
    public HumanData humanData;

    [Header("移動範囲")]
    public float moveRadius = 8f;

    [Header("目的地到達距離")]
    public float arriveDistance = 0.5f;

    /// <summary>
    /// 現在吸血されているか
    /// </summary>
    public bool IsBeingDrained { get; private set; }

    private Vector3 targetPosition;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;

        ChooseNewTarget();

        // 特殊人間ならエフェクト生成
        if (humanData != null &&
            humanData.isSpecialHuman &&
            humanData.specialEffect != null)
        {
            Instantiate(humanData.specialEffect, transform);
        }

        // HumanManagerへ登録
        if (HumanManager.Instance != null)
        {
            HumanManager.Instance.Register(this);
        }
    }

    private void OnDestroy()
    {
        if (HumanManager.Instance != null)
        {
            HumanManager.Instance.Unregister(this);
        }
    }

    private void Update()
    {
        Move();
    }

    /// <summary>
    /// ランダム移動
    /// </summary>
    private void Move()
    {
        // 吸血中は移動しない
        if (IsBeingDrained)
            return;

        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.magnitude < arriveDistance)
        {
            ChooseNewTarget();
            return;
        }

        dir.Normalize();

        float speed = humanData != null ? humanData.moveSpeed : 2f;

        transform.position += dir * speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = dir;
        }
    }

    /// <summary>
    /// 新しい目的地を決定
    /// </summary>
    private void ChooseNewTarget()
    {
        Vector2 random = Random.insideUnitCircle * moveRadius;

        targetPosition = startPosition +
                         new Vector3(random.x, 0f, random.y);
    }

    /// <summary>
    /// 吸血開始
    /// </summary>
    public void BeginDrain()
    {
        IsBeingDrained = true;
    }

    /// <summary>
    /// 吸血終了（人間消滅）
    /// </summary>
    public void FinishDrain()
    {
        Destroy(gameObject);
    }
}