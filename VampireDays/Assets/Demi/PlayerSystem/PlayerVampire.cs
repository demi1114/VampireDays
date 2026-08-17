using UnityEngine;

public class PlayerVampire : MonoBehaviour
{
    [Header("‹zŒŒŠÔ")]
    public float drainTime = 1f;

    [Header("‰ñ•œ—Ê")]
    public float healAmount = 10f;

    private PlayerStatus status;

    private HumanController currentHuman;

    private float currentDrainTime;

    /// <summary>
    /// Œ»İ‹zŒŒ’†‚©
    /// </summary>
    public bool IsDraining =>
        currentHuman != null;

    /// <summary>
    /// Œ»İ‹zŒŒ‚µ‚Ä‚¢‚élŠÔ
    /// </summary>
    public HumanController CurrentDrainTarget =>
        currentHuman;


    private void Awake()
    {
        status = GetComponent<PlayerStatus>();
    }


    private void Update()
    {
        if (currentHuman == null)
            return;

        currentDrainTime += Time.deltaTime;

        if (currentDrainTime >= drainTime)
        {
            FinishDrain();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // ‚·‚Å‚É‹zŒŒ’†
        if (currentHuman != null)
            return;

        // HumanˆÈŠO
        if (!other.CompareTag("Human"))
            return;

        HumanController human =
            other.GetComponent<HumanController>();

        if (human == null)
            return;

        // ‚·‚Å‚É•Ê‚ÌƒvƒŒƒCƒ„[“™‚©‚ç‹zŒŒ‚³‚ê‚Ä‚¢‚é
        if (human.IsBeingDrained)
            return;

        // ‹zŒŒŠJn
        currentHuman = human;
        currentDrainTime = 0f;

        human.BeginDrain();
    }


    private void FinishDrain()
    {
        if (currentHuman == null)
            return;

        status.Heal(healAmount);

        currentHuman.FinishDrain();

        currentHuman = null;
        currentDrainTime = 0f;
    }
}