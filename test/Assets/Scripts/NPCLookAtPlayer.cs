using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    public Transform head;         // NPC kafasý (bone)
    public Transform player;       // Player Transform
    public float lookSpeed = 5f;   // Takip hýz
    public float viewAngle = 90f;  // Görüþ açýsý
    public float viewDistance = 10f; // Görüþ mesafesi

    private Quaternion initialRotation; // Baþlangýç kafa rotasyonu

    void Start()
    {
        if (head != null)
            initialRotation = head.localRotation;
    }

    void Update()
    {
        if (head == null || player == null) return;

        Vector3 direction = player.position - head.position;
        float angle = Vector3.Angle(transform.forward, direction);

        // Oyuncu görüþ açýsýnda ve mesafede mi?
        if (angle < viewAngle * 0.5f && direction.magnitude <= viewDistance)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            head.rotation = Quaternion.Slerp(head.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
        else
        {
            // Önüne dön
            head.localRotation = Quaternion.Slerp(head.localRotation, initialRotation, Time.deltaTime * lookSpeed);
        }
    }
}
