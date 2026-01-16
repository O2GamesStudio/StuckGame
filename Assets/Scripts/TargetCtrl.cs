using UnityEngine;
using DG.Tweening;

public class TargetCtrl : MonoBehaviour
{
    Animator animator;

    [Header("Explosion Settings")]
    [SerializeField] float explosionForce = 15f;
    [SerializeField] float scaleMultiplier = 1.5f;
    [SerializeField] float scaleDuration = 0.3f;

    [Header("Hit Feedback Settings")]
    [SerializeField] float hitAlphaValue = 0.3f;
    [SerializeField] float hitFadeDuration = 0.15f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isRotating = true;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;

    private float minStartSpeed;
    private float maxStartSpeed;
    private float minMaxSpeed;
    private float maxMaxSpeed;
    private float minSpeedChangeRate;
    private float maxSpeedChangeRate;
    private float accelerationRatio;
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

    private float holdTimer = 0f;
    private float targetHoldTime = 0f;
    private float waitTimer = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
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
        // ScriptableObject에서 설정값 받아오기
        minStartSpeed = settings.minStartSpeed;
        maxStartSpeed = settings.maxStartSpeed;
        minMaxSpeed = settings.minMaxSpeed;
        maxMaxSpeed = settings.maxMaxSpeed;
        accelerationRatio = settings.accelerationRatio;
        rotateClockwise = settings.rotateClockwise;

        minHoldTime = settings.minHoldTime;
        maxHoldTime = settings.maxHoldTime;
        reverseDeceleration = settings.reverseDeceleration;
        reverseWaitTime = settings.reverseWaitTime;
        reverseDirection = settings.reverseDirection;

        // 회전 활성화
        isRotating = true;

        InitializeRotation();
    }

    void InitializeRotation()
    {
        currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);
        targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);

        // 목표 속도에 비례한 가속도 계산
        currentSpeedChangeRate = targetSpeed * accelerationRatio;

        bool randomDirection = Random.value > 0.5f;
        currentDirection = rotateClockwise ? -1f : 1f;

        if (randomDirection)
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
                    if (reverseDirection)
                    {
                        currentDirection *= -1f;
                    }

                    targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);
                    currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);

                    // 새로운 목표 속도에 비례한 가속도 계산
                    currentSpeedChangeRate = targetSpeed * accelerationRatio;

                    rotationState = RotationState.Accelerating;

                    Debug.Log($"Direction changed - New speed: {currentSpeed} -> {targetSpeed}, New acceleration: {currentSpeedChangeRate} ({accelerationRatio * 100}% of target)");
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
                .OnComplete(() =>
                {
                    spriteRenderer.DOFade(1f, hitFadeDuration);
                });
        }
    }

    public void ClearStage()
    {
        StopRotation();

        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();
        foreach (StuckObj knife in stuckKnives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);

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

        StopKnivesRotation();
    }

    void StopKnivesRotation()
    {
        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();
        foreach (StuckObj knife in stuckKnives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);

                Rigidbody2D knifeRb = knife.GetComponent<Rigidbody2D>();
                if (knifeRb != null)
                {
                    knifeRb.bodyType = RigidbodyType2D.Kinematic;
                    knifeRb.linearVelocity = Vector2.zero;
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

        // 부모 해제 전에 칼들 가져오기
        StuckObj[] stuckKnives = GetComponentsInChildren<StuckObj>();

        // 칼들의 부모를 먼저 해제
        foreach (StuckObj knife in stuckKnives)
        {
            if (knife != null)
            {
                knife.transform.SetParent(null);
            }
        }

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
                Vector2 launchDirection = (knife.transform.position - transform.position).normalized;
                knife.Launch(launchDirection, explosionForce);
            }
        }
    }
}