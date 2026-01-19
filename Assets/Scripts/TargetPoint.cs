using UnityEngine;
using Lean.Pool;

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

    void OnSpawn()
    {
        isCompleted = false;
        if (pointCollider != null)
        {
            pointCollider.enabled = true;
        }
    }

    public bool IsCompleted => isCompleted;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("StuckObj"))
        {
            StuckObj stuckObj = collision.GetComponent<StuckObj>();
            if (stuckObj != null && stuckObj.IsStuckToTarget())
            {
                return;
            }

            isCompleted = true;
            TargetPointManager.Instance?.OnPointCompleted(this);

            LeanPool.Despawn(gameObject);
        }
    }
}