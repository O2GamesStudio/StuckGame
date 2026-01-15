using UnityEngine;
using DG.Tweening;

public class TargetCtrl : MonoBehaviour
{
    Animator animator;

    [Header("Rotation Settings")]
    [SerializeField] float minStartSpeed = 30f;
    [SerializeField] float maxStartSpeed = 80f;
    [SerializeField] float minMaxSpeed = 100f;
    [SerializeField] float maxMaxSpeed = 200f;
    [SerializeField] float speedChangeRate = 20f; // 관성값 (속도 변화율)
    [SerializeField] bool rotateClockwise = true;

    [Header("Reverse Rotation Settings")]
    [SerializeField] bool enableReverseRotation = false;
    [SerializeField] float minHoldTime = 1f; // 최소 유지 시간
    [SerializeField] float maxHoldTime = 3f; // 최대 유지 시간
    [SerializeField] float reverseDeceleration = 60f; // 감속 속도
    [SerializeField] float reverseWaitTime = 0.3f; // 정지 후 대기 시간
    [SerializeField] bool reverseDirection = true; // true: 반대 방향, false: 원래 방향

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

    private float currentSpeed;
    private float targetSpeed;
    private float currentDirection; // 1 또는 -1

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

        InitializeRotation();
    }

    void InitializeRotation()
    {
        // 랜덤 시작 속도 설정
        currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);

        // 랜덤 목표 속도 설정
        targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);

        // 초기 방향 설정
        currentDirection = rotateClockwise ? -1f : 1f;

        // 초기 상태는 가속
        rotationState = RotationState.Accelerating;

        if (rb != null)
        {
            rb.angularVelocity = currentDirection * currentSpeed;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || !isRotating) return;

        if (enableReverseRotation)
        {
            HandleReverseRotationState();
        }
        else
        {
            HandleNormalRotation();
        }
    }

    void HandleNormalRotation()
    {
        // 목표 속도로 서서히 변경 (관성 적용)
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.fixedDeltaTime);
        rb.angularVelocity = currentDirection * currentSpeed;
    }

    void HandleReverseRotationState()
    {
        switch (rotationState)
        {
            case RotationState.Accelerating:
                // 목표 속도까지 가속
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.fixedDeltaTime);
                rb.angularVelocity = currentDirection * currentSpeed;

                // 목표 속도에 도달하면 유지 상태로 전환
                if (Mathf.Abs(currentSpeed - targetSpeed) < 0.5f)
                {
                    rotationState = RotationState.Holding;
                    targetHoldTime = Random.Range(minHoldTime, maxHoldTime);
                    holdTimer = 0f;
                }
                break;

            case RotationState.Holding:
                // 목표 속도 유지
                rb.angularVelocity = currentDirection * targetSpeed;
                holdTimer += Time.fixedDeltaTime;

                // 유지 시간이 지나면 감속 시작
                if (holdTimer >= targetHoldTime)
                {
                    rotationState = RotationState.Decelerating;
                }
                break;

            case RotationState.Decelerating:
                // 감속
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, reverseDeceleration * Time.fixedDeltaTime);
                rb.angularVelocity = currentDirection * currentSpeed;

                // 완전히 멈추면 대기 상태로 전환
                if (currentSpeed <= 0.1f)
                {
                    rotationState = RotationState.Waiting;
                    currentSpeed = 0f;
                    rb.angularVelocity = 0f;
                    waitTimer = 0f;
                }
                break;

            case RotationState.Waiting:
                // 정지 상태 유지
                rb.angularVelocity = 0f;
                waitTimer += Time.fixedDeltaTime;

                // 대기 시간이 지나면 다시 가속 시작
                if (waitTimer >= reverseWaitTime)
                {
                    // 방향 결정
                    if (reverseDirection)
                    {
                        currentDirection *= -1f; // 반대 방향
                    }
                    // reverseDirection이 false면 원래 방향 유지

                    // 새로운 목표 속도와 시작 속도 설정
                    targetSpeed = Random.Range(minMaxSpeed, maxMaxSpeed);
                    currentSpeed = Random.Range(minStartSpeed, maxStartSpeed);

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