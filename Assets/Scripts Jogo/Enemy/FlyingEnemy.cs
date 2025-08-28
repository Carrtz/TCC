using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    public float speed;
    public bool chase = false;
    public Transform startingPoint;
    public int contactDamage = 1;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    private GameObject player;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null) return;

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
            }
            transform.Translate(knockbackDirection * knockbackForce * Time.deltaTime, Space.World);
        }
        else
        {
            if (chase)
                Chase();
            else
                ReturnEnemy();
        }

        Flip();
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
            // Verifica se o jogador está aparando o contato
            if (playerParry != null && playerParry.CanBlockAttack(transform.position))
            {
                // Contato bloqueado - aplica knockback no inimigo
                Vector2 knockbackDir = (transform.position - playerObject.transform.position).normalized;
                ApplyKnockback(knockbackDir);
                Debug.Log("Contato com inimigo voador bloqueado!");
            }
            else
            {
                // Causa dano por contato
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }

    private void ReturnEnemy()
    {
        transform.position = Vector2.MoveTowards(transform.position, startingPoint.position, speed * Time.deltaTime);
    }

    private void Chase()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void Flip()
    {
        if (player != null && transform.position.x > player.transform.position.x)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public void ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        knockbackDirection = direction.normalized;
    }
}