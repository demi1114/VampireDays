using UnityEngine;

public class BloodCollector : MonoBehaviour
{
    [Header("âÒé˚îÕàÕ")]
    public float collectRadius = 2f;

    private PlayerLevel level;

    private void Awake()
    {
        level = GetComponent<PlayerLevel>();

        if (level == null)
        {
            Debug.LogError(
                "BloodCollector : ìØÇ∂GameObjectÇ…PlayerLevelÇ™Ç†ÇËÇ‹ÇπÇÒÅB"
            );
        }
    }
    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, collectRadius);

        foreach (Collider hit in hits)
        {
            BloodItem blood = hit.GetComponent<BloodItem>();

            if (blood == null)
                continue;

            blood.AttractTo(transform, level);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}