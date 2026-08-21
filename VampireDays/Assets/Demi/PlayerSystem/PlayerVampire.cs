using UnityEngine;

public class PlayerVampire : MonoBehaviour
{
    [Header("吸血時間")]
    [Tooltip("パッシブスキル適用前の基本吸血時間")]
    public float drainTime = 1f;

    [Header("回復量")]
    public float healAmount = 10f;


    //==================================================
    // プレイヤー
    //==================================================

    private PlayerStatus status;

    private SkillManager skillManager;

    private DrainSpeedUpSkill drainSpeedUpSkill;


    //==================================================
    // 吸血
    //==================================================

    private HumanController currentHuman;

    private float currentDrainTime;

    private float currentRequiredDrainTime;


    //==================================================
    // プロパティ
    //==================================================

    /// <summary>
    /// 現在吸血中か
    /// </summary>
    public bool IsDraining =>
        currentHuman != null;


    /// <summary>
    /// 現在吸血している人間
    /// </summary>
    public HumanController CurrentDrainTarget =>
        currentHuman;


    /// <summary>
    /// 現在の吸血に必要な時間
    /// </summary>
    public float CurrentRequiredDrainTime =>
        currentRequiredDrainTime;


    //==================================================
    // Unity
    //==================================================

    private void Awake()
    {
        status =
            GetComponent<PlayerStatus>();

        skillManager =
            GetComponent<SkillManager>();

        drainSpeedUpSkill =
            GetComponent<DrainSpeedUpSkill>();
    }


    private void Update()
    {
        if (currentHuman == null)
            return;


        currentDrainTime +=
            Time.deltaTime;


        if (currentDrainTime >=
            currentRequiredDrainTime)
        {
            FinishDrain();
        }
    }


    //==================================================
    // 人間との接触
    //==================================================

    private void OnTriggerEnter(
        Collider other)
    {
        // すでに吸血中
        if (currentHuman != null)
            return;


        // Human以外
        if (!other.CompareTag("Human"))
            return;


        HumanController human =
            other.GetComponent<HumanController>();


        if (human == null)
            return;


        // すでに別の対象から吸血されている
        if (human.IsBeingDrained)
            return;


        //==============================================
        // 吸血開始
        //==============================================

        currentHuman =
            human;

        currentDrainTime =
            0f;


        //==============================================
        // 基本吸血時間
        //==============================================

        currentRequiredDrainTime =
            drainTime;


        //==============================================
        // 吸血速度UP
        //==============================================

        if (drainSpeedUpSkill != null)
        {
            currentRequiredDrainTime =
                drainSpeedUpSkill.CalculateDrainTime(
                    drainTime
                );
        }


        //==============================================
        // 吸血開始
        //==============================================

        human.BeginDrain();


        Debug.Log(
            $"吸血開始 : " +
            $"必要時間={currentRequiredDrainTime:F2}秒"
        );
    }


    //==================================================
    // 吸血終了
    //==================================================

    private void FinishDrain()
    {
        if (currentHuman == null)
            return;


        // 回復
        if (status != null)
        {
            status.Heal(
                healAmount
            );
        }


        // 人間から血液生成
        currentHuman.FinishDrain();


        Debug.Log(
            $"吸血完了 : " +
            $"吸血時間={currentRequiredDrainTime:F2}秒"
        );


        // リセット
        currentHuman = null;

        currentDrainTime = 0f;

        currentRequiredDrainTime = 0f;
    }
}