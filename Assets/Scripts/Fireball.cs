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

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<SphereCollider>();
    }

    public void Launch(Vector3 direction, Transform owner)
    {
        this.owner = owner;

        foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>(true))
            Physics.IgnoreCollision(projectileCollider, ownerCollider);

        body.linearVelocity = direction.normalized * speed;
        Invoke(nameof(Explode), lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Hit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnemyController>() != null)
            Hit(other);
    }

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

        Explode();
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;
        CancelInvoke(nameof(Explode));
        projectileCollider.enabled = false;
        body.linearVelocity = Vector3.zero;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab, transform.position, Quaternion.identity);

            Destroy(explosion, 4f);
        }

        Destroy(gameObject);
    }
}