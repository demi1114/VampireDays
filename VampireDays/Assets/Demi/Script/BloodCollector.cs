using UnityEngine;

public class BloodCollector : MonoBehaviour
{
    [Header("‰ñŽû”ÍˆÍ")]
    public float collectRadius = 2f;

    private PlayerLevel level;

    private void Awake()
    {
        level = GetComponent<PlayerLevel>();
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