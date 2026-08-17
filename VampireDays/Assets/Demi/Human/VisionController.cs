using UnityEngine;

/// <summary>
/// 人間の視線を管理する
/// </summary>
public class VisionController : MonoBehaviour
{
    [Header("視線設定")]
    [SerializeField]
    public float viewDistance = 8f;

    [Header("視線表示")]
    [SerializeField]
    private LineRenderer visionLine;

    /// <summary>
    /// プレイヤーが現在視線に入っているか
    /// </summary>
    public bool IsPlayerVisible { get; private set; }

    /// <summary>
    /// 現在視線誘導しているバット
    /// </summary>
    private Transform attractedBat;

    /// <summary>
    /// 視線誘導中か
    /// </summary>
    public bool IsAttractedToBat =>
        attractedBat != null;


    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        UpdateVisionDirection();

        CheckPlayer();

        UpdateVisionLine();
    }


    //==================================================
    // 視線方向
    //==================================================

    /// <summary>
    /// 現在の視線方向を更新
    /// </summary>
    private void UpdateVisionDirection()
    {
        if (attractedBat != null)
        {
            Vector3 direction =
                attractedBat.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }
    }


    //==================================================
    // バット誘導
    //==================================================

    /// <summary>
    /// バットへ視線を誘導する
    /// </summary>
    public void SetBatAttraction(Transform bat)
    {
        attractedBat = bat;
    }


    /// <summary>
    /// バットによる視線誘導を解除する
    /// </summary>
    public void ClearBatAttraction(Transform bat)
    {
        // 現在誘導しているバットと同じ場合のみ解除
        if (attractedBat == bat)
        {
            attractedBat = null;
        }
    }


    //==================================================
    // プレイヤー検知
    //==================================================

    /// <summary>
    /// プレイヤーが直線状の視線に入っているか確認
    /// </summary>
    private void CheckPlayer()
    {
        IsPlayerVisible = false;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        Vector3 origin =
            transform.position;

        Vector3 direction =
            transform.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        direction.Normalize();

        Vector3 playerPosition =
            player.transform.position;

        playerPosition.y = origin.y;

        Vector3 toPlayer =
            playerPosition - origin;

        float distance =
            toPlayer.magnitude;

        // 視線距離外
        if (distance > viewDistance)
            return;

        toPlayer.Normalize();

        // 直線上にいるか
        float dot =
            Vector3.Dot(direction, toPlayer);

        // ほぼ真正面にいる場合のみ検知
        if (dot < 0.98f)
            return;

        // 障害物がないか確認
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


    //==================================================
    // 視線表示
    //==================================================

    private void UpdateVisionLine()
    {
        if (visionLine == null)
            return;

        Vector3 start =
            transform.position;

        Vector3 end =
            start +
            transform.forward *
            viewDistance;

        visionLine.SetPosition(0, start);
        visionLine.SetPosition(1, end);
    }
}