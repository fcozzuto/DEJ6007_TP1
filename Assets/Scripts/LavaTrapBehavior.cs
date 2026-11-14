using UnityEngine;

public class LavaTrapBehavior : MonoBehaviour
{

    // No startup setup is needed for the lava trigger.
    void Start()
    {

    }

    // The lava reacts to trigger entry; there is no per-frame work here.
    void Update()
    {

    }
    // Kill the player on contact with the lava and let the GameManager handle the death delay.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered spike trap trigger, triggering Ragdoll and then respawn!");
            other.gameObject.GetComponent<PlayerMovement>().Die();
        }
    }
}
