using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Transform ragdollTarget;

    public Vector3 offset;
    public float smoothSpeed = 5f;

    public Quaternion rotationOffset;
    private ZoomBehavior zoomBehavior;

    private bool wasDead;
    private Vector3 deathPositionOffset;
    private PlayerMovement playerWaitingForCamera;

    private bool bossRoomViewActive;
    public bool BossRoomViewActive => bossRoomViewActive;
    private Vector3 savedRoomOffset;
    private float currentViewYaw;
    private float targetViewYaw;

    public void WaitForCamera(PlayerMovement player)
    {
        playerWaitingForCamera = player;
        player.PausePlayerController();
    }

    public void SetBossRoomView(bool useBossView, PlayerMovement player)
    {
        if (bossRoomViewActive == useBossView)
            return;

        if (useBossView)
        {
            // Change default plane clipping settings
            gameObject.GetComponent<Camera>().nearClipPlane = 3f;
            savedRoomOffset = offset;
            offset.x = 0f;
            targetViewYaw = 180f;
        }
        else
        {
            // Restore default plane clipping settings
            gameObject.GetComponent<Camera>().nearClipPlane = 4.1f;
            offset = savedRoomOffset;
            targetViewYaw = 0f;
        }

        bossRoomViewActive = useBossView;
        WaitForCamera(player);
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
        if (target == null)
            return;

        bool isDead = zoomBehavior != null && zoomBehavior.playerIsDead;

        // A failed fight respawns the player outside the arena.
        if (!isDead && wasDead && bossRoomViewActive)
            SetBossRoomView(false, target.GetComponent<PlayerMovement>());

        if (isDead && !wasDead && ragdollTarget != null)
        {
            deathPositionOffset =
                ragdollTarget.position - target.position;
        }

        wasDead = isDead;

        Vector3 followPosition = target.position;

        if (isDead && ragdollTarget != null)
            followPosition = ragdollTarget.position - deathPositionOffset;

        float blend = smoothSpeed * Time.deltaTime;

        // Turn the offset gradually to take the camera around the player.
        currentViewYaw = Mathf.LerpAngle(currentViewYaw, targetViewYaw, blend);
        Quaternion viewTurn = Quaternion.Euler(0f, currentViewYaw, 0f);

        Vector3 desiredPosition =
            followPosition + target.TransformDirection(viewTurn * offset);

        Quaternion desiredRotation =
            target.rotation * viewTurn * rotationOffset;

        transform.position = Vector3.Lerp(
            transform.position, desiredPosition, blend);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, desiredRotation, blend);

        // Resume against the final destination, rather than an intermediate pose.
        Quaternion finalViewTurn = Quaternion.Euler(0f, targetViewYaw, 0f);
        Vector3 finalPosition =
            followPosition + target.TransformDirection(finalViewTurn * offset);
        Quaternion finalRotation =
            target.rotation * finalViewTurn * rotationOffset;

        if (playerWaitingForCamera != null &&
            Vector3.Distance(transform.position, finalPosition) <= 0.15f &&
            Quaternion.Angle(transform.rotation, finalRotation) <= 2f)
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

        currentViewYaw = targetViewYaw;
        Quaternion viewTurn = Quaternion.Euler(0f, currentViewYaw, 0f);

        transform.SetPositionAndRotation(
            target.position + target.TransformDirection(viewTurn * offset),
            target.rotation * viewTurn * rotationOffset);
    }
}