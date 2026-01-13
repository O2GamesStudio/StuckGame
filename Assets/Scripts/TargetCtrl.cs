using UnityEngine;

public class TargetCtrl : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 50f;
    [SerializeField] bool rotateClockwise = true;

    private Rigidbody2D rb;
    private bool isRotating = true;

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
        if (rb != null && isRotating)
        {
            float direction = rotateClockwise ? -1f : 1f;
            rb.angularVelocity = direction * rotationSpeed;
        }
    }

    public void StopRotation()
    {
        isRotating = false;
        if (rb != null)
        {
            rb.angularVelocity = 0f;
        }
    }
}