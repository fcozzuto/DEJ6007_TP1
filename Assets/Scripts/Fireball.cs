using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody body;
    private SphereCollider projectileCollider;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<SphereCollider>();
    }

    public void Launch(Vector3 direction, Transform owner)
    {
        foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(projectileCollider, ownerCollider);
        }

        body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
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
        if (hasHit)
            return;

        hasHit = true;

        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
            enemy.gameObject.SetActive(false);

        Destroy(gameObject);
    }
}