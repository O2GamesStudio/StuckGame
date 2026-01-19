using UnityEngine;
using DG.Tweening;

public class GameOverVFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] float maxScale = 2f;
    [SerializeField] float scaleDuration = 0.5f;
    [SerializeField] float fadeDuration = 0.3f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;
        }
    }

    void Start()
    {
        PlayEffect();
    }

    void PlayEffect()
    {
        if (spriteRenderer == null) return;

        Sequence sequence = DOTween.Sequence();

        transform.localScale = Vector3.zero;
        sequence.Append(transform.DOScale(maxScale, scaleDuration).SetEase(Ease.OutQuad));

        sequence.Join(spriteRenderer.DOFade(1f, fadeDuration * 0.5f).SetEase(Ease.OutQuad));
        sequence.Append(spriteRenderer.DOFade(0f, fadeDuration * 0.5f).SetEase(Ease.InQuad));

        sequence.OnComplete(() => Destroy(gameObject));
    }
}