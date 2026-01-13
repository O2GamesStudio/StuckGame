using UnityEngine;

public class TargetCtrl : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] bool rotateClockwise = true;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.angularVelocity = rotateClockwise ? -rotationSpeed : rotationSpeed;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            float direction = rotateClockwise ? -1f : 1f;
            rb.angularVelocity = direction * rotationSpeed;
        }
    }
}