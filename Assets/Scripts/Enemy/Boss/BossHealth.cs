using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int maxHealth = 50;
    public float invincibilityTime = 1f;
    
    [Header("Referências")]
    public BossController bossController;
    
    [Header("Efeito de Dano")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("O boss levou dano e está com " + currentHealth + " de vida.");
        if (isDead || isInvincible) return;
        
        currentHealth -= damage;
        
        // Inicia o efeito de flash branco
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }
        
        if (currentHealth <= 0)
        {
            isDead = true;
            bossController.StartDeath();
        }
        else
        {
            StartCoroutine(InvincibilityFrame());
        }
    }

    IEnumerator InvincibilityFrame()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    IEnumerator FlashWhite()
    {
        // Muda para a cor branca
        spriteRenderer.color = flashColor;
        
        // Espera um curto período
        yield return new WaitForSeconds(flashDuration);
        
        // Volta para a cor original
        spriteRenderer.color = originalColor;
    }

    // Métodos úteis para UI ou outros sistemas
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
}