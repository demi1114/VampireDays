using UnityEngine;

public class BloodItem : MonoBehaviour
{
    [Header("æ“¾ŒŒ‰t—Ê")]
    public int bloodAmount = 1;

    [Header("•‚—Vİ’è")]
    public float rotateSpeed = 90f;
    public float floatHeight = 0.2f;
    public float floatSpeed = 2f;

    [Header("‹z‚¢Šñ‚¹‘¬“x")]
    public float attractSpeed = 8f;

    private Vector3 startPos;
    private bool collected = false;
    private bool attracting = false;
    private Transform target;
    private PlayerLevel targetLevel;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (collected)
            return;

        if (attracting && target != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                attractSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) <= 0.3f)
            {
                Collect();
            }
            return;
        }

        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = pos;
    }

    public void AttractTo(Transform player, PlayerLevel level)
    {
        if (collected)
            return;

        attracting = true;
        target = player;
        targetLevel = level;
    }

    private void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (targetLevel != null)
        {
            targetLevel.AddBlood(bloodAmount);
        }

        Destroy(gameObject);
    }
}