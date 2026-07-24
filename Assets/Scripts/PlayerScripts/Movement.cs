using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;       // Fast base speed
    public float acceleration = 25f;    // How fast you reach top speed (Higher = snappier)
    public float deceleration = 20f;    // How fast you stop (Lower = more slide/momentum)

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
        HandleCameraShake();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
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

        // Smoothly transition between current velocity and target velocity for momentum
        float lerpRate = (moveDirection.magnitude > 0.1f) ? acceleration : deceleration;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, lerpRate * Time.deltaTime);
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
}