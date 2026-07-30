using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject humanPrefab;

    [Header("通常人間データ")]
    public HumanData normalHumanData;

    [Header("初期人数")]
    public int initialCount = 20;

    [Header("スポーン範囲")]
    public float spawnRadius = 20f;

    private void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            SpawnHuman();
        }
    }

    private void SpawnHuman()
    {
        Vector2 random = Random.insideUnitCircle * spawnRadius;

        Vector3 pos = transform.position +
                      new Vector3(random.x, 0f, random.y);

        GameObject obj = Instantiate(humanPrefab, pos, Quaternion.identity);

        HumanController human = obj.GetComponent<HumanController>();

        human.humanData = normalHumanData;
    }
}