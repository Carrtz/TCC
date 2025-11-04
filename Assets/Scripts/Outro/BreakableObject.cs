using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    // VARIÁVEIS DA PAREDE
    [SerializeField] private int hitsToBreak = 3;        // Quantidade de hits necessários para destruir
    [SerializeField] private float breakAnimationDelay = 1f; // Tempo de espera antes de destruir o objeto após a animação
    private int currentHits = 0;                         // Contador de hits que a parede já recebeu
    private Animator animator;                           // Referência ao componente Animator para controlar animações
    private bool isBroken = false;                       // Flag para evitar que a parede seja quebrada múltiplas vezes

    private void Awake()
    {
        // Obtém o componente Animator do objeto atual
        animator = GetComponent<Animator>();
    }

    // Chamado quando a parede recebe dano
    public void TakeDamage(int damage)
    {
        // Se a parede já está quebrada, ignora qualquer dano adicional
        if (isBroken) return;

        // Adiciona o dano recebido ao contador de hits
        currentHits += damage;

        // Se ainda não atingiu os hits necessários para quebrar
        if (currentHits < hitsToBreak)
        {
            // Mostra animação de dano
            ShowDamageAnimation();
        }

        // Se atingiu ou ultrapassou a quantidade de hits necessária
        if (currentHits >= hitsToBreak)
        {
            // Marca a parede como quebrada para evitar processamento adicional
            isBroken = true;
            // Executa a animação de quebra total da parede
            ShowBreakAnimation();
        }
    }

    // Controla a animação de dano parcial
    private void ShowDamageAnimation()
    {
        // Verifica se existe um Animator para evitar erros
        if (animator != null)
        {
            animator.SetTrigger("Damage"); // "Damage" = nome do Tiggrer do animator
        }
    }

    // Controla a animação de quebra total
    private void ShowBreakAnimation()
    {
        // Verifica se existe um Animator
        if (animator != null)
        {
            animator.SetBool("Break", true); // "Break" = nome do Bool do animator

            // Desativa o Collider imediatamente
            DisableCollider();

            // Agenda a destruição do objeto após o tempo da animação
            Invoke("DestroyWall", breakAnimationDelay);
        }
        else
        {
            // Se não há Animator, destrói o objeto
            DestroyWall();
        }
    }

    // Desativa os colliders do objeto
    private void DisableCollider()
    {
        // Tenta obter o componente Collider2D (para jogos 2D)
        Collider2D collider2D = GetComponent<Collider2D>();

        // Se existe um Collider2D, desativa ele
        if (collider2D != null)
        {
            collider2D.enabled = false;
        }
    }

    // Destroi o objeto da parede
    private void DestroyWall()
    {
        Destroy(gameObject);
    }

    // Verifica se um parâmetro existe no Animator
    private bool AnimatorHasParameter(string paramName)
    {
        // Se não há Animator, retorna falso
        if (animator == null) return false;

        // Percorre todos os parâmetros do Animator
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            // Se encontrar um parâmetro com o nome especificado, retorna true
            if (param.name == paramName)
            {
                return true;
            }
        }
        // Se não encontrou o parâmetro, retorna false
        return false;
    }
}