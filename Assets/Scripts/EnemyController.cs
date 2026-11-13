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

    public bool IsDead { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
        SetRagdoll(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyPosition = this.gameObject.transform.position.x;
    }

    // Update is called once per frame
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

        if (!gameObject.name.Contains("Viking"))
        {
            UpdateSoldier();
            return;
        }

        if (playerDetected)
        {
            // Save the patrol position and heading once.
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

    private void ChasePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // Face the player, including when already close enough to stop.
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
        transform.Translate(Vector3.forward * step);
    }

    private void ReturnToPatrol()
    {
        Vector3 direction = patrolResumePosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.000001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            patrolResumePosition,
            2f * Time.deltaTime);

        animator.Play("Walk");

        if (transform.position == patrolResumePosition)
        {
            transform.position = patrolResumePosition;
            transform.rotation = patrolResumeRotation;
            patrolInterrupted = false;
        }
    }


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
        transform.position = Vector3.MoveTowards(
            transform.position, destination, approachSpeed * Time.deltaTime);
    }
    private void Patrol()
    {
        if (bossFightEnemy)
        {
            ApproachPlayer();
            return;
        }
        // The enemy will patrol by walking forward between two points, and when it reaches one of the points, it will turn around and walk to the other point
        this.gameObject.transform.Translate(Vector3.forward * Time.deltaTime * 2);
        animator.Play("Walk");

        // If the enemy reaches the end of the patrol path, it will turn around
        if (this.gameObject.transform.position.x <= enemyPosition - 10 && transform.forward.x < 0f)
        {
            this.gameObject.transform.Rotate(0, 180, 0);
        }
        else if (this.gameObject.transform.position.x >= enemyPosition && transform.forward.x > 0f)
        {
            this.gameObject.transform.Rotate(0, 180, 0);
        }

    }

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

    private void ResumeSoldierPatrol()
    {
        soldierState = SoldierState.Patrol;
        transform.rotation = patrolResumeRotation;
        Patrol();
    }

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

    private void SetRagdoll(bool active)
    {
        foreach (Rigidbody body in ragdollBodies)
        {
            // The root Rigidbody belongs to the living enemy's collider.
            if (body.gameObject == gameObject)
                continue;

            body.isKinematic = !active;
            body.useGravity = active;

            foreach (Collider boneCollider in body.GetComponents<Collider>())
                boneCollider.enabled = active;
        }
    }

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