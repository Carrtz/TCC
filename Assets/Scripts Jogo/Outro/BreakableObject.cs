using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private int hitsToBreak = 3;
    private int currentHits = 0;
    private Animator animator;

    // Chamada quando o script for iniciado
    private void Awake()
    {
        // Obtém o componente Animator do objeto
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHits += damage;
        Debug.Log($"Parede atingida! Hits: {currentHits}/{hitsToBreak}");

        // Se a parede não foi destruída ainda, mostra a animação de dano
        if (currentHits < hitsToBreak)
        {
            ShowDamageAnimation();
        }

        // Se atingiu ou ultrapassou os hits necessários para quebrar a parede
        if (currentHits >= hitsToBreak)
        {
            DestroyWall();
        }
    }

    // Método que inicia a animação de dano
    private void ShowDamageAnimation()
    {
        // Verifica se a animação de dano ainda não foi executada
        if (animator != null)
        {
            animator.SetTrigger("hit");  // "Damaged" é o nome do Trigger no Animator
        }
    }

    private void DestroyWall()
    {
        // Destroi o objeto após a animação de quebra, se necessário, pode esperar um pouco
        Destroy(gameObject);
    }
}