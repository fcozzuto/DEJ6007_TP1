using UnityEngine;

public class TriggerCameraRotation180AndResetOffset : MonoBehaviour
{
    [SerializeField] private bool restorePreviousView = true;
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

    // Restore the selected side view, pause control for the turn, and re-enable the boss-view trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.gameObject.CompareTag("Player")) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player == null || player.IsDead)
            return;

        SmoothFollow follow = Camera.main.GetComponent<SmoothFollow>();
        follow.SetBossRoomView(!restorePreviousView, player);
        activated = true;
        GetComponent<Collider>().enabled = false;
        GameObject triggerArea = GameObject.Find("TriggerCameraRotation180AndCentering");
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
