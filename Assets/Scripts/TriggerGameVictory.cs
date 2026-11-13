using UnityEngine;

public class TriggerGameVictory : MonoBehaviour
{
    [SerializeField] private GameManager GameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
