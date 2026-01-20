using UnityEngine;
using System.Collections;

public class TargetPoint : MonoBehaviour
{
    private bool isCompleted = false;
    private Collider2D pointCollider;

    void Awake()
    {
        pointCollider = GetComponent<Collider2D>();
        if (pointCollider == null)
        {
            pointCollider = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)pointCollider).radius = 0.3f;
        }
        pointCollider.isTrigger = true;
    }

    void Start()
    {
        StartCoroutine(CheckOverlapNextFrame());
    }

    IEnumerator CheckOverlapNextFrame()
    {
        yield return new WaitForFixedUpdate();
        CheckOverlapWithObstacles();
    }

    void CheckOverlapWithObstacles()
    {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (Collider2D overlap in overlaps)
        {
            if (overlap.CompareTag("StuckObj"))
            {
                StuckObj stuckObj = overlap.GetComponent<StuckObj>();
                if (stuckObj != null && stuckObj.IsStuckToTarget())
                {
                    CompletePoint();
                    return;
                }
            }
        }
    }

    void CompletePoint()
    {
        if (isCompleted) return;

        isCompleted = true;
        TargetPointManager.Instance?.OnPointCompleted(this);
        Destroy(gameObject);
    }

    public bool IsCompleted => isCompleted;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("StuckObj"))
        {
            Debug.Log(collision.transform.name);
            StuckObj stuckObj = collision.GetComponent<StuckObj>();
            if (stuckObj != null)
            {
                CompletePoint();
            }
        }
    }
}