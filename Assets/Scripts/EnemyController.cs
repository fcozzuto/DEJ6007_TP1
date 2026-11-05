using System;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    private bool playerDetected = false;
    private float enemyPosition;

    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float stoppingDistance = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyPosition = this.gameObject.transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        playerDetected = player != null && Vector3.Distance(this.gameObject.transform.position, player.position) <= detectionRange;
        // Until the player is detected, the enemy will patrol
        if (!playerDetected)
        {
            Patrol();
        }
        else
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        float offsetX = player.position.x - this.gameObject.transform.position.x;
        float distanceToWalk = Mathf.Abs(offsetX) - stoppingDistance;

        if (distanceToWalk <= 0.01f)
        {
            // Stop moving if the player is within the stopping distance
            this.gameObject.GetComponent<Animator>().Play("Idle");
            return;
        }

        float facingAngle = offsetX > 0 ? 90f : -90f;
        this.transform.rotation = Quaternion.Euler(0, facingAngle, 0);

        if (this.gameObject.name == "Viking")
        {
            this.gameObject.GetComponent<Animator>().Play("Viking Charge");
        }
        else
        {
            this.gameObject.GetComponent<Animator>().Play("Run");
        }
        float step = Mathf.Min(chaseSpeed * Time.deltaTime, distanceToWalk);
        this.gameObject.transform.Translate(Vector3.forward * step);
    }

    private void Patrol()
    {
        // The enemy will patrol by walking forward between two points, and when it reaches one of the points, it will turn around and walk to the other point
        this.gameObject.transform.Translate(Vector3.forward * Time.deltaTime * 2);
        this.gameObject.GetComponent<Animator>().Play("Walk");

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Debug.Log("Player collided with enemy, triggering Ragdoll and then respawn!");
            PlayerDeath(collision.collider);
        }
    }

    private static void PlayerDeath(Collider other)
    {
        // Call the Ragdoll function on the player and then respawn the player after 3 seconds
        other.gameObject.GetComponentInChildren<Animator>().enabled = false;
        other.gameObject.GetComponent<PlayerMovement>().PausePlayerController();
        ZoomBehavior zoomBehavior = Camera.main.GetComponent<ZoomBehavior>();
        if (zoomBehavior != null)
            zoomBehavior.playerIsDead = true;
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
            gameManager.GetComponent<GameManager>().RespawnWithDelay(3f);
    }
}
