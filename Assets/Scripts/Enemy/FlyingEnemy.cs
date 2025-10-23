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
    public LayerMask wallLayer = 1 << 7;

    [Header("Player Push Settings")]
    public float playerPushForce = 10f;
    public float pushCheckDistance = 0.5f;

    private GameObject player;
    private Rigidbody2D rb;
    private Rigidbody2D playerRb;
    private WallCollisionHandler wallCollisionHandler;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackDirection;
    private Vector2 movementDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        wallCollisionHandler = gameObject.AddComponent<WallCollisionHandler>();
        wallCollisionHandler.wallLayer = wallLayer;
    }

    void Update()
    {
        if (player == null) return;
        HandleKnockback();

        if (!isKnockedBack && !wallCollisionHandler.IsAgainstWall())
        {
            if (chase)
                Chase();
            else
                ReturnEnemy();
        }

        // Verificar se o jogador está em cima e empurrar
        CheckAndPushPlayer();
    }

    void CheckAndPushPlayer()
    {
        if (player == null || playerRb == null) return;

        // Verificar se o jogador está em cima do inimigo
        Vector2 checkDirection = Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, pushCheckDistance, LayerMask.GetMask("Player"));

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            PushPlayerToSide();
        }
    }

    void PushPlayerToSide()
    {
        if (playerRb == null) return;

        // Determinar a direção do empurrão (esquerda ou direita baseado na posição relativa)
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);

        // Aplicar força lateral no jogador
        Vector2 pushForce = new Vector2(direction * playerPushForce, playerPushForce * 0.5f);
        playerRb.linearVelocity = new Vector2(pushForce.x, playerRb.linearVelocity.y);

        Debug.Log("Empurrando jogador para o lado! Direção: " + direction);
    }

    void HandleKnockback()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            // Para knockback se atingir parede
            if (wallCollisionHandler.IsAgainstWall())
            {
                isKnockedBack = false;
                knockbackTimer = 0f;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
            }
            else
            {
                rb.linearVelocity = knockbackDirection * knockbackForce * (knockbackTimer / knockbackDuration);
            }
        }
    }

    private void ReturnEnemy()
    {
        float distanceToStart = Vector2.Distance(transform.position, startingPoint.position);

        if (distanceToStart < 0.3f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        movementDirection = (startingPoint.position - transform.position).normalized;
        rb.linearVelocity = movementDirection * speed;
    }

    private void Chase()
    {
        movementDirection = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = movementDirection * speed;
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

    // Visualização do raio de detecção (apenas no Editor)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * pushCheckDistance);

        // Mostrar direção do empurrão
        if (Application.isPlaying && player != null)
        {
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(direction, 0.5f, 0) * 2f);
        }
    }
}