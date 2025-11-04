using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float invincibilityTime = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private int maxHealingUses = 2;
    [SerializeField] private int healAmount = 4;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentHealth;
    private int remainingHealingUses;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private float blinkTimer = 0f;
    private bool isVisible = true;

    public event System.Action OnPlayerDeath;
    public event System.Action<int> OnHealthChanged;
    public event System.Action<int> OnHealingUsed;

    private void Start()
    {
        currentHealth = maxHealth;
        remainingHealingUses = maxHealingUses;
        OnHealthChanged?.Invoke(currentHealth);

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseHealing();
        }
    }

    private void UseHealing()
    {
        if (remainingHealingUses > 0 && currentHealth < maxHealth)
        {
            Heal(healAmount);
            remainingHealingUses--;
            
            Debug.Log($"Curou {healAmount} de vida! Usos restantes: {remainingHealingUses}");
        }
        else if (remainingHealingUses <= 0)
        {
            Debug.Log("Sem usos de cura restantes!");
        }
        else if (currentHealth >= maxHealth)
        {
            Debug.Log("Vida já está no máximo!");
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
        
        if (spriteRenderer != null)
        {
            SetSpriteAlpha(1f);
        }
    }

    private void EndInvincibility()
    {
        isInvincible = false;
        
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

    public void AddHealingUses(int amount)
    {
        remainingHealingUses = Mathf.Min(remainingHealingUses + amount, maxHealingUses);
    }

    public void ResetHealingUses()
    {
        remainingHealingUses = maxHealingUses;
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

    public int GetRemainingHealingUses()
    {
        return remainingHealingUses;
    }

    public int GetMaxHealingUses()
    {
        return maxHealingUses;
    }
}