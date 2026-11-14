using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    // Kill an enemy touched by the sword while this hitbox is enabled by the attack animation.
    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();

        if (enemy != null)
            enemy.Die();
    }
}