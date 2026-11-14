using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject Player;
    [SerializeField] private Animator[] heartAnimators = new Animator[3];
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private Checkpoint startingCheckpoint;

    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject controlsUI;
    [SerializeField] private GameObject creditsUI;

    private static int resumeFrame = -1;
    private static bool restartIntoGameplay;

    // Block controls while paused and for the frame in which the menu closes.
    public static bool GameplayInputBlocked =>
        Time.timeScale == 0f || Time.frameCount == resumeFrame;

    private Checkpoint activeCheckpoint;

    private int lives = 3;
    // Set the initial spawn, then show the menu or continue directly after a restart.
    private void Start()
    {
        if (startingCheckpoint == null)
        {
            Debug.LogError("Assign a starting checkpoint on GameManager.");
            return;
        }

        ActivateCheckpoint(startingCheckpoint);

        Player.GetComponent<PlayerMovement>().RespawnAt(
            startingCheckpoint.transform.position,
            startingCheckpoint.transform.rotation);

        Camera.main.GetComponent<SmoothFollow>().SnapToTarget();

        resumeFrame = -1;
        // The flag survives the reload so Retry can skip the title menu.
        if (restartIntoGameplay)
        {
            restartIntoGameplay = false;
            ResumeGame();
        }
        else
        {
            OpenMainMenu();
        }
    }

    // Handle menu input and selection, then check for a living player falling out of the level.
    void Update()
    {
        bool menuPressed =
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

        // The end screens cannot be dismissed by the normal pause-menu shortcut.
        if (!gameOverUI.activeSelf && !victoryUI.activeSelf)
        {
            if (menuPressed)
            {
                if (mainMenuUI.activeSelf)
                    ResumeGame();
                else
                    OpenMainMenu();
            }
            else if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame &&
                     (controlsUI.activeSelf || creditsUI.activeSelf))
            {
                OpenMainMenu();
            }
        }

        if (GameplayInputBlocked)
        {
            SelectMenuButton();
            return;
        }

        PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();

        if (!playerMovement.IsDead && Player.transform.position.y < -10f)
            playerMovement.Die(1f);
    }

    // Close the submenus and show the main menu with gameplay paused.
    public void OpenMainMenu()
    {
        controlsUI.SetActive(false);
        creditsUI.SetActive(false);
        mainMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    // Hide the menu panels and resume gameplay without treating the closing click as a gameplay input.
    public void ResumeGame()
    {
        mainMenuUI.SetActive(false);
        controlsUI.SetActive(false);
        creditsUI.SetActive(false);
        Time.timeScale = 1f;
        // Block this frame's UI click or A press so it cannot also attack or jump.
        resumeFrame = Time.frameCount;
    }

    // Select a visible menu button when the previous selection is missing or belongs to a hidden panel.
    private void SelectMenuButton()
    {
        EventSystem events = EventSystem.current;
        if (events == null)
            return;

        GameObject selected = events.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy)
            return;

        // All panels share the UI parent; this search skips the inactive panels.
        Button button = mainMenuUI.transform.parent.GetComponentInChildren<Button>();
        if (button != null)
            button.Select();
    }

    // Remember the last checkpoint reached, including one revisited while backtracking.
    public void ActivateCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null || checkpoint == activeCheckpoint)
            return;

        activeCheckpoint = checkpoint;

        Debug.Log($"Checkpoint activated: {activeCheckpoint.name}");
    }

    // Consume a life, move the player to the active checkpoint, and reset the death zoom.
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

    // Wait for the death effect before respawning, or show Game Over if no lives remain.
    public void RespawnWithDelay(float delay)
    {
        if (lives <= 1)
        {
            LoseLife();
            // Keep time running through the final ragdoll fall instead of freezing the player on impact.
            Invoke(nameof(ShowGameOver), delay);
            return;
        }

        Invoke(nameof(RespawnPlayer), delay);
    }

    // Show the Game Over screen after the final death and select a button while the world keeps running.
    private void ShowGameOver()
    {
        gameOverUI.SetActive(true);
        SelectMenuButton();
    }

    // Remove one life and trigger the matching heart animation without going below zero.
    private void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        // Decrement first: the remaining life count is the index of the heart that was just lost.
        heartAnimators[lives].SetTrigger("Life Lost");
    }

    // Reload the current scene and start the new run directly instead of showing the title menu again.
    public void RestartLevel()
    {
        restartIntoGameplay = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Close the built game when the player chooses Exit.
    public void QuitGame()
    {
        Application.Quit();
    }

    // Stop player control, show the Victory screen, and pause the completed game.
    public void WinGame()
    {
        Player.GetComponent<PlayerMovement>().PausePlayerController();

        victoryUI.SetActive(true);

        Time.timeScale = 0f;
    }
}