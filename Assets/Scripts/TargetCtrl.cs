using UnityEngine;
using DG.Tweening;

public class TargetCtrl : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] bool rotateClockwise = true;
    [SerializeField] float explosionForce = 15f; // 칼 날리는 힘
    [SerializeField] float scaleMultiplier = 1.5f; // 커지는 배율
    [SerializeField] float scaleDuration = 0.3f; // 애니메이션 시간

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isRotating = true;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.angularVelocity = rotateClockwise ? -rotationSpeed : rotationSpeed;
        }
    }

    void FixedUpdate()
    {
        if (rb != null && isRotating)
        {
            float direction = rotateClockwise ? -1f : 1f;
            rb.angularVelocity = direction * rotationSpeed;
        }
    }

    public void StopRotation()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }
    }

    public void ExplodeKnives()
    {
        StopRotation();

        if (col != null)
        {
            col.enabled = false;
        }

        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();

        Sequence scaleSequence = DOTween.Sequence();

        scaleSequence.Append(transform.DOScale(originalScale * scaleMultiplier, scaleDuration)
            .SetEase(Ease.OutBack)
            .OnStart(() =>
            {
                LaunchKnives(stuckKnives);
            }))
            .Append(transform.DOScale(originalScale, scaleDuration)
            .SetEase(Ease.InBack));
    }

    void LaunchKnives(StuckObj[] knives)
    {
        foreach (StuckObj knife in knives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);

                Vector2 localDown = -knife.transform.up;

                knife.Launch(localDown, explosionForce);
            }
        }
    }
}