using UnityEngine;

public class TriggerCameraRotationLeft : MonoBehaviour
{
    private bool activated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (activated && !GetComponent<Collider>().enabled)
        {
            activated = false;
        }
    }

    // If the player enters the trigger, rotate the player to the right by 90 degrees
    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.gameObject.CompareTag("Player")) return;

        Debug.Log("Player entered trigger, rotating camera to the left!");
        // Rotate the player to the left by 90 degrees
        other.gameObject.transform.Rotate(0, 90, 0);
        activated = true;
        GetComponent<Collider>().enabled = false;
        GameObject triggerArea = GameObject.Find("TriggerCameraRotationRight");
        if (triggerArea != null)
        {
            Rearm(triggerArea);
        }
    }

    private static void Rearm(GameObject triggerArea)
    {
        Collider triggerCollider = triggerArea.GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}
