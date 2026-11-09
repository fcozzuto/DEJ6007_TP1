using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject[] Checkpoints;
    [SerializeField] private GameObject Player;
    [SerializeField] private Animator[] heartAnimators = new Animator[3];
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;

    private int lives = 3;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();

        if (!playerMovement.IsDead && Player.transform.position.y < -10f)
            playerMovement.Die(1f);
    }

    private void RespawnPlayer()
    {
        // Find the closest active checkpoint
        LoseLife();
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
                closestCheckpoint.transform.position,
                closestCheckpoint.transform.rotation);
            ZoomBehavior zoomBehavior = Camera.main.GetComponent<ZoomBehavior>();
            if (zoomBehavior != null)
                zoomBehavior.playerIsDead = false;
        }
    }

    public void RespawnWithDelay(float delay)
    {
        if (lives <= 1)
        {
            LoseLife();
            gameOverUI.SetActive(true);
            return;
        }

        Invoke(nameof(RespawnPlayer), delay);
    }

    private void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        heartAnimators[lives].SetTrigger("Life Lost");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}