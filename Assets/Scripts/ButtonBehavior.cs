using System;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        // If the Player touches the button, the button will move down and the hidden platform will activate
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Button pressed!");
            // Move the button down
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z);
            // Activate the hidden platform
            GameObject hiddenPlatform = GameObject.Find("HiddenPlatform");
            if (hiddenPlatform != null)
            {
                hiddenPlatform.SetActive(true);
            }
        }
    }*/
    private void OnTriggerEnter(Collider other)
    {
        // If the Player touches the button, the button will move down and the hidden platform will activate
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Button pressed!");
            // Move the button down slowly using an animation
            gameObject.GetComponent<Animator>().SetTrigger("PressButton");

            //transform.position = new Vector3(transform.position.x, transform.position.y - 0.4f, transform.position.z);
            // Activate the hidden platform
            GameObject hiddenPlatform = GameObject.FindGameObjectWithTag("HiddenPlatform");
            if (hiddenPlatform != null)
            {
                Debug.Log("Hidden platform exists!");
                hiddenPlatform.gameObject.GetComponentInChildren<Renderer>().enabled = true;
                hiddenPlatform.gameObject.GetComponentInChildren<Collider>().enabled = true;
                // Disable the button's collider so it can't be pressed again
                gameObject.GetComponentInChildren<Collider>().enabled = false;
            }
        }
    }
}
