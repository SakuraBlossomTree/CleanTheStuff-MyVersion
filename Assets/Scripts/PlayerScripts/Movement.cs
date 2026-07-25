using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float acceleration = 25f;
    public float deceleration = 20f;

    [Header("Momentum")]
    public float groundControl = 15f;   // Snappier ground movement (higher = snappier)
    public float airControl = 3f;       // Air steering strength (lower = more momentum conserved)
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;       // Optional: leave empty to detect any floor

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;
    public float shakeAmount = 0.05f;
    public float shakeSpeed = 10f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private float rotationX = 0f;
    private Vector3 originalCamPos;
    private bool isGrounded;

    [HideInInspector] public bool grappleActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        originalCamPos = cameraTransform.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        LookAround();
        Move();
        HandleJump();
        HandleCameraShake();
    }

    void FixedUpdate()
    {
        CheckGround();

        // During a grapple, the Grapple script controls velocity.
        if (grappleActive) return;

        // Where input wants to take you (computed in Move()).
        Vector3 inputVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        // Your actual horizontal momentum (grapple swings, rocket jumps, etc.).
        Vector3 currentHorizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        // Grounded = snap to input (snappy). Airborne = conserve momentum, steer gently.
        float blend = isGrounded ? groundControl : airControl;
        Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, inputVelocity, blend * Time.fixedDeltaTime);

        // Preserve vertical velocity so gravity and rocket jumps keep working.
        rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        moveDirection = (forward * moveZ + right * moveX).normalized;
        Vector3 targetVelocity = moveDirection * moveSpeed;

        float lerpRate = (moveDirection.magnitude > 0.1f) ? acceleration : deceleration;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, lerpRate * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    void CheckGround()
    {
        int mask = groundLayer.value != 0 ? groundLayer.value : ~0;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, mask, QueryTriggerInteraction.Ignore)
                     && !hit.collider.transform.IsChildOf(transform);
    }

    void HandleCameraShake()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float shakeY = Mathf.Cos(Time.time * shakeSpeed * 2f) * shakeAmount * 0.5f;
            cameraTransform.localPosition = originalCamPos + new Vector3(shakeX, shakeY, 0);
        }
        else
        {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                originalCamPos,
                Time.deltaTime * shakeSpeed
            );
        }
    }

    // Visual debug line in the Scene view so you can see the ground check ray
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}