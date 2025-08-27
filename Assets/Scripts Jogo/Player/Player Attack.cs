using TarodevController;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask breakableLayer;
    [SerializeField] private float attackRate = 2f;

    private PlayerController playerController;
    private float nextAttackTime = 0f;
    private Vector3 initialAttackPointLocal;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        if (playerController == null)
        {
            Debug.LogError("PlayerAttack: PlayerController not found on this GameObject! Make sure it's attached.");
            enabled = false;
            return;
        }

        if (attackPoint == null)
        {
            Debug.LogError("PlayerAttack: Attack Point Transform not assigned!");
            enabled = false;
            return;
        }

        // Store the initial local position of the attack point
        initialAttackPointLocal = attackPoint.localPosition;
    }

    void Update()
    {
        // Check if enough time has passed since the last attack
        if (Time.time >= nextAttackTime)
        {
            // Use the input from the PlayerController or a specific key
            if (Input.GetKeyDown(KeyCode.F))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void Attack()
    {
        Debug.Log("Player is attacking!");

        // Find all colliders within the attack range
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        // Iterate through all found colliders
        foreach (Collider2D hit in hitObjects)
        {
            // Check if the object is an enemy using the enemyLayer mask
            if (((1 << hit.gameObject.layer) & enemyLayer) != 0)
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                FlyingEnemy flyingEnemy = hit.GetComponent<FlyingEnemy>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                    Debug.Log($"Hit enemy: {hit.name}");

                    // Aplica knockback se for um FlyingEnemy
                    if (flyingEnemy != null)
                    {
                        // Calcula a direção do knockback (do player para o inimigo)
                        Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                        flyingEnemy.ApplyKnockback(knockbackDir);
                    }
                }
            }

            // Check if the object is a breakable wall using the breakableLayer mask
            if (((1 << hit.gameObject.layer) & breakableLayer) != 0)
            {
                // Try to get the BreakableWall component
                BreakableWall wall = hit.GetComponent<BreakableWall>();
                if (wall != null)
                {
                    wall.TakeDamage(1); // Or use a separate damage value for walls
                    Debug.Log($"Hit breakable wall: {hit.name}");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // Draw a wire sphere in the editor to visualize the attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}