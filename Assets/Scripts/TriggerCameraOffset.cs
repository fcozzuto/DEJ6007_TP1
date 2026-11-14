using UnityEngine;

public class TriggerCameraOffset : MonoBehaviour
{
    private bool activated = false;

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

    // Restore the side-scrolling offset and near plane, then enable the centering trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.gameObject.CompareTag("Player")) return;

        Debug.Log("Player entered trigger, restoring side follow!");
        Camera.main.GetComponent<SmoothFollow>().offset = new Vector3(3, 3.9f, -5.61f);
        Camera.main.nearClipPlane = 4.1f;
        activated = true;
        GetComponent<Collider>().enabled = false;
        GameObject triggerArea = GameObject.Find("TriggerCameraCentering");
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
