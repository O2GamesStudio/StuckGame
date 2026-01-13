using UnityEngine;

public class StuckObj : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    private bool isStuck = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // 초기 설정
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // StuckObj 태그 설정 (프리팹에도 설정 필요)
        gameObject.tag = "StuckObj";
    }

    public void Throw(float force)
    {
        if (isStuck) return;

        // 위쪽으로 발사
        rb.linearVelocity = Vector2.up * force;
    }

    void OnCollisionEnter2D(Collision2D co)
    {
        if (co.transform.CompareTag("Target") && !isStuck)
        {
            Debug.Log("Stuck");
            Stick(co.transform);
        }
        else if (co.transform.CompareTag("StuckObj") && !isStuck)
        {
            Debug.Log("Hit another knife! Game Over!");
            Stick(co.transform);
            GameManager.Instance.GameOver();
        }
    }

    void Stick(Transform target)
    {
        isStuck = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        transform.SetParent(target);
    }
}