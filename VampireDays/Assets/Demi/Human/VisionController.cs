
using UnityEngine;

/// <summary>
/// 人間の視線を管理する
///
/// ・視線は直線
/// ・一定距離内のみプレイヤーを検知
/// ・バットによる視線誘導に対応
/// ・現在吸血されている人間はGameOver判定から除外
/// ・魅了中は視線を無効化
/// </summary>
public class VisionController : MonoBehaviour
{
    //==================================================
    // 視線設定
    //==================================================

    [Header("視線設定")]
    [Tooltip("視線が届く最大距離")]
    [SerializeField]
    public float viewDistance = 8f;

    public float ViewDistance => viewDistance;


    //==================================================
    // プレイヤー検知
    //==================================================

    /// <summary>
    /// プレイヤーが現在視線に入っているか
    /// </summary>
    public bool IsPlayerVisible { get; private set; }


    //==================================================
    // バット誘導
    //==================================================

    private Transform attractedBat;

    public bool IsAttractedToBat =>
        attractedBat != null;


    //==================================================
    // HumanController
    //==================================================

    private HumanController human;


    //==================================================
    // 初期化
    //==================================================

    private void Awake()
    {
        human = GetComponent<HumanController>();

        IsPlayerVisible = false;
    }


    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        // 魅了中は視線を無効化する
        if (IsCharmedOrBeingDrained())
        {
            IsPlayerVisible = false;
            return;
        }

        UpdateVisionDirection();
        CheckPlayer();
    }


    /// <summary>
    /// 魅了中または吸血中か
    /// </summary>
    private bool IsCharmedOrBeingDrained()
    {
        if (human == null)
            return false;

        return human.IsCharmed || human.IsBeingDrained;
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
        {
            attractedBat = null;
        }
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
    // プレイヤー検知
    //==================================================

    private void CheckPlayer()
    {
        IsPlayerVisible = false;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        // 視線開始位置
        Vector3 origin = transform.position;

        // 視線方向
        Vector3 direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();

        // プレイヤー位置
        Vector3 playerPosition =
            player.transform.position;

        playerPosition.y = origin.y;

        Vector3 toPlayer = playerPosition - origin;

        float distance = toPlayer.magnitude;

        // 距離チェック
        if (distance > viewDistance || distance <= 0.01f)
            return;

        toPlayer.Normalize();

        // 直線上にいるか
        float dot = Vector3.Dot(direction, toPlayer);

        if (dot < 0.98f)
            return;

        // 障害物チェック
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
