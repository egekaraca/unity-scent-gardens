using UnityEngine;

public class NPCLookAtPlayer : MonoBehaviour
{
    public Transform head;         // NPC kafas� (bone)
    public Transform player;       // Player Transform
    public float lookSpeed = 5f;   // Takip h�z
    public float viewAngle = 90f;  // G�r�� a��s�
    public float viewDistance = 10f; // G�r�� mesafesi

    private Quaternion initialRotation; // Ba�lang�� kafa rotasyonu

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

        // Oyuncu g�r�� a��s�nda ve mesafede mi?
        if (angle < viewAngle * 0.5f && direction.magnitude <= viewDistance)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            head.rotation = Quaternion.Slerp(head.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
        else
        {
            // �n�ne d�n
            head.localRotation = Quaternion.Slerp(head.localRotation, initialRotation, Time.deltaTime * lookSpeed);
        }
    }
}
