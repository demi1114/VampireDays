using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 夜のとばりの画面演出と視線消去を管理する
/// </summary>
public class NightCurtainScreenEffect : MonoBehaviour
{
    [Header("画面暗転")]
    [SerializeField]
    private Image screenOverlay;

    [SerializeField]
    private float fadeDuration = 0.3f;

    [Header("視線消去")]
    [Tooltip("視線を消去しておく時間（秒）")]
    [SerializeField]
    private float visionSuppressionDuration = 5f;

    [Header("カメラ")]
    [SerializeField]
    private Camera targetCamera;

    // 視線を消去した人間
    private readonly HashSet<VisionController> suppressedVisions =
        new HashSet<VisionController>();

    private Coroutine effectCoroutine;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (screenOverlay != null)
        {
            screenOverlay.color =
                new Color(0f, 0f, 0f, 0f);

            screenOverlay.raycastTarget = false;
        }
    }

    /// <summary>
    /// 夜のとばりを発動
    /// </summary>
    public void PlayEffect()
    {
        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        // 前回の視線消去を解除
        RestoreAllVisions();

        effectCoroutine =
            StartCoroutine(PlayEffectCoroutine());
    }

    private IEnumerator PlayEffectCoroutine()
    {
        if (screenOverlay == null || targetCamera == null)
        {
            Debug.LogWarning(
                "夜のとばりのImageまたはCameraが設定されていません。"
            );

            yield break;
        }

        // 画面を暗くする
        yield return FadeScreen(0f, 1f);

        // 暗転完了時に視線を消去
        SuppressVisibleHumans();

        // 画面を元の明るさに戻す
        yield return FadeScreen(1f, 0f);

        // 指定時間だけ視線を消去
        yield return new WaitForSeconds(
            visionSuppressionDuration
        );

        // 視線を復活
        RestoreAllVisions();

        effectCoroutine = null;
    }

    /// <summary>
    /// 画面の暗さを徐々に変更する
    /// </summary>
    private IEnumerator FadeScreen(
        float startAlpha,
        float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / fadeDuration);

            float alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    progress
                );

            screenOverlay.color =
                new Color(0f, 0f, 0f, alpha);

            yield return null;
        }

        screenOverlay.color =
            new Color(0f, 0f, 0f, endAlpha);
    }

    /// <summary>
    /// 画面内の人間の視線を消去
    /// </summary>
    private void SuppressVisibleHumans()
    {
        Plane[] cameraPlanes =
            GeometryUtility.CalculateFrustumPlanes(
                targetCamera
            );

        HumanController[] humans =
            FindObjectsByType<HumanController>(
                FindObjectsSortMode.None
            );

        foreach (HumanController human in humans)
        {
            if (human == null)
                continue;

            // 吸血中は対象外
            if (human.IsBeingDrained)
                continue;

            Renderer[] renderers =
                human.GetComponentsInChildren<Renderer>();

            bool isVisible = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null ||
                    !renderer.enabled)
                    continue;

                if (GeometryUtility.TestPlanesAABB(
                    cameraPlanes,
                    renderer.bounds))
                {
                    isVisible = true;
                    break;
                }
            }

            if (!isVisible)
                continue;

            VisionController vision =
                human.GetComponent<VisionController>();

            if (vision == null)
                continue;

            vision.SuppressVision();

            suppressedVisions.Add(vision);
        }
    }

    /// <summary>
    /// 消去した人間の視線をすべて復活
    /// </summary>
    private void RestoreAllVisions()
    {
        foreach (VisionController vision in suppressedVisions)
        {
            if (vision != null)
            {
                vision.RestoreVision();
            }
        }

        suppressedVisions.Clear();
    }

    private void OnDisable()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        RestoreAllVisions();

        if (screenOverlay != null)
        {
            screenOverlay.color =
                new Color(0f, 0f, 0f, 0f);
        }
    }
}