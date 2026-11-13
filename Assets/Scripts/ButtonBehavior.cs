using System;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [SerializeField] private Animator platformsAnimator;
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
        // If the Player touches the button, the button will move down and the hidden platform will activate
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Button pressed!");
            // Move the button down slowly using an animation
            if (this.gameObject.name == "Button Hidden Platforms")
            {
                this.gameObject.GetComponent<Animator>().Play("PressPlatformButton");
                // Activate the hidden platforms
                GameObject[] hiddenPlatforms = GameObject.FindGameObjectsWithTag("HiddenPlatform");
                foreach (GameObject hiddenPlatform in hiddenPlatforms)
                {
                    Debug.Log("Hidden platform exists!");
                    hiddenPlatform.gameObject.GetComponentInChildren<Renderer>().enabled = true;
                    hiddenPlatform.gameObject.GetComponentInChildren<Collider>().enabled = true;
                    // Disable the button's collider so it can't be pressed again
                }
                platformsAnimator.Play("Hidden Platforms Entrance motion");
                this.gameObject.GetComponentInChildren<Collider>().enabled = false;
            }
            else if (this.gameObject.name == "Button Hidden Door")
            {
                this.gameObject.GetComponent<Animator>().Play("PressHiddenDoorButton");
                GameObject fakeWall = GameObject.Find("Fake Wall");
                if (fakeWall != null)
                {
                    Debug.Log("Fake wall exists!");
                    fakeWall.gameObject.GetComponentInChildren<Animator>().SetTrigger("HiddenDoorFound");
                    this.gameObject.GetComponentInChildren<Collider>().enabled = false;
                }
                else
                {
                    Debug.LogWarning("Fake wall not found!");
                }
                GameObject fakeBrazier = GameObject.Find("FakeBrazier");
                if (fakeBrazier != null)
                {
                    Debug.Log("Fake brazier exists!");
                    Destroy(fakeBrazier);
                }
                else
                {
                    Debug.LogWarning("Fake brazier not found!");
                }
            }
        }
    }
}
