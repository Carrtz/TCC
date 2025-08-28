using UnityEngine;

public class EnemyGround : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3f;
    public float chaseRange = 5f;
    public float attackRange = 1f;

    [Header("References")]
    public Transform startingPoint;
    public LayerMask groundLayer;

    private GameObject player;
    private bool isChasing = false;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    [Header("Damage Settings")]
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float damageCooldown = 1f;
    private float lastDamageTime;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        if (startingPoint == null)
        {
            startingPoint = new GameObject("StartingPoint").transform;
            startingPoint.position = transform.position;
        }
    }

    void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();
        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            Chase();
        }
        else
        {
            ReturnToStart();
        }
    }

    private void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    private void Chase()
    {
        Vector2 targetPosition = new Vector2(player.transform.position.x, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }

    private void ReturnToStart()
    {
        Vector2 targetPosition = new Vector2(startingPoint.position.x, rb.position.y);
        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);
    }

    private void FlipSprite()
    {
        if (player == null) return;

        bool shouldFaceRight = (player.transform.position.x > transform.position.x);

        if (shouldFaceRight != isFacingRight)
        {
            isFacingRight = shouldFaceRight;
            transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Time.time > lastDamageTime + damageCooldown)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastDamageTime = Time.time;
            }
        }
    }
}