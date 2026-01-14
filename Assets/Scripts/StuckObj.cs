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

        // 초기 물리 설정
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
        // 날라간 상태일 때
        if (isLaunched)
        {
            // Border에만 충돌 처리
            if (co.transform.CompareTag("Border"))
            {
                StickToBorder(co);
            }
            // 다른 충돌은 모두 무시
            return;
        }

        // 일반 게임 중 충돌 처리
        if (isStuck) return;

        if (co.transform.CompareTag("Target"))
        {
            Stick(co.transform);
            // Target에 박혔을 때 카운트 증가
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

        // 충돌 정보 가져오기
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 hitPoint = contact.point;
        Vector2 hitNormal = contact.normal;

        // 물리 완전 정지
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 충돌 지점에서 약간 박힌 위치로 조정
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

        // 일정 시간 후 제거
        Destroy(gameObject, 5f);
    }

    // 외부에서 Collider 접근용
    public Collider2D GetCollider()
    {
        return col;
    }
}