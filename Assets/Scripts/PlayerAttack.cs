using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(-10)]
public class PlayerAttack : MonoBehaviour
{
    private enum Phase
    {
        Ready, Drawing, First, ReadyForSecond, Second, Sheathing, Casting
    }
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private GameObject swordInHand;
    [SerializeField] private GameObject swordSheathed;
    [SerializeField] private BoxCollider blade;
    [SerializeField] private float firstAttackDistance = 1f;
    [SerializeField] private float secondAttackDistance = 2f;
    [SerializeField] private float secondAttackHeight = 2f;
    [SerializeField] private Fireball fireballPrefab;
    [SerializeField] private Transform fireballSpawn;

    private InputAction fireballAction;
    private bool fireballReleased;
    private Vector3 attackForward;
    private float previousTravel;
    private float previousLift;
    private bool hopDecided;
    private bool canHop;

    private Animator animator;
    private CharacterController controller;
    private InputAction attackAction;

    private Phase phase;
    private string activeState;
    private bool secondQueued;
    private float sheathAt;

    private int queuedAttacks;

    // Only the ready states allow normal movement or a new combat action.
    public bool IsBusy => phase != Phase.Ready && phase != Phase.ReadyForSecond;
    public bool SwordDrawn { get; private set; }

    // Cache the combat components and start with the sword sheathed and the hitbox closed.
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = movement.GetComponent<CharacterController>();
        ResetCombat();
    }

    // Get the sword and fireball input actions used by the combat sequence.
    private void Start()
    {
        attackAction = InputSystem.actions.FindAction("Player/Attack", true);
        fireballAction = InputSystem.actions.FindAction("Player/Fireball", true);
    }

    // Read combat inputs, queue the combo, and advance the sequence when each animation finishes.
    private void Update()
    {
        if (GameManager.GameplayInputBlocked)
            return;

        if (movement.IsDead || !controller.enabled)
            return;

        bool pressed = attackAction.WasPressedThisFrame();

        if (!IsBusy)
        {
            if (!controller.isGrounded)
                return;

            if (fireballAction.WasPressedThisFrame())
            {
                // Keep casting level even if the animation tilts or turns the character.
                attackForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                fireballReleased = false;
                queuedAttacks = 0;

                // Free the casting hand; SwordDrawn remembers which sword models to restore.
                swordInHand.SetActive(false);
                swordSheathed.SetActive(true);

                Begin(Phase.Casting, "Gunplay");
                return;
            }

            if (SwordDrawn && Time.time >= sheathAt)
            {
                Begin(Phase.Sheathing, "Sheath Sword");
            }
            else if (pressed)
            {
                if (!SwordDrawn)
                    Begin(Phase.Drawing, "Withdrawing Sword");
                else if (phase == Phase.ReadyForSecond)
                    BeginSwing(Phase.Second, "Second Attack");
                else
                    BeginSwing(Phase.First, "First Attack");
            }

            return;
        }

        // Queue up to two swings during the draw, or one follow-up during the first swing.
        if (pressed && phase == Phase.Drawing)
            queuedAttacks = Mathf.Min(queuedAttacks + 1, 2);
        else if (pressed && phase == Phase.First)
            queuedAttacks = 1;

        // Only advance after the active clip has finished, regardless of its playback speed.
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0) ||
            !state.IsName(activeState) || state.normalizedTime < 1f)
            return;

        switch (phase)
        {
            case Phase.Drawing:
                SetSwordDrawn(true);

                if (queuedAttacks > 0)
                {
                    queuedAttacks--;
                    BeginSwing(Phase.First, "First Attack");
                }
                else
                {
                    WaitWithSwordDrawn(Phase.Ready);
                }
                break;

            case Phase.First:
                if (queuedAttacks > 0)
                {
                    queuedAttacks = 0;
                    Begin(Phase.Second, "Second Attack");
                }
                else
                {
                    WaitWithSwordDrawn(Phase.ReadyForSecond);
                }
                break;

            case Phase.Second:
                WaitWithSwordDrawn(Phase.Ready);
                break;

            case Phase.Sheathing:
                ResetCombat();
                break;

            case Phase.Casting:
                SetSwordDrawn(SwordDrawn);
                WaitWithSwordDrawn(Phase.Ready);
                break;
        }
    }

    // Reset the movement tracking for an action and blend into its animation from the start.
    private void Begin(Phase next, string stateName)
    {
        previousTravel = 0f;
        previousLift = 0f;
        hopDecided = false;
        canHop = false;
        CloseHitWindow();
        phase = next;
        activeState = "Base Layer." + stateName;
        animator.CrossFadeInFixedTime(activeState, 0.08f, 0, 0f);
    }

    // Remember the direction of the swing before starting its animation.
    private void BeginSwing(Phase next, string stateName)
    {
        // Keep travel in this direction even when the attack animation rotates the model.
        attackForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Begin(next, stateName);
    }

    // Close the hitbox and give the player three seconds to continue before sheathing the sword.
    private void WaitWithSwordDrawn(Phase next)
    {
        CloseHitWindow();
        queuedAttacks = 0;
        phase = next;
        sheathAt = Time.time + 3f;
    }

    // Clear the combo and return to the ready state with the sword sheathed.
    public void ResetCombat()
    {
        CloseHitWindow();
        queuedAttacks = 0;
        phase = Phase.Ready;
        SetSwordDrawn(false);
    }

    // Keep the sword state and the visible hand/sheath models in sync.
    private void SetSwordDrawn(bool drawn)
    {
        SwordDrawn = drawn;
        swordInHand.SetActive(drawn);
        swordSheathed.SetActive(!drawn);
    }

    // Switch to the sword-in-hand model when the draw animation reaches its event.
    public void ShowDrawnSword()
    {
        if (phase == Phase.Drawing)
            SetSwordDrawn(true);
    }

    // Switch to the sheathed sword when the sheath animation reaches its event.
    public void ShowSheathedSword()
    {
        if (phase == Phase.Sheathing)
            SetSwordDrawn(false);
    }

    // Enable blade damage only during the active part of a sword attack.
    public void OpenHitWindow()
    {
        blade.enabled = SwordDrawn &&
            (phase == Phase.First || phase == Phase.Second);
    }

    // Disable blade damage between attack windows.
    public void CloseHitWindow()
    {
        blade.enabled = false;
    }

    // Calculate this frame's attack travel and jump movement for the CharacterController.
    public Vector3 GetAttackDisplacement(out bool drivesHeight)
    {
        drivesHeight = false;

        if (phase != Phase.First && phase != Phase.Second)
            return Vector3.zero;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0) || !state.IsName(activeState))
            return Vector3.zero;

        // Use clip progress so movement stays in sync when animation speed changes.
        float t = Mathf.Clamp01(state.normalizedTime);
        float travel;
        float lift = 0f;

        if (phase == Phase.First)
        {
            travel = firstAttackDistance * EaseBetween(0.18f, 0.60f, t);
        }
        else
        {
            travel = secondAttackDistance * EaseBetween(0.15f, 0.65f, t);

            // These fractions mark takeoff, peak height, and landing in the second attack clip.
            const float takeoff = 16f / 70f;
            const float apex = 26f / 70f;
            const float landing = 37f / 70f;

            if (!hopDecided && t >= takeoff)
            {
                hopDecided = true;
                // Check grounding once at takeoff so a mid-attack landing cannot start a new hop.
                canHop = controller.isGrounded;
            }

            if (canHop)
            {
                float rise = EaseBetween(takeoff, apex, t);
                float fall = EaseBetween(apex, landing, t);
                lift = secondAttackHeight * (rise - fall);

                // Include the last downward step before handing vertical movement back to normal gravity.
                drivesHeight = (t >= takeoff && t < landing) || previousLift > 0f;
            }
        }

        // Subtract the last travel and height values to apply only this frame's movement.
        Vector3 delta = attackForward * (travel - previousTravel)
            + Vector3.up * (lift - previousLift);

        previousTravel = travel;
        previousLift = lift;
        return delta;
    }

    // Map part of an animation to a smooth 0-to-1 progression for the attack movement.
    private static float EaseBetween(float start, float end, float t)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, t));
    }

    // Launch one fireball from the hand when the casting animation reaches its release event.
    public void ReleaseFireball()
    {
        if (phase != Phase.Casting || fireballReleased || movement.IsDead)
            return;

        fireballReleased = true;

        // Start slightly in front of the hand so the projectile clears the character.
        Vector3 position = fireballSpawn.position + attackForward * 0.2f;

        Fireball projectile = Instantiate(
            fireballPrefab,
            position,
            Quaternion.LookRotation(attackForward));

        projectile.Launch(attackForward, movement.transform);
    }

    // Stop the current action and return to a ready stance, keeping the current sword state.
    public void CancelCurrentAction()
    {
        SetSwordDrawn(SwordDrawn);
        WaitWithSwordDrawn(Phase.Ready);
    }
}