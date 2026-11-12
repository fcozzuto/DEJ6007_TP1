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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        initialRotation = characterVisual.localRotation;
        combat = animator.GetComponent<PlayerAttack>();
    }

    private void Start()
    {
        var actions = InputSystem.actions;
        moveAction = actions.FindAction("Player/Move", throwIfNotFound: true);
        jumpAction = actions.FindAction("Player/Jump", throwIfNotFound: true);
        sprintAction = actions.FindAction("Player/Sprint", throwIfNotFound: true);
    }

    private void Update()
    {
        if (IsDead || !controller.enabled)
            return;

        // Reset jumps after landing. Keep a small downward push.
        if (controller.isGrounded && verticalSpeed <= 0f)
        {
            verticalSpeed = -2f;
            jumpsUsed = 0;
        }

        bool busy = combat != null && combat.IsBusy;
        bool armed = combat != null && combat.SwordDrawn;

        Vector2 input = busy
            ? Vector2.zero
            : Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        Vector3 movement = new Vector3(input.x, 0f, input.y);
        bool isRunning = sprintAction.IsPressed();

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

        // Each press jumps once, with two jumps allowed before landing.
        if (!busy && jumpAction.WasPressedThisFrame() && jumpsUsed < 2)
        {
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;

            // Allow the second jump to restart the jump animation.
            currentAnimation = null;
            PlayAnimation("Forward Jump");
        }

        bool attackDrivesHeight = false;
        Vector3 attackDelta = combat != null
            ? combat.GetAttackDisplacement(out attackDrivesHeight)
            : Vector3.zero;

        if (attackDrivesHeight)
            verticalSpeed = 0f; // The attack supplies the rise and descent.
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

    private void PlayAnimation(string stateName)
    {
        // Only switch when the requested animation changes.
        if (animator == null || currentAnimation == stateName)
            return;

        animator.CrossFadeInFixedTime(stateName, 0.15f);
        currentAnimation = stateName;
    }

    public void RespawnAt(Vector3 position, Quaternion facingRotation)
    {
        transform.SetPositionAndRotation(
        position,
        facingRotation * Quaternion.Inverse(initialRotation));

        characterVisual.rotation = facingRotation;

        Physics.SyncTransforms(); // Ensure the CharacterController is in sync with the new position

        // Start the new life without the previous fall's momentum.
        verticalSpeed = 0f;
        jumpsUsed = 0;
        currentAnimation = null;
        IsDead = false;

        ResumePlayerController();
        animator.enabled = true;
    }

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

    public void ResumePlayerController()
    {
        if (IsDead)
            return;

        controller.enabled = true;
    }

    public void Die(float respawnDelay = 3f)
    {
        if (IsDead)
            return;

        IsDead = true;
        combat?.ResetCombat();
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