using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private float blinkTimer = 0f;
    private bool isVisible = true;

    public event System.Action OnPlayerDeath;
    public event System.Action<int> OnHealthChanged;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);

        // Se o SpriteRenderer não foi atribuído pelo Inspector, tenta encontrar no próprio objeto
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer não encontrado! Atribua manualmente pelo Inspector.");
            }
        }
    }

    private void Update()
    {
        if (isInvincible && invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            
            // Controle do efeito de piscar
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0)
            {
                ToggleVisibility();
                blinkTimer = blinkInterval;
            }

            if (invincibilityTimer <= 0)
            {
                EndInvincibility();
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
            StartInvincibility();
        }
    }

    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        blinkTimer = blinkInterval;
        isVisible = true;
        
        // Garante que o sprite está visível no início da invencibilidade
        if (spriteRenderer != null)
        {
            SetSpriteAlpha(1f);
        }
    }

    private void EndInvincibility()
    {
        isInvincible = false;
        
        // Garante que o sprite fique totalmente visível ao final da invencibilidade
        if (spriteRenderer != null)
        {
            SetSpriteAlpha(1f);
        }
    }

    private void ToggleVisibility()
    {
        if (spriteRenderer == null) return;

        isVisible = !isVisible;
        float alpha = isVisible ? 1f : 0.3f;
        SetSpriteAlpha(alpha);
    }

    private void SetSpriteAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    public void SetInvincible(bool invincible)
    {
        if (invincible)
        {
            StartInvincibility();
            invincibilityTimer = float.MaxValue;
        }
        else
        {
            EndInvincibility();
            invincibilityTimer = 0f;
        }
    }

    private void Die()
    {
        OnPlayerDeath?.Invoke();
        SceneManager.LoadScene("Death");
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

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}