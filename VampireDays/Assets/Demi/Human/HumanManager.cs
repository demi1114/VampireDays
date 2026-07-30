using System.Collections.Generic;
using UnityEngine;

public class HumanManager : MonoBehaviour
{
    public static HumanManager Instance { get; private set; }

    private readonly List<HumanController> humans = new();

    public IReadOnlyList<HumanController> Humans => humans;

    public int HumanCount => humans.Count;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Register(HumanController human)
    {
        if (!humans.Contains(human))
            humans.Add(human);
    }

    public void Unregister(HumanController human)
    {
        humans.Remove(human);
    }
}