using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleMaskController : MonoBehaviour
{
    private Material mat;

    [SerializeField, Min(0f)] private float duration = 1.0f;
    [SerializeField] private float startRadius = 1.5f;
    [SerializeField] private float endRadius = 0.15f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int CenterID = Shader.PropertyToID("_Center");

    private Coroutine animCo;

    private void Awake()
    {
        var image = GetComponent<Image>();
        mat = new Material(image.material);
        image.material = mat;

        // 초기 상태 설정
        SetRadius(startRadius);
    }

    private void OnDestroy()
    {
        if (mat != null)
        {
            Destroy(mat);
            mat = null;
        }
    }

    // 게임오버 시 호출할 메서드
    public void ShowAndFocus(Vector2 screenPos, Action onComplete)
    {
        // 오브젝트 활성화
        gameObject.SetActive(true);

        // 초기 상태로 리셋 (화면 전체 투명)
        SetRadius(startRadius);
        SetCenterFromScreenPoint(screenPos);

        // 포커스 애니메이션 시작
        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(Animate(onComplete));
    }

    public void StartFocusAnimation(Vector2 screenPos, Action onComplete)
    {
        SetCenterFromScreenPoint(screenPos);

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(Animate(onComplete));
    }

    public void ResetMask(Vector2? screenPos = null)
    {
        if (screenPos.HasValue)
        {
            SetCenterFromScreenPoint(screenPos.Value);
        }

        if (animCo != null)
        {
            StopCoroutine(animCo);
            animCo = null;
        }

        SetRadius(startRadius);
    }

    // 오버레이 숨기기
    public void Hide()
    {
        if (animCo != null)
        {
            StopCoroutine(animCo);
            animCo = null;
        }
        gameObject.SetActive(false);
    }

    private void SetRadius(float radius)
    {
        if (mat == null) return;
        mat.SetFloat(RadiusID, radius);
    }

    public void StartRadiusAnimation(
        Vector2 screenPos,
        float fromRadius,
        float toRadius,
        float overrideDuration,
        Action onComplete = null,
        AnimationCurve overrideCurve = null)
    {
        SetCenterFromScreenPoint(screenPos);

        if (animCo != null) StopCoroutine(animCo);

        var useCurve = overrideCurve != null ? overrideCurve : curve;
        animCo = StartCoroutine(AnimateCustom(fromRadius, toRadius, overrideDuration, useCurve, onComplete));
    }

    private IEnumerator AnimateCustom(float from, float to, float dur, AnimationCurve useCurve, Action onComplete)
    {
        float d = Mathf.Max(dur, 0.0001f);

        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;

            float normalized = Mathf.Clamp01(t / d);
            float k = useCurve.Evaluate(normalized);

            mat.SetFloat(RadiusID, Mathf.LerpUnclamped(from, to, k));
            yield return null;
        }

        mat.SetFloat(RadiusID, to);
        animCo = null;
        onComplete?.Invoke();
    }

    private IEnumerator Animate(Action onComplete)
    {
        float d = Mathf.Max(duration, 0.0001f);

        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;

            float normalized = Mathf.Clamp01(t / d);
            float k = curve.Evaluate(normalized);

            mat.SetFloat(RadiusID, Mathf.LerpUnclamped(startRadius, endRadius, k));
            yield return null;
        }

        mat.SetFloat(RadiusID, endRadius);
        animCo = null;
        onComplete?.Invoke();
    }

    public void SetCenterFromScreenPoint(Vector2 screenPos)
    {
        float u = (Screen.width > 0) ? (screenPos.x / Screen.width) : 0.5f;
        float v = (Screen.height > 0) ? (screenPos.y / Screen.height) : 0.5f;
        mat.SetVector(CenterID, new Vector4(u, v, 0, 0));
    }
}