using UnityEngine;

public class TriggerCameraRestorePosition : MonoBehaviour
{
    [SerializeField] private Collider behindPlayerTrigger;

    // Restore the saved side view, disable this zone, and re-enable entry to the rear view.
    private void OnTriggerEnter(Collider other)
    {
        if (!GetComponent<Collider>().enabled)
            return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player == null || player.IsDead)
            return;

        if (behindPlayerTrigger == null)
        {
            Debug.LogError("Assign the rear camera trigger's collider.", this);
            return;
        }

        Camera.main.GetComponent<SmoothFollow>().SetBehindPlayerView(false, player);

        GetComponent<Collider>().enabled = false;
        behindPlayerTrigger.enabled = true;
    }
}