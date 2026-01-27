// TargetCtrl.cs
using UnityEngine;
using DG.Tweening;

public class TargetCtrl : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] float explosionForce = 15f;
    [SerializeField] float scaleMultiplier = 1.5f;
    [SerializeField] float scaleDuration = 0.3f;

    [Header("Hit Feedback Settings")]
    [SerializeField] float hitAlphaValue = 0.3f;
    [SerializeField] float hitFadeDuration = 0.15f;
    [SerializeField] float hitPunchY = 0.2f;
    [SerializeField] float hitPunchDuration = 0.15f;

    [Header("GameOver Feedback Settings")]
    [SerializeField] float gameOverPunchY = 0.5f;
    [SerializeField] float gameOverPunchDuration = 0.3f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isRotating = true;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;

    private float minStartSpeed;
    private float maxStartSpeed;
    private float minMaxSpeed;
    private float maxMaxSpeed;
    private float currentSpeedChangeRate;
    private bool rotateClockwise;

    private float minHoldTime;
    private float maxHoldTime;
    private float reverseDeceleration;
    private float reverseWaitTime;
    private bool reverseDirection;

    private float currentSpeed;
    private float targetSpeed;
    private float currentDirection;

    private enum RotationState { Accelerating, Holding, Decelerating, Waiting }
    private RotationState rotationState = RotationState.Accelerating;
    private float maxAccelerationRatio;
    private const float MIN_ACCELERATION_RATIO = 0.3f;

    private float holdTimer = 0f;
    private float targetHoldTime = 0f;
    private float waitTimer = 0f;
    private static readonly Vector2 zeroVelocity = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void InitializeStage(ChapterData.StageSettings settings)
    {
        minStartSpeed = settings.minStartSpeed;
        maxStartSpeed = settings.maxStartSpeed;
        minMaxSpeed = settings.minMaxSpeed;
        maxMaxSpeed = settings.maxMaxSpeed;
        maxAccelerationRatio = settings.accelerationRatio;
        rotateClockwise = settings.rotateClockwise;

        minHoldTime = settings.minHoldTime;
        maxHoldTime = settings.maxHoldTime;
        reverseDeceleration = settings.reverseDeceleration;
        reverseWaitTime = settings.reverseWaitTime;
        reverseDirection = settings.reverseDirection;

        if (col != null)
        {
            col.enabled = true;
        }

        float scaleFactor = GameManager.Instance != null ? GameManager.Instance.GetScaleFactor() : 1f;
        transform.localScale = originalScale * scaleFactor;

        isRotating = true;

        InitializeRotation();
    }

    void InitializeRotation()
    {
        currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);
        targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);

        float randomRatio = Random.Range(MIN_ACCELERATION_RATIO, maxAccelerationRatio);
        currentSpeedChangeRate = targetSpeed * randomRatio;

        currentDirection = rotateClockwise ? -1f : 1f;
        if (Random.value > 0.5f)
        {
            currentDirection *= -1f;
        }

        rotationState = RotationState.Accelerating;

        if (rb != null)
        {
            rb.angularVelocity = currentDirection * currentSpeed;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || !isRotating) return;

        HandleReverseRotationState();
    }

    void HandleReverseRotationState()
    {
        switch (rotationState)
        {
            case RotationState.Accelerating:
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, currentSpeedChangeRate * Time.fixedDeltaTime);
                rb.angularVelocity = currentDirection * currentSpeed;

                if (Mathf.Abs(currentSpeed - targetSpeed) < 0.5f)
                {
                    rotationState = RotationState.Holding;
                    targetHoldTime = Random.Range(minHoldTime, maxHoldTime);
                    holdTimer = 0f;
                }
                break;

            case RotationState.Holding:
                rb.angularVelocity = currentDirection * targetSpeed;
                holdTimer += Time.fixedDeltaTime;

                if (holdTimer >= targetHoldTime)
                {
                    rotationState = RotationState.Decelerating;
                }
                break;

            case RotationState.Decelerating:
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, reverseDeceleration * Time.fixedDeltaTime);
                rb.angularVelocity = currentDirection * currentSpeed;

                if (currentSpeed <= 0.1f)
                {
                    rotationState = RotationState.Waiting;
                    currentSpeed = 0f;
                    rb.angularVelocity = 0f;
                    waitTimer = 0f;
                }
                break;

            case RotationState.Waiting:
                rb.angularVelocity = 0f;
                waitTimer += Time.fixedDeltaTime;

                if (waitTimer >= reverseWaitTime)
                {
                    if (reverseDirection && Random.value > 0.5f)
                    {
                        currentDirection *= -1f;
                    }

                    targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);
                    currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);

                    float randomRatio = Random.Range(MIN_ACCELERATION_RATIO, maxAccelerationRatio);
                    currentSpeedChangeRate = targetSpeed * randomRatio;

                    rotationState = RotationState.Accelerating;
                }
                break;
        }
    }

    public void OnKnifeHit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.DOFade(hitAlphaValue, hitFadeDuration)
                .OnComplete(() => spriteRenderer.DOFade(1f, hitFadeDuration));
        }

        float halfDuration = hitPunchDuration * 0.5f;
        float targetY = transform.position.y;

        transform.DOKill();
        transform.DOMoveY(targetY + hitPunchY, halfDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DOMoveY(targetY, halfDuration).SetEase(Ease.InQuad));
    }

    public void OnGameOverHit()
    {
        float halfDuration = gameOverPunchDuration * 0.5f;
        float targetY = transform.position.y;

        transform.DOKill();
        transform.DOMoveY(targetY + gameOverPunchY, halfDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => transform.DOMoveY(targetY, halfDuration).SetEase(Ease.InQuad));
    }

    public void TransitionToNextStage()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }

        transform.DOKill();
        spriteRenderer.DOKill();

        const float shrinkDuration = 0.15f;
        const float expandDuration = 0.2f;

        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();

        float scaleFactor = GameManager.Instance != null ? GameManager.Instance.GetScaleFactor() : 1f;
        Vector3 targetScale = originalScale * scaleFactor;

        transform.DOScale(Vector3.zero, shrinkDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                for (int i = 0; i < stuckKnives.Length; i++)
                {
                    if (stuckKnives[i] != null)
                    {
                        stuckKnives[i].transform.SetParent(null);

                        Rigidbody2D knifeRb = stuckKnives[i].GetComponent<Rigidbody2D>();
                        if (knifeRb != null)
                        {
                            knifeRb.bodyType = RigidbodyType2D.Kinematic;
                            knifeRb.linearVelocity = zeroVelocity;
                            knifeRb.angularVelocity = 0f;
                        }
                    }
                }

                transform.DOScale(targetScale, expandDuration).SetEase(Ease.OutBack);
            });
    }

    public void StopRotation()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }

        StopKnivesRotation();
    }

    void StopKnivesRotation()
    {
        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();
        for (int i = 0; i < stuckKnives.Length; i++)
        {
            if (stuckKnives[i] != null)
            {
                stuckKnives[i].transform.SetParent(null);

                Rigidbody2D knifeRb = stuckKnives[i].GetComponent<Rigidbody2D>();
                if (knifeRb != null)
                {
                    knifeRb.bodyType = RigidbodyType2D.Kinematic;
                    knifeRb.linearVelocity = zeroVelocity;
                    knifeRb.angularVelocity = 0f;
                }
            }
        }
    }

    public void ExplodeKnives()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();
        for (int i = 0; i < stuckKnives.Length; i++)
        {
            if (stuckKnives[i] != null)
            {
                stuckKnives[i].transform.SetParent(null);
            }
        }

        float scaleFactor = GameManager.Instance != null ? GameManager.Instance.GetScaleFactor() : 1f;
        Vector3 targetScale = originalScale * scaleFactor;

        transform.DOScale(targetScale * scaleMultiplier, scaleDuration)
            .SetEase(Ease.OutBack)
            .OnStart(() => LaunchKnives(stuckKnives))
            .OnComplete(() => transform.DOScale(targetScale, scaleDuration).SetEase(Ease.InBack));
    }

    public void StopRotationOnly()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }
    }

    void LaunchKnives(StuckObj[] knives)
    {
        for (int i = 0; i < knives.Length; i++)
        {
            if (knives[i] != null)
            {
                Vector2 launchDirection = (knives[i].transform.position - transform.position).normalized;
                knives[i].Launch(launchDirection, explosionForce);
            }
        }
    }
}