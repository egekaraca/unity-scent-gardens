using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float minSpeed = 1.0f;
    public float maxSpeed = 3.5f;
    public float rotationSpeed = 2f;

    [Header("Behavior Timing")]
    public float minChangeInterval = 2f;
    public float maxChangeInterval = 6f;

    [Header("Environment")]
    public Transform waterArea;

    private Bounds swimBounds;
    private Vector3 targetPosition;
    private float currentSpeed;
    private float changeTargetTimer;

    void Start()
    {
        if (waterArea == null)
        {
            Debug.LogError("WaterArea atanmadý!");
            enabled = false;
            return;
        }

        Collider areaCollider = waterArea.GetComponent<Collider>();
        if (areaCollider == null)
        {
            Debug.LogError("WaterArea objesinde Collider eksik!");
            enabled = false;
            return;
        }

        swimBounds = areaCollider.bounds;
        PickNewTargetPosition();
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        SetNextChangeTime();
    }

    void Update()
    {
        MoveTowardTarget();

        changeTargetTimer -= Time.deltaTime;
        if (changeTargetTimer <= 0f || !swimBounds.Contains(transform.position))
        {
            PickNewTargetPosition();
            SetNextChangeTime();
        }
    }

    void PickNewTargetPosition()
    {
        targetPosition = new Vector3(
            Random.Range(swimBounds.min.x, swimBounds.max.x),
            Random.Range(swimBounds.min.y, swimBounds.max.y),
            Random.Range(swimBounds.min.z, swimBounds.max.z)
        );

        currentSpeed = Random.Range(minSpeed, maxSpeed); // hýz da deðiþsin
    }

    void MoveTowardTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    void SetNextChangeTime()
    {
        changeTargetTimer = Random.Range(minChangeInterval, maxChangeInterval);
    }

    // Debug için sýnýrý göster
    void OnDrawGizmosSelected()
    {
        if (waterArea != null)
        {
            Collider col = waterArea.GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}

