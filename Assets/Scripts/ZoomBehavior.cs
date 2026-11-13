using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ZoomBehavior : MonoBehaviour
{
    [SerializeField] private float deathFieldOfView = 35f;
    [SerializeField, Min(0.01f)] private float zoomDuration = 2f;

    public bool playerIsDead = false;

    private Camera viewCamera;
    public float normalFieldOfView;

    private void Awake()
    {
        viewCamera = GetComponent<Camera>();

        // Remember the camera's normal Inspector setting.
        normalFieldOfView = viewCamera.fieldOfView;
    }

    private void LateUpdate()
    {
        float targetFieldOfView = playerIsDead
            ? deathFieldOfView
            : normalFieldOfView;

        // Degrees per second: complete either transition in zoomDuration.
        float zoomSpeed =
            Mathf.Abs(normalFieldOfView - deathFieldOfView)
            / Mathf.Max(0.01f, zoomDuration);

        viewCamera.fieldOfView = Mathf.MoveTowards(
            viewCamera.fieldOfView,
            targetFieldOfView,
            zoomSpeed * Time.deltaTime);
    }
}