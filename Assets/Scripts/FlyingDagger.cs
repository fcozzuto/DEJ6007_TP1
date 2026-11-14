using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class FlyingDagger : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private float speed = 12f;
    [SerializeField] private float spinSpeed = 900f;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody body;
    private SphereCollider daggerCollider;

    // Cache the dagger Rigidbody and collider before launching it.
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        daggerCollider = GetComponent<SphereCollider>();
    }

    // Ignore the throwing enemy, launch the dagger, and remove it if it never hits anything.
    public void Launch(Vector3 direction, Transform owner)
    {
        // The dagger starts inside the held weapon, so it must ignore the soldier who threw it.
        foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(daggerCollider, ownerCollider);
        }

        body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    // Spin the dagger mesh while leaving the projectile collider on its flight path.
    private void Update()
    {
        visual.Rotate(Vector3.back, spinSpeed * Time.deltaTime, Space.Self);
    }

    // Kill the player if hit, then destroy the dagger on any physical collision.
    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovement hitPlayer =
            collision.collider.GetComponentInParent<PlayerMovement>();

        if (hitPlayer != null)
            hitPlayer.Die();

        Destroy(gameObject);
    }
}