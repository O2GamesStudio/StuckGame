// StuckObj.cs - 전체 코드
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class StuckObj : MonoBehaviour
{
    [Header("Stick Settings")]
    [SerializeField] float targetStickOffset = 0.3f;
    [SerializeField] float borderStickOffset = 0.3f;

    [Header("VFX")]
    [SerializeField] GameObject stuckVFXPrefab;

    [Header("Collision Fail Settings")]
    [SerializeField] float fallGravityScale = 2f;
    [SerializeField] float fallRotationSpeed = 360f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;
    private bool isLaunched = false;
    private bool isStuckToTarget = false;
    private bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Throw(float force)
    {
        if (isStuck) return;

        rb.linearVelocity = Vector2.up * force;
    }

    void OnCollisionEnter2D(Collision2D co)
    {
        Debug.Log(co.transform.name);
        if (isLaunched)
        {
            if (co.transform.CompareTag("Border"))
            {
                StickToBorder(co);
            }
            return;
        }

        if (isStuck || isFalling) return;

        if (co.transform.CompareTag("Target"))
        {
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
            if (VFXManager.Instance != null)
            {
                GameObject vfxPrefab = VFXManager.Instance.GetGameOverVFX();
                if (vfxPrefab != null)
                {
                    ContactPoint2D contact = co.GetContact(0);
                    Instantiate(vfxPrefab, contact.point, Quaternion.identity);
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
    }

    void StartFalling()
    {
        isFalling = true;

        rb.gravityScale = fallGravityScale;
        rb.bodyType = RigidbodyType2D.Dynamic;

        float rotationDirection = Random.value > 0.5f ? 1f : -1f;
        rb.angularVelocity = fallRotationSpeed * rotationDirection;

        rb.linearVelocity = new Vector2(0f, 0f);

        Destroy(gameObject, 3f);
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

        Vector2 centerToHit = (hitPoint - (Vector2)collision.transform.position).normalized;

        float angle = Mathf.Atan2(centerToHit.y, centerToHit.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector2 offset = centerToHit * targetStickOffset;
        transform.position = hitPoint + offset;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(collision.transform);

        if (stuckVFXPrefab != null)
        {
            Instantiate(stuckVFXPrefab, hitPoint, Quaternion.identity);
        }
    }

    void Stick(Transform target)
    {
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(target);

        if (target.CompareTag("Target"))
        {
            isStuckToTarget = true;
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