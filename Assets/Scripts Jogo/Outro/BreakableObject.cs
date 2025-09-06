using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private int hitsToBreak = 3;
    private int currentHits = 0;
    private Animator animator;

    // Chamada quando o script for iniciado
    private void Awake()
    {
        // Obt�m o componente Animator do objeto
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHits += damage;
        Debug.Log($"Parede atingida! Hits: {currentHits}/{hitsToBreak}");

        // Se a parede n�o foi destru�da ainda, mostra a anima��o de dano
        if (currentHits < hitsToBreak)
        {
            ShowDamageAnimation();
        }

        // Se atingiu ou ultrapassou os hits necess�rios para quebrar a parede
        if (currentHits >= hitsToBreak)
        {
            DestroyWall();
        }
    }

    // M�todo que inicia a anima��o de dano
    private void ShowDamageAnimation()
    {
        // Verifica se a anima��o de dano ainda n�o foi executada
        if (animator != null)
        {
            animator.SetTrigger("hit");  // "Damaged" � o nome do Trigger no Animator
        }
    }

    private void DestroyWall()
    {
        // Destroi o objeto ap�s a anima��o de quebra, se necess�rio, pode esperar um pouco
        Destroy(gameObject);
    }
}