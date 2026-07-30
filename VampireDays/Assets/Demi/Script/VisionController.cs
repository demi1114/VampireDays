using UnityEngine;

/// <summary>
/// 人間の視線判定を管理する
/// 赤い直線上にプレイヤーがいる時のみ検知する
/// 吸血されている本人の視線はゲームオーバー判定から除外する
/// </summary>
public class VisionController : MonoBehaviour
{
    [Header("視線距離")]
    public float viewDistance = 6f;

    [Header("直線判定の許容幅")]
    public float lineThreshold = 0.1f;

    [Header("障害物レイヤー")]
    public LayerMask obstacleMask;

    private Transform player;
    private PlayerVampire vampire;
    private HumanController human;

    public bool IsPlayerVisible { get; private set; }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            vampire = playerObject.GetComponent<PlayerVampire>();
        }

        human = GetComponent<HumanController>();
    }

    private void Update()
    {
        CheckPlayer();
    }

    private void CheckPlayer()
    {
        IsPlayerVisible = false;

        if (player == null)
            return;

        // 吸血されている本人の視線は無効
        if (human != null && human.IsBeingDrained)
            return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward.normalized;

        Vector3 toPlayer = player.position - origin;
        toPlayer.y = 0f;

        float forwardDistance = Vector3.Dot(forward, toPlayer);

        if (forwardDistance < 0f || forwardDistance > viewDistance)
            return;

        Vector3 projectedPoint = forward * forwardDistance;
        float offset = (toPlayer - projectedPoint).magnitude;

        if (offset > lineThreshold)
            return;

        if (Physics.Raycast(origin, forward, out RaycastHit hit, viewDistance, obstacleMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return;
        }

        IsPlayerVisible = true;

        // 吸血中に見られたらゲームオーバー
        if (vampire != null && vampire.IsDraining)
        {
            GameManager.Instance.GameOver();
        }
    }
}