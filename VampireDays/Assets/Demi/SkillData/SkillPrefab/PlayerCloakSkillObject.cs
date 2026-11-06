using System.Collections;
using UnityEngine;

public class PlayerCloakSkillObject : MonoBehaviour, ISkillObject
{
    [Header("隠れマント設定")]
    [SerializeField]
    private float effectDuration = 5f;

    [Range(0.05f, 1f)]
    [SerializeField]
    private float transparentAlpha = 0.35f;

    public static bool IsPlayerHidden { get; private set; }

    private Renderer[] playerRenderers;
    private Material[] playerMaterials;
    private Color[] originalColors;

    private bool isInitialized;
    private bool isEffectActive;

    public void Initialize(RuntimeSkill skill)
    {
        if (isInitialized)
            return;

        isInitialized = true;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Playerが見つかりません。");
            Destroy(gameObject);
            return;
        }

        playerRenderers =
            player.GetComponentsInChildren<Renderer>();

        System.Collections.Generic.List<Material> materials =
            new System.Collections.Generic.List<Material>();

        foreach (Renderer renderer in playerRenderers)
        {
            materials.AddRange(renderer.materials);
        }

        playerMaterials = materials.ToArray();
        originalColors = new Color[playerMaterials.Length];

        for (int i = 0; i < playerMaterials.Length; i++)
        {
            Material material = playerMaterials[i];

            if (material.HasProperty("_BaseColor"))
                originalColors[i] = material.GetColor("_BaseColor");
            else if (material.HasProperty("_Color"))
                originalColors[i] = material.GetColor("_Color");
        }

        // 半透明化
        for (int i = 0; i < playerMaterials.Length; i++)
        {
            Material material = playerMaterials[i];

            Color color = originalColors[i];
            color.a = transparentAlpha;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }

        isEffectActive = true;
        IsPlayerHidden = true;

        StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        yield return new WaitForSeconds(effectDuration);

        EndEffect();
        Destroy(gameObject);
    }

    private void EndEffect()
    {
        if (!isEffectActive)
            return;

        isEffectActive = false;

        // 元の透明度に戻す
        for (int i = 0; i < playerMaterials.Length; i++)
        {
            Material material = playerMaterials[i];

            if (material == null)
                continue;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", originalColors[i]);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", originalColors[i]);
        }

        IsPlayerHidden = false;
    }

    private void OnDisable()
    {
        EndEffect();
    }

    private void OnDestroy()
    {
        EndEffect();
    }
}