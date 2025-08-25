using UnityEngine;

public class SimpleCameraOrbit : MonoBehaviour
{
    public Transform target;          // Karakter
    public float distance = 4f;       // Kamera uzaklığı
    public float mouseSensitivity = 2f;
    public float verticalClamp = 60f;

    private float yaw = 0f;
    private float pitch = 0f;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // Mouse input ile dönüş
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        // Kamera pivot'un pozisyonu: karakterin üstü
        transform.position = target.position + Vector3.up * 1.5f;

        // Pivot'u döndür
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Kamera offset ile arkaya
        cam.localPosition = new Vector3(0, 0, -distance);
        cam.LookAt(transform.position);
    }
}
