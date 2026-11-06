using System.Collections.Generic;
using UnityEngine;

public class CurseScytheSkillObject : MonoBehaviour, ISkillObject
{
    [Header("カースサイス設定")]
    [Tooltip("Playerの周囲を回る半径")]
    [SerializeField]
    private float orbitRadius = 2f;

    [Tooltip("回転速度（度/秒）")]
    [SerializeField]
    private float orbitSpeed = 180f;

    // このエフェクトが視線を無効化した人間
    private readonly HashSet<VisionController> affectedHumans = new();

    private Transform playerTransform;

    private float currentAngle;
    private float heightOffset;

    private bool isInitialized;

    public void Initialize(RuntimeSkill skill)
    {
        if (isInitialized)
            return;

        isInitialized = true;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Playerが見つかりません。");
            Destroy(gameObject);
            return;
        }

        playerTransform = player.transform;

        // 生成された位置から初期角度を決める
        Vector3 offset = transform.position - playerTransform.position;

        heightOffset = offset.y;

        Vector3 horizontalOffset =
            new Vector3(offset.x, 0f, offset.z);

        if (horizontalOffset.sqrMagnitude > 0.01f)
        {
            currentAngle =
                Mathf.Atan2(horizontalOffset.z, horizontalOffset.x)
                * Mathf.Rad2Deg;
        }
        else
        {
            currentAngle = Random.Range(0f, 360f);
        }

        // 最初の位置も円周上に合わせる
        UpdateOrbitPosition();
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        // 時計回りに回転
        currentAngle -= orbitSpeed * Time.deltaTime;

        if (currentAngle < 0f)
            currentAngle += 360f;

        UpdateOrbitPosition();
    }

    //==================================================
    // 公転処理
    //==================================================

    private void UpdateOrbitPosition()
    {
        float radian = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(radian) * orbitRadius,
            heightOffset,
            Mathf.Sin(radian) * orbitRadius
        );

        transform.position = playerTransform.position + offset;
    }

    //==================================================
    // 人間との接触判定
    //==================================================

    private void OnTriggerEnter(Collider other)
    {
        HumanController human =
            other.GetComponentInParent<HumanController>();

        if (human == null)
            return;

        VisionController vision =
            human.GetComponent<VisionController>();

        if (vision == null)
            return;

        if (affectedHumans.Contains(vision))
            return;

        affectedHumans.Add(vision);

        // 視線を無効化
        vision.SuppressVision();
    }

    //==================================================
    // エフェクト終了時の処理
    //==================================================

    private void OnDisable()
    {
        RestoreAllVision();
    }

    private void OnDestroy()
    {
        RestoreAllVision();
    }

    private void RestoreAllVision()
    {
        foreach (VisionController vision in affectedHumans)
        {
            if (vision == null)
                continue;

            vision.RestoreVision();
        }

        affectedHumans.Clear();
    }
}