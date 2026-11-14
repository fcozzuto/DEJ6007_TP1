using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ZoomBehavior : MonoBehaviour
{
    [SerializeField] private float deathFieldOfView = 35f;
    [SerializeField, Min(0.01f)] private float zoomDuration = 2f;

    public bool playerIsDead = false;

    private Camera viewCamera;
    public float normalFieldOfView;

    // Cache the camera and remember its normal field of view for the return from the death zoom.
    private void Awake()
    {
        viewCamera = GetComponent<Camera>();

        normalFieldOfView = viewCamera.fieldOfView;
    }

    // Zoom in while the player is dead and move back toward the normal field of view after respawning.
    private void LateUpdate()
    {
        float targetFieldOfView = playerIsDead
            ? deathFieldOfView
            : normalFieldOfView;

        // Convert duration to degrees per second for a steady zoom in either direction.
        float zoomSpeed =
            Mathf.Abs(normalFieldOfView - deathFieldOfView)
            / Mathf.Max(0.01f, zoomDuration);

        viewCamera.fieldOfView = Mathf.MoveTowards(
            viewCamera.fieldOfView,
            targetFieldOfView,
            zoomSpeed * Time.deltaTime);
    }
}