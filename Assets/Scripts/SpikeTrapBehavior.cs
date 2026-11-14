using System;
using UnityEngine;

public class SpikeTrapBehavior : MonoBehaviour
{

    // No startup setup is needed for the spike trigger.
    void Start()
    {

    }

    // The spikes react to trigger entry; there is no per-frame work here.
    void Update()
    {

    }

    // Kill the player on contact with the spikes and let the GameManager handle the death delay.
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered spike trap trigger, triggering Ragdoll and then respawn!");
            other.gameObject.GetComponent<PlayerMovement>().Die();
        }
    }
}
