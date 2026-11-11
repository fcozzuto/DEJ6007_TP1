using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
            enemy.gameObject.SetActive(false);
    }
}