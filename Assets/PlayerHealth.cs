using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1f;

    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    public event System.Action OnPlayerDeath;
    public event System.Action<int> OnHealthChanged;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void Update()
    {
        if (isInvincible && invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            invincibilityTimer = invincibilityTime;
        }
    }

    // Método para definir invencibilidade externamente
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        if (invincible)
        {
            invincibilityTimer = float.MaxValue; // Timer muito longo
        }
        else
        {
            invincibilityTimer = 0f;
        }
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();
        gameObject.SetActive(false);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public bool IsInvincible()
    {
        return isInvincible;
    }
}