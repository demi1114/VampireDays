using UnityEngine;

public class PlayerVampire : MonoBehaviour
{
    [Header("‹zŒŒŽžŠÔ")]
    public float drainTime = 1f;

    [Header("‰ñ•œ—Ê")]
    public float healAmount = 10f;

    private PlayerStatus status;

    private HumanController currentHuman;

    private float currentDrainTime;

    public bool IsDraining => currentHuman != null;

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
        if (currentHuman != null)
            return;

        if (!other.CompareTag("Human"))
            return;

        HumanController human = other.GetComponent<HumanController>();

        if (human == null)
            return;

        if (human.IsBeingDrained)
            return;

        currentHuman = human;
        currentDrainTime = 0f;

        human.BeginDrain();
    }

    private void FinishDrain()
    {
        status.Heal(healAmount);

        currentHuman.FinishDrain();

        currentHuman = null;
        currentDrainTime = 0f;
    }
}