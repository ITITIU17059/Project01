using UnityEngine;

public class CardPhysics : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [SerializeField] private float maxRotationAngle = 30f;
    [SerializeField] private float sensitivity = 0.3f;
    [SerializeField] private float tension = 20f;
    [SerializeField] private float damping = 3f;

    private Vector3 lastPosition;
    private float currentAngle;
    private float angularVelocity;

    public void ResetPhysics()
    {
        currentAngle = 0f;
        angularVelocity = 0f;
        lastPosition = transform.position;
    }

    public void UpdatePendulumRotation()
    {
        float cardVelocityX = (transform.position.x - lastPosition.x) / Time.deltaTime;

        // Công thức tính lực quán tính con lắc cơ học
        float force = (-tension * currentAngle) - (damping * angularVelocity) - (cardVelocityX * sensitivity);
        angularVelocity += force * Time.deltaTime;
        currentAngle += angularVelocity * Time.deltaTime;

        currentAngle = Mathf.Clamp(currentAngle, -maxRotationAngle, maxRotationAngle);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        lastPosition = transform.position;
    }
}