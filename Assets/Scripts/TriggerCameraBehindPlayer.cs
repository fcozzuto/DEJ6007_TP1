using UnityEngine;

public class TriggerCameraBehindPlayer : MonoBehaviour
{
    [SerializeField] private Collider restoreTrigger;

    public void SetUnlocked(bool unlocked)
    {
        // Unlock only the entrance zone; the restore zone is armed after entering it.
        GetComponent<Collider>().enabled = unlocked;

        if (restoreTrigger != null)
            restoreTrigger.enabled = false;
    }

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