using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
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

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        initialRotation = characterVisual.localRotation;
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

        Vector2 input = Vector2.ClampMagnitude(
            moveAction.ReadValue<Vector2>(), 1f);
        Vector3 movement = new Vector3(input.x, 0f, input.y);
        bool isMoving = movement.sqrMagnitude > 0f;
        bool isRunning = sprintAction.IsPressed();

        if (!isMoving)
            PlayAnimation("Idle");
        else if (isRunning)
            PlayAnimation("Run");
        else
            PlayAnimation("Walk");

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
        if (jumpAction.WasPressedThisFrame() && jumpsUsed < 2)
        {
            verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;
        }

        verticalSpeed += gravity * Time.deltaTime;
        movement.y = verticalSpeed;

        if (controller.enabled)
            controller.Move(movement * Time.deltaTime);

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
    }
    public void ResumePlayerController()
    {
        if (!IsDead)
        {
            controller.enabled = true;
        }
    }

    public void Die(float respawnDelay = 3f)
    {
        if (IsDead)
            return;

        IsDead = true;
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