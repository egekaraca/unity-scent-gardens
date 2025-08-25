using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Camera")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Lock cursor for mouse look
        Cursor.lockState = CursorLockMode.Locked;

        // Create GroundCheck object if not assigned
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void HandleInput()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        bool isMoving = inputDir.magnitude > 0.1f;

        if (isMoving)
        {
            // Kamera yönüne göre hareket
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;

            Vector3 moveDir = (camForward * v + camRight * h).normalized;
            Vector3 targetVelocity = moveDir * moveSpeed;
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 velocityChange = new Vector3(
                targetVelocity.x - currentVelocity.x,
                0,
                targetVelocity.z - currentVelocity.z
            );

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            animator?.SetBool("isWalking", true);
        }
        else
        {
            animator?.SetBool("isWalking", false);

            // Hafif fren etkisi
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(-horizontalVelocity * 4f, ForceMode.Force);
        }
    }

    void HandleRotation()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v);

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
