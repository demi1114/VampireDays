using UnityEngine;

/// <summary>
/// 人間の視線を管理する
///
/// ・視線は直線
/// ・一定距離内のみプレイヤーを検知
/// ・バットによる視線誘導に対応
/// ・魅了中も視線を維持
/// ・現在吸血されている人間だけGameOver判定から除外
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class VisionController : MonoBehaviour
{
    [Header("視線設定")]
    [Tooltip("視線が届く最大距離")]
    [SerializeField]
    public float viewDistance = 8f;

    public float ViewDistance => viewDistance;

    [Header("視線表示")]
    [SerializeField]
    private LineRenderer visionLine;

    [SerializeField]
    private float lineHeight = 1f;

    /// <summary>
    /// プレイヤーが現在視線に入っているか
    /// </summary>
    public bool IsPlayerVisible { get; private set; }

    /// <summary>
    /// 夜のとばりなどで視線が消去されているか
    /// </summary>
    public bool IsVisionSuppressed { get; private set; }

    /// <summary>
    /// 視線を永久に消去する
    /// </summary>
    public void SuppressVision()
    {
        IsVisionSuppressed = true;
        IsPlayerVisible = false;
    }

    /// <summary>
    /// 視線の消去を解除する
    /// </summary>
    public void RestoreVision()
    {
        IsVisionSuppressed = false;
    }

    // バット誘導
    private Transform attractedBat;

    public bool IsAttractedToBat => attractedBat != null;

    private HumanController human;

    private void Awake()
    {
        human = GetComponent<HumanController>();

        if (visionLine == null)
            visionLine = GetComponent<LineRenderer>();

        IsPlayerVisible = false;

        if (visionLine != null)
        {
            visionLine.positionCount = 2;
            visionLine.enabled = true;
        }
    }

    private void Update()
    {
        // 視線消去中は表示・検知の両方を停止
        if (IsVisionSuppressed)
        {
            IsPlayerVisible = false;
            return;
        }

        // 吸血中だけ視線判定を無効化
        if (human != null && human.IsBeingDrained)
        {
            IsPlayerVisible = false;
            return;
        }

        if (visionLine != null)
            visionLine.enabled = true;

        // 魅了中はHumanController側の回転を優先
        if (human == null || !human.IsCharmed)
        {
            UpdateVisionDirection();
        }

        // 視線の表示とプレイヤー検知は魅了中も行う
        UpdateVisionDirection();
        CheckPlayer();
    }

    //==================================================
    // バットによる視線誘導
    //==================================================

    public void SetBatAttraction(Transform bat)
    {
        if (bat == null)
            return;

        attractedBat = bat;
    }

    public void ClearBatAttraction(Transform bat)
    {
        if (attractedBat == bat)
            attractedBat = null;
    }

    public void ClearBatAttraction()
    {
        attractedBat = null;
    }

    //==================================================
    // 視線方向
    //==================================================

    private void UpdateVisionDirection()
    {
        if (attractedBat == null)
            return;

        Vector3 direction =
            attractedBat.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    //==================================================
    // 視線表示
    //==================================================

    private void UpdateVisionLine()
    {
        if (visionLine == null)
            return;

        Vector3 origin =
            transform.position + Vector3.up * lineHeight;

        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        Vector3 endPosition =
            origin + direction * viewDistance;

        // 障害物があれば、その位置まで表示
        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            viewDistance))
        {
            endPosition = hit.point;
        }

        visionLine.SetPosition(0, origin);
        visionLine.SetPosition(1, endPosition);
    }

    //==================================================
    // プレイヤー検知
    //==================================================

    private void CheckPlayer()
    {
        IsPlayerVisible = false;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 origin = transform.position;

        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();

        Vector3 playerPosition =
            player.transform.position;

        playerPosition.y = origin.y;

        Vector3 toPlayer =
            playerPosition - origin;

        float distance = toPlayer.magnitude;

        if (distance > viewDistance || distance <= 0.01f)
            return;

        toPlayer.Normalize();

        float dot =
            Vector3.Dot(direction, toPlayer);

        if (dot < 0.98f)
            return;

        if (Physics.Raycast(
            origin,
            direction,
            out RaycastHit hit,
            viewDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                IsPlayerVisible = true;
            }
        }
    }
}