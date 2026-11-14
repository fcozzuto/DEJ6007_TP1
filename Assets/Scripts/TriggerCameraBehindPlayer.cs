using UnityEngine;

public class TriggerCameraBehindPlayer : MonoBehaviour
{
    [SerializeField] private Collider restoreTrigger;

    // Lock or unlock entry to the ending view, keeping the restore zone disabled until entry.
    public void SetUnlocked(bool unlocked)
    {

        GetComponent<Collider>().enabled = unlocked;

        if (restoreTrigger != null)
            restoreTrigger.enabled = false;
    }

    // Switch to the rear camera view, disable this zone, and enable the zone that restores the side view.
    private void OnTriggerEnter(Collider other)
    {
        if (!GetComponent<Collider>().enabled)
            return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player == null || player.IsDead)
            return;

        if (restoreTrigger == null)
        {
            Debug.LogError("Assign the restore camera trigger's collider.", this);
            return;
        }

        Camera.main.GetComponent<SmoothFollow>().SetBehindPlayerView(true, player);

        GetComponent<Collider>().enabled = false;
        restoreTrigger.enabled = true;
    }
}