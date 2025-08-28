using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject hitEffect;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Efeito de hit
        if (hitEffect != null && currentHealth > 0)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        // Barra de saúde visual
        float healthPercent = (float)currentHealth / maxHealth;
        Vector3 barPosition = transform.position + Vector3.up * 1f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(barPosition - Vector3.right * 0.5f, barPosition - Vector3.right * 0.5f + Vector3.right * healthPercent);
    }
}