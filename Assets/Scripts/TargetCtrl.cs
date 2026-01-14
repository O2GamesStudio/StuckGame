using UnityEngine;
using DG.Tweening;

public class TargetCtrl : MonoBehaviour
{
    Animator animator;

    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] bool rotateClockwise = true;
    [SerializeField] float explosionForce = 15f;
    [SerializeField] float scaleMultiplier = 1.5f;
    [SerializeField] float scaleDuration = 0.3f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isRotating = true;
    private Vector3 originalScale;

    void Awake()
    {
        animator = GetComponent<Animator>();
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
    public void ClearStage()
    {
        StopRotation();

        // 박혀있는 칼들의 부모를 해제하여 회전 상속 방지
        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();
        foreach (StuckObj knife in stuckKnives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);

                // 칼의 물리도 정지시킴
                Rigidbody2D knifeRb = knife.GetComponent<Rigidbody2D>();
                if (knifeRb != null)
                {
                    knifeRb.bodyType = RigidbodyType2D.Kinematic;
                    knifeRb.linearVelocity = Vector2.zero;
                    knifeRb.angularVelocity = 0f;
                }
            }
        }

        animator.SetTrigger("Win");
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