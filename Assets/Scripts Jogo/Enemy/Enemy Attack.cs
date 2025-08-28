using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damage = 1;
    public float attackRange = 1f;
    public Transform attackPoint;

    public void PerformAttack()
    {
        // Detecta se atingiu o jogador
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, LayerMask.GetMask("Player"));

        foreach (Collider2D player in hitPlayers)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            PlayerParry playerParry = player.GetComponent<PlayerParry>();

            if (playerHealth != null)
            {
                // Verifica se o jogador está aparando
                if (playerParry != null && playerParry.CanBlockAttack(attackPoint.position))
                {
                    // Ataque foi bloqueado, não causa dano
                    Debug.Log("Ataque bloqueado pelo jogador!");
                }
                else
                {
                    // Ataque acertou o jogador
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}