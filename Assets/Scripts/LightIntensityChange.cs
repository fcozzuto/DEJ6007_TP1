using UnityEngine;

public class LightIntensityChange : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float startDistance = 10f;
    [SerializeField] private float fullIntensityDistance = 1f;
    [SerializeField] private float exponentialStrength = 5f;

    // The Inspector supplies the light settings, so no startup setup is needed.
    void Start()
    {
    }

    // Increase the exit light intensity as the player gets closer, with the strongest rise near the light.
    void Update()
    {
        if (playerMovement == null)
            return;

        Vector3 offset = playerMovement.transform.position - transform.position;
        // Ignore the height difference so the raised light can still reach full brightness.
        offset.y = 0f;

        float distance = offset.magnitude;

        // Convert distance to a value from 0 at the outer edge to 1 at full brightness.
        float t = Mathf.InverseLerp(startDistance, fullIntensityDistance, distance);

        // Keep the curve between 0 and 1, with most of the increase close to the light.
        t = (Mathf.Exp(exponentialStrength * t) - 1f)
            / (Mathf.Exp(exponentialStrength) - 1f);

        GetComponent<Light>().intensity = Mathf.Lerp(100, 10000000, t);
    }
}
