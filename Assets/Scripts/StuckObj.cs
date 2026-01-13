using UnityEngine;

public class StuckObj : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;
    private bool isLaunched = false; // 날라간 상태

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

        // 위쪽으로 발사
        rb.linearVelocity = Vector2.up * force;
    }

    void OnCollisionEnter2D(Collision2D co)
    {
        // 날라간 상태에서 Border에 충돌
        if (isLaunched && co.transform.CompareTag("Border"))
        {
            Debug.Log("Stuck to Border");
            StickToBorder(co);
            return;
        }

        // 날라간 상태면 다른 충돌 무시
        if (isLaunched)
        {
            return;
        }

        if (co.transform.CompareTag("Target") && !isStuck)
        {
            Debug.Log("Stuck");
            Stick(co.transform);
        }
        else if (co.transform.CompareTag("StuckObj") && !isStuck)
        {
            Debug.Log("Hit another knife! Game Over!");
            Stick(co.transform);

            // 0.2초 딜레이 후 게임오버
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

        CancelInvoke();
    }

    public void Launch(Vector2 direction, float force)
    {
        isLaunched = true;
        isStuck = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = direction.normalized * force;

        //Destroy(gameObject, 5f);
    }
}