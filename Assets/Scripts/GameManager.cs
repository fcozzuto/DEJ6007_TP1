using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    public static bool GameplayInputBlocked =>
        Time.timeScale == 0f || Time.frameCount == resumeFrame;

    private Checkpoint activeCheckpoint;

    private int lives = 3;
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

    // Update is called once per frame
    void Update()
    {
        bool menuPressed =
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

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

    public void OpenMainMenu()
    {
        controlsUI.SetActive(false);
        creditsUI.SetActive(false);
        mainMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        mainMenuUI.SetActive(false);
        controlsUI.SetActive(false);
        creditsUI.SetActive(false);
        Time.timeScale = 1f;
        resumeFrame = Time.frameCount; // The closing UI click must not also attack or jump.
    }

    private void SelectMenuButton()
    {
        EventSystem events = EventSystem.current;
        if (events == null)
            return;

        GameObject selected = events.currentSelectedGameObject;
        if (selected != null && selected.activeInHierarchy)
            return;

        // All your menu panels share the same UI parent.
        Button button = mainMenuUI.transform.parent.GetComponentInChildren<Button>();
        if (button != null)
            button.Select();
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
            Invoke(nameof(ShowGameOver), delay);
            return;
        }

        Invoke(nameof(RespawnPlayer), delay);
    }

    private void ShowGameOver()
    {
        gameOverUI.SetActive(true);
        SelectMenuButton();
    }

    private void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        heartAnimators[lives].SetTrigger("Life Lost");
    }

    public void RestartLevel()
    {
        restartIntoGameplay = true;
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void WinGame()
    {
        Player.GetComponent<PlayerMovement>().PausePlayerController();

        victoryUI.SetActive(true);

        Time.timeScale = 0f;
    }
}