using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Transform ragdollTarget;

    public Vector3 offset;
    public float smoothSpeed = 5f;

    private Quaternion rotationOffset;
    private ZoomBehavior zoomBehavior;

    private bool wasDead;
    private Vector3 deathPositionOffset;

    private void Awake()
    {
        zoomBehavior = GetComponent<ZoomBehavior>();
    }

    private void Start()
    {
        if (target == null) return;

        // Preserve the camera's initial angle relative to the player.
        rotationOffset = Quaternion.Inverse(target.rotation)
            * transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool isDead = zoomBehavior != null && zoomBehavior.playerIsDead;

        if (isDead && !wasDead && ragdollTarget != null)
        {
            deathPositionOffset =
                ragdollTarget.position - target.position;
        }

        wasDead = isDead;

        Vector3 followPosition = target.position;

        if (isDead && ragdollTarget != null)
        {
            // Follow the body's displacement without copying its rotation.
            followPosition =
                ragdollTarget.position - deathPositionOffset;
        }

        float blend = smoothSpeed * Time.deltaTime;

        // Rotate the camera's offset with the player.
        Vector3 desiredPosition =
            followPosition + target.TransformDirection(offset);

        Quaternion desiredRotation =
            target.rotation * rotationOffset;

        transform.position = Vector3.Lerp(
            transform.position, desiredPosition, blend);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, desiredRotation, blend);
    }
}