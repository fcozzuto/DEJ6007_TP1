using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 9f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform characterVisual;
    [SerializeField] private float turnSpeed = 12f;

    private CharacterController controller;
    private float verticalSpeed;
    private int jumpsUsed;
    private string currentAnimation;

    public bool IsDead { get; private set; }
    private Quaternion initialRotation;
    private PlayerAttack combat;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private SmoothFollow cameraFollow;

    // Cache the controller, combat script, and camera, and remember the model's initial local rotation.
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        initialRotation = characterVisual.localRotation;
        combat = animator.GetComponent<PlayerAttack>();
        cameraFollow = Camera.main.GetComponent<SmoothFollow>();
    }

    // Get the movement, jump, and sprint actions from the project input settings.
    private void Start()
    {
        var actions = InputSystem.actions;
        moveAction = actions.FindAction("Player/Move", throwIfNotFound: true);
        jumpAction = actions.FindAction("Player/Jump", throwIfNotFound: true);
        sprintAction = actions.FindAction("Player/Sprint", throwIfNotFound: true);
    }

    // Handle movement, facing, locomotion animations, jumps, and movement from attacks.
    private void Update()
    {
        if (GameManager.GameplayInputBlocked)
            return;

        if (IsDead || !controller.enabled)
            return;

        if (controller.isGrounded && verticalSpeed <= 0f)
        {
            // A small downward push keeps the CharacterController grounded between frames.
            verticalSpeed = -2f;
            jumpsUsed = 0;
        }

        bool busy = combat != null && combat.IsBusy;
        bool armed = combat != null && combat.SwordDrawn;

        Vector2 input = busy
            ? Vector2.zero
            : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        Vector3 movement = new Vector3(input.x, 0f, input.y);

        // Apply the view angle first; TransformDirection below adds the Player root rotation.
        movement = cameraFollow.MovementRotation * movement;

        bool isRunning = sprintAction.IsPressed();

        // Combat owns the Animator here; clear the cache so locomotion can resume afterward.
        if (busy)
        {
            currentAnimation = null;
        }
        else if (!controller.isGrounded)
        {
            if (verticalSpeed > 0f)
                PlayAnimation("Forward Jump");
            else
                PlayAnimation("Falling Idle");
        }
        else if (movement.sqrMagnitude == 0f)
        {
            PlayAnimation("Idle");
        }
        else if (isRunning)
        {
            PlayAnimation(armed ? "Sprint With Sword" : "Sprint");
        }
        else
        {
            PlayAnimation(armed ? "Walking With Sword" : "Walk");
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        movement = transform.TransformDirection(movement) * speed;

        if (movement.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(movement, Vector3.up);

            characterVisual.rotation = Quaternion.Slerp(
                characterVisual.rotation,
                desiredRotation,
                turnSpeed * Time.deltaTime);
        }

        if (!busy && jumpAction.WasPressedThisFrame() && jumpsUsed < 2)
        {
            // Convert jump height to takeoff speed using the current gravity.
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;

            // Clear the cached state so the second jump can restart the same clip.
            currentAnimation = null;
            PlayAnimation("Forward Jump");
        }

        bool attackDrivesHeight = false;
        Vector3 attackDelta = combat != null
            ? combat.GetAttackDisplacement(out attackDrivesHeight)
            : Vector3.zero;

        // Use the attack's vertical motion during the hop instead of adding normal gravity.
        if (attackDrivesHeight)
            verticalSpeed = 0f;
        else
            verticalSpeed += gravity * Time.deltaTime;

        movement.y = verticalSpeed;

        if (controller.enabled)
            controller.Move(movement * Time.deltaTime + attackDelta);

        if ((controller.collisionFlags & CollisionFlags.Above) != 0
            && verticalSpeed > 0f)
        {
            verticalSpeed = 0f;
        }
    }

    // Blend to a different locomotion state without restarting the same animation every frame.
    private void PlayAnimation(string stateName)
    {

        if (animator == null || currentAnimation == stateName)
            return;

        animator.CrossFadeInFixedTime(stateName, 0.15f);
        currentAnimation = stateName;
    }

    // Turn the camera and movement reference while preserving the model's world rotation.
    public void RotateForCamera(float angle)
    {
        // Store world rotation, since rotating the parent also turns its child model.
        Quaternion visualRotation = characterVisual.rotation;

        transform.Rotate(0f, angle, 0f);

        characterVisual.rotation = visualRotation;
    }

    // Restore the checkpoint position, facing, and controls, clearing the last jump or fall.
    public void RespawnAt(Vector3 position, Quaternion facingRotation)
    {
        transform.SetPositionAndRotation(
        position,
        // Undo the model's local offset so its world facing matches the checkpoint.
        facingRotation * Quaternion.Inverse(initialRotation));

        characterVisual.rotation = facingRotation;

        // Update the physics position before the CharacterController is enabled again.
        Physics.SyncTransforms();

        verticalSpeed = 0f;
        jumpsUsed = 0;
        currentAnimation = null;
        IsDead = false;

        ResumePlayerController();
        animator.enabled = true;
    }

    // Pause movement for a camera turn, cancel combat, and put a living player in Idle.
    public void PausePlayerController()
    {
        controller.enabled = false;

        if (IsDead)
            return;

        if (combat != null && combat.IsBusy)
            combat.CancelCurrentAction();

        currentAnimation = null;
        PlayAnimation("Idle");
    }

    // Restore movement after the camera settles, unless the player is dead.
    public void ResumePlayerController()
    {
        if (IsDead)
            return;

        controller.enabled = true;
    }

    // Enable the death ragdoll and zoom, then request a delayed respawn or Game Over.
    public void Die(float respawnDelay = 3f)
    {
        if (IsDead)
            return;

        IsDead = true;
        combat?.ResetCombat();
        // The ragdoll bones can take over once the Animator stops driving the skeleton.
        animator.enabled = false;
        PausePlayerController();

        ZoomBehavior zoomBehavior = Camera.main.GetComponent<ZoomBehavior>();
        if (zoomBehavior != null)
            zoomBehavior.playerIsDead = true;

        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
            gameManager.GetComponent<GameManager>().RespawnWithDelay(respawnDelay);
    }
}