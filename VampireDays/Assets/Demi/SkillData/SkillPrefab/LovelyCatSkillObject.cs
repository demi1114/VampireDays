
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ラブリーキャットの召喚スキル。
/// 召喚地点の周囲を自由に移動し、近くの人間の視線を非表示にする。
/// 範囲外に出ると視線を再表示する。
/// </summary>
public class LovelyCatSkillObject : MonoBehaviour, ISkillObject
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveRange = 5f;
    [SerializeField] private float arrivalDistance = 0.2f;

    [Header("視線非表示効果")]
    [SerializeField] private float effectRadius = 5f;

    [Header("更新設定")]
    [SerializeField] private float checkInterval = 0.1f;

    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = true;

    // スキル情報
    private RuntimeSkill runtimeSkill;

    // 移動情報
    private Vector3 startPosition;
    private Vector3 targetPosition;

    // 視線非表示を適用中の人間
    private readonly HashSet<HumanController> affectedHumans =
        new HashSet<HumanController>();

    // 複数キャットによる視線非表示を管理
    private static readonly Dictionary<HumanController, int> hideCounts =
        new Dictionary<HumanController, int>();

    // 非表示にする前の視線の状態
    private class VisionState
    {
        public LineRenderer[] lines;
        public bool[] originalEnabled;
    }

    private static readonly Dictionary<HumanController, VisionState>
        originalVisionStates =
            new Dictionary<HumanController, VisionState>();

    private float checkTimer;

    //==================================================
    // 初期化
    //==================================================

    public void Initialize(RuntimeSkill skill)
    {
        runtimeSkill = skill;

        // 召喚地点を記録し、この周囲を移動する
        startPosition = transform.position;

        // 最初の移動先を決める
        SetRandomTarget();

        // 召喚時点で範囲内にいる人間も検知する
        UpdateVisionTargets();

        if (showDebugLog)
        {
            Debug.Log("ラブリーキャット召喚！");
        }
    }

    //==================================================
    // 更新
    //==================================================

    private void Update()
    {
        // 自由に動き回る
        MoveToTarget();

        // 一定間隔で視線の効果範囲を更新
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            UpdateVisionTargets();
        }
    }

    //==================================================
    // ランダム移動
    //==================================================

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 目的地に到着したら次の目的地を決める
        if (Vector3.Distance(transform.position, targetPosition)
            <= arrivalDistance)
        {
            SetRandomTarget();
        }
    }

    private void SetRandomTarget()
    {
        Vector2 random =
            Random.insideUnitCircle * moveRange;

        targetPosition = startPosition + new Vector3(
            random.x,
            0f,
            random.y
        );
    }

    //==================================================
    // 視線の効果範囲判定
    //==================================================

    private void UpdateVisionTargets()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            effectRadius
        );

        HashSet<HumanController> currentHumans =
            new HashSet<HumanController>();

        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;

            // 子Colliderにも対応
            HumanController human =
                hit.GetComponentInParent<HumanController>();

            if (human != null)
            {
                currentHumans.Add(human);
            }
        }

        // 範囲外に出た人間を探す
        List<HumanController> removedHumans =
            new List<HumanController>();

        foreach (HumanController human in affectedHumans)
        {
            if (human == null || !currentHumans.Contains(human))
            {
                removedHumans.Add(human);
            }
        }

        // 範囲外の人間は視線を復元
        foreach (HumanController human in removedHumans)
        {
            affectedHumans.Remove(human);

            if (human != null)
            {
                ReleaseVision(human);
            }
        }

        // 新しく範囲内に入った人間に効果を適用
        foreach (HumanController human in currentHumans)
        {
            if (human == null)
                continue;

            if (affectedHumans.Add(human))
            {
                HideVision(human);
            }
        }
    }

    //==================================================
    // 視線を非表示にする
    //==================================================

    private void HideVision(HumanController human)
    {
        // ほかのキャットが既に非表示にしている場合
        if (hideCounts.TryGetValue(human, out int count))
        {
            hideCounts[human] = count + 1;
            return;
        }

        // 視線のLineRendererを取得
        LineRenderer[] lines =
            human.GetComponentsInChildren<LineRenderer>(true);

        bool[] originalEnabled = new bool[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            originalEnabled[i] = lines[i].enabled;
            lines[i].enabled = false;
        }

        originalVisionStates[human] = new VisionState
        {
            lines = lines,
            originalEnabled = originalEnabled
        };

        hideCounts[human] = 1;

        if (showDebugLog)
        {
            Debug.Log(
                $"ラブリーキャット：{human.name}の視線を非表示"
            );
        }
    }

    //==================================================
    // 視線を再表示する
    //==================================================

    private void ReleaseVision(HumanController human)
    {
        if (!hideCounts.TryGetValue(human, out int count))
            return;

        count--;

        // ほかのキャットの範囲内にいる場合は非表示を維持
        if (count > 0)
        {
            hideCounts[human] = count;
            return;
        }

        hideCounts.Remove(human);

        if (originalVisionStates.TryGetValue(
            human, out VisionState state))
        {
            for (int i = 0; i < state.lines.Length; i++)
            {
                if (state.lines[i] != null)
                {
                    state.lines[i].enabled =
                        state.originalEnabled[i];
                }
            }

            originalVisionStates.Remove(human);
        }

        if (showDebugLog)
        {
            Debug.Log(
                $"ラブリーキャット：{human.name}の視線を再表示"
            );
        }
    }

    //==================================================
    // 消滅時の後処理
    //==================================================

    private void OnDestroy()
    {
        foreach (HumanController human in affectedHumans)
        {
            if (human != null)
            {
                ReleaseVision(human);
            }
        }

        affectedHumans.Clear();
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

        // 現在位置を中心とした視線非表示範囲
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            effectRadius
        );
    }
}
