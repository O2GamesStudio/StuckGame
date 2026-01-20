using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Lean.Pool;

public class StuckObj : MonoBehaviour, IPoolable
{
    [Header("Stick Settings")]
    [SerializeField] float targetStickOffset = 0.3f;
    [SerializeField] float borderStickOffset = 0.3f;

    [Header("VFX")]
    [SerializeField] GameObject stuckVFXPrefab;

    [Header("SFX")]
    [SerializeField] AudioClip hitTargetSFX;
    [SerializeField] AudioClip hitKnifeSFX;

    [Header("Collision Fail Settings")]
    [SerializeField] float fallGravityScale = 2f;
    [SerializeField] float fallRotationSpeed = 360f;
    [SerializeField] float despawnDelay = 3f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;
    private bool isLaunched = false;
    private bool isStuckToTarget = false;
    private bool isFalling = false;
    private float cachedKnifeLength = -1f;
    private bool hasTriggeredGameOver = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void OnEnable()
    {
        if (col != null)
        {
            col.enabled = true;
        }
    }

    void IPoolable.OnSpawn()
    {
        ResetForSpawn();
    }

    void IPoolable.OnDespawn()
    {
        StopAllCoroutines();

        if (GameManager.Instance != null && col != null)
        {
            List<StuckObj> allKnives = GameManager.Instance.GetAllKnives();
            foreach (StuckObj other in allKnives)
            {
                if (other != null && other != this && other.GetCollider() != null)
                {
                    Physics2D.IgnoreCollision(col, other.GetCollider(), false);
                }
            }
        }

        transform.SetParent(null);
    }

    public void ResetForSpawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        isStuck = false;
        isLaunched = false;
        isStuckToTarget = false;
        isFalling = false;
        hasTriggeredGameOver = false;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.isKinematic = false;
        }

        if (col != null)
        {
            col.enabled = true;
        }

        transform.SetParent(null);
        transform.rotation = Quaternion.identity;
    }

    public void SetupAsObstacle()
    {
        isStuck = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    void TriggerStuckObjCollision(Vector2 contactPoint)
    {
        if (hitKnifeSFX != null)
        {
            SoundManager.Instance?.PlaySFX(hitKnifeSFX);
        }

        if (VFXManager.Instance != null)
        {
            GameObject vfxPrefab = VFXManager.Instance.GetGameOverVFX();
            if (vfxPrefab != null)
            {
                LeanPool.Spawn(vfxPrefab, contactPoint, Quaternion.identity);
            }
        }

        GameManager.Instance.OnKnifeCollision();

        if (col != null)
        {
            col.enabled = false;
        }

        StartFalling();
        IgnoreStuckObjCollisions();
    }

    public float GetKnifeLength()
    {
        if (cachedKnifeLength > 0)
        {
            return cachedKnifeLength;
        }

        if (col != null)
        {
            cachedKnifeLength = col.bounds.size.y * 0.5f;
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                cachedKnifeLength = sr.bounds.size.y * 0.5f;
            }
            else
            {
                cachedKnifeLength = 0.5f;
            }
        }

        return cachedKnifeLength;
    }

    public float GetTargetStickOffset()
    {
        return targetStickOffset;
    }

    public void Throw(float force)
    {
        if (isStuck || rb == null) return;

        rb.linearVelocity = Vector2.up * force;
    }

    void OnCollisionEnter2D(Collision2D co)
    {
        if (isLaunched)
        {
            if (co.transform.CompareTag("Border"))
            {
                StickToBorder(co);
            }
            return;
        }

        if (isStuck || isFalling || hasTriggeredGameOver) return;

        if (co.transform.CompareTag("Target"))
        {
            if (hitTargetSFX != null)
            {
                SoundManager.Instance?.PlaySFX(hitTargetSFX);
            }

            StickToTarget(co);

            TargetCtrl targetCtrl = co.transform.GetComponent<TargetCtrl>();
            if (targetCtrl != null)
            {
                targetCtrl.OnKnifeHit();
            }

            GameManager.Instance.OnKnifeStuck();
        }
        else if (co.transform.CompareTag("StuckObj"))
        {
            hasTriggeredGameOver = true;
            ContactPoint2D contact = co.GetContact(0);
            TriggerStuckObjCollision(contact.point);
        }
    }

    void StartFalling()
    {
        isFalling = true;

        rb.gravityScale = fallGravityScale;
        rb.bodyType = RigidbodyType2D.Dynamic;

        float rotationDirection = Random.value > 0.5f ? 1f : -1f;
        rb.angularVelocity = fallRotationSpeed * rotationDirection;

        rb.linearVelocity = new Vector2(0f, 0f);

        StartCoroutine(DespawnAfterDelay(despawnDelay));
    }

    IEnumerator DespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LeanPool.Despawn(gameObject);
    }

    void IgnoreStuckObjCollisions()
    {
        if (GameManager.Instance == null) return;

        List<StuckObj> allKnives = GameManager.Instance.GetAllKnives();

        foreach (StuckObj other in allKnives)
        {
            if (other != null && other != this && other.GetCollider() != null)
            {
                Physics2D.IgnoreCollision(col, other.GetCollider(), true);
            }
        }
    }

    void StickToTarget(Collision2D collision)
    {
        isStuck = true;
        isStuckToTarget = true;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 hitPoint = contact.point;

        Vector2 centerToObj = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;

        CircleCollider2D targetCollider = collision.transform.GetComponent<CircleCollider2D>();
        float targetRadius = targetCollider != null ? targetCollider.radius * collision.transform.localScale.x : 1f;

        transform.position = (Vector2)collision.transform.position + centerToObj * (targetRadius + targetStickOffset);

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(collision.transform);

        if (stuckVFXPrefab != null)
        {
            LeanPool.Spawn(stuckVFXPrefab, hitPoint, Quaternion.identity);
        }
    }

    public bool IsStuckToTarget()
    {
        return isStuckToTarget;
    }

    void StickToBorder(Collision2D collision)
    {
        isStuck = true;

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 hitPoint = contact.point;
        Vector2 hitNormal = contact.normal;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Vector2 offset = -hitNormal * borderStickOffset;
        transform.position = hitPoint + offset;
    }

    public void Launch(Vector2 direction, float force)
    {
        isLaunched = true;
        isStuck = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1;
        rb.linearVelocity = direction.normalized * force;
        rb.angularVelocity = Random.Range(-360f, 360f);
    }

    public void StickAsObstacle(Transform target)
    {
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public Collider2D GetCollider()
    {
        return col;
    }
}