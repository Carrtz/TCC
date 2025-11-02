using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Projectile hit: {collision.name}");
        
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("Projectile hit player!");
            Destroy(gameObject);
        }
    }
}