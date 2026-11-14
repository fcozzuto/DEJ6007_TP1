using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 0.6f;
    [SerializeField] private GameObject explosionPrefab;

    private Rigidbody body;
    private SphereCollider projectileCollider;
    private Transform owner;
    private bool hasExploded;

    // Cache the projectile Rigidbody and collider before it is launched.
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<SphereCollider>();
    }

    // Ignore the caster, send the fireball forward, and schedule an explosion at the end of its lifetime.
    public void Launch(Vector3 direction, Transform owner)
    {
        this.owner = owner;

        // Ignore all caster colliders so the fireball can clear the player's hand.
        foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>(true))
            Physics.IgnoreCollision(projectileCollider, ownerCollider);

        body.linearVelocity = direction.normalized * speed;
        Invoke(nameof(Explode), lifetime);
    }

    // Handle contact with a physical surface through the shared hit logic.
    private void OnCollisionEnter(Collision collision)
    {
        Hit(collision.collider);
    }

    // Handle enemy trigger hits while allowing the fireball through unrelated trigger zones.
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnemyController>() != null)
            Hit(other);
    }

    // Ignore the caster; otherwise kill or ignite the hit object and explode.
    private void Hit(Collider other)
    {
        if (hasExploded || (owner != null && other.transform.IsChildOf(owner)))
            return;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
            enemy.Die();

        FireActivator activator = other.GetComponentInParent<FireActivator>();
        if (activator != null)
            activator.Ignite();

        BossFightActivator bossActivator =
        other.GetComponentInParent<BossFightActivator>();

        if (bossActivator != null)
            bossActivator.Ignite();

        Explode();
    }

    // Stop the projectile, play a separate explosion effect, and clean up both objects.
    private void Explode()
    {
        if (hasExploded)
            return;

        // The timer and a collision can arrive together; only create one explosion.
        hasExploded = true;
        CancelInvoke(nameof(Explode));
        projectileCollider.enabled = false;
        body.linearVelocity = Vector3.zero;

        if (explosionPrefab != null)
        {
            // Spawn the effect separately so destroying the projectile does not cut it off.
            GameObject explosion = Instantiate(
                explosionPrefab, transform.position, Quaternion.identity);

            Destroy(explosion, 4f);
        }

        Destroy(gameObject);
    }
}