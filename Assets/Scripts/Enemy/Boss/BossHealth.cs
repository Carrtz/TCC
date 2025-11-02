using UnityEngine;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public int maxHealth = 50;
    public float invincibilityTime = 1f;
    
    [Header("Referências")]
    public BossController bossController;
    
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("O boss levou dano e está com " + currentHealth + " de vida.");
        if (isDead || isInvincible) return;
        
        currentHealth -= damage;
        
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