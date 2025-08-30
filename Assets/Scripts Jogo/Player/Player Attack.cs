using TarodevController;
using UnityEngine;
using System;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector3 attackRange = new Vector3(1, 1, 1);
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask breakableLayer;
    [SerializeField] private float attackRate = 2f;

    // Evento para notificar quando o jogador ataca
    public static event Action OnAttack;

    private PlayerController playerController;
    private float nextAttackTime = 0f;
    private Vector3 initialAttackPointLocal;
    private int attackCooldownFrames = 0; // Contador de frames de cooldown
    private const int COOLDOWN_FRAMES = 10; // Cooldown de 10 frames

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        initialAttackPointLocal = attackPoint.localPosition;
    }

    void Update()
    {
        // Atualiza o cooldown baseado em frames
        if (attackCooldownFrames > 0)
        {
            attackCooldownFrames--;
        }

        if (Time.time >= nextAttackTime && attackCooldownFrames <= 0)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
                attackCooldownFrames = COOLDOWN_FRAMES; // Inicia o cooldown de frames
            }
        }
    }

    public void Attack()
    {
        Debug.Log("Player is attacking!");

        // Dispara o evento de ataque para a animacao
        OnAttack?.Invoke();

        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackPoint.position, attackRange, 0);

        foreach (Collider2D hit in hitObjects)
        {
            if (((1 << hit.gameObject.layer) & enemyLayer) != 0)
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                FlyingEnemy flyingEnemy = hit.GetComponent<FlyingEnemy>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                    Debug.Log($"Hit enemy: {hit.name}");

                    if (flyingEnemy != null)
                    {
                        Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                        flyingEnemy.ApplyKnockback(knockbackDir);
                    }
                }
            }

            if (((1 << hit.gameObject.layer) & breakableLayer) != 0)
            {
                BreakableWall wall = hit.GetComponent<BreakableWall>();
                if (wall != null)
                {
                    wall.TakeDamage(1);
                    Debug.Log($"Hit breakable wall: {hit.name}");
                }
            }
        }
    }

    // Método para verificar se o jogador pode atacar (útil para UI de cooldown)
    public bool CanAttack()
    {
        return attackCooldownFrames <= 0 && Time.time >= nextAttackTime;
    }

    // Método para obter o progresso do cooldown (0 a 1, útil para UI)
    public float GetCooldownProgress()
    {
        if (attackCooldownFrames <= 0) return 1f;
        return 1f - ((float)attackCooldownFrames / COOLDOWN_FRAMES);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackRange);
    }
}