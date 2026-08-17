using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バットスキルによって生成されるバット
/// </summary>
public class BatSkillObject : MonoBehaviour, ISkillObject
{
    [Header("移動")]
    [SerializeField]
    private float moveSpeed = 2f;

    [Header("生成地点からの移動範囲")]
    [SerializeField]
    private float moveRange = 5f;

    [Header("到着判定距離")]
    [SerializeField]
    private float arrivalDistance = 0.2f;

    [Header("視線誘導")]
    [SerializeField]
    private float attractionRange = 6f;

    private RuntimeSkill runtimeSkill;

    /// 生成地点
    private Vector3 startPosition;

    /// 現在向かっている目的地
    private Vector3 targetPosition;

    /// 現在影響を与えている人間
    private readonly HashSet<VisionController> attractedHumans = new();

    //==================================================
    // 初期化
    //==================================================

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;

        startPosition = transform.position;

        SetRandomTarget();
    }

    private void Update()
    {
        MoveToTarget();

        AttractHumanVision();
    }

    //==================================================
    // ランダム移動
    //==================================================

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        if (Vector3.Distance(
            transform.position,
            targetPosition) <= arrivalDistance)
        {
            SetRandomTarget();
        }
    }

    private void SetRandomTarget()
    {
        Vector2 random =
            Random.insideUnitCircle * moveRange;

        targetPosition =
            startPosition +
            new Vector3(random.x, 0f, random.y);
    }

    //==================================================
    // 視線誘導
    //==================================================

    private void AttractHumanVision()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attractionRange);

        HashSet<VisionController> current = new();

        foreach (Collider hit in hits)
        {
            VisionController vision =
                hit.GetComponent<VisionController>();

            if (vision == null)
                continue;

            current.Add(vision);

            vision.SetBatAttraction(transform);
        }

        // 範囲外へ出た人間は解除
        foreach (VisionController vision in attractedHumans)
        {
            if (vision == null)
                continue;

            if (!current.Contains(vision))
            {
                vision.ClearBatAttraction(transform);
            }
        }

        attractedHumans.Clear();

        foreach (VisionController vision in current)
        {
            attractedHumans.Add(vision);
        }
    }

    private void OnDestroy()
    {
        foreach (VisionController vision in attractedHumans)
        {
            if (vision == null)
                continue;

            vision.ClearBatAttraction(transform);
        }

        attractedHumans.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            Application.isPlaying
                ? startPosition
                : transform.position,
            moveRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attractionRange);
    }
}