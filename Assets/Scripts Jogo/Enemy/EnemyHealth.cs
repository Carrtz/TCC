using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject hitEffect;

    private int currentHealth;
    private Animator animator; // Referência ao Animator do inimigo
    private bool isHit = false; // Flag para verificar se o inimigo está sendo atingido

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>(); // Obtém o Animator do inimigo
    }

    public void TakeDamage(int damage)
    {
        // Verifica se o inimigo já está sendo atingido (não pode ser atingido enquanto na animação de dano)
        if (isHit)
            return;

        // Marca o inimigo como "sendo atingido" e inicia a animação de dano
        isHit = true;

        currentHealth -= damage;

        // Efeito de hit
        if (hitEffect != null && currentHealth > 0)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        // Aciona a animação de dano
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("Damaged"); // Aciona o Trigger "Damaged"
        }

        // Inicia a corrotina que vai liberar o inimigo para receber dano novamente após a animação
        if (currentHealth > 0)
        {
            StartCoroutine(ResetHitStateAfterAnimation());
        }
        else
        {
            Die();
        }
    }

    private IEnumerator ResetHitStateAfterAnimation()
    {
        // Espera o tempo da animação de hit (ajuste o tempo conforme sua animação)
        yield return new WaitForSeconds(1f); // Por exemplo, 1 segundo de animação

        // Libera o inimigo para receber dano novamente
        isHit = false;
    }

    private void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Aciona a animação de morte (se você tiver)
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Aqui você pode ter um trigger de "Die", se desejar.
        }

        Destroy(gameObject); // Destroi o inimigo
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