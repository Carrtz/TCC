using UnityEngine;

public class DeadlyBlock : MonoBehaviour
{
    public int damageAmount = 100; // Dano suficiente para matar

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other.gameObject);
        }
    }

    private void HandlePlayerCollision(GameObject playerObject)
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            // Causa dano fatal
            playerHealth.TakeDamage(damageAmount);
        }
    }
}