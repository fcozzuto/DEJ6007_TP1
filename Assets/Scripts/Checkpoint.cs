using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null && !player.IsDead)
            gameManager.ActivateCheckpoint(this);
    }
}