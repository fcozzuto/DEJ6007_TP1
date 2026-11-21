using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AnimationSound : MonoBehaviour
{
    // Play the sound assigned to this object's audio source.
    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }

    // Cut off a longer sound when the animated movement ends.
    public void StopSound()
    {
        GetComponent<AudioSource>().Stop();
    }
}