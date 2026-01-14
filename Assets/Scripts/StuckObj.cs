using UnityEngine;

public class StuckObj : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;
    private bool isLaunched = false;

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
            Stick(co.transform);
            GameManager.Instance.OnKnifeStuck();
        }
        else if (co.transform.CompareTag("StuckObj"))
        {
            Stick(co.transform);
            StartCoroutine(GameOverAfterDelay());
        }
    }

    System.Collections.IEnumerator GameOverAfterDelay()
    {
        yield return new WaitForSeconds(0.2f);
        GameManager.Instance.GameOver();
    }

    void Stick(Transform target)
    {
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(target);
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

        Vector2 offset = -hitNormal * 0.3f;
        transform.position = hitPoint + offset;
    }

    public void Launch(Vector2 direction, float force)
    {
        isLaunched = true;
        isStuck = false;

        // 물리 활성화
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1;
        rb.linearVelocity = direction.normalized * force;
        rb.angularVelocity = Random.Range(-360f, 360f);

        Destroy(gameObject, 5f);
    }

    public Collider2D GetCollider()
    {
        return col;
    }
}