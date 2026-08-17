using UnityEngine;

/// <summary>
/// 人間の視線を管理する
///
/// ・視線は直線
/// ・一定距離内のみプレイヤーを検知
/// ・バットによる視線誘導に対応
/// ・現在吸血されている人間の視線はGameOver判定から除外
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

    /// <summary>
    /// 外部から視線距離を取得
    /// </summary>
    public float ViewDistance =>
        viewDistance;


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

    /// <summary>
    /// 現在視線誘導しているバット
    /// </summary>
    private Transform attractedBat;

    /// <summary>
    /// バットによる視線誘導中か
    /// </summary>
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
        human =
            GetComponent<HumanController>();

        IsPlayerVisible = false;
    }


    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        UpdateVisionDirection();

        CheckPlayer();
    }


    //==================================================
    // バットによる視線誘導
    //==================================================

    /// <summary>
    /// バットへ視線を誘導する
    /// </summary>
    public void SetBatAttraction(Transform bat)
    {
        if (bat == null)
            return;

        attractedBat = bat;
    }


    /// <summary>
    /// 指定されたバットによる視線誘導を解除
    /// </summary>
    public void ClearBatAttraction(Transform bat)
    {
        if (attractedBat == bat)
        {
            attractedBat = null;
        }
    }


    /// <summary>
    /// 視線誘導を強制解除
    /// </summary>
    public void ClearBatAttraction()
    {
        attractedBat = null;
    }


    //==================================================
    // 視線方向
    //==================================================

    /// <summary>
    /// 視線方向を更新
    ///
    /// バット誘導時のみ人間を回転させる。
    /// 吸血対象になったことによる回転は行わない。
    /// </summary>
    private void UpdateVisionDirection()
    {
        if (attractedBat == null)
            return;

        Vector3 direction =
            attractedBat.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }


    //==================================================
    // プレイヤー検知
    //==================================================

    /// <summary>
    /// プレイヤーが直線状の視線に入っているか確認する
    /// </summary>
    private void CheckPlayer()
    {
        IsPlayerVisible = false;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;


        //==============================================
        // 視線開始位置
        //==============================================

        Vector3 origin =
            transform.position;


        //==============================================
        // 視線方向
        //==============================================

        Vector3 direction =
            transform.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();


        //==============================================
        // プレイヤー位置
        //==============================================

        Vector3 playerPosition =
            player.transform.position;

        playerPosition.y =
            origin.y;


        Vector3 toPlayer =
            playerPosition - origin;

        float distance =
            toPlayer.magnitude;


        //==============================================
        // 距離チェック
        //==============================================

        if (distance > viewDistance)
            return;

        if (distance <= 0.01f)
            return;

        toPlayer.Normalize();


        //==============================================
        // 直線上にいるか
        //==============================================

        float dot =
            Vector3.Dot(
                direction,
                toPlayer
            );

        // ほぼ真正面のみ
        if (dot < 0.98f)
            return;


        //==============================================
        // 障害物チェック
        //==============================================

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