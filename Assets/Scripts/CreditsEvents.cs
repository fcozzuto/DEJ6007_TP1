using UnityEngine;

public class CreditsEvents : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public void CreditsFinished()
    {
        gameManager.OpenMainMenu();
    }
}