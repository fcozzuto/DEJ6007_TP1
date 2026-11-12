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
    private PlayerMovement playerWaitingForCamera;

    public void WaitForCamera(PlayerMovement player)
    {
        playerWaitingForCamera = player;
        player.PausePlayerController();
    }

    private void Awake()
    {
        zoomBehavior = GetComponent<ZoomBehavior>();

        if (target != null)
        {
            rotationOffset = Quaternion.Inverse(target.rotation)
                * transform.rotation;
        }
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

        if (playerWaitingForCamera != null &&
            Vector3.Distance(transform.position, desiredPosition) <= 0.15f &&
            Quaternion.Angle(transform.rotation, desiredRotation) <= 2f)
        {
            PlayerMovement player = playerWaitingForCamera;
            playerWaitingForCamera = null;
            player.ResumePlayerController();
        }
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        transform.SetPositionAndRotation(
            target.position + target.TransformDirection(offset),
            target.rotation * rotationOffset);
    }
}