using UnityEngine;

public class TriggerCameraCentering : MonoBehaviour
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

    // If the player enters the trigger, center the camera behind the player
    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.gameObject.CompareTag("Player")) return;

        Debug.Log("Player entered trigger, centering camera behind the player!");
        Camera.main.GetComponent<SmoothFollow>().offset = new Vector3(0, 3.9f, -5.61f);
        activated = true;
        GetComponent<Collider>().enabled = false;
        GameObject triggerArea = GameObject.Find("TriggerCameraOffset");
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
