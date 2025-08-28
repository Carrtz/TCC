using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthFill; // imagem da barra cheia

    private void Start()
    {
        // Atualiza na inicialização
        UpdateHealthUI(playerHealth.GetCurrentHealth());

        // Se inscreve no evento de vida mudando
        playerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void UpdateHealthUI(int currentHealth)
    {
        float fillAmount = (float)currentHealth / playerHealth.GetMaxHealth();
        healthFill.fillAmount = fillAmount;
    }
}
