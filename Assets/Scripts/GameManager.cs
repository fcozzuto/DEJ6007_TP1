using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject[] Checkpoints;
    [SerializeField] private GameObject Player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.transform.position.y < -10f)
        {
            Debug.Log("Player fell below -10, respawning...");
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        // Find the closest active checkpoint
        GameObject closestCheckpoint = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject checkpoint in Checkpoints)
        {
            float distance = Vector3.Distance(Player.transform.position, checkpoint.transform.position);
            Debug.Log($"Distance to checkpoint {checkpoint.name}: {distance}");
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCheckpoint = checkpoint;
            }
        }

        // Respawn the player at the closest checkpoint
        if (closestCheckpoint != null)
        {
            Debug.Log($"Respawning player at checkpoint {closestCheckpoint.name}");
            Player.GetComponent<PlayerMovement>().RespawnAt(
                closestCheckpoint.transform.position);
        }
    }
}