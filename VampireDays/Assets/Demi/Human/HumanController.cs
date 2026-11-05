using UnityEngine;

public class HumanController : MonoBehaviour
{
    [Header("データ")]
    public HumanData humanData;

    [Header("移動範囲")]
    public float moveRadius = 8f;

    [Header("目的地到達距離")]
    public float arriveDistance = 0.5f;

    [Header("血液Prefab")]
    public GameObject bloodPrefab;

    /// <summary>
    /// 現在吸血されているか
    /// </summary>
    public bool IsBeingDrained { get; private set; }

    /// <summary>
    /// 現在、魅了の瞳に制御されているか
    /// </summary>
    public bool IsCharmed => charmEyeSource != null;

    private Vector3 targetPosition;
    private Vector3 startPosition;

    // 現在、この人間を制御している魅了の瞳
    private Transform charmEyeSource;


    //==================================================
    // Unity
    //==================================================

    private void Start()
    {
        startPosition = transform.position;

        ChooseNewTarget();

        // 特殊人間ならエフェクト生成
        if (humanData != null &&
            humanData.isSpecialHuman &&
            humanData.specialEffect != null)
        {
            Instantiate(
                humanData.specialEffect,
                transform
            );
        }

        // HumanManagerへ登録
        if (HumanManager.Instance != null)
        {
            HumanManager.Instance.Register(this);
        }
    }

    private void Update()
    {
        // 魅了中ならエフェクトの方向を向く
        UpdateCharmEyeLook();

        Move();
    }

    private void OnDestroy()
    {
        if (HumanManager.Instance != null)
        {
            HumanManager.Instance.Unregister(this);
        }
    }


    //==================================================
    // 移動
    //==================================================

    private void Move()
    {
        // 吸血中・魅了中は移動しない
        if (IsBeingDrained || IsCharmed)
            return;

        Vector3 dir =
            targetPosition -
            transform.position;

        dir.y = 0f;

        if (dir.magnitude < arriveDistance)
        {
            ChooseNewTarget();
            return;
        }

        dir.Normalize();

        float speed =
            humanData != null
                ? humanData.moveSpeed
                : 2f;

        transform.position +=
            dir *
            speed *
            Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = dir;
        }
    }

    private void ChooseNewTarget()
    {
        Vector2 random =
            Random.insideUnitCircle *
            moveRadius;

        targetPosition =
            startPosition +
            new Vector3(
                random.x,
                0f,
                random.y
            );
    }


    //==================================================
    // 魅了の瞳
    //==================================================

    /// <summary>
    /// 魅了の瞳による制御を開始
    /// </summary>
    public void BeginCharmEye(Transform source)
    {
        if (source == null)
            return;

        // 吸血中の人間には適用しない
        if (IsBeingDrained)
            return;

        // すでに別の瞳に制御されている場合は変更しない
        if (charmEyeSource != null)
            return;

        charmEyeSource = source;
    }

    /// <summary>
    /// 魅了の瞳による制御を解除
    /// </summary>
    public void EndCharmEye(Transform source)
    {
        // 現在の制御元と解除元が一致した場合だけ解除
        if (charmEyeSource != source)
            return;

        charmEyeSource = null;
    }

    /// <summary>
    /// 魅了の瞳が存在する間、その方向を向く
    /// </summary>
    private void UpdateCharmEyeLook()
    {
        if (charmEyeSource == null)
            return;

        Vector3 direction =
            charmEyeSource.position -
            transform.position;

        // 水平方向だけを向く
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }


    //==================================================
    // 吸血
    //==================================================

    public void BeginDrain()
    {
        IsBeingDrained = true;

        // 吸血開始時に魅了を解除
        charmEyeSource = null;
    }

    /// <summary>
    /// 吸血終了
    /// 血液を生成して人間を削除する
    /// </summary>
    public void FinishDrain()
    {
        SpawnBlood();

        Destroy(gameObject);
    }

    /// <summary>
    /// 人間から血液を生成する
    /// </summary>
    private void SpawnBlood()
    {
        if (bloodPrefab == null)
            return;

        // 基本血液量
        int baseAmount =
            humanData != null
                ? humanData.bloodAmount
                : 1;

        // Player取得
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        // 最終血液量
        int finalAmount = baseAmount;

        if (player != null)
        {
            DropUpSkill dropUpSkill =
                player.GetComponent<DropUpSkill>();

            if (dropUpSkill != null)
            {
                finalAmount =
                    dropUpSkill.CalculateDropAmount(
                        baseAmount
                    );
            }
        }

        // 血液生成
        for (int i = 0; i < finalAmount; i++)
        {
            Vector3 offset =
                Random.insideUnitSphere * 0.3f;

            offset.y = 0f;

            Instantiate(
                bloodPrefab,
                transform.position + offset,
                Quaternion.identity
            );
        }

        Debug.Log(
            $"血液ドロップ : " +
            $"基本={baseAmount} → " +
            $"最終={finalAmount}"
        );
    }


    //==================================================
    // ドロップ量計算
    //==================================================

    private int CalculateDropAmount(int baseAmount)
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return baseAmount;

        DropUpSkill dropUpSkill =
            player.GetComponent<DropUpSkill>();

        if (dropUpSkill == null)
            return baseAmount;

        return dropUpSkill.CalculateDropAmount(
            baseAmount
        );
    }


    //==================================================
    // バット
    //==================================================

    public void LookAtBat(Transform bat)
    {
        if (bat == null)
            return;

        // 吸血中・魅了中は視線を変更しない
        if (IsBeingDrained || IsCharmed)
            return;

        Vector3 direction =
            bat.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }
}