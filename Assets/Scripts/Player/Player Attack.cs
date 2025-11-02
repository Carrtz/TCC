using TarodevController;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector3 attackRange = new Vector3(1, 1, 1);
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask breakableLayer;
    [SerializeField] private float attackRate = 2f;

    public static event Action OnAttack;

    private PlayerController playerController;
    private float nextAttackTime = 0f;
    private Vector3 initialAttackPointLocal;
    private int attackCooldownFrames = 0;
    private const int COOLDOWN_FRAMES = 10;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        initialAttackPointLocal = attackPoint.localPosition;
    }

    void Update()
    {
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
                attackCooldownFrames = COOLDOWN_FRAMES;
            }
        }
    }

    public void Attack()
    {
        Debug.Log("Player is attacking!");
        OnAttack?.Invoke();

        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(
            attackPoint.position, attackRange, 0, enemyLayer);

        foreach (Collider2D hit in hitObjects)
        {
            TryDamageBoss(hit);

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            FlyingEnemy flyingEnemy = hit.GetComponent<FlyingEnemy>();
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Hit enemy: {hit.name}");

                Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                
                if (flyingEnemy != null)
                {
                    flyingEnemy.ApplyKnockback(knockbackDir);
                }
                
                if (enemy != null)
                {
                    enemy.ApplyKnockback(knockbackDir);
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

    private void TryDamageBoss(Collider2D colliderHit)
    {
        BossHealth bossHealth = colliderHit.GetComponent<BossHealth>();

        if (bossHealth != null)
        {
            bossHealth.TakeDamage(1);
        }
    }

    public bool CanAttack()
    {
        return attackCooldownFrames <= 0 && Time.time >= nextAttackTime;
    }

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