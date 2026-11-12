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

    public bool IsBusy => phase != Phase.Ready && phase != Phase.ReadyForSecond;
    public bool SwordDrawn { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = movement.GetComponent<CharacterController>();
        ResetCombat();
    }

    private void Start()
    {
        attackAction = InputSystem.actions.FindAction("Player/Attack", true);
        fireballAction = InputSystem.actions.FindAction("Player/Fireball", true);
    }

    private void Update()
    {
        if (movement.IsDead || !controller.enabled)
            return;

        bool pressed = attackAction.WasPressedThisFrame();

        if (!IsBusy)
        {
            if (!controller.isGrounded)
                return;

            if (fireballAction.WasPressedThisFrame())
            {
                attackForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                fireballReleased = false;
                queuedAttacks = 0;

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

        if (pressed && phase == Phase.Drawing)
            queuedAttacks = Mathf.Min(queuedAttacks + 1, 2);
        else if (pressed && phase == Phase.First)
            queuedAttacks = 1;

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

    private void BeginSwing(Phase next, string stateName)
    {
        attackForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Begin(next, stateName);
    }

    private void WaitWithSwordDrawn(Phase next)
    {
        CloseHitWindow();
        queuedAttacks = 0;
        phase = next;
        sheathAt = Time.time + 3f;
    }

    public void ResetCombat()
    {
        CloseHitWindow();
        queuedAttacks = 0;
        phase = Phase.Ready;
        SetSwordDrawn(false);
    }

    private void SetSwordDrawn(bool drawn)
    {
        SwordDrawn = drawn;
        swordInHand.SetActive(drawn);
        swordSheathed.SetActive(!drawn);
    }

    public void ShowDrawnSword()
    {
        if (phase == Phase.Drawing)
            SetSwordDrawn(true);
    }

    public void ShowSheathedSword()
    {
        if (phase == Phase.Sheathing)
            SetSwordDrawn(false);
    }

    public void OpenHitWindow()
    {
        blade.enabled = SwordDrawn &&
            (phase == Phase.First || phase == Phase.Second);
    }

    public void CloseHitWindow()
    {
        blade.enabled = false;
    }

    public Vector3 GetAttackDisplacement(out bool drivesHeight)
    {
        drivesHeight = false;

        if (phase != Phase.First && phase != Phase.Second)
            return Vector3.zero;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0) || !state.IsName(activeState))
            return Vector3.zero;

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

            const float takeoff = 16f / 70f;
            const float apex = 26f / 70f;
            const float landing = 37f / 70f;

            if (!hopDecided && t >= takeoff)
            {
                hopDecided = true;
                canHop = controller.isGrounded;
            }

            if (canHop)
            {
                float rise = EaseBetween(takeoff, apex, t);
                float fall = EaseBetween(apex, landing, t);
                lift = secondAttackHeight * (rise - fall);

                // Include the final downward step.
                drivesHeight = (t >= takeoff && t < landing) || previousLift > 0f;
            }
        }

        Vector3 delta = attackForward * (travel - previousTravel)
            + Vector3.up * (lift - previousLift);

        previousTravel = travel;
        previousLift = lift;
        return delta;
    }

    private static float EaseBetween(float start, float end, float t)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, t));
    }

    public void ReleaseFireball()
    {
        if (phase != Phase.Casting || fireballReleased || movement.IsDead)
            return;

        fireballReleased = true;

        Vector3 position = fireballSpawn.position + attackForward * 0.2f;

        Fireball projectile = Instantiate(
            fireballPrefab,
            position,
            Quaternion.LookRotation(attackForward));

        projectile.Launch(attackForward, movement.transform);
    }

    public void CancelCurrentAction()
    {
        SetSwordDrawn(SwordDrawn);
        WaitWithSwordDrawn(Phase.Ready);
    }
}