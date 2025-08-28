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

   

      
        initialAttackPointLocal = attackPoint.localPosition;
    }

    void Update()
    {
        
        if (Time.time >= nextAttackTime)
        {
           
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

      
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

     
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

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}