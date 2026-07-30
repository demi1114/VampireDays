using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void Damage(float value)
    {
        currentHP -= value;

        if (currentHP < 0f)
            currentHP = 0f;
    }

    public void Heal(float value)
    {
        currentHP += value;

        if (currentHP > maxHP)
            currentHP = maxHP;
    }

    public bool IsDead()
    {
        return currentHP <= 0f;
    }
}