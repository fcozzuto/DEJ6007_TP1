using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject Player;
    [SerializeField] private Animator[] heartAnimators = new Animator[3];
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private Checkpoint startingCheckpoint;

    private Checkpoint activeCheckpoint;

    private int lives = 3;
    private void Start()
    {
        ActivateCheckpoint(startingCheckpoint);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();

        if (!playerMovement.IsDead && Player.transform.position.y < -10f)
            playerMovement.Die(1f);
    }

    public void ActivateCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null || checkpoint == activeCheckpoint)
            return;

        activeCheckpoint = checkpoint;

        Debug.Log($"Checkpoint activated: {activeCheckpoint.name}");
    }

    private void RespawnPlayer()
    {
        if (activeCheckpoint == null)
        {
            Debug.LogError("Assign a starting checkpoint on GameManager.");
            return;
        }

        LoseLife();

        Player.GetComponent<PlayerMovement>().RespawnAt(
            activeCheckpoint.transform.position,
            activeCheckpoint.transform.rotation);

        ZoomBehavior zoomBehavior = Camera.main.GetComponent<ZoomBehavior>();
        if (zoomBehavior != null)
            zoomBehavior.playerIsDead = false;
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