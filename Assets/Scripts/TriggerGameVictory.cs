using UnityEngine;

public class TriggerGameVictory : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;

    // No startup setup is needed; the GameManager reference is assigned in the Inspector.
    void Start()
    {

    }

    // Victory is handled on trigger entry, so there is no per-frame work here.
    void Update()
    {

    }

    // Ask the GameManager to show Victory when the player reaches the exit.
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager != null)
            {
                GameManager.WinGame();
            }
        }
    }
}
