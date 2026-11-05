using System;
using UnityEngine;

public class SpikeTrapBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered spike trap trigger, triggering Ragdoll and then respawn!");
            PlayerDeath(other);
        }
    }

    private static void PlayerDeath(Collider other)
    {
        // Call the Ragdoll function on the player and then respawn the player after 3 seconds
        other.gameObject.GetComponentInChildren<Animator>().enabled = false;
        other.gameObject.GetComponent<PlayerMovement>().PausePlayerController();
        ZoomBehavior zoomBehavior = Camera.main.GetComponent<ZoomBehavior>();
        if (zoomBehavior != null)
            zoomBehavior.playerIsDead = true;
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
            gameManager.GetComponent<GameManager>().RespawnWithDelay(3f);
    }
}
