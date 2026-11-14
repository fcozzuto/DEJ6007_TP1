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

    // Give movement the same horizontal orientation as the selected camera view.
    public Quaternion MovementRotation => Quaternion.Euler(0f, targetViewYaw, 0f);

    private bool behindPlayerViewActive;
    private Vector3 savedSideOffset;
    private float savedSideYaw;
    private float savedSideNearClip;

    // Pause player control until the camera is close to its new position and rotation.
    public void WaitForCamera(PlayerMovement player)
    {
        playerWaitingForCamera = player;
        player.PausePlayerController();
    }

    // Switch between the centered, reversed boss-room view and the normal side-scrolling view.
    public void SetBossRoomView(bool useBossView, PlayerMovement player)
    {
        if (behindPlayerViewActive)
            SetBehindPlayerView(false, player);

        if (bossRoomViewActive == useBossView)
            return;

        if (useBossView)
        {

            followCamera.nearClipPlane = 3f;
            // Keep the normal view separate from the view saved when entering the ending corridor.
            savedRoomOffset = offset;
            offset.x = 0f;
            targetViewYaw = 180f;
        }
        else
        {

            followCamera.nearClipPlane = 4.1f;
            offset = savedRoomOffset;
            targetViewYaw = 0f;
        }

        bossRoomViewActive = useBossView;
        WaitForCamera(player);
    }

    // Switch to the rear view for the exit corridor, or restore the side view saved before entering it.
    public void SetBehindPlayerView(bool useBehindView, PlayerMovement player)
    {
        if (behindPlayerViewActive == useBehindView)
            return;

        if (useBehindView)
        {

            // Save the angle and near plane too, since the previous view may be the boss-room view.
            savedSideOffset = offset;
            savedSideYaw = targetViewYaw;
            savedSideNearClip = followCamera.nearClipPlane;

            offset = behindPlayerOffset;
            // This quarter turn looks down the exit corridor relative to the Player root.
            targetViewYaw = 90f;
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

    // Cache the camera components and remember the initial rotation relative to the Player root.
    private void Awake()
    {
        zoomBehavior = GetComponent<ZoomBehavior>();
        followCamera = GetComponent<Camera>();

        if (target != null)
        {
            // Remove the root rotation to keep the camera's original tilt as a relative offset.
            rotationOffset = Quaternion.Inverse(target.rotation)
                * transform.rotation;
        }
    }

    // Follow the player or ragdoll, blend views, and restore control when the camera settles.
    private void LateUpdate()
    {
        if (target == null)
            return;

        bool isDead = zoomBehavior != null && zoomBehavior.playerIsDead;

        // Returning from dead to alive means a respawn outside the arena; restore the normal view.
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
            // Keep the bone-to-root offset from death so following the ragdoll does not cause a jump.
            deathPositionOffset =
                ragdollTarget.position - target.position;
        }

        wasDead = isDead;

        Vector3 followPosition = target.position;

        if (isDead && ragdollTarget != null)
            followPosition = ragdollTarget.position - deathPositionOffset;

        float blend = smoothSpeed * Time.deltaTime;

        // Turn the offset around the player so the camera does not cut straight through them.
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

        // Use the final view for the arrival check so control does not resume halfway through.
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

    // Snap to the selected view at the starting spawn so the camera does not slide into place.
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