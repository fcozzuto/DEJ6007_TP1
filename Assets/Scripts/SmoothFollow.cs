using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    /*public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }*/
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 5f;

    private Quaternion rotationOffset;

    private void Start()
    {
        if (target == null) return;

        // Preserve the camera's initial angle relative to the player.
        rotationOffset = Quaternion.Inverse(target.rotation)
            * transform.rotation;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float blend = smoothSpeed * Time.deltaTime;

        // Rotate the camera's offset with the player.
        Vector3 desiredPosition =
            target.position + target.TransformDirection(offset);

        Quaternion desiredRotation =
            target.rotation * rotationOffset;

        transform.position = Vector3.Lerp(
            transform.position, desiredPosition, blend);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, desiredRotation, blend);
    }
}