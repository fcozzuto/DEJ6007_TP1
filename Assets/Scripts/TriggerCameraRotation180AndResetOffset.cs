using UnityEngine;

public class TriggerCameraRotation180AndResetOffset : MonoBehaviour
{
    [SerializeField] private bool restorePreviousView = true;
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

    private static void Rearm(GameObject triggerArea)
    {
        Collider triggerCollider = triggerArea.GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }
}
