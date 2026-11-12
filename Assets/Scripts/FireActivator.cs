using UnityEngine;

public class FireActivator : MonoBehaviour
{
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Animator lavaHiddenPlatform;

    private bool isLit;

    private void Awake()
    {
        if (fireEffect != null)
            fireEffect.SetActive(false);
    }

    public void Ignite()
    {
        if (isLit)
            return;

        if (fireEffect == null || gateAnimator == null)
        {
            Debug.LogError("Assign Fire Effect and Gate Animator on this object.", this);
            return;
        }

        isLit = true;
        fireEffect.SetActive(true);
        gateAnimator.Play("Base Layer.Lava Room Gate motion", 0, 0f);

        if (lavaHiddenPlatform != null && this.gameObject.name == "Lava_Room_Brazier")
            lavaHiddenPlatform.Play("Base Layer.Hidden Lava Platform motion", 0, 0f);
    }
}