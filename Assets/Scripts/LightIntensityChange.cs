using UnityEngine;

public class LightIntensityChange : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float startDistance = 10f;
    [SerializeField] private float fullIntensityDistance = 1f;
    [SerializeField] private float exponentialStrength = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement == null)
            return;

        Vector3 offset = playerMovement.transform.position - transform.position;
        offset.y = 0f; // Measure approach along the floor, ignoring the light's height.

        float distance = offset.magnitude;

        // 0 at or beyond Start Distance; 1 at or within Full Intensity Distance.
        float t = Mathf.InverseLerp(startDistance, fullIntensityDistance, distance);

        // Keep the endpoints at 0 and 1, with a sharper rise near the light.
        t = (Mathf.Exp(exponentialStrength * t) - 1f)
            / (Mathf.Exp(exponentialStrength) - 1f);

        GetComponent<Light>().intensity = Mathf.Lerp(100, 10000000, t);
    }
}
