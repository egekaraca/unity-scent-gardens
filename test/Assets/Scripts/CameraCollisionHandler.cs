using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    public Transform target;                // Genelde CameraPivot
    public float maxDistance = 4f;          // Normalde ne kadar geride olsun
    public float minDistance = 0.5f;        // Minimum mesafe (çok yaklaştığında)
    public float smoothSpeed = 10f;         // Kameranın geçiş hızı
    public LayerMask collisionLayers;       // Hangi layer’lar ile çarpışsın

    private float currentDistance;
    private Vector3 desiredPosition;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        currentDistance = maxDistance;
    }

    void LateUpdate()
    {
        Vector3 direction = -transform.forward;
        Vector3 origin = target.position;

        RaycastHit hit;

        if (Physics.SphereCast(origin, 0.2f, direction, out hit, maxDistance, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            currentDistance = maxDistance;
        }

        desiredPosition = origin + direction * currentDistance;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);
        transform.LookAt(target);
    }
}
