using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownTrash : MonoBehaviour
{
    [HideInInspector] public TrashType trashType;

    [Header("Explosion (Red)")]
    public float explosionRadius = 5f;
    public float explosionForce = 900f;    // Blasts objects away horizontally
    public float rocketJumpForce = 12f;    // How high the rocket jump launches YOU
    public float explosionDamage = 50f;

    private Rigidbody rb;
    private bool hasImpacted = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 direction, float force)
    {
        rb.linearVelocity = direction * force;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        if (collision.gameObject.CompareTag("Player")) return;

        hasImpacted = true;

        switch (trashType)
        {
            case TrashType.Red:
                Explode();
                break;
            default:
                Destroy(gameObject);
                break;
        }
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        Rigidbody playerRb = null;

        foreach (Collider hit in hits)
        {
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null && hitRb != rb)
            {
                // Blast objects away from the explosion
                hitRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                if (hit.CompareTag("Player"))
                    playerRb = hitRb;
            }

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(explosionDamage);
        }

        // Rocket jump: directly launch the player upward
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector3(
                playerRb.linearVelocity.x,
                rocketJumpForce,
                playerRb.linearVelocity.z
            );
        }

        Destroy(gameObject);
    }
}