using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    // Make this the active respawn checkpoint when a living player reaches it.
    private void OnTriggerEnter(Collider other)
    {
        // Find the player on the parents as well, since the collider may be on a child.
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null && !player.IsDead)
            gameManager.ActivateCheckpoint(this);
    }
}