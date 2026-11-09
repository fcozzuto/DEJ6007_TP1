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
            other.gameObject.GetComponent<PlayerMovement>().Die();
        }
    }
}
