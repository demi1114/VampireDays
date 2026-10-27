
using UnityEngine;

/// <summary>
/// 魔法エフェクトの再生終了を監視する。
/// SkillManagerから生成された魔法Prefabに付与される。
///
/// 召喚オブジェクトの寿命とは別に管理し、
/// Particle Systemの再生と残存パーティクルが
/// すべて終了したタイミングでオブジェクトを破棄する。
/// </summary>
public class MagicEffectLifetime : MonoBehaviour
{
    [Header("Particle Systemがない場合の寿命")]
    [Tooltip("Particle Systemが見つからない場合の予備寿命。0以下なら自動破棄しない")]
    [Min(0f)]
    [SerializeField] private float fallbackLifetime = 3f;

    private ParticleSystem[] particleSystems;

    private float elapsedTime;
    private bool hasPlayed;
    private bool initialized;
    private bool warnedNoParticles;

    /// <summary>
    /// SkillManagerから生成された直後に呼び出す。
    /// </summary>
    public void Initialize()
    {
        particleSystems =
            GetComponentsInChildren<ParticleSystem>(true);

        elapsedTime = 0f;
        hasPlayed = false;
        initialized = true;

        if (particleSystems.Length == 0 && !warnedNoParticles)
        {
            warnedNoParticles = true;

            Debug.LogWarning(
                $"{name}: Particle Systemがありません。" +
                "fallbackLifetimeを使用します。",
                this
            );
        }
    }

    private void Update()
    {
        if (!initialized)
            Initialize();

        elapsedTime += Time.deltaTime;

        // Particle Systemがない場合は予備寿命を使う。
        if (particleSystems == null || particleSystems.Length == 0)
        {
            if (fallbackLifetime > 0f &&
                elapsedTime >= fallbackLifetime)
            {
                Destroy(gameObject);
            }

            return;
        }

        bool anyAlive = false;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps == null)
                continue;

            // 再生中、またはパーティクルが残っていれば生存扱い。
            if (ps.isPlaying || ps.IsAlive(true))
            {
                anyAlive = true;
                hasPlayed = true;
            }
        }

        // 再生が一度も始まっていない状態では破棄しない。
        if (!hasPlayed)
            return;

        // すべてのParticle Systemが終了した。
        if (!anyAlive)
        {
            // 念のため、残っているColliderを無効化する。
            Collider[] colliders =
                GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col != null)
                    col.enabled = false;
            }

            Destroy(gameObject);
        }
    }
}
