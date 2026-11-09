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

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        daggerCollider = GetComponent<SphereCollider>();
    }

    public void Launch(Vector3 direction, Transform owner)
    {
        foreach (Collider ownerCollider in owner.GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(daggerCollider, ownerCollider);
        }

        body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        visual.Rotate(Vector3.back, spinSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovement hitPlayer =
            collision.collider.GetComponentInParent<PlayerMovement>();

        if (hitPlayer != null)
            hitPlayer.Die();

        Destroy(gameObject);
    }
}