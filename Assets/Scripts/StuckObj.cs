// StuckObj.cs - 수정된 부분
using System.Collections;
using UnityEngine;

public class StuckObj : MonoBehaviour
{
    [Header("Stick Settings")]
    [SerializeField] private float targetStickOffset = 0.3f;
    [SerializeField] private float borderStickOffset = 0.3f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;
    private bool isLaunched = false;
    private bool isStuckToTarget = false;

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

        if (isStuck) return;

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
            Stick(co.transform);
            StartCoroutine(GameOverAfterDelay());
        }
    }

    IEnumerator GameOverAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        GameManager.Instance.GameOver();
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

        Destroy(gameObject, 5f);
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