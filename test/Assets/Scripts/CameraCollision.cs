using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    public Transform target;            // Takip edilen oyuncu
    public float cameraDistance = 4f;   // Kamera ile hedef arasındaki mesafe
    public float smoothSpeed = 10f;     // Kamera geçiş hızı
    public float minDistance = 0.5f;    // Kameranın en fazla yaklaşabileceği mesafe
    public LayerMask collisionLayers;   // Çarpışma yapılacak layer’lar

    private Vector3 desiredPosition;
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        // İdeal kamera pozisyonu (arkada ve yukarıda)
        Vector3 direction = (transform.position - target.position).normalized;
        desiredPosition = target.position + direction * cameraDistance;

        // Kamera ile karakter arasında ray at
        RaycastHit hit;
        if (Physics.Raycast(target.position, direction, out hit, cameraDistance, collisionLayers))
        {
            float hitDistance = Mathf.Clamp(hit.distance, minDistance, cameraDistance);
            desiredPosition = target.position + direction * hitDistance;
        }

        // Kamerayı smooth olarak yerleştir
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Hedefe bak
        transform.LookAt(target);
    }
}
