using UnityEngine;

public class CreditsEvents : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    // Return to the main menu when the scrolling credits animation reaches its end event.
    public void CreditsFinished()
    {
        gameManager.OpenMainMenu();
    }
}