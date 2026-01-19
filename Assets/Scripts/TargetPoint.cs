using UnityEngine;

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

    public bool IsCompleted => isCompleted;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCompleted) return;

        if (collision.CompareTag("StuckObj"))
        {
            isCompleted = true;
            TargetPointManager.Instance?.OnPointCompleted(this);
            Debug.Log($"Target point {gameObject.name} hit and destroyed!");
            Destroy(gameObject);
        }
    }
}