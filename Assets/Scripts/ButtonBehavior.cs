using System;
using UnityEngine;

public class ButtonBehavior : MonoBehaviour
{
    [SerializeField] private Animator platformsAnimator;

    // No startup setup is needed for these floor buttons.
    void Start()
    {

    }

    // The buttons react to trigger entry; there is no per-frame work here.
    void Update()
    {

    }
    // Play the pressed button animation, then reveal the platforms or open the hidden passage.
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Button pressed!");

            if (this.gameObject.name == "Button Hidden Platforms")
            {
                this.gameObject.GetComponent<Animator>().Play("PressPlatformButton");

                // The platforms are already in the scene; reveal their renderers and enable their colliders.
                GameObject[] hiddenPlatforms = GameObject.FindGameObjectsWithTag("HiddenPlatform");
                foreach (GameObject hiddenPlatform in hiddenPlatforms)
                {
                    Debug.Log("Hidden platform exists!");
                    hiddenPlatform.gameObject.GetComponentInChildren<Renderer>().enabled = true;
                    hiddenPlatform.gameObject.GetComponentInChildren<Collider>().enabled = true;

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
                // Remove the brazier that would otherwise block the newly opened passage.
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
