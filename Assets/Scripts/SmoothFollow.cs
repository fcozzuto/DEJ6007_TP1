using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Transform ragdollTarget;

    public Vector3 offset;
    public float smoothSpeed = 5f;

    public Quaternion rotationOffset;
    private ZoomBehavior zoomBehavior;
    private Camera followCamera;
    [SerializeField] private Vector3 behindPlayerOffset = new Vector3(0f, 3f, -5f);

    private bool wasDead;
    private Vector3 deathPositionOffset;
    private PlayerMovement playerWaitingForCamera;

    private bool bossRoomViewActive;
    public bool BossRoomViewActive => bossRoomViewActive;
    private Vector3 savedRoomOffset;
    private float currentViewYaw;
    private float targetViewYaw;

    public Quaternion MovementRotation => Quaternion.Euler(0f, targetViewYaw, 0f);

    private bool behindPlayerViewActive;
    private Vector3 savedSideOffset;
    private float savedSideYaw;
    private float savedSideNearClip;

    public void WaitForCamera(PlayerMovement player)
    {
        playerWaitingForCamera = player;
        player.PausePlayerController();
    }

    public void SetBossRoomView(bool useBossView, PlayerMovement player)
    {
        if (behindPlayerViewActive)
            SetBehindPlayerView(false, player);

        if (bossRoomViewActive == useBossView)
            return;

        if (useBossView)
        {
            // Change default plane clipping settings
            followCamera.nearClipPlane = 3f;
            savedRoomOffset = offset;
            offset.x = 0f;
            targetViewYaw = 180f;
        }
        else
        {
            // Restore default plane clipping settings
            followCamera.nearClipPlane = 4.1f;
            offset = savedRoomOffset;
            targetViewYaw = 0f;
        }

        bossRoomViewActive = useBossView;
        WaitForCamera(player);
    }

    public void SetBehindPlayerView(bool useBehindView, PlayerMovement player)
    {
        if (behindPlayerViewActive == useBehindView)
            return;

        if (useBehindView)
        {
            // Save the side view, including the boss room's reversed camera angle.
            savedSideOffset = offset;
            savedSideYaw = targetViewYaw;
            savedSideNearClip = followCamera.nearClipPlane;

            offset = behindPlayerOffset;
            targetViewYaw = 90f; // Look along the exit corridor, relative to the Player root.
            followCamera.nearClipPlane = 0.3f;
        }
        else
        {
            offset = savedSideOffset;
            targetViewYaw = savedSideYaw;
            followCamera.nearClipPlane = savedSideNearClip;
        }

        behindPlayerViewActive = useBehindView;
        WaitForCamera(player);
    }

    private void Awake()
    {
        zoomBehavior = GetComponent<ZoomBehavior>();
        followCamera = GetComponent<Camera>();

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

        // Respawning outside the arena restores the normal side view and controls.
        if (!isDead && wasDead)
        {
            PlayerMovement player = target.GetComponent<PlayerMovement>();

            if (behindPlayerViewActive)
                SetBehindPlayerView(false, player);

            if (bossRoomViewActive)
                SetBossRoomView(false, player);
        }

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