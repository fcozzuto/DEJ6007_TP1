using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private enum SoldierState
    {
        Patrol,
        Idle,
        Unsheath,
        Attack,
        Sheath
    }

    [SerializeField, Min(0f)] private float soldierIdleDuration = 0.5f;
    private SoldierState soldierState = SoldierState.Patrol;
    private float soldierIdleEndTime;
    private Animator animator;

    private bool playerDetected = false;
    private float enemyPosition;

    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private bool bossFightEnemy;
    [SerializeField] private float approachSpeed = 2f;
    [SerializeField] private FlyingDagger daggerPrefab;
    [SerializeField] private MeshRenderer heldDagger;

    private bool patrolInterrupted = false;
    private Vector3 patrolResumePosition;
    private Quaternion patrolResumeRotation;
    private Rigidbody[] ragdollBodies;
    private Rigidbody body;
    [SerializeField, Min(0f)] private float enemyGap = 0.1f;

    public bool IsDead { get; private set; }

    // Cache the enemy components and keep the ragdoll inactive while the enemy is alive.
    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        SetRagdoll(false);
    }

    // Remember the starting X position used as one end of the regular patrol route.
    void Start()
    {
        enemyPosition = this.gameObject.transform.position.x;
    }

    // Detect the player and choose between the soldier attack cycle, a Viking charge, or patrol movement.
    void Update()
    {
        if (IsDead)
            return;

        playerDetected = false;

        if (player != null)
        {
            Vector3 offset = player.position - transform.position;
            offset.y = 0f;

            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            playerDetected = playerMovement != null &&
                !playerMovement.IsDead &&
                offset.magnitude <= detectionRange;
        }

        // Names containing Viking use the charge behavior; the others use the soldier cycle.
        if (!gameObject.name.Contains("Viking"))
        {
            UpdateSoldier();
            return;
        }

        if (playerDetected)
        {

            // Save the return point once at the start of the chase, rather than moving it each frame.
            if (!bossFightEnemy && !patrolInterrupted)
            {
                patrolResumePosition = transform.position;
                patrolResumeRotation = transform.rotation;
                patrolInterrupted = true;
            }

            ChasePlayer();
        }
        else if (bossFightEnemy)
        {
            detectionRange = 10f;
            ApproachPlayer();
        }
        else if (patrolInterrupted)
        {
            ReturnToPatrol();
        }
        else
        {
            Patrol();
        }
    }

    // Move toward the requested position while leaving space around other living enemies.
    private void MoveEnemy(Vector3 displacement)
    {
        float distance = displacement.magnitude;

        if (distance <= 0.000001f)
            return;

        Vector3 direction = displacement / distance;
        float allowedDistance = distance;

        // Movement uses Transforms, so refresh the physics positions before checking the next step.
        Physics.SyncTransforms();

        RaycastHit[] hits = body.SweepTestAll(
            direction, distance + enemyGap, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            EnemyController otherEnemy =
                hit.collider.GetComponentInParent<EnemyController>();

            // This check only blocks other living enemies, allowing movement past dead bodies.
            if (otherEnemy == null || otherEnemy == this || otherEnemy.IsDead)
                continue;

            // Hits can arrive out of order, so keep the shortest allowed step and leave a small gap.
            allowedDistance = Mathf.Min(
                allowedDistance, Mathf.Max(0f, hit.distance - enemyGap));
        }

        transform.position += direction * allowedDistance;
    }

    // Face the player and charge until the Viking reaches its stopping distance.
    private void ChasePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        float distanceToWalk = distance - stoppingDistance;

        if (distanceToWalk <= 0.01f)
        {
            animator.Play("Idle");
            return;
        }

        animator.Play("Viking Charge");

        float step = Mathf.Min(chaseSpeed * Time.deltaTime, distanceToWalk);
        MoveEnemy(transform.forward * step);
    }

    // Walk back to the point where the Viking left its patrol and restore the original heading.
    private void ReturnToPatrol()
    {
        Vector3 direction = patrolResumePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.000001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            patrolResumePosition,
            2f * Time.deltaTime);

        MoveEnemy(nextPosition - transform.position);

        animator.Play("Walk");

        if (transform.position == patrolResumePosition)
        {
            transform.position = patrolResumePosition;
            transform.rotation = patrolResumeRotation;
            patrolInterrupted = false;
        }
    }

    // Walk a boss-wave enemy toward the player while keeping it at its current height.
    private void ApproachPlayer()
    {
        if (player == null || player.GetComponent<PlayerMovement>().IsDead)
        {
            animator.Play("Idle");
            return;
        }

        Vector3 destination = player.position;
        destination.y = transform.position.y;
        Vector3 direction = destination - transform.position;

        if (direction.sqrMagnitude > 0.000001f)
            transform.rotation = Quaternion.LookRotation(direction);

        animator.Play("Walk");
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position, destination, approachSpeed * Time.deltaTime);

        MoveEnemy(nextPosition - transform.position);
    }
    // Approach the player in boss mode; otherwise walk back and forth along the regular patrol route.
    private void Patrol()
    {
        if (bossFightEnemy)
        {
            ApproachPlayer();
            return;
        }

        MoveEnemy(transform.forward * (2f * Time.deltaTime));
        animator.Play("Walk");

        // Check the heading as well as the position to avoid flipping again on the next frame.
        if (this.gameObject.transform.position.x <= enemyPosition - 10 && transform.forward.x < 0f)
        {
            this.gameObject.transform.Rotate(0, 180, 0);
        }
        else if (this.gameObject.transform.position.x >= enemyPosition && transform.forward.x > 0f)
        {
            this.gameObject.transform.Rotate(0, 180, 0);
        }

    }

    // Kill the player on contact while this enemy is alive and the player controller is active.
    private void OnTriggerEnter(Collider other)
    {
        if (IsDead)
            return;

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        CharacterController playerController =
            playerMovement.GetComponent<CharacterController>();
        if (!playerController.enabled)
            return;

        playerMovement.Die();
    }

    // Run the soldier through its wait, draw, throw, and sheath animations according to player detection.
    private void UpdateSoldier()
    {
        if (playerDetected && soldierState != SoldierState.Patrol &&
            soldierState != SoldierState.Sheath)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        AnimatorStateInfo animation = animator.GetCurrentAnimatorStateInfo(0);

        switch (soldierState)
        {
            case SoldierState.Patrol:
                if (!playerDetected)
                {
                    Patrol();
                    return;
                }

                patrolResumeRotation = transform.rotation;
                soldierIdleEndTime = Time.time + soldierIdleDuration;
                soldierState = SoldierState.Idle;
                animator.Play("Base Layer.Idle", 0, 0f);
                break;

            case SoldierState.Idle:
                if (!playerDetected)
                {
                    ResumeSoldierPatrol();
                }
                else if (Time.time >= soldierIdleEndTime)
                {
                    soldierState = SoldierState.Unsheath;
                    animator.Play("Base Layer.Soldier Unsheat Weapon", 0, 0f);
                }
                break;

            case SoldierState.Unsheath:
                if (!animation.IsName("Base Layer.Soldier Unsheat Weapon"))
                    return;

                if (!playerDetected)
                {
                    soldierState = SoldierState.Sheath;
                    animator.Play("Base Layer.Soldier Sheat Weapon", 0,
                        // Start the sheath animation at the matching point when the player leaves during the
                        // draw.
                        1f - Mathf.Clamp01(animation.normalizedTime));
                }
                else if (animation.normalizedTime >= 1f)
                {
                    soldierState = SoldierState.Attack;
                    animator.Play("Base Layer.Soldier Attack", 0, 0f);
                }
                break;

            case SoldierState.Attack:
                if (!animation.IsName("Base Layer.Soldier Attack") ||
                    animation.normalizedTime < 1f)
                    return;

                if (playerDetected)
                {
                    soldierState = SoldierState.Unsheath;
                    animator.Play("Base Layer.Soldier Unsheat Weapon", 0, 0f);
                }
                else
                {
                    ResumeSoldierPatrol();
                }
                break;

            case SoldierState.Sheath:
                if (animation.IsName("Base Layer.Soldier Sheat Weapon") &&
                    animation.normalizedTime >= 1f)
                    ResumeSoldierPatrol();
                break;
        }
    }

    // Leave the soldier attack cycle and resume movement with the saved patrol heading.
    private void ResumeSoldierPatrol()
    {
        soldierState = SoldierState.Patrol;
        transform.rotation = patrolResumeRotation;
        Patrol();
    }

    // Spawn and launch a dagger toward the player when the soldier attack animation calls its event.
    public void ThrowDagger()
    {
        if (IsDead
            || soldierState != SoldierState.Attack
            || player == null
            || player.GetComponent<PlayerMovement>().IsDead)
            return;

        Vector3 spawnPosition = heldDagger.bounds.center;
        Vector3 targetPosition = player.position + Vector3.up;
        Vector3 direction = targetPosition - spawnPosition;

        if (direction.sqrMagnitude < 0.000001f)
            return;

        FlyingDagger dagger = Instantiate(
            daggerPrefab, spawnPosition, Quaternion.LookRotation(direction));

        dagger.Launch(direction, transform);
    }

    // Switch the bone physics and colliders between animated movement and the death ragdoll.
    private void SetRagdoll(bool active)
    {
        foreach (Rigidbody body in ragdollBodies)
        {

            // Leave the living enemy's root Rigidbody alone; only the bones become dynamic.
            if (body.gameObject == gameObject)
                continue;

            body.isKinematic = !active;
            body.useGravity = active;

            foreach (Collider boneCollider in body.GetComponents<Collider>())
                boneCollider.enabled = active;
        }
    }

    // Stop the enemy, enable its ragdoll, and remove the body after two seconds.
    public void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        animator.enabled = false;

        foreach (Collider rootCollider in GetComponents<Collider>())
            rootCollider.enabled = false;

        Physics.SyncTransforms();
        SetRagdoll(true);

        Destroy(gameObject, 2f);
    }
}