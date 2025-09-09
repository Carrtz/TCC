using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    public float speed;
    public bool chase = false;
    public Transform startingPoint;
    public int contactDamage = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.5f;
    public float knockbackDecay = 0.7f;

    private GameObject player;
    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackDirection;
    private Vector2 movementDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null) return;

        HandleKnockback();
        
        if (!isKnockedBack)
        {
            if (chase)
                Chase();
            else
                ReturnEnemy();
        }

        Flip();
    }

    void HandleKnockback()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                rb.linearVelocity = movementDirection * speed;
            }
            else
            {
                float decayFactor = Mathf.Pow(knockbackDecay, Time.deltaTime * 10f);
                rb.linearVelocity = knockbackDirection * knockbackForce * (knockbackTimer / knockbackDuration);
            }
        }
    }

    private void ReturnEnemy()
    {
        movementDirection = (startingPoint.position - transform.position).normalized;
        rb.linearVelocity = movementDirection * speed;
    }

    private void Chase()
    {
        movementDirection = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = movementDirection * speed;
    }

    private void Flip()
    {
        if (player != null && transform.position.x > player.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other.gameObject);
        }
    }

    private void HandlePlayerCollision(GameObject playerObject)
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        PlayerParry playerParry = playerObject.GetComponent<PlayerParry>();

        if (playerHealth != null)
        {
            if (playerParry != null && playerParry.CanBlockAttack(transform.position))
            {
                Vector2 knockbackDir = (transform.position - playerObject.transform.position).normalized;
                ApplyKnockback(knockbackDir);
                Debug.Log("Contato com inimigo voador bloqueado!");
            }
            else
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }

    public void ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        knockbackDirection = direction.normalized;
    }
}