using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Camera Settings")]
    public float distance = 10f;
    public float height = 5f;
    public float rotationSpeed = 2f;
    public float smoothTime = 0.3f;
    
    private float currentRotationAngle;
    private float currentHeight;
    private Vector3 velocity = Vector3.zero;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned!");
            return;
        }
        
        // Initialize camera position
        currentRotationAngle = transform.eulerAngles.y;
        currentHeight = transform.position.y;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Get mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X");
        
        // Update rotation angle
        currentRotationAngle += mouseX * rotationSpeed;
        
        // Calculate desired position
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(0, currentRotationAngle, 0);
        Vector3 desiredPosition = target.position + rotation * direction;
        desiredPosition.y = target.position.y + height;
        
        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        
        // Always look at target
        transform.LookAt(target.position + Vector3.up * 2f);
    }
}