
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VisionVisualizer : MonoBehaviour
{
    [SerializeField]
    private VisionController vision;

    private LineRenderer line;
    private HumanController human;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        if (vision == null)
        {
            vision = GetComponent<VisionController>();
        }

        human = GetComponent<HumanController>();

        line.positionCount = 2;
        line.useWorldSpace = true;
        line.loop = false;
        line.widthMultiplier = 0.08f;
    }

    private void Update()
    {
        if (vision == null)
            return;

        // –£—¹’†E‹zŒŒ’†‚Í‹ü‚ğ”ñ•\¦‚É‚·‚é
        if (human != null &&
            (human.IsCharmed || human.IsBeingDrained))
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;

        Vector3 origin =
            transform.position + Vector3.up * 0.05f;

        Vector3 end =
            origin +
            transform.forward *
            vision.viewDistance;

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
    }
}
