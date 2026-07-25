using UnityEngine;

public class Grapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxDistance = 30f;       // How far the grapple can reach
    public float pullSpeed = 22f;         // How fast you're pulled toward the point
    public float steerStrength = 6f;      // How much WASD steers you mid-grapple
    public float releaseDistance = 1.5f;  // Auto-release when this close
    public LayerMask grappleableLayers;   // Optional: leave empty to grapple anything

    [Header("Rope")]
    public LineRenderer ropeRenderer;     // Optional: auto-created if left empty

    [Header("Targeting Reticle")]
    public Color reticleColor = new Color(1f, 0.1f, 0.1f);  // Red dot
    public float reticleSize = 0.15f;

    private UnityEngine.Camera playerCamera;
    private Rigidbody rb;
    private Movement movement;
    private bool isGrappling = false;
    private Vector3 grapplePoint;
    private GameObject reticle;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<Movement>();

        playerCamera = GetComponentInChildren<UnityEngine.Camera>();
        if (playerCamera == null)
            playerCamera = UnityEngine.Camera.main;

        if (ropeRenderer == null)
            ropeRenderer = gameObject.AddComponent<LineRenderer>();

        ropeRenderer.enabled = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.startWidth = 0.08f;
        ropeRenderer.endWidth = 0.08f;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            ropeRenderer.material = new Material(shader);

        CreateReticle();
    }

    void CreateReticle()
    {
        reticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        reticle.name = "GrappleReticle";

        // Remove the collider so the dot never blocks raycasts or physics
        Destroy(reticle.GetComponent<Collider>());

        Renderer rend = reticle.GetComponent<Renderer>();
        Shader unlit = Shader.Find("Unlit/Color");
        if (unlit != null)
        {
            rend.material = new Material(unlit);
            rend.material.color = reticleColor;
        }

        reticle.transform.localScale = Vector3.one * reticleSize;
        reticle.SetActive(false);
    }

    // Shared targeting logic: finds the first valid grapple surface along your aim.
    bool FindGrappleTarget(out Vector3 point)
    {
        point = Vector3.zero;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            // Ignore the player's own colliders
            if (h.transform == transform || h.transform.IsChildOf(transform))
                continue;

            // Optional layer filter
            if (grappleableLayers.value != 0 &&
                (grappleableLayers.value & (1 << h.collider.gameObject.layer)) == 0)
                continue;

            point = h.point;
            return true;
        }

        return false;
    }

    // Called by TrashThrower when Blue is used. Returns true if a grapple started.
    public bool TryGrapple()
    {
        if (isGrappling) return false;

        Vector3 point;
        if (FindGrappleTarget(out point))
        {
            StartGrapple(point);
            return true;
        }

        return false;
    }

    void StartGrapple(Vector3 point)
    {
        isGrappling = true;
        grapplePoint = point;

        if (movement != null)
            movement.grappleActive = true;

        ropeRenderer.enabled = true;
        reticle.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!isGrappling) return;

        Vector3 dir = (grapplePoint - transform.position).normalized;
        Vector3 pullVelocity = dir * pullSpeed;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 steer = (transform.right * moveX + transform.forward * moveZ) * steerStrength;

        rb.linearVelocity = pullVelocity + steer;
    }

    void Update()
    {
        UpdateReticle();

        if (!isGrappling) return;

        ropeRenderer.SetPosition(0, transform.position + Vector3.up * 1f);
        ropeRenderer.SetPosition(1, grapplePoint);

        float dist = Vector3.Distance(transform.position, grapplePoint);
        if (dist < releaseDistance || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(1))
        {
            EndGrapple();
        }
    }

    void UpdateReticle()
    {
        if (isGrappling || reticle == null)
        {
            if (reticle != null) reticle.SetActive(false);
            return;
        }

        Vector3 point;
        if (FindGrappleTarget(out point))
        {
            // Nudge slightly toward the camera so the dot doesn't clip into the surface
            Vector3 toCamera = (playerCamera.transform.position - point).normalized;
            reticle.transform.position = point + toCamera * 0.05f;
            reticle.SetActive(true);
        }
        else
        {
            reticle.SetActive(false);
        }
    }

    void EndGrapple()
    {
        isGrappling = false;

        if (movement != null)
            movement.grappleActive = false;

        ropeRenderer.enabled = false;
    }
}