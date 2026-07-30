using UnityEngine;

[CreateAssetMenu(fileName = "HumanData", menuName = "Game/Human Data")]
public class HumanData : ScriptableObject
{
    [Header("基本情報")]
    public string humanName;

    [Header("移動")]
    public float moveSpeed = 2f;

    [Header("血液ドロップ量")]
    public int bloodAmount = 1;

    [Header("特殊")]
    public bool isSpecialHuman = false;

    [Header("エフェクト")]
    public GameObject specialEffect;
}