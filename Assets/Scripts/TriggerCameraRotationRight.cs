using UnityEngine;

public class TriggerCameraRotationRight : MonoBehaviour
{
    public bool activated = false;

    // No startup setup is needed; the Inspector defines this camera trigger.
    void Start()
    {

    }

    // Clear the activation flag while this collider is disabled so the paired trigger can rearm it.
    void Update()
    {
        if (activated && !GetComponent<Collider>().enabled)
        {
            activated = false;
        }
    }

    // Rotate the camera and controls right, keep the model's facing, and arm the left trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.gameObject.CompareTag("Player")) return;

        Debug.Log("Player entered trigger, rotating camera to the right!");
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        player.RotateForCamera(-90f);
        Camera.main.GetComponent<SmoothFollow>().WaitForCamera(player);

        activated = true;
        GetComponent<Collider>().enabled = false;
        GameObject triggerArea = GameObject.Find("TriggerCameraRotationLeft");
        if (triggerArea != null)
        {
            Rearm(triggerArea);
        }
    }

    // Enable the paired zone's collider so it can handle the return trip.
    private static void Rearm(GameObject triggerArea)
    {
        Collider triggerCollider = triggerArea.GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}
